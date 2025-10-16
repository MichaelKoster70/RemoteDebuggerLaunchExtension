// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebRazor.Pages;

public class IndexModel : PageModel
{
   private readonly ILogger<IndexModel> logger;

   public IndexModel(ILogger<IndexModel> logger)
   {
      this.logger = logger;
   }

   public void OnGet()
   {
      //EMPTY_BODY
   }
}