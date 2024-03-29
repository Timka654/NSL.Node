$ver = $args[0]
dotnet build --version-suffix "$ver" --configuration Unity --output "build/Release/unity_dll_$ver" "NSL.Node.Unity.sln"
dotnet pack --version-suffix "$ver" --configuration Unity --output "build/Release/unity_package_$ver" "NSL.Node.Unity.sln"


$buildPath = "build/Release/unity_dll_$ver"

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