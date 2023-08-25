using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OnlineBoardGames.CantStop
{
    public class CantStopRoomManager : RoomManager
    {
        public override BoardGame GameType => BoardGame.CantStop;

        protected override void BeginGame() { }
    }
}
