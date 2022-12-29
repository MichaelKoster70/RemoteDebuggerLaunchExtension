# RemoteDebuggerLauncherUI
This projects holds all UI components for the commands.

The WPF based UI elements must be placed in a separate assembly because of a problem related to Roslyn source generators.
The MSBUILD based build process for WPF does not properly pass source generators to the generated project to build a temporary assembly. The missing generated code causes the build to fail.