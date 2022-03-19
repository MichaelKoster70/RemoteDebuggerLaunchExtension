// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Immutable;
using Microsoft.Build.Framework.XamlTypes;
using Microsoft.VisualStudio.ProjectSystem.Debug;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Class intended to hold the encryption/decription logic needed to store password in a launchsettings.json file.
   /// Implements the <see cref="" />
   /// </summary>
   /// <remarks>
   /// Implemented once the required interfaces are publically available
   /// </remarks>
   internal sealed class PasswordPropertyValueProvider // : ILaunchProfileExtensionValueProvider
   {
      public string OnGetPropertyValue(string propertyName, ILaunchProfile launchProfile, ImmutableDictionary<string, object> globalSettings, Rule rule)
      {
         throw new NotImplementedException();
      }

      public void OnSetPropertyValue(string propertyName, string propertyValue, IWritableLaunchProfile launchProfile, ImmutableDictionary<string, object> globalSettings, Rule rule)
      {
         throw new NotImplementedException();
      }
   }
}
