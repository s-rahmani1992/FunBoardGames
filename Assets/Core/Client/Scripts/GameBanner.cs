using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OnlineBoardGames.Client 
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

        LobbyManager lobbyManager;

        private void Start()
        {
            lobbyManager = FindObjectOfType<LobbyManager>();
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
            throw new NotImplementedException();
        }

        private void OnCreateClicked()
        {
            DialogManager.Instance.ShowDialog(roomDialog, DialogShowOptions.OverAll, (lobbyManager, gameType, roomNameField.text, (int?)null));
        }
    } 
}
