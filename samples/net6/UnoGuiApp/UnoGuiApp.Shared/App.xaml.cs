// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;

namespace UnoGuiApp
{
   /// <summary>
   /// Provides application-specific behavior to supplement the default Application class.
   /// </summary>
   public sealed partial class App : Application
   {
      private Window window;

      /// <summary>
      /// Initializes the singleton application object.  This is the first line of authored code
      /// executed, and as such is the logical equivalent of main() or WinMain().
      /// </summary>
      public App()
      {
         InitializeLogging();

         this.InitializeComponent();

#if HAS_UNO || NETFX_CORE
         Suspending += OnSuspending;
#endif
      }

      /// <summary>
      /// Invoked when the application is launched normally by the end user.  Other entry points
      /// will be used such as when the application is launched to open a specific file.
      /// </summary>
      /// <param name="args">Details about the launch request and process.</param>
      protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
      {
         ArgumentNullException.ThrowIfNull(args);

#if NET6_0_OR_GREATER && WINDOWS && !HAS_UNO
            window = new Window();
            window.Activate();
#else
         window = Window.Current;
#endif

         // Do not repeat app initialization when the Window already has content,
         // just ensure that the window is active
         if (window.Content is not Frame rootFrame)
         {
            // Create a Frame to act as the navigation context and navigate to the first page
            rootFrame = new Frame();

            rootFrame.NavigationFailed += OnNavigationFailed;

            // Place the frame in the current Window
            window.Content = rootFrame;
         }

#if !(NET6_0_OR_GREATER && WINDOWS)
         if (!args.UWPLaunchActivatedEventArgs.PrelaunchActivated)
#endif
         {
            if (rootFrame.Content == null)
            {
               // When the navigation stack isn't restored navigate to the first page,
               // configuring the new page by passing required information as a navigation
               // parameter
               rootFrame.Navigate(typeof(MainPage), args.Arguments);
            }
            // Ensure the current window is active
            window.Activate();
         }
      }

      /// <summary>
      /// Invoked when Navigation to a certain page fails
      /// </summary>
      /// <param name="sender">The Frame which failed navigation</param>
      /// <param name="e">Details about the navigation failure</param>
      private static void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
      {
         throw new InvalidOperationException($"Failed to load {e.SourcePageType.FullName}: {e.Exception}");
      }

      /// <summary>
      /// Invoked when application execution is being suspended.  Application state is saved
      /// without knowing whether the application will be terminated or resumed with the contents
      /// of memory still intact.
      /// </summary>
      /// <param name="sender">The source of the suspend request.</param>
      /// <param name="e">Details about the suspend request.</param>
      private static void OnSuspending(object sender, SuspendingEventArgs e)
      {
         var deferral = e.SuspendingOperation.GetDeferral();
         deferral.Complete();
      }

      /// <summary>
      /// Configures global Uno Platform logging
      /// </summary>
      private static void InitializeLogging()
      {
#if DEBUG
         // Logging is disabled by default for release builds, as it incurs a significant
         // initialization cost from Microsoft.Extensions.Logging setup. If startup performance
         // is a concern for your application, keep this disabled. If you're running on web or 
         // desktop targets, you can use url or command line parameters to enable it.
         //
         // For more performance documentation: https://platform.uno/docs/articles/Uno-UI-Performance.html

         var factory = LoggerFactory.Create(builder =>
         {
#if NETFX_CORE
                builder.AddDebug();
#else
            builder.AddConsole();
#endif

            // Exclude logs below this level
            builder.SetMinimumLevel(LogLevel.Information);

            // Default filters for Uno Platform namespaces
            builder.AddFilter("Uno", LogLevel.Warning);
            builder.AddFilter("Windows", LogLevel.Warning);
            builder.AddFilter("Microsoft", LogLevel.Warning);
         });

         global::Uno.Extensions.LogExtensionPoint.AmbientLoggerFactory = factory;

#if HAS_UNO
         global::Uno.UI.Adapter.Microsoft.Extensions.Logging.LoggingAdapter.Initialize();
#endif
#endif
         }
   }
}
