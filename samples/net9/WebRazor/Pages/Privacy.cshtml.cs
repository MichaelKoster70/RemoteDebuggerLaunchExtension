// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebRazorApp.Pages
{
   public class PrivacyModel : PageModel
   {
      private readonly ILogger<PrivacyModel> _logger;

      public PrivacyModel(ILogger<PrivacyModel> logger)
      {
         _logger = logger;
      }

      public void OnGet()
      {
      }
   }
}