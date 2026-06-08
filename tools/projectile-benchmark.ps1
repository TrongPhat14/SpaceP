param(
    [Parameter(Mandatory = $true)]
    [ValidateSet(
        "Pair",
        "Connect",
        "Devices",
        "Install",
        "Clear",
        "Pull",
        "Summarize"
    )]
    [string]$Action,

    [string]$Endpoint,
    [string]$ApkPath,
    [string]$Name = "projectile-pooling-benchmark",
    [string]$InputPath,
    [string]$AdbPath =
        "D:\Mygame\ProgramFiles\Unity\Hub\Editor\6000.3.10f1\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\platform-tools\adb.exe"
)

$ErrorActionPreference = "Stop"

$packageName = "com.trongphat.spacep"
$remoteCsvPath =
    "/sdcard/Android/data/$packageName/files/projectile-pooling-benchmark.csv"
$projectRoot = Split-Path -Parent $PSScriptRoot
$resultDirectory = Join-Path $projectRoot "Temp\BenchmarkResults"

function Assert-Adb {
    if (-not (Test-Path -LiteralPath $AdbPath)) {
        throw "ADB was not found at: $AdbPath"
    }
}

function Assert-Endpoint {
    if ([string]::IsNullOrWhiteSpace($Endpoint)) {
        throw "-Endpoint is required for action $Action."
    }
}

function Invoke-AdbText {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Arguments
    )

    $startInfo = New-Object System.Diagnostics.ProcessStartInfo
    $startInfo.FileName = $AdbPath
    $startInfo.Arguments = $Arguments
    $startInfo.UseShellExecute = $false
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    $startInfo.CreateNoWindow = $true

    $process = New-Object System.Diagnostics.Process
    $process.StartInfo = $startInfo
    [void]$process.Start()

    $standardOutput = $process.StandardOutput.ReadToEnd()
    $standardError = $process.StandardError.ReadToEnd()
    $process.WaitForExit()

    if ($process.ExitCode -ne 0) {
        throw "ADB failed ($($process.ExitCode)): $standardError"
    }

    return $standardOutput
}

function Get-Median {
    param(
        [Parameter(Mandatory = $true)]
        [double[]]$Values
    )

    if ($Values.Count -eq 0) {
        return [double]::NaN
    }

    $sorted = @($Values | Sort-Object)
    $middle = [math]::Floor($sorted.Count / 2)

    if ($sorted.Count % 2 -eq 1) {
        return [double]$sorted[$middle]
    }

    return ([double]$sorted[$middle - 1] + [double]$sorted[$middle]) / 2
}

function Get-ValidMedian {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [object[]]$Values
    )

    $validValues = @(
        $Values |
            ForEach-Object { [double]$_ } |
            Where-Object { $_ -ge 0 }
    )

    return Get-Median $validValues
}

function Get-PercentChange {
    param(
        [double]$Baseline,
        [double]$Pooling,
        [switch]$LowerIsBetter
    )

    if ($Baseline -eq 0) {
        return [double]::NaN
    }

    if ($LowerIsBetter) {
        return (($Baseline - $Pooling) / $Baseline) * 100
    }

    return (($Pooling - $Baseline) / $Baseline) * 100
}

function Format-Number {
    param(
        [double]$Value,
        [string]$Format = "F2"
    )

    if ([double]::IsNaN($Value)) {
        return "N/A"
    }

    return $Value.ToString(
        $Format,
        [Globalization.CultureInfo]::InvariantCulture
    )
}

