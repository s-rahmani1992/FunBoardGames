using FishNet.Broadcast;
using Mirror;
using System;

namespace OnlineBoardGames
{
    public struct AuthRequestMessage : IBroadcast
    {
        public string requestedName;
    }

    public struct AuthResponseMessage : IBroadcast
    {
        public byte resultCode;
        public string message;
    }

    public struct CreateRoomMessage : NetworkMessage
    {
        public string reqName;
        public BoardGame gameType;
    }

    public struct GetRoomListMessage : NetworkMessage
    {
        public BoardGame gameType;
    }

    public struct RoomListResponse : NetworkMessage
    {
        public RoomData[] rooms;
    }

    public struct JoinMatchMessage : NetworkMessage 
    {
        public BoardGame gameType;
        public Guid matchID;
    }

    public struct LeaveRoomMessage : NetworkMessage 
    {
        public BoardGame gameType; 
    }

    public struct NotifyJoinRoom : NetworkMessage
    {
        public RoomManager room;
    }
}
