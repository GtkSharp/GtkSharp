#!/usr/bin/env pwsh

param(
    [string]$Version = "<latest>",
    [string]$InstallDir = "<auto>",
    [string]$TargetSdkVersionBand = "<auto>",
    [string]$ManifestVersionBand = "<auto>",
    [string]$Source = "<nuget>"
)

$MANIFEST_BASE_NAME = "gtksharp.net.sdk.gtk.manifest"
$DOTNET_DEFAULT_PATH_LINUX = "/usr/share/dotnet"
$DOTNET_DEFAULT_PATH_MACOS = "/usr/local/share/dotnet"
$DOTNET_DEFAULT_PATH_WINDOWS = "$env:ProgramFiles\dotnet"

function Ensure-Directory {
    param([string]$Path)
    try {
        New-Item -ItemType Directory -Path $Path -Force | Out-Null
        $testFile = Join-Path $Path ".write_test_$(Get-Random)"
        [System.IO.File]::Create($testFile).Close()
        Remove-Item $testFile -Force
    }
    catch {
        Write-Error "No permission to install manifest. Try again with elevated privileges (sudo/Administrator)."
        exit 1
    }
}

function Install-Workload {
    param([string]$DotNetSdkVersion)
    
    $array = $DotNetSdkVersion.Split('.')
    $CURRENT_DOTNET_VERSION = [int]$array[0]
    $DOTNET_VERSION_BAND = "$($array[0]).$($array[1]).$($array[2].Substring(0,1))00"
    $MANIFEST_NAME = "$MANIFEST_BASE_NAME-$DOTNET_VERSION_BAND"
    
    # Check .NET SDK version band
    $dotnetSdkBand = $TargetSdkVersionBand
    if ($dotnetSdkBand -eq "<auto>") {
        if ($CURRENT_DOTNET_VERSION -ge 7) {
            if (($DotNetSdkVersion -match "-preview" -or $DotNetSdkVersion -match "-rc" -or $DotNetSdkVersion -match "-alpha") -and $array.Length -ge 4) {
                $dotnetSdkBand = "$DOTNET_VERSION_BAND$($array[2].Substring(3)).$($array[3])"
                $MANIFEST_NAME = "$MANIFEST_BASE_NAME-$dotnetSdkBand"
            }
            elseif ($DotNetSdkVersion -match "-rtm" -and $array.Length -ge 3) {
                $dotnetSdkBand = "$DOTNET_VERSION_BAND$($array[2].Substring(3))"
                $MANIFEST_NAME = "$MANIFEST_BASE_NAME-$dotnetSdkBand"
            }
            else {
                $dotnetSdkBand = $DOTNET_VERSION_BAND
            }
        }
        else {
            $dotnetSdkBand = $DOTNET_VERSION_BAND
        }
    }
    Write-Host "Target .NET SDK version band: $dotnetSdkBand"

    # Check manifest version band
    if ($ManifestVersionBand -eq "<auto>") {
        $ManifestVersionBand = $dotnetSdkBand
    } else {
        $MANIFEST_NAME = "$MANIFEST_BASE_NAME-$ManifestVersionBand"
    }
    Write-Host "Manifest version band: $ManifestVersionBand"
    
    # Check latest version of manifest
    $manifestVersion = $Version
    if ($manifestVersion -eq "<latest>") {
        if ($Source -ne "<nuget>") {
            Write-Warning "Only <nuget> source is supported for getting latest version."
            return
        }
        try {
            Write-Host "Getting latest version of $MANIFEST_NAME from NuGet..."
            $response = Invoke-RestMethod -Uri "https://api.nuget.org/v3-flatcontainer/$($MANIFEST_NAME.ToLower())/index.json" -ErrorAction Stop
            if ($response.versions -and $response.versions.Count -gt 0) {
                $manifestVersion = $response.versions[-1]
            }
            
            if ($manifestVersion -match "BlobNotFound") {
                Write-Warning "Failed to get the latest version of $MANIFEST_NAME."
                return
            } else {
                Write-Host "Latest version of $MANIFEST_NAME is $manifestVersion"
            }
        }
        catch {
            Write-Warning "Failed to get the latest version of $MANIFEST_NAME."
            return
        }
    }
    Write-Host "Version of $MANIFEST_NAME is $manifestVersion"
    
    # Check workload manifest directory
    $SDK_MANIFESTS_DIR = Join-Path $InstallDir "sdk-manifests\$dotnetSdkBand"
    Ensure-Directory $SDK_MANIFESTS_DIR
    
    $TMPDIR = Join-Path ([System.IO.Path]::GetTempPath()) ([System.IO.Path]::GetRandomFileName())
    New-Item -ItemType Directory -Path $TMPDIR | Out-Null
    
    Write-Host "Installing $MANIFEST_NAME/$manifestVersion to $SDK_MANIFESTS_DIR..."
    
    # Download and extract the manifest nuget package
    $manifestZip = Join-Path $TMPDIR "manifest.zip"
    try {
        if ($Source -ne "<nuget>") {
            $nupkgPath = Join-Path $Source "$MANIFEST_NAME.$manifestVersion.nupkg"
            if (-not (Test-Path $Source)) {
                Write-Error "Source path '$Source' does not exist."
                Remove-Item -Path $TMPDIR -Recurse -Force
                return
            }
            if (-not (Test-Path "$nupkgPath")) {
                Write-Error "Manifest package '$nupkgPath' not found."
                Remove-Item -Path $TMPDIR -Recurse -Force
                return
            }
            Copy-Item -Path "$nupkgPath" -Destination $manifestZip -ErrorAction Stop
        } else {
            Invoke-WebRequest -Uri "https://www.nuget.org/api/v2/package/$MANIFEST_NAME/$manifestVersion" -OutFile $manifestZip -ErrorAction Stop
        }
    }
    catch {
        Write-Error "Failed to download manifest package."
        Remove-Item -Path $TMPDIR -Recurse -Force
        return
    }
    
    $unzippedDir = Join-Path $TMPDIR "unzipped"
    Expand-Archive -Path $manifestZip -DestinationPath $unzippedDir -Force
    
    $dataDir = Join-Path $unzippedDir "data"
    if (-not (Test-Path $dataDir)) {
        Write-Error "No such files to install."
        Remove-Item -Path $TMPDIR -Recurse -Force
        return
    }

    # Open WorkloadManifest.json and extract version
    $workloadManifestFile = Join-Path $dataDir "WorkloadManifest.json"
    if (-not (Test-Path $workloadManifestFile)) {
        Write-Error "No WorkloadManifest.json found."
        Remove-Item -Path $TMPDIR -Recurse -Force
        return
    }
    $workloadManifest = Get-Content $workloadManifestFile | ConvertFrom-Json
    if (-not $workloadManifest.version) {
        Write-Error "No version found in WorkloadManifest.json."
        Remove-Item -Path $TMPDIR -Recurse -Force
        return
    }
    $manifestFileVersion = $workloadManifest.version
    Write-Host "Installing GTK workload version $manifestFileVersion"

    # Copy manifest files to dotnet sdk
    $targetManifestDir = Join-Path $SDK_MANIFESTS_DIR "gtksharp.net.sdk.gtk\$manifestFileVersion"
    Ensure-Directory $targetManifestDir
    Copy-Item -Path "$dataDir\*" -Destination $targetManifestDir -Force
    
    if (-not (Test-Path (Join-Path $targetManifestDir "WorkloadManifest.json"))) {
        Write-Error "Installation failed."
        Remove-Item -Path $TMPDIR -Recurse -Force
        return
    }
    
    $workingDir = Join-Path $TMPDIR "working"
    New-Item -ItemType Directory -Path $workingDir | Out-Null
    Push-Location $workingDir

    try {
        # Install workload packs
        & $DOTNET_COMMAND new globaljson --sdk-version $DotNetSdkVersion | Out-Null
        if ($Source -ne "<nuget>") {
            & $DOTNET_COMMAND workload install gtk --skip-manifest-update --source $Source
        } else {
            & $DOTNET_COMMAND workload install gtk --skip-manifest-update
        }
    } catch {
        Write-Error "Installation failed."
        Remove-Item -Path $TMPDIR -Recurse -Force
        return
    } finally {
        Pop-Location
    }

    # Clean-up
    Remove-Item -Path $TMPDIR -Recurse -Force
    Remove-Item "global.json" -Force -ErrorAction SilentlyContinue
    
    Write-Host "Done installing GTK workload $manifestVersion"
    Write-Host ""
}

