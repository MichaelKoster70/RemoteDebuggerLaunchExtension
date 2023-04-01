# Features
The Remote Debugger Laucher includes the following features.

## Deploy and publish
The deploy and publish feature require that the the active launch profile is set to the profile supported by this plugin. The matching launch profile have the following command name:

```
"commandName": "SshRemoteLaunch"
```

### Deploy to target
Deploys the content of the project output folder recursivly to the conigured folder on the target device.
The implementation relies on Secure Copy (scp) for the copy operation.

The deploy operation can trigger in three ways:
- Choose the Visual Studio Menu Item 'Build' -> 'Deploy APP. Or you can find the 'Deploy' entry in the project context menu.
- If configured in the Solution Configuration by selecting the 'Deploy' checkbox, Visual Studio will deploy just before launching the app.
- If configured in the launch profile ("deployOnStartDebugging": true), the plugin performs the deploy operation itself. Prefer the previous option as (legacy) feature might get removed in a future release.

### Publish before deploy
This features will publish the application (running 'dotnet publish') before starting the deployment operation. The current implementation publishes the application as framework dependant.
Publishing as self contained application is not yet supported.

This feature becomes handy for applications types like ASP.NET, ASP.NET SPA, Blazer that need to deploy additional files outside the output folder.

## Debugging
The debugger launcher will be launching the app as framework dependant application only. 
The plugin supports the 'Start Debugging (F5)' and 'Start without Debugging (Ctrl+F5)'.

The following properties are passed to the debugger independant of the application type:
- Command line arguments
- Environment variables

### Debugging Console Application
No additional features are available targeting console applications.

### Debugging GUI Applications
Use environment variabes for instance to:
- Export the 'DISPLAY' variable to configure the display for GUI apps.

### Debugging Web Application
The plugin supports launching a web browser on the development PC together with the server app running on the target. The plugin support Chromium based browsers installed on the development PC. It starts the browser as configured in Visual Studio.

To do so configure the follwing properties in your Launch profile:
- 'launchBrowser' (boolean) : set to true
- 'launchUrl' (string) : set to URL the browser should navigate to

Use environment variabes to:
- Define 'ASPNETCORE_URLS' to configure the URL where ASP.NET webserver should listen.

The plugin not yet supports ASP.NET HTTPS developer certificates from the target device to be trusted on the developer PC.

### Install Debugger
The plugin can be configured to install vsdbg before launching the application. In interest of deployment time, this feature is off by default.




