param(
    [string]$BaseUrl = "https://localhost:5001"
)

Write-Host "Ejecutando cliente de la API: $BaseUrl"

dotnet run --project ..\Client\Client.csproj --verbosity minimal -- ""
