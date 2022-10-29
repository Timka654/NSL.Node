$ver = $args[0]
dotnet build --version-suffix "$ver" --configuration UnityDebug --output "build/Debug/unity_build" "NSL.Node.Unity.sln"
dotnet pack --version-suffix "$ver" --configuration UnityDebug --output "build/Debug/unity_package" "NSL.Node.Unity.sln"