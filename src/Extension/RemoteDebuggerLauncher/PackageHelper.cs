using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;

namespace RemoteDebuggerLauncher
{
   internal static class PackageHelper
   {
      public static TServiceInterface GetGlobalService<TServiceInterface, TServiceName>()
      {
         return (TServiceInterface)Package.GetGlobalService(typeof(TServiceName));
      }
   }
}
