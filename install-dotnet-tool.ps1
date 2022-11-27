$appName = "Amba.CodeStatistics"
#dotnet tool uninstall -g $appName
dotnet tool install $appName --global --add-source ./publish/tool