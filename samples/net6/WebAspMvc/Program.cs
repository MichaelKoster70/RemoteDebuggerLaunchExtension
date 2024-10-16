// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

namespace WebAspMvc
{
   internal static class Program
   {
      private static void Main(string[] args)
      {
         var builder = WebApplication.CreateBuilder(args);

         // Add services to the container.
         _ = builder.Services.AddControllersWithViews();
         _ = builder.Services.AddRazorPages();

         var app = builder.Build();

         // Configure the HTTP request pipeline.
         if (!app.Environment.IsDevelopment())
         {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
         }

         app.UseHttpsRedirection();
         app.UseStaticFiles();

         app.UseRouting();

         app.UseAuthorization();

         app.MapControllerRoute(
             name: "default",
             pattern: "{controller=Home}/{action=Index}/{id?}");

         app.MapRazorPages();
         app.Run();
      }
   }
}