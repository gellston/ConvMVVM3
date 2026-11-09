[CmdletBinding()]
param(
    [ValidateSet('ExampleActivatorUtilities','ExampleModuleManager','ExampleRegionManager',
        'ExampleMvvmBasics','ExampleAsyncOperations','ExampleMessaging','ExampleUIDispatcher',
        'ExampleBehaviors','ExampleCollections','ExampleResourceLifecycle')]
    [string[]]$Samples = @('ExampleActivatorUtilities','ExampleModuleManager','ExampleRegionManager',
        'ExampleMvvmBasics','ExampleAsyncOperations','ExampleMessaging','ExampleUIDispatcher',
        'ExampleBehaviors','ExampleCollections','ExampleResourceLifecycle'),
    [string]$PreviewDirectory
)
$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $repositoryRoot 'ConvMVVM3/Examples.SmokeTests/Examples.SmokeTests.csproj'
& dotnet build $projectPath --configuration Release -p:NuGetAudit=false -p:RestoreSources=https://api.nuget.org/v3/index.json --verbosity minimal
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
$runnerPath = Join-Path $repositoryRoot 'ConvMVVM3/Examples.SmokeTests/bin/Release/net10.0-windows/Examples.SmokeTests.dll'
foreach ($sample in $Samples) {
    $runnerArgs = @($runnerPath, $sample, $repositoryRoot)
    if ($PreviewDirectory) {
        $runnerArgs += $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($PreviewDirectory)
    }
    # Each sample gets a fresh process: WPF Application and ServiceLocator are process-wide.
    & dotnet @runnerArgs
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}
Write-Output "All $($Samples.Count) example scenarios passed."
