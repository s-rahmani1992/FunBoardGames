using FunBoardGames.Client;
using FunBoardGames.Network;
using System.Collections.Generic;
using UnityEngine;

namespace FunBoardGames
{
    public class MenuDirectGameController : MonoBehaviour
    {
        [SerializeField] RoomDialog roomDialog;
        [SerializeField] bool autoJoin;
        [SerializeField] BoardGame gameType;

        ILobbyHandler lobbyHandler;

        void Start()
        {
            lobbyHandler = NetworkSingleton.NetworkManager.LobbyHandler; 

            if (autoJoin)
            {
                DialogManager.Instance.ShowDialog(roomDialog, DialogShowOptions.OverAll, (lobbyHandler, gameType, "Direct Game " + gameType, (int?)-1));
            }
        }
    }
}
