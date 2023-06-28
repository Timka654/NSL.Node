$basePath = ".."

New-Item -ItemType Directory -Path "$basePath\NSL.Node.RoomServer.Shared"

Copy-Item -Path "NSL.Node.RoomServer.Shared\*" -Destination "$basePath\NSL.Node.RoomServer.Shared" -Recurse -exclude bin,obj
Copy-Item -Path "Examples\NSL.Node.AspRoomServerExample\*" -Destination "$basePath\NSL.Node.AspRoomServerExample" -Recurse -exclude bin,obj
Copy-Item -Path "Examples\NSL.Node.LocalRoomServerExample\*" -Destination "$basePath\NSL.Node.LocalRoomServerExample" -Recurse -exclude bin,obj
Copy-Item -Path "NSL.Node.RoomServer.sln.example" -Destination "$basePath\NSL.Node.RoomServer.sln"


Copy-Item -Path "$basePath\NSL.Node.AspRoomServerExample\NSL.Node.AspRoomServerExample.csproj.example" -Destination "$basePath\NSL.Node.AspRoomServerExample\NSL.Node.AspRoomServerExample.csproj" -Force
Copy-Item -Path "$basePath\NSL.Node.LocalRoomServerExample\NSL.Node.LocalRoomServerExample.csproj.example" -Destination "$basePath\NSL.Node.LocalRoomServerExample\NSL.Node.LocalRoomServerExample.csproj" -Force

Remove-Item "$basePath\NSL.Node.AspRoomServerExample\NSL.Node.AspRoomServerExample.csproj.example"
Remove-Item "$basePath\NSL.Node.LocalRoomServerExample\NSL.Node.LocalRoomServerExample.csproj.example"