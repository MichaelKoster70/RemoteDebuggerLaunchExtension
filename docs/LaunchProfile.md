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
      "hostName": "192.168.1.106",
      "hostPort": "22",
      "userName": "pi",
      "privateKey": "%userprofile%\\.ssh\\id_rsa",
      "dotNetInstallFolderPath": "~\\.dotnet",
      "debuggerInstallFolderPath": "~\\.vsdbg",
      "appFolderPath": "~\\AppFolder",
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
