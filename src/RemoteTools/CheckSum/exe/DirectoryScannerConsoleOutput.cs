// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using Newtonsoft.Json;

namespace RemoteDebuggerLauncher.CheckSum
{
   /// <summary>
   /// Console helper that scans a directory and writes the <see cref="ScanResult"/> as JSON to stdout.
   /// </summary>
   internal sealed class DirectoryScannerConsoleOutput : DirectoryScanner
   {
      /// <summary>
      /// Creates a new instance that scans below <paramref name="rootFolder"/>.
      /// </summary>
      /// <param name="rootFolder"></param>
      public DirectoryScannerConsoleOutput(string rootFolder) : base(rootFolder)
      {
         //EMPTY_BODY
      }

      /// <summary>
      /// Scans <paramref name="startFolder"/> and writes the result as JSON to Console.Out.
      /// </summary>
      /// <param name="startFolder">Folder to scan.</param>
      public int ComputeHashesAndPrintAsJson()
      {
         int returnCode = 0;
         var result = ComputeHashes();

         if (result.InaccessibleFiles.Count > 0)
         {
            // Indicate that not all files could be processed
            returnCode = 2;
         }

         // just convert the Hashes dictionary to JSON
         var json = JsonConvert.SerializeObject(result.Hashes, Formatting.None);

         Console.Out.WriteLine(json);

         return returnCode;
      }
   }
}
