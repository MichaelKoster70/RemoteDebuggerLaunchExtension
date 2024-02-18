// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using Microsoft.JSInterop;

namespace WebRazorClassLibrary
{
   // This class provides an example of how JavaScript functionality can be wrapped
   // in a .NET class for easy consumption. The associated JavaScript module is
   // loaded on demand when first needed.
   //
   // This class can be registered as scoped DI service and then injected into Blazor
   // components for use.

   public class ExampleJsInterop : IAsyncDisposable
   {
      private readonly Lazy<Task<IJSObjectReference>> moduleTask;

      public ExampleJsInterop(IJSRuntime jsRuntime)
      {
         moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
             "import", "./_content/WebRazorClassLibrary/exampleJsInterop.js").AsTask());
      }

      public async ValueTask<string> Prompt(string message)
      {
         var module = await moduleTask.Value;
         return await module.InvokeAsync<string>("showPrompt", message);
      }

      public async ValueTask DisposeAsync()
      {
         await DisposeAsyncCore().ConfigureAwait(false);

         GC.SuppressFinalize(this);
      }

      protected virtual async ValueTask DisposeAsyncCore()
      {
         if (moduleTask.IsValueCreated)
         {
            var module = await moduleTask.Value;
            await module.DisposeAsync();
         }
      }
   }
}