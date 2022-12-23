# Extension Source Code
The extension consists of multiple projects.

 ## Projects
The extension is split into the following assemblies:
| Project                      | Purpose                                                         |
|:-----------------------------|:----------------------------------------------------------------|
| RemoteDebuggerLauncher       | The main assembly providing the visual studio extension package |
| RemoteDebuggerLauncherShared | Assembly holding assets shared between the main and UI assembly |
| RemoteDebuggerLauncherUI     | The assembly providing all UI elements for the extension        |

## Files
| File                  | Contents                                                             |
|:----------------------|:---------------------------------------------------------------------|
| GlobalAssemblyInfo.cs | Holds assembly info metadata shared between all extension assembiles |