# Check dotnet install directory
if ($InstallDir -eq "<auto>") {
    if ($env:DOTNET_ROOT -and (Test-Path $env:DOTNET_ROOT)) {
        $InstallDir = $env:DOTNET_ROOT
    }
    elseif (Test-Path $DOTNET_DEFAULT_PATH_WINDOWS) {
        $InstallDir = $DOTNET_DEFAULT_PATH_WINDOWS
    }
    elseif (Test-Path $DOTNET_DEFAULT_PATH_LINUX) {
        $InstallDir = $DOTNET_DEFAULT_PATH_LINUX
    }
    elseif (Test-Path $DOTNET_DEFAULT_PATH_MACOS) {
        $InstallDir = $DOTNET_DEFAULT_PATH_MACOS
    }
    else {
        $dotnetPath = Get-Command dotnet -ErrorAction SilentlyContinue
        if ($dotnetPath) {
            try {
                $resolved = Resolve-Path $dotnetPath.Source -ErrorAction Stop
                $InstallDir = Split-Path -Parent $resolved.Path
            }
            catch {
                $InstallDir = $null
            }
        } else {
            $InstallDir = $null
        }
    }
}

if (-not (Test-Path $InstallDir)) {
    Write-Error "No installed dotnet at '$InstallDir'."
    exit 1
}

# Check installed dotnet version
$DOTNET_COMMAND = Join-Path $InstallDir "dotnet"
if ($IsWindows -or $env:OS -eq "Windows_NT") {
    $DOTNET_COMMAND = "$DOTNET_COMMAND.exe"
}

if (-not (Test-Path $DOTNET_COMMAND)) {
    Write-Error "$DOTNET_COMMAND command not found"
    exit 1
}

# Get installed .NET SDKs
$INSTALLED_DOTNET_SDKS = @(& $DOTNET_COMMAND --version)

if (-not $INSTALLED_DOTNET_SDKS -or $INSTALLED_DOTNET_SDKS.Count -eq 0) {
    Write-Error ".NET SDK version 6 or later is required to install GTK Workload."
}
else {
    foreach ($DOTNET_SDK in $INSTALLED_DOTNET_SDKS) {
        Write-Host "Check GTK Workload for sdk $DOTNET_SDK."
        Install-Workload $DOTNET_SDK
    }
}