function Convert-ToMedianResult {
    param(
        [Parameter(Mandatory = $true)]
        [object]$Group
    )

    $rows = @($Group.Group)

    return [pscustomobject]@{
        Mode = $rows[0].mode
        Scenario = $rows[0].scenario
        Runs = $rows.Count
        AverageFps = Get-Median @(
            $rows | ForEach-Object { [double]$_.average_fps }
        )
        OnePercentLowFps = Get-Median @(
            $rows | ForEach-Object { [double]$_.one_percent_low_fps }
        )
        P95FrameMs = Get-Median @(
            $rows | ForEach-Object { [double]$_.p95_frame_ms }
        )
        TotalGcBytes = Get-ValidMedian @(
            $rows | ForEach-Object { [double]$_.total_gc_bytes }
        )
        PeakGcPerFrameBytes = Get-ValidMedian @(
            $rows | ForEach-Object { [double]$_.peak_gc_per_frame_bytes }
        )
        MemoryDeltaBytes = Get-Median @(
            $rows | ForEach-Object {
                $start = [double]$_.memory_start_bytes
                $end = [double]$_.memory_end_bytes

                if ($start -lt 0 -or $end -lt 0) {
                    continue
                }

                $end - $start
            }
        )
        ProjectilesCreated = Get-Median @(
            $rows | ForEach-Object { [double]$_.projectiles_created }
        )
        ProjectilesReused = Get-Median @(
            $rows | ForEach-Object { [double]$_.projectiles_reused }
        )
        PeakActiveProjectiles = Get-Median @(
            $rows | ForEach-Object {
                [double]$_.peak_active_projectiles
            }
        )
    }
}

function Write-BenchmarkSummary {
    param(
        [Parameter(Mandatory = $true)]
        [string]$CsvPath
    )

    if (-not (Test-Path -LiteralPath $CsvPath)) {
        throw "CSV was not found: $CsvPath"
    }

    $rows = @(Import-Csv -LiteralPath $CsvPath)
    if ($rows.Count -eq 0) {
        throw "CSV contains no benchmark rows."
    }

    $groups = @(
        $rows |
            Group-Object mode, scenario |
            ForEach-Object { Convert-ToMedianResult $_ } |
            Sort-Object Scenario, Mode
    )

    $expectedGroups = @(
        "Pooling|Gameplay",
        "InstantiateDestroy|Gameplay",
        "Pooling|Stress",
        "InstantiateDestroy|Stress"
    )

    $warnings = New-Object System.Collections.Generic.List[string]

    foreach ($expected in $expectedGroups) {
        $parts = $expected.Split("|")
        $match = @(
            $groups |
                Where-Object {
                    $_.Mode -eq $parts[0] -and
                    $_.Scenario -eq $parts[1]
                }
        )

        if ($match.Count -eq 0) {
            $warnings.Add("Missing result group: $expected")
        }
        elseif ($match[0].Runs -ne 3) {
            $warnings.Add(
                "$expected has $($match[0].Runs) run(s); expected 3."
            )
        }
    }

    $lines = New-Object System.Collections.Generic.List[string]
    $lines.Add("# Projectile Pooling Benchmark Summary")
    $lines.Add("")
    $lines.Add("Source: ``$CsvPath``")
    $lines.Add("")
    $lines.Add(
        "| Mode | Scenario | Runs | Avg FPS | 1% Low | P95 ms | " +
        "GC total bytes | GC peak/frame | Memory delta bytes | " +
        "Created | Reused | Peak active |"
    )
    $lines.Add(
        "|---|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|"
    )

    foreach ($group in $groups) {
        $lines.Add(
            "| $($group.Mode) | $($group.Scenario) | $($group.Runs) | " +
            "$(Format-Number $group.AverageFps) | " +
            "$(Format-Number $group.OnePercentLowFps) | " +
            "$(Format-Number $group.P95FrameMs) | " +
            "$(Format-Number $group.TotalGcBytes 'F0') | " +
            "$(Format-Number $group.PeakGcPerFrameBytes 'F0') | " +
            "$(Format-Number $group.MemoryDeltaBytes 'F0') | " +
            "$(Format-Number $group.ProjectilesCreated 'F0') | " +
            "$(Format-Number $group.ProjectilesReused 'F0') | " +
            "$(Format-Number $group.PeakActiveProjectiles 'F0') |"
        )
    }

    $lines.Add("")
    $lines.Add("## Pooling vs Instantiate/Destroy")
    $lines.Add("")
    $lines.Add(
        "| Scenario | Avg FPS improvement | 1% low improvement | " +
        "P95 reduction | GC reduction |"
    )
    $lines.Add("|---|---:|---:|---:|---:|")

    foreach ($scenario in @("Gameplay", "Stress")) {
        $pooling = $groups |
            Where-Object {
                $_.Mode -eq "Pooling" -and $_.Scenario -eq $scenario
            } |
            Select-Object -First 1
        $baseline = $groups |
            Where-Object {
                $_.Mode -eq "InstantiateDestroy" -and
                $_.Scenario -eq $scenario
            } |
            Select-Object -First 1

        if ($null -eq $pooling -or $null -eq $baseline) {
            continue
        }

        $fps = Get-PercentChange $baseline.AverageFps $pooling.AverageFps
        $low = Get-PercentChange `
            $baseline.OnePercentLowFps `
            $pooling.OnePercentLowFps
        $p95 = Get-PercentChange `
            $baseline.P95FrameMs `
            $pooling.P95FrameMs `
            -LowerIsBetter
        $gc = Get-PercentChange `
            $baseline.TotalGcBytes `
            $pooling.TotalGcBytes `
            -LowerIsBetter

        $lines.Add(
            "| $scenario | $(Format-Number $fps)% | " +
            "$(Format-Number $low)% | $(Format-Number $p95)% | " +
            "$(Format-Number $gc)% |"
        )
    }

    if ($warnings.Count -gt 0) {
        $lines.Add("")
        $lines.Add("## Validation Warnings")
        $lines.Add("")

        foreach ($warning in $warnings) {
            $lines.Add("- $warning")
        }
    }

    New-Item -ItemType Directory -Force $resultDirectory | Out-Null
    $summaryPath = Join-Path $resultDirectory "benchmark-summary.md"
    $lines | Set-Content -LiteralPath $summaryPath -Encoding utf8
    $lines | ForEach-Object { Write-Host $_ }
    Write-Host ""
    Write-Host "Summary written to: $summaryPath"
}

