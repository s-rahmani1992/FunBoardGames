using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        [SerializeField] RoomRequestContainer roomContainer;

        public void OnPull(params object[] parameters)
        {
            transform.parent = parameters[0] as Transform;
            RefreshUI(parameters[1] as RoomData);
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
            id = room.Id;
        }

        public void Join()
        {
            roomContainer.SetParameters(id);
            SceneManager.LoadScene("Room");
        }
    }
}