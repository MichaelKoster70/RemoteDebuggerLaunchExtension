// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;
using static RemoteDebuggerLauncher.Infrastructure.NativeStructs;

namespace RemoteDebuggerLauncher.Infrastructure
{
   /// <summary>
   /// Class similar to <see cref="System.Diagnostics.Process"/> launching a process in a Pseudo Console.
   /// </summary>
   internal class PseudoConsoleProcess : IDisposable
   {
      private readonly ProcessStartInfo startInfo;
      private bool disposedValue;

      private SafeProcessHandle processHandle; // Native process handle
      private IntPtr pseudoConsoleHandle = IntPtr.Zero; // native Pseudo console handle
      private StreamReader standardOutput;
      private StreamWriter standardInput;

      public PseudoConsoleProcess(ProcessStartInfo startInfo)
      {
         this.startInfo = startInfo;
      }

      /// <summary>
      /// Gets the standard input stream.
      /// </summary>
      public StreamWriter StandardInput => standardInput;

      /// <summary>
      /// Gets the standard output stream.
      /// </summary>
      public StreamReader StandardOutput => standardOutput;

      /// <summary>
      /// Starts the specified start information.
      /// </summary>
      /// <param name="startInfo">The start information.</param>
      /// <returns>the <see cref="PseudoConsoleProcess"/> instance.</returns>
      public static PseudoConsoleProcess Start(ProcessStartInfo startInfo)
      {
         var process = new PseudoConsoleProcess(startInfo);
         process.Start();
         return process;
      }

