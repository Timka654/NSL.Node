using NSL.WebSockets.Server.AspNetPoint;

namespace WelcomeServer.Data.Models
{
    public class LobbyNetworkClientModel : AspNetWSNetworkServerClient
    {
        private LobbyRoomInfoModel currentRoom;

        public Guid UID { get; set; }
        public string Username { get; set; }

        public Guid CurrentRoomId { get; set; }

        public LobbyRoomInfoModel CurrentRoom { get => currentRoom; set { currentRoom = value; CurrentRoomId = value?.Id ?? default; } }
    }
}
