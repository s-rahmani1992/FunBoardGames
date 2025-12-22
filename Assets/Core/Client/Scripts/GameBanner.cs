using FunBoardGames.Network;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FunBoardGames.Client 
{
    public class GameBanner : MonoBehaviour
    {
        [SerializeField] BoardGame gameType; 
        [SerializeField] TMP_Text titleText;
        [SerializeField] Button createButton;
        [SerializeField] Button joinListButton;
        [SerializeField] Button joinRandomButton;
        [SerializeField] TMP_InputField roomNameField;
        [SerializeField] RoomDialog roomDialog;
        [SerializeField] RoomListDialog roomListDialog;

        ILobbyHandler lobbyHandler;

        private void Start()
        {
            lobbyHandler = NetworkSingleton.NetworkManager.LobbyHandler;
            titleText.text = gameType.ToString();
            createButton.onClick.AddListener(OnCreateClicked);
            joinListButton.onClick.AddListener(OnJoinListClicked);
            joinRandomButton.onClick.AddListener(OnJoinRandomClicked);
            roomNameField.onValueChanged.AddListener(OnroomNameChanged);
            OnroomNameChanged("");
        }

        private void OnroomNameChanged(string textValue)
        {
            createButton.interactable = textValue.Length > 1;
        }

        private void OnJoinRandomClicked()
        {
            throw new NotImplementedException();
        }

        private void OnJoinListClicked()
        {
            DialogManager.Instance.ShowDialog(roomListDialog, DialogShowOptions.OverAll, (gameType, lobbyHandler));
        }

        private void OnCreateClicked()
        {
            DialogManager.Instance.ShowDialog(roomDialog, DialogShowOptions.OverAll, (lobbyHandler, gameType, roomNameField.text, (int?)null));
        }
    } 
}
