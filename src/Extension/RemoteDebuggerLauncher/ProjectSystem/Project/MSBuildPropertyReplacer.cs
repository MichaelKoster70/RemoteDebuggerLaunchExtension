// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Service that replaces MSBuild properties $(...) in strings using CPS.
   /// Implements <see cref="IMSBuildPropertyReplacer"/>.
   /// </summary>
   [Export(typeof(IMSBuildPropertyReplacer))]
   [Shared(ExportContractNames.Scopes.ConfiguredProject)]
   internal sealed class MSBuildPropertyReplacer : IMSBuildPropertyReplacer
   {
      private static readonly Regex propertyRegex = new Regex(@"\$\(([^)]+)\)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
      private readonly ConfiguredProject configuredProject;

      [ImportingConstructor]
      public MSBuildPropertyReplacer(ConfiguredProject configuredProject)
      {
         ThrowIf.ArgumentNull(configuredProject, nameof(configuredProject));
         this.configuredProject = configuredProject;
      }

      /// <inheritdoc />
      [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "To prevent exceptions from being unhandled")]
      public async Task<string> ReplacePropertiesAsync(string input)
      {
         ThrowIf.ArgumentNull(input, nameof(input));

         var projectPropertiesProvider = configuredProject.Services.ProjectPropertiesProvider;
         ThrowIf.NotPresent(projectPropertiesProvider);

         var commonProps = projectPropertiesProvider.GetCommonProperties();

         // Start with the original input
         var result = input;
         
         // Replace each $(PropertyName) occurrence
         var matches = propertyRegex.Matches(input);
         foreach (Match match in matches)
         {
            var propertyName = match.Groups[1].Value;
            if (string.IsNullOrEmpty(propertyName))
            {
               continue;
            }

            try
            {
               // Get the evaluated property value, if available replace it
               var evaluated = await commonProps.GetEvaluatedPropertyValueAsync(propertyName);
               if (!string.IsNullOrEmpty(evaluated))
               {
                  result = result.Replace(match.Value, evaluated);
               }
            }
            catch (Exception)
            {
               // If evaluation fails, skip replacement
            }
         }

         return result;
      }
   }
}
