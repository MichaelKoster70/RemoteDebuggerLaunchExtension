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

         /// <summary>Name of the STS channel expected by the .NET install scripts.</summary>
         public const string ChannelSTS = "STS";

         /// <summary>Name of the 7.0 channel expected by the .NET install scripts.</summary>
         public const string Channel70 = "7.0";

         /// <summary>Name of the 6.0 channel expected by the .NET install scripts.</summary>
         public const string Channel60 = "6.0";

         /// <summary>Name of complete runtime expected by the .NET install scripts.</summary>
         public const string RuntimeNet = "dotnet";

         /// <summary>Name of the ASP.NET runtime expected by the .NET install scripts.</summary>
         public const string RuntimeAspNet = "aspnetcore";
      }
   }
}
