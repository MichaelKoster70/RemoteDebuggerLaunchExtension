using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteDebuggerLauncher.Shared
{
   public static class Constants
   {
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
