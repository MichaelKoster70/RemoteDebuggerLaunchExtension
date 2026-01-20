# Launch Profile Reference

## Common
The following launch profile properties are independant of the application type:
```
{
  "profiles": {
    "SSH Remote": {
      "commandName": "SshRemoteLaunch",
      "commandLineArgs": "arg0 arg1",
      "environmentVariables": {
        "answer": "42"
      },
      "copyEnvironmentFrom": "gnome-shell",
      "hostName": "192.168.1.106",
      "hostPort": "22",
      "userName": "pi",
      "privateKey": "%userprofile%\\.ssh\\id_rsa",
      "dotNetInstallFolderPath": "~\\.dotnet",
      "debuggerInstallFolderPath": "~\\.vsdbg",
      "appFolderPath": "~\\AppFolder",
      "additionalFiles": "Data\\Files\\data.txt|hello.txt;Data\\Files\\data.txt|/home/mko/hello.txt",
      "additionalDirectories": "Data\\Directory|data;Data\\Directory|/home/mko/data"
      "publishMode": "SelfContained"
      "publishOnDeploy": false,
      "deployOnStartDebugging": false,
      "installDebuggerOnStartDebugging": false
    }
  }
}
```

| Name        | required | Type | Value | Comment |
|:----------- |:-------- |:---- |:----- | :------ | 
| commandName | yes | string | "SshRemoteLaunch" | |
| hostName    | yes | string | Valid IP address or DNS name | The SSH host to connect to |
| hostPort    | no | string or integer | Valid IP port (1..65535) | The SSH port number |
| userName    | no | string | Valid linux user name | The user name for the SSH connection |
| privateKey  | no | string | Windows absolute path | The private key for SSH autentication |
| dotNetInstallFolderPath   | no | string | Linux path | The .NET install path |
| debuggerInstallFolderPath | no | string | Linux path | The vsdbg install path |
| appFolderPath | no | string | Linux path | The path where the app gets deployed to |
| copyEnvironmentFrom | no | string | Process name | Name of a process to copy environment variables from (e.g., gnome-shell, plasmashell) |
| additionalFiles | no | string | Multiple entries separated by ';'. Each entry: 'SourcePath|TargetPath' | Additional files to deploy |
| additionalDirectories | no | string | Multiple entries separated by ';'. Each entry: 'SourcePath|TargetPath' | Additional directories to deploy |
| publishMode | no | enum | SelfContained/FrameworkDependant | Publish mode |
| publishOnDeploy | no | boolean | true/false  | Publish the app before deploy |
| deployOnStartDebugging | no | boolean | true/false | Deploy the app |
| installDebuggerOnStartDebugging | no | boolean |  true to install debugger | Will automatically install the debugger |

If an optional value is not specified in the launch profile, the configured value in the global options is used.

## Web Apps
The following properties are specific to web projects:
```
{
  "profiles": {
    "SSH Remote": {
      "commandName": "SshRemoteLaunch",
      "environmentVariables": {
        "ASPNETCORE_URLS": "https://192.168.1.106:7172;http://192.168.1.106:5132"
      },
      "hostName": "192.168.1.106",
      "launchBrowser": true,
      "launchUrl": "https://192.168.1.106:7172",
    }
  }
}
```

| Name        | required | Type | Value | Comment |
|:----------- |:-------- |:---- |:----- | :------ | 
| launchBrowser | no | boolean | true to launch the Webbrowser | Will launch the default browser as configured in Visual Studio |
| launchUrl    | yes | string | Valid URL | The URL the browser should navigate to |

## Additional Files and Directories
The additional files and directories to deploy can be specified in the launch profile using the properties `additionalFiles` and `additionalDirectories`.
* All relative source paths for Windows are relative to the project folder.
* All relative target paths (do not begin with /) for Linux are relative to the configured `appFolderPath`.
* The entries in the additionalFiles are assumed to be files, the entries in additionalDirectories are assumed to be directories.

## GUI Applications and Desktop Environment
When debugging GUI applications that need to run inside a desktop environment, you typically need specific environment variables set. This is because an SSH session doesn't have access to the desktop environment's configuration.

### Copy Environment From Process
The `copyEnvironmentFrom` property allows you to copy environment variables from a running process that has the correct desktop environment setup. This is particularly useful for GUI applications that need variables like:
- `DISPLAY`
- `WAYLAND_DISPLAY`
- `DBUS_SESSION_BUS_ADDRESS`
- `XDG_RUNTIME_DIR`
- `XDG_CURRENT_DESKTOP`
- `XAUTHORITY`

**Syntax:**
The `copyEnvironmentFrom` property supports two formats:
1. **Copy all variables**: `processName`
   - Example: `"gnome-shell"` - copies all environment variables from the gnome-shell process
2. **Copy specific variables**: `processName|var1;var2;var3`
   - Example: `"gnome-shell|DISPLAY;XAUTHORITY;DBUS_SESSION_BUS_ADDRESS"` - copies only the specified variables

**Usage:**
1. Log into the target machine through a different connection having a desktop session (e.g., remote desktop via RDP/VNC or the virtual display of a VM)
2. In your launch profile, set `copyEnvironmentFrom` to the name of a desktop process, such as:
   - `gnome-shell` (GNOME desktop)
   - `plasmashell` or `ksmserver` (KDE Plasma)
   - `xfce4-session` (XFCE)
   - `sway` (Sway compositor)
   - `kwin_wayland` or `kwin_x11` (KWin window manager)
   - `Xwayland` or `Xorg` (X server)

**Examples:**

Copy all environment variables:
```json
{
  "profiles": {
    "SSH Remote GUI": {
      "commandName": "SshRemoteLaunch",
      "hostName": "192.168.1.106",
      "copyEnvironmentFrom": "gnome-shell",
      "environmentVariables": {
        "MY_CUSTOM_VAR": "value"
      }
    }
  }
}
```

Copy only specific environment variables:
```json
{
  "profiles": {
    "SSH Remote GUI": {
      "commandName": "SshRemoteLaunch",
      "hostName": "192.168.1.106",
      "copyEnvironmentFrom": "gnome-shell|DISPLAY;WAYLAND_DISPLAY;XAUTHORITY;DBUS_SESSION_BUS_ADDRESS",
      "environmentVariables": {
        "MY_CUSTOM_VAR": "value"
      }
    }
  }
}
```

**Behavior:**
- The extension finds a running process with the specified name that is owned by the same user as the SSH connection
- It reads all environment variables from that process (via `/proc/{pid}/environ`)
- These variables are added to the debugger launch configuration
- Any environment variables explicitly specified in the `environmentVariables` property will override the copied ones

**Note:** If the specified process is not found, the feature is silently skipped and debugging proceeds with only the explicitly configured environment variables.
