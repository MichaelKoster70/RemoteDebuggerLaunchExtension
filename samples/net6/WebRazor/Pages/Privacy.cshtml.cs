// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebRazor.Pages;

public class PrivacyModel : PageModel
{
   private readonly ILogger<PrivacyModel> logger;

   public PrivacyModel(ILogger<PrivacyModel> logger)
   {
      this.logger = logger;
   }

   public void OnGet()
   {
      //EMPTY_BODY
   }
}