// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Threading.Tasks;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Interface for editing launch profile settings.
   /// </summary>
   internal interface ILaunchProfileEditor
   {
      /// <summary>
      /// Gets the name of the profile.
      /// </summary>
      string ProfileName { get; }

      /// <summary>
      /// Updates a property in the active launch profile.
      /// </summary>
      /// <param name="propertyName">The name of the property to update.</param>
      /// <param name="propertyValue">The value to set for the property.</param>
      /// <returns>A <see cref="Task{bool}"/> representing the asynchronous operation. <c>true</c> if the property was updated successfully; otherwise, <c>false</c>.</returns>
      Task<bool> UpdateProfilePropertyAsync(string propertyName, string propertyValue);
   }
}
