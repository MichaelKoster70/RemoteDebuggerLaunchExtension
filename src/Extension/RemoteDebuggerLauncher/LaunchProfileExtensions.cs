// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem.Debug;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Extensions methods for <see cref="ILaunchProfile"/> access.
   /// </summary>
   internal static class LaunchProfileExtensions
   {


      public static string QueryAuthentication(this ILaunchProfile launchProfile)
      {
         ThrowIf.NotPresent(launchProfile);
         if (launchProfile.OtherSettings.TryGetValue(SecureShellRemoteLaunchProfile.authenticationProperty, out var authentication))
         { 
            return authentication as string ?? SecureShellRemoteLaunchProfile.authenticationValues.privateKey; 
         }

         // Default to private key if launch profile has no value
         return SecureShellRemoteLaunchProfile.authenticationValues.privateKey;
      }

      public static string QueryHostName(this ILaunchProfile launchProfile)
      {
         ThrowIf.NotPresent(launchProfile);

         if (launchProfile.OtherSettings.TryGetValue(SecureShellRemoteLaunchProfile.hostNameProperty, out var hostName))
         {
            return hostName as string ?? string.Empty;
         }

         return string.Empty;
      }
      public static string QueryUserName(this ILaunchProfile launchProfile)
      {
         ThrowIf.NotPresent(launchProfile);

         if (launchProfile.OtherSettings.TryGetValue(SecureShellRemoteLaunchProfile.userNameProperty, out var userName))
         {
            return userName as string ?? string.Empty;
         }

         return string.Empty;
      }

      public static string QueryPrivateKey(this ILaunchProfile launchProfile)
      {
         ThrowIf.NotPresent(launchProfile);

         if (launchProfile.OtherSettings.TryGetValue(SecureShellRemoteLaunchProfile.privateKeyProperty, out var privateKeyFile))
         {
            return privateKeyFile as string ?? string.Empty;
         }

         return string.Empty;
      }
   }
}
