#!/usr/bin/env bash
set -euo pipefail

otp_script_directory="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
otp_project_root="$(cd -- "$otp_script_directory/.." && pwd)"
otp_project="$otp_project_root/OTP.csproj"
otp_dotnet="${OTP_DOTNET:-dotnet}"

cd -- "$otp_project_root"

if ! command -v "$otp_dotnet" >/dev/null 2>&1; then
  echo "OTP requires .NET 10 SDK, but '$otp_dotnet' was not found." >&2
  exit 1
fi

if ! otp_sdk_version="$("$otp_dotnet" --version 2>/dev/null)"; then
  echo "OTP requires .NET 10 SDK, but no compatible SDK was selected." >&2
  exit 1
fi

otp_sdk_major="${otp_sdk_version%%.*}"
if [[ ! "$otp_sdk_major" =~ ^[0-9]+$ ]] || (( otp_sdk_major < 10 )); then
  echo "OTP requires .NET 10 SDK or newer; found $otp_sdk_version." >&2
  exit 1
fi

exec "$otp_dotnet" run --project "$otp_project" -- "$@"
