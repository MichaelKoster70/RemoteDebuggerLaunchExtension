// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Collections;

#pragma warning disable CA1303 // Do not pass literals as localized parameters
Console.WriteLine("Hello from .NET 8.0\n");

var os = Environment.OSVersion;
Console.WriteLine("Current OS Information:");
Console.WriteLine("Platform: {0:G}", os.Platform);
Console.WriteLine("Version String: {0}", os.VersionString);
Console.WriteLine("Version Information:");
Console.WriteLine("   Major: {0}", os.Version.Major);
Console.WriteLine("   Minor: {0}", os.Version.Minor);

// dump environment variables
var variable = Environment.GetEnvironmentVariables();
Console.WriteLine("\nEnvironment Variables:");
foreach (DictionaryEntry v in variable)
{
    Console.WriteLine("{0} = {1}", v.Key, v.Value);
}
#pragma warning restore CA1303
