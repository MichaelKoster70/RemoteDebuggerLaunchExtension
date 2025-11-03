// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using Microsoft.VisualStudio.PlatformUI;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Utility class providing factory methods to create dialog windows.
   /// </summary>
   public static class DialogFactory
   {
      /// <summary>
      /// Creates and shows dialog.
      /// </summary>
      /// <typeparam name="TView">The type of the view.</typeparam>
      /// <typeparam name="TViewModel">The type of the view model.</typeparam>
      /// <param name="viewModel">The view model.</param>
      /// <returns>System.ValueTuple&lt;TViewModel, System.Nullable&lt;System.Boolean&gt;&gt;.</returns>
      public static (TViewModel viewModel, bool? result) CreateAndShowDialog<TView, TViewModel>(TViewModel viewModel) where TView : DialogWindow, new()
      {
         var view = Create<TView, TViewModel>(viewModel);

         var result = view.ShowDialog();
         return (viewModel, result);
      }
      
      internal static TView Create<TView, TViewModel>(TViewModel viewModel) where TView : DialogWindow, new()
      {
         var view = new TView()
         {
            DataContext = viewModel
         };

         return view;
      }
   }
}
