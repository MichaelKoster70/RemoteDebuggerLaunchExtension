// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Diagnostics.Contracts;
using System.IO;
using Microsoft;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Helper class providing data and argument validation services.
   /// </summary>
   internal static class ThrowIf
   {
      /// <summary>
      /// Throws an exception if the supplied component is null.
      /// </summary>
      /// <typeparam name="T">The type of component.</typeparam>
      /// <param name="component">The component instance to validate.</param>
      /// <exception cref="InvalidOperationException"></exception>
      public static void NotPresent<T>(T component)
      {
         if (component == null)
         {
            var componentType = typeof(T);
            throw new InvalidOperationException($"{componentType.FullName} is not present");
         }
      }

      /// <summary>
      /// Throws an exception if the supplied file does not exist.
      /// </summary>
      /// <param name="filePath">The absolute path to the file to validate.</param>
      /// <param name="exceptionMessage">The exception message to use.</param>
      /// <exception cref="RemoteDebuggerLauncherException"></exception>
      public static void FileNotPresent(string filePath, string exceptionMessage)
      {
         if (!File.Exists(filePath))
         {
            throw new RemoteDebuggerLauncherException(exceptionMessage);
         }
      }

      /// <summary>
      /// Throws an exception if the supplied file does not exist.
      /// </summary>
      /// <param name="filePath">The absolute path to the file to validate.</param>
      /// <param name="exceptionMessage">The exception message to use.</param>
      /// <exception cref="RemoteDebuggerLauncherException"></exception>
      public static void EmptyFilePath(string filePath, string exceptionMessage)
      {
         if (string.IsNullOrWhiteSpace(filePath))
         {
            throw new RemoteDebuggerLauncherException(exceptionMessage);
         }
      }

      /// <summary>
      /// Throws an <see cref="ArgumentNullException" /> if the <paramref name="value" /> is null, otherwise passes it through
      /// </summary>
      /// <typeparam name="T"></typeparam>
      /// <param name="value">The argument value to check.</param>
      /// <param name="name">The parameter name to report.</param>
      /// <exception cref="ArgumentNullException">If <paramref name="value" /> is null.</exception>
      [ContractArgumentValidator]
      public static void ArgumentNull<T>([ValidatedNotNull] T value, string name) where T : class
      {
         if (value == null)
         {
            throw new ArgumentNullException(name);
         }
      }

      /// <summary>
      /// Throws an <see cref="ArgumentException" /> if the <paramref name="value" /> is null or an empty string.
      /// </summary>
      /// <typeparam name="T"></typeparam>
      /// <param name="value">The argument value to check.</param>
      /// <param name="name">The parameter name to report.</param>
      /// <exception cref="ArgumentException">If <paramref name="value" /> is null.</exception>
      [ContractArgumentValidator]
      public static void ArgumentNullOrEmpty([ValidatedNotNull] string value, string name)
      {
         if (string.IsNullOrEmpty(value))
         {
            throw new ArgumentException(name);
         }
      }
   }
}
