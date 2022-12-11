// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Management.Automation.Host;

namespace RemoteDebuggerLauncher.PowerShellHost
{
   /// <summary>
   /// Provides a implementation of the PSHostRawUserInterface for console applications. 
   /// Members of this class that easily map to the .NET console class are implemented.    
   /// Implements the <see cref="PSHostRawUserInterface"/> interface.
   /// </summary>
   /// <seealso cref="PSHostRawUserInterface" />
   internal class ConsoleRawUserInterface : PSHostRawUserInterface
   {
      /// <summary>
      /// Gets or sets the background color of the displayed text.
      /// This maps to the corresponding Console.Background property.
      /// </summary>
      /// <value>The color of the background.</value>
      public override ConsoleColor BackgroundColor
      {
         get { return Console.BackgroundColor; }
         set { Console.BackgroundColor = value; }
      }

      /// <summary>
      /// Gets or sets the size of the host buffer. In this example the
      /// buffer size is adapted from the Console buffer size members.
      /// </summary>
      /// <value>The size of the buffer.</value>
      public override Size BufferSize
      {
         get { return new Size(Console.BufferWidth, Console.BufferHeight); }
         set { Console.SetBufferSize(value.Width, value.Height); }
      }

      /// <summary>
      /// Gets or sets the cursor position.
      /// </summary>
      /// <value>The cursor position.</value>
      /// <exception cref="System.NotImplementedException">The method or operation is not implemented.</exception>
      public override Coordinates CursorPosition
      {
         get
         {
            throw new NotImplementedException($"The property '{nameof(CursorPosition)}' is not implemented.");
         }
         set
         {
            throw new NotImplementedException($"The property '{nameof(CursorPosition)}' is not implemented.");
         }
      }

      /// <summary>
      /// Gets or sets the size of the displayed cursor.
      /// </summary>
      /// <value>The size of the cursor.</value>
      public override int CursorSize
      {
         get { return Console.CursorSize; }
         set { Console.CursorSize = value; }
      }

      /// <summary>
      /// Gets or sets the foreground color of the displayed text.
      /// This maps to the corresponding Console.ForegroundColor property.
      /// </summary>
      /// <value>The color of the foreground.</value>
      public override ConsoleColor ForegroundColor
      {
         get { return Console.ForegroundColor; }
         set { Console.ForegroundColor = value; }
      }

      /// <summary>
      /// Gets a value indicating whether the user has pressed a key. This maps
      /// to the corresponding Console.KeyAvailable property.
      /// </summary>
      /// <value>True if a keystroke is waiting in the input buffer, false if not.</value>
      public override bool KeyAvailable => Console.KeyAvailable;

      /// <summary>
      /// Gets the dimensions of the largest window that could be rendered in the current display, 
      /// if the buffer was at the least that large.
      /// </summary>
      /// <value>The maximum size of the physical window.</value>
      public override Size MaxPhysicalWindowSize => new Size(Console.LargestWindowWidth, Console.LargestWindowHeight);

      /// <summary>
      /// Gets the dimensions of the largest window size that can be displayed.
      /// </summary>
      /// <value>The largest dimensions the window can be resized to without resizing the screen buffer.</value>
      public override Size MaxWindowSize => new Size(Console.LargestWindowWidth, Console.LargestWindowHeight);

      /// <summary>
      /// Gets or sets the position of the displayed window. This example
      /// uses the Console window position APIs to determine the returned
      /// value of this property.
      /// </summary>
      public override Coordinates WindowPosition
      {
         get { return new Coordinates(Console.WindowLeft, Console.WindowTop); }
         set { Console.SetWindowPosition(value.X, value.Y); }
      }

      /// <summary>
      /// Gets or sets the size of the displayed window.
      /// </summary>
      /// <value>The size of the window.</value>
      public override Size WindowSize
      {
         get { return new Size(Console.WindowWidth, Console.WindowHeight); }
         set { Console.SetWindowSize(value.Width, value.Height); }
      }

      /// <summary>
      /// Gets or sets the title of the displayed window.
      /// </summary>
      /// <value>The window title.</value>
      public override string WindowTitle
      {
         get { return Console.Title; }
         set { Console.Title = value; }
      }

      /// <summary>
      /// This API resets the input buffer.
      /// </summary>
      public override void FlushInputBuffer()
      {
      }

      /// <summary>
      /// This API returns a rectangular region of the screen buffer.
      /// </summary>
      /// <param name="rectangle">Defines the size of the rectangle.</param>
      /// <returns>Throws a NotImplementedException exception.</returns>
      public override BufferCell[,] GetBufferContents(Rectangle rectangle)
      {
         throw new NotImplementedException($"The method '{nameof(BufferCell)}' is not implemented.");
      }

      /// <summary>
      /// This API reads a pressed, released, or pressed and released keystroke
      /// from the keyboard device, blocking processing until a keystroke is
      /// typed that matches the specified keystroke options.
      /// </summary>
      /// <param name="options">Options, such as IncludeKeyDown, used when reading the keyboard.</param>
      /// <returns>Throws a NotImplementedException exception.</returns>
      /// <exception cref="System.NotImplementedException">The method or operation is not implemented.</exception>
      public override KeyInfo ReadKey(ReadKeyOptions options)
      {
         throw new NotImplementedException($"The method '{nameof(ReadKey)}' is not implemented.");
      }

      /// <summary>
      /// This API crops a region of the screen buffer.
      /// </summary>
      /// <param name="source">The region of the screen to be scrolled.</param>
      /// <param name="destination">The region of the screen to receive the
      /// source region contents.</param>
      /// <param name="clip">The region of the screen to include in the operation.</param>
      /// <param name="fill">The character and attributes to be used to fill all cell.</param>
      /// <exception cref="System.NotImplementedException">The method or operation is not implemented.</exception>
      public override void ScrollBufferContents(Rectangle source, Coordinates destination, Rectangle clip, BufferCell fill)
      {
         throw new NotImplementedException($"The method '{nameof(ScrollBufferContents)}' is not implemented.");
      }

      /// <summary>
      /// This method copies an array of buffer cells into the screen buffer at a specified location.
      /// </summary>
      /// <param name="origin">The parameter is not used.</param>
      /// <param name="contents">The parameter is not used.</param>
      /// <exception cref="System.NotImplementedException">The method or operation is not implemented.</exception>
      public override void SetBufferContents(Coordinates origin, BufferCell[,] contents)
      {
         throw new NotImplementedException($"The method '{nameof(SetBufferContents)}' is not implemented.");
      }

      /// <summary>
      /// This method copies a given character, foreground color, and background
      /// color to a region of the screen buffer.
      /// </summary>
      /// <param name="rectangle">Defines the area to be filled.</param>
      /// <param name="fill">Defines the fill character.</param>
      /// <exception cref="System.NotImplementedException">The method or operation is not implemented.</exception>
      public override void SetBufferContents(Rectangle rectangle, BufferCell fill)
      {
         throw new NotImplementedException($"The method '{nameof(SetBufferContents)}' is not implemented.");
      }
   }
}

