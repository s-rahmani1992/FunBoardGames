using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OnlineBoardGames
{
    public class MenuUIManager : MonoBehaviour
    {
        [SerializeField]
        InputField roomNameIn;
        [SerializeField]
        ObjectPoolManager pool;
        [SerializeField]
        Transform content;

        // Start is called before the first frame update
        void Start()
        {
            SingletonUIHandler.GetInstance<MenuUIEventHandler>().OnRoomListRefresh += (list) =>
            {
                while (content.childCount > 0)
                    pool.Push2List(content.GetChild(0).gameObject);
                foreach (var r in list)
                    pool.PullFromList(0, content, r);
            };
        }

        public void SendCreateRoom()
        {
            Mirror.NetworkClient.Send(new CreateRoomMessage {reqName = roomNameIn.text, gameType = BoardGameTypes.SET });
        }

        public void SendRoomListRequest()
        {
            Mirror.NetworkClient.Send(new GetRoomListMessage { gameType = BoardGameTypes.SET });
        }
    }
}