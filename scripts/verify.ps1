$ErrorActionPreference = 'Stop'

$otpProjectRoot = Split-Path -Parent $PSScriptRoot
$otpTestProject = Join-Path $otpProjectRoot 'tests/OTP.Web.Tests/OTP.Web.Tests.csproj'
$otpDotnet = if ($env:OTP_DOTNET) { $env:OTP_DOTNET } else { 'dotnet' }
$otpConfiguration = if ($env:OTP_CONFIGURATION) { $env:OTP_CONFIGURATION } else { 'Debug' }
$otpCallerLocation = (Get-Location).Path
$otpExitCode = 0

try {
    Set-Location -LiteralPath $otpProjectRoot

    try {
        Get-Command $otpDotnet -ErrorAction Stop | Out-Null
        $otpSdkVersion = (& $otpDotnet --version).Trim()
    }
    catch {
        throw "OTP requires .NET 10 SDK, but '$otpDotnet' is not available or compatible."
    }

    $otpSdkMajor = 0
    if (-not [int]::TryParse(($otpSdkVersion -split '\.')[0], [ref]$otpSdkMajor) -or $otpSdkMajor -lt 10) {
        throw "OTP requires .NET 10 SDK or newer; found $otpSdkVersion."
    }

    & $otpDotnet restore $otpTestProject
    $otpExitCode = $LASTEXITCODE

    if ($otpExitCode -eq 0) {
        & $otpDotnet build $otpTestProject --no-restore --configuration $otpConfiguration
        $otpExitCode = $LASTEXITCODE
    }

    if ($otpExitCode -eq 0) {
        & $otpDotnet test $otpTestProject --no-build --no-restore --configuration $otpConfiguration
        $otpExitCode = $LASTEXITCODE
    }
}
finally {
    Set-Location -LiteralPath $otpCallerLocation
}

if ($otpExitCode -ne 0) { exit $otpExitCode }
