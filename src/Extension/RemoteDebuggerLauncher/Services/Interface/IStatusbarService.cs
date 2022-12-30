// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Defines the supported animations.
   /// </summary>
   internal enum StatusbarAnimation : short
   {
      /// <summary>Standard animation icon. </summary>
      General = 0,

      /// <summary>The deploy animation icon. </summary>
      Deploy = 3,

      /// <summary>The build animation icon. </summary>
      Build = 5
   }

   /// <summary>
   /// Interface defining a service writing to the VS status bar.
   /// </summary>
   internal interface IStatusbarService
   {
      /// <summary>
      /// Sets the status bar text in the text area.
      /// </summary>
      /// <param name="text">The text to display in the status text area.</param>
      void SetText(string text);

      /// <summary>
      /// Sets the status bar text in the text area.
      /// </summary>
      /// <param name="text">The text to display in the status text area.</param>
      /// <param name="arg0">The first object to format.</param>
      void SetText(string text, object arg0);

      /// <summary>
      /// Starts a predefined animation.
      /// </summary>
      /// <param name="animation">The animation to start.</param>
      void StartAnimation(StatusbarAnimation animation);

      /// <summary>
      /// Stops the animation.
      /// </summary>
      void StopAnimation();

      /// <summary>
      /// Clears the status bar text and stops a running animation.
      /// </summary>
      void Clear();
   }
}
