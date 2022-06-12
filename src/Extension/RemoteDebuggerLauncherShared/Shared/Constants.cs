// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

namespace RemoteDebuggerLauncher.Shared
{
   public static class Constants
   {
      public static class Debugger
      {
         public const string VersionLatest = "latest";
         public const string VersionVs2022 = "vs2022";
      }

      public static class Dotnet
      {
         /// <summary>Name of the current LTS channel expected by the .NET install scripts.</summary>
         public const string ChannelLTS = "LTS";

         /// <summary>Name of the Current channel expected by the .NET install scripts.</summary>
         public const string ChannelCurrent = "Current";

         public const string RuntimeNet = "dotnet";
         public const string RuntimeAspNet = "aspnetcore";
      }
   }
}
