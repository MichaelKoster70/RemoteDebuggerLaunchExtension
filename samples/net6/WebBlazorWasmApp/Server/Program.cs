// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using Microsoft.AspNetCore.ResponseCompression;

namespace WebBlazorWasmApp.Server;

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
      if (app.Environment.IsDevelopment())
      {
         app.UseWebAssemblyDebugging();
      }
      else
      {
         _ = app.UseExceptionHandler("/Error");
         // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
         _ = app.UseHsts();
      }

      _ = app.UseHttpsRedirection();

      _ = app.UseBlazorFrameworkFiles();
      _ = app.UseStaticFiles();

      _ = app.UseRouting();


      _ = app.MapRazorPages();
      _ = app.MapControllers();
      _ = app.MapFallbackToFile("index.html");

      app.Run();
   }
}