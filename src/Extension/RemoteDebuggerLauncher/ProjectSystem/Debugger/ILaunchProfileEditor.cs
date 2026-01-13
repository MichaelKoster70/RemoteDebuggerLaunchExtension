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
      /// Updates a property in the active launch profile.
      /// </summary>
      /// <param name="propertyName">The name of the property to update.</param>
      /// <param name="propertyValue">The value to set for the property.</param>
      /// <returns>A <see cref="Task{Boolean}"/> representing the asynchronous operation: <c>true</c> if successful; else <c>false</c>.</returns>
      Task<bool> UpdateProfilePropertyAsync(string propertyName, string propertyValue);
   }
}
