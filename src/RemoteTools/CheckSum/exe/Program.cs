// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;

namespace RemoteDebuggerLauncher.CheckSum
{
   internal static class Program
   {
      static int Main(string[] args)
      {
         // verify arguments, we expect exactly one argument: the path to scan
         if (args == null || args.Length == 0)
         {
            return 1;
         }

         var scanner = new DirectoryScannerConsoleOutput(args[0]);
         scanner.ComputeHashesAndPrintAsJson();

         return 0;
      }
   }
}
