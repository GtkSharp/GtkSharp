#addin "nuget:?package=Microsoft.Win32.Registry&version=5.0.0"
using System;
using D = System.IO.Directory;
using F = System.IO.File;
using P = System.IO.Path;
using Pr = System.Diagnostics.Process;
using PSI = System.Diagnostics.ProcessStartInfo;
using R = Microsoft.Win32.Registry;
using RI = System.Runtime.InteropServices.RuntimeInformation;
using Z = System.IO.Compression.ZipFile;

class TargetEnvironment
{
    public static string DotNetInstallPath { get; private set; }
    public static string DotNetInstalledWorkloadsMetadataPath { get; private set; }
    public static string DotNetInstallerTypeMetadataPath { get; private set; }
    public static string DotNetManifestPath { get; private set; }
    public static string DotNetPacksPath { get; private set; }
    public static string DotNetTemplatePacksPath { get; private set; }

    public static string DotNetCliPath { get; private set; }
    public static string DotNetCliFeatureBand { get; private set; }

    static string DotNetExeName() => OperatingSystem.IsWindows() ? "dotnet.exe" : "dotnet";

    static string FindOnPath(string exe)
    {
        var path = Environment.GetEnvironmentVariable("PATH") ?? "";
        foreach (var dir in path.Split(P.PathSeparator))
        {
            if (string.IsNullOrWhiteSpace(dir))
                continue;

            var candidate = P.Combine(dir.Trim(), exe);
            if (F.Exists(candidate))
                return candidate;
        }

        return null;
    }

    static TargetEnvironment()
    {
        DotNetInstallPath = Environment.GetEnvironmentVariable("DOTNET_ROOT");
        if (DotNetInstallPath == null)
        {
            if (OperatingSystem.IsWindows())
            {
                DotNetInstallPath = P.Combine(Environment.GetEnvironmentVariable("ProgramFiles"), "dotnet");
            }
            else if (OperatingSystem.IsLinux())
            {
                DotNetInstallPath = "/usr/share/dotnet";
            }
            else if (OperatingSystem.IsMacOS())
            {
                DotNetInstallPath = "/usr/local/share/dotnet";
            }
        }

        DotNetCliPath = P.Combine(DotNetInstallPath, DotNetExeName());

        // The guesses above are only conventions, and they are wrong more often
        // than not off Windows: Debian's dotnet-sdk package installs to
        // /usr/lib/dotnet, and dotnet-install.sh puts it wherever it was asked
        // to. Getting this wrong surfaced as "The type initializer for
        // 'TargetEnvironment' threw an exception", which says nothing about the
        // cause, so fall back to whatever `dotnet` is actually on PATH.
        if (!F.Exists(DotNetCliPath))
        {
            var found = FindOnPath(DotNetExeName());
            if (found != null)
            {
                DotNetCliPath = found;
                DotNetInstallPath = P.GetDirectoryName(found);
            }
        }

        if (!F.Exists(DotNetCliPath))
            throw new Exception($"Could not find the dotnet CLI. Looked in '{DotNetInstallPath}' and on PATH. Set DOTNET_ROOT to the SDK install directory.");

        var proc = Pr.Start(new PSI()
        {
            FileName = DotNetCliPath,
            Arguments = "--version",
            RedirectStandardOutput = true,
        });

        proc.WaitForExit();

        DotNetCliFeatureBand = proc.StandardOutput.ReadToEnd().Trim();
        DotNetCliFeatureBand = DotNetCliFeatureBand.Substring(0, DotNetCliFeatureBand.Length - 2) + "00";

        DotNetInstalledWorkloadsMetadataPath = P.Combine(DotNetInstallPath, "metadata", "workloads", DotNetCliFeatureBand, "InstalledWorkloads");
        DotNetInstallerTypeMetadataPath = P.Combine(DotNetInstallPath, "metadata", "workloads", DotNetCliFeatureBand, "InstallerType");
        DotNetManifestPath = P.Combine(DotNetInstallPath, "sdk-manifests", DotNetCliFeatureBand);
        DotNetPacksPath = P.Combine(DotNetInstallPath, "packs");
        DotNetTemplatePacksPath = P.Combine(DotNetInstallPath, "template-packs");
    }

