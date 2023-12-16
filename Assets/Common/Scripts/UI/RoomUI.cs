using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace OnlineBoardGames
{
    public class RoomUI : MonoBehaviour, IPoolable
    {
        public string ObjectTag { get; set; }
        RoomData roomData;
        BoardGame gameType;
        [SerializeField]
        Text nameTxt, numberTxt;
        [SerializeField]
        Button joinBtn;
        [SerializeField] RoomRequestContainer roomContainer;

        public void OnPull(params object[] parameters)
        {
            transform.parent = parameters[0] as Transform;
            RefreshUI(parameters[1] as RoomData);
            gameType = (BoardGame)parameters[2];
            gameObject.SetActive(true);
        }

        public void OnPush(params object[] parameters)
        {
            transform.parent = null;
            gameObject.SetActive(false);
        }

        void RefreshUI(RoomData room){
            nameTxt.text = room.Name;
            numberTxt.text = room.PlayerCount.ToString();
            roomData = room;
        }

        public void Join()
        {
            roomContainer.SetParameters(false, roomData, gameType);
            SceneManager.LoadScene("Room");
        }
    }
}