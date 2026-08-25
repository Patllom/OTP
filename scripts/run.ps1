param(
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$ApplicationArguments
)

$ErrorActionPreference = 'Stop'

$otpProjectRoot = Split-Path -Parent $PSScriptRoot
$otpProject = Join-Path $otpProjectRoot 'OTP.csproj'
$otpDotnet = if ($env:OTP_DOTNET) { $env:OTP_DOTNET } else { 'dotnet' }

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

& $otpDotnet run --project $otpProject -- @ApplicationArguments
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
