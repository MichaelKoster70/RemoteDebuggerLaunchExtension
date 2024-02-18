// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using WebAspSpa.Infrastructure;

namespace AspNetSpa
{
   public static class Program
   {
      public static void Main(string[] args)
      {
         var builder = WebApplication.CreateBuilder(args);

         // Add services to the container.

         _ = builder.Services.AddControllersWithViews()
            .AddJsonOptions(options =>
            {
               options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
            });

         var app = builder.Build();

         // Configure the HTTP request pipeline.
         if (!app.Environment.IsDevelopment())
         {
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            _ = app.UseHsts();
         }

         //app.UseHttpsRedirection();
         _ = app.UseStaticFiles();
         _ = app.UseRouting();


         _ = app.MapControllerRoute(
                name: "default",
                pattern: "{controller}/{action=Index}/{id?}");

         _ = app.MapFallbackToFile("index.html");

         app.Run();
      }
   }
}