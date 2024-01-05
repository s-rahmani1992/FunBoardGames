using System;
using UnityEngine;
using UnityEngine.UI;

namespace OnlineBoardGames
{
    public class RoomUI : MonoBehaviour
    {
        [SerializeField] Text nameTxt, numberTxt;
        [SerializeField] Button joinBtn;

        public event Action<RoomUI> JoinClicked;

        public RoomData roomData { get; private set; }

        public void Initialize(RoomData roomData)
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