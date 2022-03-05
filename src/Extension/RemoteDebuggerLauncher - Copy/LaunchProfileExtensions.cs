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
            return authentication as string; 
         }

         // Default to private key if launch profile has no value
         return SecureShellRemoteLaunchProfile.authenticationValues.PrivateKey;
      }

      public static string QueryUserName(this ILaunchProfile launchProfile)
      {
         ThrowIf.NotPresent(launchProfile);

         if (launchProfile.OtherSettings.TryGetValue(SecureShellRemoteLaunchProfile.userNameProperty, out var userName))
         {
            return userName as string;
         }

         return string.Empty;
      }
   }
}
