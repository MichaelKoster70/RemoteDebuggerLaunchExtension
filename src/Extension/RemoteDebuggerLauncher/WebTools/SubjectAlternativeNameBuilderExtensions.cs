// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace RemoteDebuggerLauncher.WebTools
{
   internal static class SubjectAlternativeNameBuilderExtensions
   {
      public static void AddDnsNameAndIpAddress(this SubjectAlternativeNameBuilder builder, string subject)
      {
         if (IPAddress.TryParse(subject, out var address) && address.AddressFamily == AddressFamily.InterNetwork)
         {
            builder.AddIpAddress(address);
         }
         else
         {
            var ipAddress = Dns.GetHostEntry(subject).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            if (ipAddress != null)
            {
               builder.AddIpAddress(ipAddress);
            }
            builder.AddDnsName(subject);
         }
      }
   }
}
