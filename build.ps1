#build dotnet tool
$version = "1.0.0"
$appName = "Amba.CodeStatistics"
$toolName = "amba-code-stats"
$csprojPath = "$appName/$appName.csproj"

#build nuget package
dotnet pack $csprojPath --configuration Release --output ./publish/tool `
    -p:PackAsTool=true `
    -p:ToolCommandName=$toolName `
    -p:Version=$version

#build single file
dotnet publish $csprojPath `
    --configuration Release `
    -r win10-x64 `
    --output ./publish/exe  `
    --self-contained true `
    -p:Version=$version  `
    -p:PublishSingleFile=true `
    -p:PublishTrimmed=false `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:DebugSymbols=false `
    -p:CopyOutputSymbolsToPublishDirectory=false

Move-Item -Path "./publish/exe/$appName.exe" "./publish/exe/$toolName.exe" -Force
