Copy-Item -Path "NSL.Node.RoomServer.Shared\*" -Destination "..\NSL.Node.RoomServer.Shared" -Recurse
Copy-Item -Path "Examples\NSL.Node.AspRoomServerExample\*" -Destination "..\NSL.Node.AspRoomServerExample" -Recurse -exclude bin,obj
Copy-Item -Path "Examples\NSL.Node.LocalRoomServerExample\*" -Destination "..\NSL.Node.LocalRoomServerExample" -Recurse -exclude bin,obj
Copy-Item -Path "NSL.Node.RoomServer.sln.example" -Destination "..\NSL.Node.RoomServer.sln"


Copy-Item -Path "..\NSL.Node.AspRoomServerExample\NSL.Node.AspRoomServerExample.csproj.example" -Destination "..\NSL.Node.AspRoomServerExample\NSL.Node.AspRoomServerExample.csproj" -Force
Copy-Item -Path "..\NSL.Node.LocalRoomServerExample\NSL.Node.LocalRoomServerExample.csproj.example" -Destination "..\NSL.Node.LocalRoomServerExample\NSL.Node.LocalRoomServerExample.csproj" -Force