switch ($Action) {
    "Pair" {
        Assert-Adb
        Assert-Endpoint
        & $AdbPath pair $Endpoint
        if ($LASTEXITCODE -ne 0) {
            throw "ADB pairing failed."
        }
    }

    "Connect" {
        Assert-Adb
        Assert-Endpoint
        & $AdbPath connect $Endpoint
        if ($LASTEXITCODE -ne 0) {
            throw "ADB connection failed."
        }
    }

    "Devices" {
        Assert-Adb
        & $AdbPath devices
        if ($LASTEXITCODE -ne 0) {
            throw "Unable to list ADB devices."
        }
    }

    "Install" {
        Assert-Adb

        if ([string]::IsNullOrWhiteSpace($ApkPath)) {
            throw "-ApkPath is required for action Install."
        }

        $resolvedApkPath = (Resolve-Path -LiteralPath $ApkPath).Path
        & $AdbPath install -r $resolvedApkPath
        if ($LASTEXITCODE -ne 0) {
            throw "APK installation failed."
        }
    }

    "Clear" {
        Assert-Adb
        $output = Invoke-AdbText("shell rm -f $remoteCsvPath")
        Write-Host $output
        Write-Host "Remote benchmark CSV cleared."
    }

    "Pull" {
        Assert-Adb
        New-Item -ItemType Directory -Force $resultDirectory | Out-Null

        $safeName = $Name -replace "[^A-Za-z0-9._-]", "-"
        $outputPath = Join-Path $resultDirectory "$safeName.csv"
        $csv = Invoke-AdbText("exec-out cat $remoteCsvPath")

        if ([string]::IsNullOrWhiteSpace($csv)) {
            throw "The remote benchmark CSV is empty or unavailable."
        }

        [IO.File]::WriteAllText(
            $outputPath,
            $csv,
            (New-Object Text.UTF8Encoding($false))
        )
        Write-Host "CSV written to: $outputPath"
    }

    "Summarize" {
        if ([string]::IsNullOrWhiteSpace($InputPath)) {
            $InputPath = Join-Path `
                $resultDirectory `
                "projectile-pooling-benchmark.csv"
        }

        $resolvedInputPath = (Resolve-Path -LiteralPath $InputPath).Path
        Write-BenchmarkSummary $resolvedInputPath
    }
}
