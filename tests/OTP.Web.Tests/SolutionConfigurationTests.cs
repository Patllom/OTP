using System.Diagnostics;

namespace OTP.Web.Tests;

public sealed class SolutionConfigurationTests
{
    private static readonly string ProjectRoot = FindProjectRoot();

    [Fact]
    public async Task SolutionIncludesWebTestProject()
    {
        var dotnet = Environment.GetEnvironmentVariable("DOTNET_HOST_PATH") ?? "dotnet";
        var startInfo = new ProcessStartInfo
        {
            FileName = dotnet,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            WorkingDirectory = ProjectRoot
        };
        startInfo.ArgumentList.Add("sln");
        startInfo.ArgumentList.Add(Path.Combine(ProjectRoot, "OTT.sln"));
        startInfo.ArgumentList.Add("list");

        using var process = new Process { StartInfo = startInfo };
        process.Start();
        var standardOutput = process.StandardOutput.ReadToEndAsync();
        var standardError = process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        var combinedOutput = $"{await standardOutput}{await standardError}";
        Assert.True(process.ExitCode == 0, combinedOutput);
        Assert.Contains(
            "tests/OTP.Web.Tests/OTP.Web.Tests.csproj",
            combinedOutput.Replace('\\', '/'));
    }

    private static string FindProjectRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "OTT.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new DirectoryNotFoundException("Could not locate OTT.sln from the test output directory.");
    }
}