    public static void RegisterInstalledWorkload(string workloadName)
    {
        D.CreateDirectory(DotNetInstalledWorkloadsMetadataPath);
        F.WriteAllText(P.Combine(DotNetInstalledWorkloadsMetadataPath, workloadName), string.Empty);
        if (F.Exists(P.Combine(DotNetInstallerTypeMetadataPath, "msi")))
        {
            //HKLM:\SOFTWARE\Microsoft\dotnet\InstalledWorkloads\Standalone\x64\6.0.300\gtk

            // TODO: Check for other Windows architectures (x86 and arm64)
            var archString = RI.OSArchitecture.ToString().ToLower();

            var hklm = R.LocalMachine;
            var software = hklm.CreateSubKey("SOFTWARE");
            var microsoft = software.CreateSubKey("Microsoft");
            var dotnet = microsoft.CreateSubKey("dotnet");
            var installedWorkloads = dotnet.CreateSubKey("InstalledWorkloads");
            var standalone = installedWorkloads.CreateSubKey("Standalone");
            var arch = standalone.CreateSubKey(archString);
            var version = arch.CreateSubKey(DotNetCliFeatureBand);
            var workload = version.CreateSubKey(workloadName);

            workload.Close();
            version.Close();
            arch.Close();
            standalone.Close();
            installedWorkloads.Close();
            dotnet.Close();
            microsoft.Close();
            software.Close();
            hklm.Close();
        }
    }

    public static void UnregisterInstalledWorkload(string workloadName)
    {
        F.Delete(P.Combine(DotNetInstalledWorkloadsMetadataPath, workloadName));
        if (F.Exists(P.Combine(DotNetInstallerTypeMetadataPath, "msi")))
        {
            var archString = RI.OSArchitecture.ToString().ToLower();

            var hklm = R.LocalMachine;
            var software = hklm.CreateSubKey("SOFTWARE");
            var microsoft = software.CreateSubKey("Microsoft");
            var dotnet = microsoft.CreateSubKey("dotnet");
            var installedWorkloads = dotnet.CreateSubKey("InstalledWorkloads");
            var standalone = installedWorkloads.CreateSubKey("Standalone");
            var arch = standalone.CreateSubKey(archString);
            var version = arch.CreateSubKey(DotNetCliFeatureBand);

            version.DeleteSubKey(workloadName, false);

            version.Close();
            arch.Close();
            standalone.Close();
            installedWorkloads.Close();
            dotnet.Close();
            microsoft.Close();
            software.Close();
            hklm.Close();
        }
    }

    public static void InstallManifests(string manifestName, string manifestPackPath)
    {
        var targetDirectory = P.Combine(DotNetManifestPath, manifestName);
        var tempDirectory = P.Combine(targetDirectory, "temp");

        // Delete existing installations to avoid conflict.
        if (D.Exists(targetDirectory))
        {
            D.Delete(targetDirectory, true);
        }

        // Also creates the target
        D.CreateDirectory(tempDirectory);

        Z.ExtractToDirectory(manifestPackPath, tempDirectory);
        var tempDataDirectory = P.Combine(tempDirectory, "data");

        foreach (var filePath in D.GetFiles(tempDataDirectory))
        {
            var targetFilePath = P.Combine(targetDirectory, P.GetFileName(filePath));
            F.Copy(filePath, targetFilePath, true);
        }

        D.Delete(tempDirectory, true);
    }

    public static void UninstallManifests(string manifestName)
    {
        var targetDirectory = P.Combine(DotNetManifestPath, manifestName);
        if (D.Exists(targetDirectory))
        {
            D.Delete(targetDirectory, true);
        }
    }

    public static void InstallPack(string name, string version, string packPath)
    {
        var targetDirectory = P.Combine(DotNetPacksPath, name, version);

        if (D.Exists(targetDirectory))
        {
            D.Delete(targetDirectory, true);
        }

        D.CreateDirectory(targetDirectory);

        Z.ExtractToDirectory(packPath, targetDirectory);
    }

    public static void UninstallPack(string name, string version)
    {
        var packInstallDirectory = P.Combine(DotNetPacksPath, name);

        var targetDirectory = P.Combine(packInstallDirectory, version);
        if (D.Exists(targetDirectory))
        {
            D.Delete(targetDirectory, true);
        }

        // Clean up the template if no other verions exist.
        try
        {
            D.Delete(packInstallDirectory, false);
        }
        catch (System.IO.IOException)
        {
            // Silently fail. Mostly because the directory is not empty (there are other verions installed).
        }
    }

    public static void InstallTemplatePack(string packName, string packPath)
    {
        D.CreateDirectory(DotNetTemplatePacksPath);
        var targetPath = P.Combine(DotNetTemplatePacksPath, packName);
        F.Copy(packPath, targetPath, true);
    }

    public static void UninstallTemplatePack(string packName)
    {
        var targetPath = P.Combine(DotNetTemplatePacksPath, packName);
        if (F.Exists(targetPath))
        {
            F.Delete(targetPath);
        }
    }
}