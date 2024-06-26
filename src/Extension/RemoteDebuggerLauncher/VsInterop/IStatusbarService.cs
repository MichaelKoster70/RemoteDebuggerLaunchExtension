﻿// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

namespace RemoteDebuggerLauncher
{
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
      /// Sets the status bar text in the text area.
      /// </summary>
      /// <param name="predicate">predicate whether to write the message or not.</param>
      /// <param name="text">The text to display in the status text area.</param>
      /// <param name="arg0">The first object to format.</param>
      void SetText(bool predicate, string text, object arg0);

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
