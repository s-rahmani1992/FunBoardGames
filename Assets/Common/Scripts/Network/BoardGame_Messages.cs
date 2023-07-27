using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

namespace OnlineBoardGames
{
    public struct PlayerReadyMessage : NetworkMessage { }

    public struct CreateRoomMessage : NetworkMessage
    {
        public string reqName;
        public BoardGameTypes gameType;
    }

    public struct GetRoomListMessage : NetworkMessage
    {
        public BoardGameTypes gameType;
    }

    public struct RoomListResponse : NetworkMessage
    {
        public SerializableRoom[] rooms;
    }

    public struct JoinMatchMessage : NetworkMessage 
    {
        public Guid matchID;
    }

    public struct LeaveRoomMessage : NetworkMessage { }

    public struct NotifyJoinRoom : NetworkMessage
    {
        public BoardGameRoomManager room;
    }
}