      /// <summary>
      /// Starts the process resource
      /// </summary>
      public void Start()
      {
         try
         {
            // Stage 1: Create pipes (App -> PTY, PTY -> App)
#pragma warning disable CA2000 // Dispose objects before losing scope
            if (!NativeMethods.CreatePipe(out var inputReadSide, out var inputWriteSide, IntPtr.Zero, 0))
            {
               throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            if (!NativeMethods.CreatePipe(out var outputReadSide, out var outputWriteSide, IntPtr.Zero, 0))
            {
               DisposePipeHandles(inputReadSide, inputWriteSide);
               throw new Win32Exception(Marshal.GetLastWin32Error());
            }
#pragma warning restore CA2000 // Dispose objects before losing scope

            // Stage 2: Create the pseudo console. App writes to inputWriteSide, reads from outputReadSide.
            var size = new COORD(80, 25);
            var result = NativeMethods.CreatePseudoConsole(size, inputReadSide, outputWriteSide, 0, out pseudoConsoleHandle);

            // Close the ends we gave to the PTY, in all cases to avoid handle leaks
            DisposePipeHandles(inputReadSide, outputWriteSide);

            if (result != 0)
            {
               DisposePipeHandles(outputReadSide, inputWriteSide);
               pseudoConsoleHandle = IntPtr.Zero;
               throw new Win32Exception(result);
            }

            //Stage 3: Prepare STARTUPINFOEX with PTY attribute
            PrepareStartupInformation(pseudoConsoleHandle, out var startupInfoEx);

            // Stage 4: Launch the process in the pseudo console
            StringBuilder stringBuilder = BuildCommandLine(startInfo.FileName, startInfo.Arguments);

            bool success = NativeMethods.CreateProcess(
               null,
               stringBuilder,
               IntPtr.Zero, IntPtr.Zero,
               false,
               NativeConstants.EXTENDED_STARTUPINFO_PRESENT,
               IntPtr.Zero,
               null,
               ref startupInfoEx,
               out PROCESS_INFORMATION processInfo);

            // Cleanup the startup info attribute list
            CleanupStartupInformation(ref startupInfoEx);

            // close unneeded handles 
            if (!success)
            {
               DisposePipeHandles(outputReadSide, inputWriteSide);

               _ = NativeMethods.CloseHandle(processInfo.hProcess);
               _ = NativeMethods.CloseHandle(processInfo.hThread);

               throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            // Store the process handle
            processHandle = new SafeProcessHandle(processInfo.hProcess, true);
            _ = NativeMethods.CloseHandle(processInfo.hThread);

            // Create Streams for communication
            var inputPipe = new AnonymousPipeClientStream(PipeDirection.Out, inputWriteSide);
            var outputPipe = new AnonymousPipeClientStream(PipeDirection.In, outputReadSide);
            standardOutput = new StreamReader(outputPipe, Encoding.UTF8);
            standardInput = new StreamWriter(inputPipe, Encoding.UTF8) { AutoFlush = true };
         }
         catch
         {
            // Close pseudo console handle
            if (pseudoConsoleHandle != IntPtr.Zero)
            {
               NativeMethods.ClosePseudoConsole(pseudoConsoleHandle);
               pseudoConsoleHandle = IntPtr.Zero;
            }

            throw;
         }
      }

      private static void PrepareStartupInformation(IntPtr pseudoConsoleHandle, out STARTUPINFOEX startupInfoEx)
      {
         // Prepare the STARTUPINFOEX structure
         startupInfoEx = new STARTUPINFOEX();
         startupInfoEx.StartupInfo.cb = Marshal.SizeOf<STARTUPINFOEX>();

         // First call to get the required buffer size
         IntPtr lpSize = IntPtr.Zero;
         _ = NativeMethods.InitializeProcThreadAttributeList(IntPtr.Zero, 1, 0, ref lpSize);

         // Allocate memory for the attribute list
         startupInfoEx.lpAttributeList = Marshal.AllocHGlobal(lpSize);
         if (!NativeMethods.InitializeProcThreadAttributeList(startupInfoEx.lpAttributeList, 1, 0, ref lpSize))
         {
            Marshal.FreeHGlobal(startupInfoEx.lpAttributeList);
            throw new Win32Exception(Marshal.GetLastWin32Error());
         }

         // Set the pseudo console attribute in the attribute list
         if (!NativeMethods.UpdateProcThreadAttribute(startupInfoEx.lpAttributeList, 0,
             (IntPtr)NativeConstants.PROC_THREAD_ATTRIBUTE_PSEUDOCONSOLE,
             pseudoConsoleHandle, (IntPtr)IntPtr.Size,
             IntPtr.Zero, IntPtr.Zero))
         {
            Marshal.FreeHGlobal(startupInfoEx.lpAttributeList);
            throw new Win32Exception(Marshal.GetLastWin32Error());
         }
      }

      private static void CleanupStartupInformation(ref STARTUPINFOEX startupInfoEx)
      {
         if (startupInfoEx.lpAttributeList != IntPtr.Zero)
         {
            NativeMethods.DeleteProcThreadAttributeList(startupInfoEx.lpAttributeList);
            Marshal.FreeHGlobal(startupInfoEx.lpAttributeList);
            startupInfoEx.lpAttributeList = IntPtr.Zero;
         }
      }

      private static StringBuilder BuildCommandLine(string executableFileName, string arguments)
      {
         StringBuilder stringBuilder = new StringBuilder();
         string text = executableFileName.Trim();
         bool hasQuotes = text.StartsWith("\"", StringComparison.Ordinal) && text.EndsWith("\"", StringComparison.Ordinal);
         if (!hasQuotes)
         {
            _ = stringBuilder.Append('\"');
         }

         _ = stringBuilder.Append(text);
         if (!hasQuotes)
         {
            _ = stringBuilder.Append('\"');
         }

         if (!string.IsNullOrEmpty(arguments))
         {
            _ = stringBuilder.Append(' ');
            _ = stringBuilder.Append(arguments);
         }

         return stringBuilder;
      }

      private static void DisposePipeHandles(SafePipeHandle pipeHandle1, SafePipeHandle pipeHandle2)
      {
         pipeHandle1?.Dispose();
         pipeHandle2?.Dispose();
      }

      protected virtual void Dispose(bool disposing)
      {
         if (!disposedValue)
         {
            if (disposing)
            {
               // dispose managed state (managed objects)
               processHandle?.Dispose();
               standardInput?.Dispose();
               standardOutput?.Dispose();
            }

            // free unmanaged resources (unmanaged objects)
            if (pseudoConsoleHandle != IntPtr.Zero)
            {
               NativeMethods.ClosePseudoConsole(pseudoConsoleHandle);
               pseudoConsoleHandle = IntPtr.Zero;
            }
            disposedValue = true;
         }
      }

      ~PseudoConsoleProcess()
      {
         // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
         Dispose(disposing: false);
      }

      public void Dispose()
      {
         // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
         Dispose(disposing: true);
         GC.SuppressFinalize(this);
      }
   }
}
