using FunBoardGames.Network;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace FunBoardGames
{
    public class RoomUI : MonoBehaviour
    {
        [SerializeField] Text nameTxt, numberTxt;
        [SerializeField] Button joinBtn;

        public event Action<RoomUI> JoinClicked;

        public RoomInfo roomData { get; private set; }

        public void Initialize(RoomInfo roomData)
        {
            this.roomData = roomData;
            nameTxt.text = roomData.Name;
            numberTxt.text = roomData.PlayerCount.ToString();
        }

        public void Join()
        {
            JoinClicked?.Invoke(this);
        }
    }
}