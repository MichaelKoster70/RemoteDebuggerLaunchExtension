dotnet publish CheckSum\exe\CheckSumExe.csproj -c Release --framework net8.0 --runtime linux-x64 -o bin\linux-x64
dotnet publish CheckSum\exe\CheckSumExe.csproj -c Release --framework net8.0 --runtime linux-arm64 -o bin\linux-arm64
dotnet publish CheckSum\exe\CheckSumExe.csproj -c Release --framework net8.0 --runtime linux-arm -o bin\linux-arm