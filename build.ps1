param (
    [string]$version = "0.0.2"
)

dotnet build .\Repono.sln -c Release -p:Version=$version
dotnet test --no-build