param(
    [Parameter(Mandatory)][string]$path,
    [Switch]$KeepVersion,
    [Parameter()][ValidateSet('major', 'minor', 'build')][string]$Increase = 'build'
)

$oldpath = Get-Location
Set-Location $path

if (-Not ($KeepVersion.IsPresent)) {
    $file = $( Get-ChildItem *.csproj )[0]
    $regex = '(?<=(?:File|Package|Assembly)?Version>)(\d+)\.(\d+)\.(\d+)(?=<)'
    $found = (Get-Content $file) | Select-String -Pattern $regex

    if ($Increase -Eq 'major') {
        $major = [int]$found.Matches.Groups[1].Value + 1
        $minor = 0
        $build = 0
    } elseif ($Increase -Eq 'minor') {
        $major = [int]$found.Matches.Groups[1].Value
        $minor = [int]$found.Matches.Groups[2].Value + 1
        $build = 0
    } else {
        $major = [int]$found.Matches.Groups[1].Value
        $minor = [int]$found.Matches.Groups[2].Value
        $build = [int]$found.Matches.Groups[3].Value + 1
    }

    (Get-Content $file) -replace $regex, "$major.$minor.$build" | Set-Content $file -Encoding UTF8
}

dotnet pack --configuration Release

foreach ($file in $( Get-ChildItem .\bin\Release\*.nupkg )) {
    dotnet nuget push $file --source "nuget.org"
    Remove-Item -Path $file
}

Set-Location $oldpath
