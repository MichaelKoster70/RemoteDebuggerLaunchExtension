using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteDebuggerLauncher
{
   internal class AdapterLaunchConfiguration
   {
      public static AdapterLaunchConfiguration CreateSecureShell()
      {
         return new AdapterLaunchConfiguration("SSH");
      }

      private AdapterLaunchConfiguration (string adapter)
      {
         Adapter = adapter;
      }

      public string Version => "0.2.0";
      public string Adapter { get; private set; };

      public string AdapterArgs { get; private set; }
   }
}
