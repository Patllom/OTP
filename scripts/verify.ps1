$ErrorActionPreference = 'Stop'

$otpProjectRoot = Split-Path -Parent $PSScriptRoot
$otpTestProject = Join-Path $otpProjectRoot 'tests/OTP.Web.Tests/OTP.Web.Tests.csproj'
$otpDotnet = if ($env:OTP_DOTNET) { $env:OTP_DOTNET } else { 'dotnet' }
$otpConfiguration = if ($env:OTP_CONFIGURATION) { $env:OTP_CONFIGURATION } else { 'Debug' }

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
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

& $otpDotnet build $otpTestProject --no-restore --configuration $otpConfiguration
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

& $otpDotnet test $otpTestProject --no-build --no-restore --configuration $otpConfiguration
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
