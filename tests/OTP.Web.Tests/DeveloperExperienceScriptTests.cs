using System.Diagnostics;

namespace OTP.Web.Tests;

public sealed class DeveloperExperienceScriptTests
{
    private static readonly string ProjectRoot = FindProjectRoot();

    [Fact]
    public async Task VerificationScriptRestoresBuildsAndTestsInOrder()
    {
        using var fakeDotnet = new FakeDotnet("10.0.400");

        var result = await RunPlatformScriptAsync("verify", fakeDotnet);

        Assert.True(result.ExitCode == 0, result.CombinedOutput);
        var calls = File.ReadAllLines(fakeDotnet.LogFile);
        Assert.Collection(
            calls,
            call => Assert.Equal("--version", call),
            call =>
            {
                Assert.StartsWith("restore ", call);
                Assert.Contains("OTP.Web.Tests.csproj", call);
            },
            call =>
            {
                Assert.StartsWith("build ", call);
                Assert.Contains("OTP.Web.Tests.csproj", call);
                Assert.Contains("--no-restore", call);
                Assert.Contains("--configuration Debug", call);
            },
            call =>
            {
                Assert.StartsWith("test ", call);
                Assert.Contains("OTP.Web.Tests.csproj", call);
                Assert.Contains("--no-build", call);
                Assert.Contains("--no-restore", call);
                Assert.Contains("--configuration Debug", call);
            });
    }

    [Fact]
    public async Task RunScriptChecksSdkAndForwardsApplicationArguments()
    {
        using var fakeDotnet = new FakeDotnet("10.0.400");

        var result = await RunPlatformScriptAsync(
            "run",
            fakeDotnet,
            "--urls",
            "http://127.0.0.1:5190");

        Assert.True(result.ExitCode == 0, result.CombinedOutput);
        var calls = File.ReadAllLines(fakeDotnet.LogFile);
        Assert.Collection(
            calls,
            call => Assert.Equal("--version", call),
            call =>
            {
                Assert.StartsWith("run --project ", call);
                Assert.Contains("OTP.csproj", call);
                Assert.EndsWith("-- --urls http://127.0.0.1:5190", call);
            });
    }

    [Fact]
    public async Task RunScriptSupportsCustomDotnetPathContainingSpaces()
    {
        using var fakeDotnet = new FakeDotnet("10.0.400", useDirectoryWithSpaces: true);

        var result = await RunPlatformScriptAsync("run", fakeDotnet);

        Assert.True(result.ExitCode == 0, result.CombinedOutput);
        Assert.Equal(2, File.ReadAllLines(fakeDotnet.LogFile).Length);
    }

    public static TheoryData<string> PlatformScripts => new()
    {
        "run",
        "verify"
    };

    [Theory]
    [MemberData(nameof(PlatformScripts))]
    public async Task ScriptUsesProjectRootAsWorkingDirectoryWhenInvokedElsewhere(string scriptName)
    {
        using var fakeDotnet = new FakeDotnet("10.0.400");
        var outsideDirectory = Directory.CreateTempSubdirectory("otp-script-caller-");

        try
        {
            var result = await RunPlatformScriptFromWorkingDirectoryAsync(
                scriptName,
                fakeDotnet,
                outsideDirectory.FullName,
                []);

            Assert.True(result.ExitCode == 0, result.CombinedOutput);
            var workingDirectories = File.ReadAllLines(fakeDotnet.WorkingDirectoryLogFile);
            Assert.NotEmpty(workingDirectories);
            Assert.All(
                workingDirectories,
                workingDirectory => Assert.Equal(
                    Path.GetFullPath(ProjectRoot),
                    Path.GetFullPath(workingDirectory)));
        }
        finally
        {
            outsideDirectory.Delete(recursive: true);
        }
    }

    [Theory]
    [MemberData(nameof(PlatformScripts))]
    public async Task ScriptRejectsSdkBeforeVersion10(string scriptName)
    {
        using var fakeDotnet = new FakeDotnet("9.0.100");

        var result = await RunPlatformScriptAsync(scriptName, fakeDotnet);

        Assert.NotEqual(0, result.ExitCode);
        Assert.Contains("requires .NET 10 SDK", result.CombinedOutput);
        Assert.Equal(["--version"], File.ReadAllLines(fakeDotnet.LogFile));
    }

    private static async Task<ScriptResult> RunPlatformScriptAsync(
        string scriptName,
        FakeDotnet fakeDotnet,
        params string[] scriptArguments)
    {
        return await RunPlatformScriptFromWorkingDirectoryAsync(
            scriptName,
            fakeDotnet,
            ProjectRoot,
            scriptArguments);
    }

