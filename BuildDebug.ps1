$ver = $args[0]
dotnet build --configuration Debug --output "build/Debug/build" --version-suffix "$ver" "NSL.Node.sln"
dotnet pack --configuration Debug --output "build/Debug/package" --version-suffix "$ver" "NSL.Node.sln"