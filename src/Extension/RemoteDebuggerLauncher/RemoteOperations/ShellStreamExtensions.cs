// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Text;
using System.Threading.Tasks;
using Renci.SshNet;

namespace RemoteDebuggerLauncher.RemoteOperations
{
   /// <summary>
   /// Extension methods for <see cref="ShellStream"/>.
   /// </summary>
   internal static class ShellStreamExtensions
   {
      /// <summary>
      /// Read available as an asynchronous operation.
      /// </summary>
      /// <param name="stream">The SSH shell stream.</param>
      /// <param name="timeout">The timeout to wait for data.</param>
      /// <returns>A Task(string) holding the read data.</returns>
      public static async Task<string> ReadAvailableAsync(this ShellStream stream, TimeSpan timeout)
      {
         StringBuilder sb = new StringBuilder();
         while(stream.DataAvailable)
         {
            _ = sb.Append(stream.Read());
            await Task.Delay(timeout);
         }

         return sb.ToString();
      }
   }
}
