using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteDebuggerLauncher
{
   internal interface IOptionsPageAccessor
   {
      string QueryUserName();

      string QueryPrivateKeyFilePath();

      AdapterProviderKind QueryAdapterProvider();

      string QueryPuttyInstallationPath();
   }
}
//DTE dte = (DTE)GetService(typeof(DTE));
//EnvDTE.Properties props = dte.get_Properties("My Category", "My Grid Page");
//int n = (int)props.Item("OptionInteger").Value;