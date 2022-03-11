using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteDebuggerLauncher
{
   internal static class UnixPath
   {
      public static string Combine(string path1, string path2)
      {
         var combinedPath = Path.Combine(path1, path2);
         return combinedPath.Replace("\\", "/");
      }
   }
}
