// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Base class for the view models implementing <see cref="INotifyPropertyChanged"/>.
   /// Used to notify the view of changes in the model for bound data.
   /// </summary>
   public class ViewModelBase : INotifyPropertyChanged
   {
      /// <inheritdoc/>
      public event PropertyChangedEventHandler PropertyChanged;

      /// <summary>
      /// Raises the <see cref="PropertyChanged"/> event.
      /// </summary>
      /// <param name="propertyName">The name of the changed property.</param>
      /// <remarks>If <paramref name="propertyName"/> is empty the name of the caller is used.</remarks>
      protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
      {
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      }

      protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
      {
         if (!object.Equals(field, newValue))
         {
            field = newValue;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
         }

         return false;
      }
   }
}
