using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OnlineBoardGames.SET
{
    public class SETResultDialog : BaseDialog
    {
        [SerializeField] SETPlayerUIResult[] players;

        public void Initialize(SETRoomManager roomManager)
        {
            List<SETPlayer> sortedPlayers = new(roomManager.Players.Select(p => p as SETPlayer));
            sortedPlayers.Sort(SETPlayer.Compare);
            sortedPlayers.Reverse();

            for (int i = 0; i < sortedPlayers.Count; i++)
                players[i].SETUI(sortedPlayers[i], i + 1);
        }

        public void OnMenuClicked()
        {
            Mirror.NetworkClient.Send(new LeaveRoomMessage { });
        }
    }
}
