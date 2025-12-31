// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

namespace UnoGuiApp.Presentation;

public class ShellViewModel
{
    private readonly INavigator navigator;

    public ShellViewModel(INavigator navigator)
    {
        this.navigator = navigator;
        // Add code here to initialize or attach event handlers to singleton services
    }
}
