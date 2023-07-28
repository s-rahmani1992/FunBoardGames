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
        public BoardGame GameType { get; private set; }
        public Guid MatchId { get; private set; }

        public void SetParameters(bool isCreate, string roomName, BoardGame gameType)
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
