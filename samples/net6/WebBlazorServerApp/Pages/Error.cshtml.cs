﻿// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebBlazorServerApp.Pages
{
   [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
   [IgnoreAntiforgeryToken]
   public class ErrorModel : PageModel
   {
      public string? RequestId { get; set; }

      public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

      private readonly ILogger<ErrorModel> logger;

      public ErrorModel(ILogger<ErrorModel> logger)
      {
         this.logger = logger;
      }

      public void OnGet()
      {
         RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
      }
   }
}