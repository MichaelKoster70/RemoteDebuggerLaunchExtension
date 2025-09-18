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
   /// Console helper that scans a folder using <see cref="FileScanner"/> and writes the
   /// <see cref="ScanResult"/> as JSON to stdout.
   /// </summary>
   internal sealed class FileScannerConsoleOutput : FileScanner
   {
      /// <summary>
      /// Scans <paramref name="startFolder"/> and writes the result as JSON to Console.Out.
      /// </summary>
      /// <param name="startFolder">Folder to scan.</param>
      public void ScanAndPrintJson(string startFolder)
      {
         var result = Scan(startFolder);

         // Use Newtonsoft.Json for serialization to support both target frameworks.
         var json = JsonConvert.SerializeObject(result, Formatting.None);

         Console.Out.WriteLine(json);
      }
   }
}
