# Release Notes

## 0.9.0
Initial release supporting Visual Studio 2022 17.2 or newer.

### Features
- SSH authentication using private keys.
- Debug .NET framework dependant application.
- Deploy the project output folder via SCP to the target device.
- Publish (running dotnet publish) before deploying the application.
- Install vsdbg automatically (configurable).
- Install .NET on the target device.
- Supporting command to setup SSH authentication.

### Bug Fixes
- none

## 0.10.0
New features and bugfix release increasing stability.

### Features
- check if cURL is installed when installing .NET or vsdbg
- fix analyzer warnings
- added message how to add .NET to search path
- added support to publish app as self contained apps
- added support for debugging self contained apps

### Bug Fixes
- fixed caption for error messages