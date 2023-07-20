using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OnlineBoardGames
{
    [CreateAssetMenu]
    public class RoomRequestContainer : ScriptableObject
    {
        public bool IsCreate { get; private set; }
        public string RoomName { get; private set; }
        public BoardGameTypes GameType { get; private set; }
        public Guid MatchId { get; private set; }

        public event Action<BoardGameRoomManager> RoomGenerated;
        public event Action<BoardGamePlayer> PlayerAdded;

        public void Invoke(BoardGamePlayer player) => PlayerAdded?.Invoke(player);
        public void Invoke(BoardGameRoomManager room) => RoomGenerated?.Invoke(room);

        public void SetParameters(bool isCreate, string roomName, BoardGameTypes gameType)
        {
            IsCreate = isCreate;
            RoomName = roomName;
            GameType = gameType;
        }

        public void SetParameters(Guid id)
        {
            IsCreate = false;
            MatchId = id;
        }
    }
}