    private static async Task<ScriptResult> RunPlatformScriptFromWorkingDirectoryAsync(
        string scriptName,
        FakeDotnet fakeDotnet,
        string workingDirectory,
        IReadOnlyList<string> scriptArguments)
    {
        var startInfo = new ProcessStartInfo
        {
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            WorkingDirectory = workingDirectory
        };

        if (OperatingSystem.IsWindows())
        {
            startInfo.FileName = "powershell.exe";
            startInfo.ArgumentList.Add("-NoProfile");
            startInfo.ArgumentList.Add("-ExecutionPolicy");
            startInfo.ArgumentList.Add("Bypass");
            startInfo.ArgumentList.Add("-File");
            startInfo.ArgumentList.Add(Path.Combine(ProjectRoot, "scripts", $"{scriptName}.ps1"));
        }
        else
        {
            startInfo.FileName = "/bin/bash";
            startInfo.ArgumentList.Add(Path.Combine(ProjectRoot, "scripts", $"{scriptName}.sh"));
        }

        foreach (var argument in scriptArguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        startInfo.Environment["OTP_DOTNET"] = fakeDotnet.CommandFile;
        startInfo.Environment["OTP_TEST_DOTNET_LOG"] = fakeDotnet.LogFile;
        startInfo.Environment["OTP_TEST_DOTNET_WORKING_DIRECTORY_LOG"] = fakeDotnet.WorkingDirectoryLogFile;
        startInfo.Environment["OTP_TEST_DOTNET_VERSION"] = fakeDotnet.Version;
        startInfo.Environment["OTP_CONFIGURATION"] = "Debug";

        using var process = new Process { StartInfo = startInfo };
        process.Start();
        var standardOutput = process.StandardOutput.ReadToEndAsync();
        var standardError = process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        return new ScriptResult(
            process.ExitCode,
            $"{await standardOutput}{await standardError}");
    }

    private static string FindProjectRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "OTP.csproj")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new DirectoryNotFoundException("Could not locate OTP.csproj from the test output directory.");
    }

    private sealed class FakeDotnet : IDisposable
    {
        private readonly string _temporaryDirectory;

        public FakeDotnet(string version, bool useDirectoryWithSpaces = false)
        {
            Version = version;
            var directoryPrefix = useDirectoryWithSpaces
                ? "otp developer experience tests"
                : "otp-developer-experience-tests";
            _temporaryDirectory = Path.Combine(
                Path.GetTempPath(),
                $"{directoryPrefix}-{Guid.NewGuid():N}");
            Directory.CreateDirectory(_temporaryDirectory);

            LogFile = Path.Combine(_temporaryDirectory, "dotnet-calls.log");
            WorkingDirectoryLogFile = Path.Combine(_temporaryDirectory, "dotnet-working-directories.log");
            CommandFile = Path.Combine(
                _temporaryDirectory,
                OperatingSystem.IsWindows() ? "dotnet.cmd" : "dotnet");

            if (OperatingSystem.IsWindows())
            {
                File.WriteAllText(
                    CommandFile,
                    "@echo off\r\n" +
                    ">>\"%OTP_TEST_DOTNET_WORKING_DIRECTORY_LOG%\" echo %CD%\r\n" +
                    ">>\"%OTP_TEST_DOTNET_LOG%\" echo %*\r\n" +
                    "if \"%~1\"==\"--version\" echo %OTP_TEST_DOTNET_VERSION%\r\n" +
                    "exit /b 0\r\n");
            }
            else
            {
                File.WriteAllText(
                    CommandFile,
                    "#!/usr/bin/env bash\n" +
                    "pwd -P >> \"$OTP_TEST_DOTNET_WORKING_DIRECTORY_LOG\"\n" +
                    "printf '%s\\n' \"$*\" >> \"$OTP_TEST_DOTNET_LOG\"\n" +
                    "if [ \"${1:-}\" = \"--version\" ]; then\n" +
                    "  printf '%s\\n' \"${OTP_TEST_DOTNET_VERSION:-10.0.400}\"\n" +
                    "fi\n");
                File.SetUnixFileMode(
                    CommandFile,
                    UnixFileMode.UserRead |
                    UnixFileMode.UserWrite |
                    UnixFileMode.UserExecute);
            }
        }

        public string CommandFile { get; }

        public string LogFile { get; }

        public string WorkingDirectoryLogFile { get; }

        public string Version { get; }

        public void Dispose()
        {
            Directory.Delete(_temporaryDirectory, recursive: true);
        }
    }

    private sealed record ScriptResult(int ExitCode, string CombinedOutput);
}
