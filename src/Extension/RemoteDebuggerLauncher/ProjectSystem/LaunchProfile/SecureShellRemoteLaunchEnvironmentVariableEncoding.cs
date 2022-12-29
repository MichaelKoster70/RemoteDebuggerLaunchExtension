using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem.VS.PropertyPages.Designer;

namespace RemoteDebuggerLauncher.ProjectSystem.LaunchProfile
{
   //[Export(typeof(INameValuePairListEncoding))]
   [ExportMetadata("Encoding", "SecureShellRemoteLaunchEnvironmentVariableEncoding")]
   internal sealed class SecureShellRemoteLaunchEnvironmentVariableEncoding
   {
   }
}
