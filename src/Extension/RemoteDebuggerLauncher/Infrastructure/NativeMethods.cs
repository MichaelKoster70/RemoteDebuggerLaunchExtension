// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;
using static RemoteDebuggerLauncher.Infrastructure.NativeStructs;

namespace RemoteDebuggerLauncher.Infrastructure
{
   /// <summary>
   /// Class holding native methods neeeded for Pseudo Console and process creation.
   /// See: https://learn.microsoft.com/en-us/windows/console/creating-a-pseudoconsole-session
   /// </summary>
   internal static class NativeMethods
   {
      [DllImport("kernel32.dll", SetLastError = true), DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
      internal static extern int CreatePseudoConsole(COORD size, SafePipeHandle hInput, SafePipeHandle hOutput, uint dwFlags, out IntPtr phPC);

      [DllImport("kernel32.dll", SetLastError = true), DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
      internal static extern void ClosePseudoConsole(IntPtr hPC);

      [DllImport("kernel32.dll", SetLastError = true), DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
      [return: MarshalAs(UnmanagedType.Bool)]
      internal static extern bool CreatePipe(out SafePipeHandle hReadPipe, out SafePipeHandle hWritePipe, IntPtr lpPipeAttributes, int nSize);

      [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "CreateProcessW"), DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
      [return: MarshalAs(UnmanagedType.Bool)]
      internal static extern bool CreateProcess(
         string lpApplicationName, 
         StringBuilder lpCommandLine, 
         IntPtr lpProcessAttributes, 
         IntPtr lpThreadAttributes, 
         bool bInheritHandles, 
         uint dwCreationFlags, 
         IntPtr lpEnvironment, 
         string lpCurrentDirectory, 
         ref STARTUPINFOEX lpStartupInfo, 
         out PROCESS_INFORMATION lpProcessInformation);

      [DllImport("kernel32.dll", SetLastError = true),DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
      [return: MarshalAs(UnmanagedType.Bool)]
      internal static extern bool InitializeProcThreadAttributeList(IntPtr lpAttributeList, int dwAttributeCount, int dwFlags, ref IntPtr lpSize);
      
      [DllImport("kernel32.dll", SetLastError = true), DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
      [return: MarshalAs(UnmanagedType.Bool)]
      internal static extern bool UpdateProcThreadAttribute(IntPtr lpAttributeList, uint dwFlags, IntPtr attribute, IntPtr lpValue, IntPtr cbSize, IntPtr lpPreviousValue, IntPtr lpReturnSize);
      
      [DllImport("kernel32.dll", SetLastError = true), DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
      internal static extern void DeleteProcThreadAttributeList(IntPtr lpAttributeList);

      [DllImport("kernel32.dll", SetLastError = true), DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
      [return: MarshalAs(UnmanagedType.Bool)]
      internal static extern bool CloseHandle(IntPtr hObject);
   }

   internal static class NativeConstants
   {
      public const int PROC_THREAD_ATTRIBUTE_PSEUDOCONSOLE = 0x00020016;
      public const uint EXTENDED_STARTUPINFO_PRESENT = 0x00080000;
   }

   /// <summary>
   /// Static class holding native structures.
   /// </summary>
   internal static class NativeStructs
   {

      [StructLayout(LayoutKind.Sequential)]
      public struct COORD
      { 
         public short X; 
         public short Y; 
         public COORD(short x, short y) { X = x; Y = y; }
      }

      [StructLayout(LayoutKind.Sequential)]
      public struct PROCESS_INFORMATION
      {
         public IntPtr hProcess, hThread;
         public int dwProcessId, dwThreadId;
      }

      [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
      public struct STARTUPINFOEX
      {
         public STARTUPINFO StartupInfo;
         public IntPtr lpAttributeList;
      }

      [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
      public struct STARTUPINFO
      {
         public int cb;
         public string lpReserved;
         public string lpDesktop;
         public string lpTitle;
         public int dwX, dwY, dwXSize, dwYSize, dwXCountChars, dwYCountChars, dwFillAttribute;
         public int dwFlags;
         public short wShowWindow;
         public short cbReserved2;
         public IntPtr lpReserved2, hStdInput, hStdOutput, hStdError;
      }
   }
}
