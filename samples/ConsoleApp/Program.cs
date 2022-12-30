// Simple console application printing out some OS information

using System.Collections;

Console.WriteLine("Hello from .NET\n");

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
