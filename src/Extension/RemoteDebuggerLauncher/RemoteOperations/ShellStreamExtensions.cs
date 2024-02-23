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
   internal static class ShellStreamExtensions
   {
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
