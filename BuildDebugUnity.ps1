$ver = $args[0]
dotnet build --version-suffix "$ver" --configuration UnityDebug --output "build/Debug/unity_build" "NSL.Node.Unity.sln"
dotnet pack --version-suffix "$ver" --configuration UnityDebug --output "build/Debug/unity_package" "NSL.Node.Unity.sln"

$buildPath = "build/Debug/unity_dll"

$patternHere  = 'UnityEngine'

$directoryInfo = [System.IO.DirectoryInfo]::new($buildPath)

if($directoryInfo.Exists)
{
    foreach($item in ($directoryInfo.GetFiles("*", 1)))
    {
        if($item.Name.Contains($patternHere))
        {
            Write-Output "Remove file $($item.FullName)"
            $item.Delete()
        }
    }
}