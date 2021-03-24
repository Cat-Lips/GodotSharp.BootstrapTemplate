<# .SYNOPSIS #>
[CmdletBinding()]
Param(
    [string]$ProjectName
   ,[string]$ProjectPath
   ,[string]$UserDataDir
   ,[string]$GodotExe
)

$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'

. $PSScriptRoot/utils.ps1

################################################################################
### Functions ##################################################################
################################################################################

function Sync-Project {

	function Sync-File($path) {
		if (!$path) { return }
		if ($path.Basename -eq $ProjectName) { return }

		$old_name = $path.Name
		$new_name = "$ProjectName$($path.Extension)"
		Write-Host "[ProjectSync] Renaming '$old_name' to '$new_name'"
		return Rename-Item -PassThru $path $new_name
	}

	function Sync-Content($path, $as_code) {
		if (!$path) { return }
		if ($ProjectName -eq $old_csproj) { return }

		$old_content = if ($as_code) { $old_csproj.Replace(' ', '') } else { $old_csproj }
		$new_content = if ($as_code) { $ProjectName.Replace(' ', '') } else { $ProjectName }
		Write-Host "[ProjectSync] Replacing '$old_content' with '$new_content' in '$($path.Name)'"
		(Get-Content $path) -Replace $old_content, $new_content | Set-Content $path -Encoding UTF8
	}

	function Sync-Shortcut($path, $target) {
		if (!$path) { return }

		$shell = New-Object -ComObject WScript.Shell
		$shortcut = $shell.CreateShortcut($path)

        $app_data = $env:APPDATA.Replace('\', '/')
		$old_target = $shortcut.TargetPath.Replace('\', '/').Replace($app_data, '%APPDATA%')
		$new_target = $target.Replace('\', '/').Replace($app_data, '%APPDATA%')

		if ($old_target -eq $new_target) { return }

		Write-Host "[ProjectSync] Redirecting '$($path.Name)' from '$old_target' to '$new_target'"
        New-Item -Type Directory -Force $target | Out-Null
		$shortcut.TargetPath = $new_target
		$shortcut.Save()
	}

	function Sync-Launcher($path) {
		if (!$path) { return }
        $godot_exe = (Get-RelativePath $GodotExe $ProjectPath).Replace('\', '/')

        function Sync-GodotExe($json, $key) {
            $old_value = $json.profiles.$key.executablePath
            if ($old_value -eq $godot_exe) { return }
            Write-Host "[ProjectSync] Setting 'profiles.$key.executablePath' to '$godot_exe' in '$($path.Name)' (from '$old_value')"
            $json.profiles.$key.executablePath = $godot_exe
        }

        $json = Get-Content $path | ConvertFrom-Json
        Sync-GodotExe $json $ProjectName
        Sync-GodotExe $json 'Run Tests'
        $json | ConvertTo-Json -Depth 100 | Format-Json | Set-Content $path -Encoding UTF8
	}

	$sln = Get-ChildItem $ProjectPath/*.sln
	$csproj = Get-ChildItem $ProjectPath/*.csproj
	$old_csproj = if ($csproj) { [IO.Path]::GetFileNameWithoutExtension($csproj) }

	$sln = Sync-File $sln
	$csproj = Sync-File $csproj

	$launch_settings = Get-ChildItem $ProjectPath/Properties/launchSettings.json
	$project_template = Get-ChildItem $ProjectPath/.editor/ProjectTemplate.cs
	$app_data_shortcut = Get-ChildItem $ProjectPath/.appdata.lnk

	Sync-Content $sln
	Sync-Content $csproj $true
	Sync-Content $launch_settings
	Sync-Content $project_template $true
	Sync-Shortcut $app_data_shortcut $UserDataDir
	Sync-Launcher $launch_settings
}

################################################################################
### MAIN #######################################################################
################################################################################

Sync-Project
