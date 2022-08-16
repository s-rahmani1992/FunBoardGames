using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OnlineBoardGames
{
    public class RoomUI : MonoBehaviour, IPoolable
    {
        public string ObjectTag { get; set; }
        System.Guid id;
        [SerializeField]
        Text nameTxt, numberTxt;
        [SerializeField]
        Button joinBtn;

        public void OnPull(params object[] parameters)
        {
            transform.parent = parameters[0] as Transform;
            RefreshUI(parameters[1] as SerializableRoom);
            gameObject.SetActive(true);
        }

        public void OnPush(params object[] parameters)
        {
            transform.parent = null;
            gameObject.SetActive(false);
        }

        // Start is called before the first frame update
        void Start()
        {
            SingletonUIHandler.GetInstance<MenuUIEventHandler>().OnJoinRoomAttempt += () => { joinBtn.gameObject.SetActive(false); };
        }

        void RefreshUI(SerializableRoom room){
            nameTxt.text = room.roomName;
            numberTxt.text = room.playerCount.ToString();
            id = room.id;
        }

        public void Join()
        {
            Mirror.NetworkClient.Send(new JoinMatchMessage { matchID = id });
        }
    }
}