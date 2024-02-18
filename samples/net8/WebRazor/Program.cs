// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

internal class Program
{
   private static void Main(string[] args)
   {
      var builder = WebApplication.CreateBuilder(args);

      // Add services to the container.
      _ = builder.Services.AddRazorPages();

      var app = builder.Build();

      // Configure the HTTP request pipeline.
      if (!app.Environment.IsDevelopment())
      {
         _ = app.UseExceptionHandler("/Error");
         // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
         app.UseHsts();
      }

      _ = app.UseHttpsRedirection();
      _ = app.UseStaticFiles();

      _ = app.UseRouting();

      _ = app.UseAuthorization();

      _ = app.MapRazorPages();

      app.Run();
   }
}