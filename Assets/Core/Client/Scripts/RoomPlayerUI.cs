using UnityEngine;
using UnityEngine.UI;


namespace FunBoardGames
{
    public class RoomPlayerUI : MonoBehaviour
    {
        [SerializeField]
        Text playerTxt;
        [SerializeField]
        RawImage readyLED;
        [SerializeField]
        Texture2D ledOn, ledOff;

        BoardGamePlayer player;

        void RefreshUI(string name, bool ready)
        {
            playerTxt.text = name;
            playerTxt.color = player.IsOwner ? Color.yellow : Color.cyan;
            readyLED.texture = (ready ? ledOn : ledOff);
        }

        void PlayerLeft()
        {
            UnSubscribe();
            Destroy(gameObject);
        }

        void OnDestroy()
        {
            if (player == null)
                return;

            UnSubscribe();
        }

        public void SetPlayer(BoardGamePlayer player)
        {
            this.player = player;
            Subscribe();
            RefreshUI(player.Name, player.IsReady);
        }

        void Subscribe()
        {
            player.ReadyChanged += OnReadyChanged;
            player.IndexChanged += OnIndexChanged;
            player.LeftGame += PlayerLeft;
        }

        void UnSubscribe()
        {
            player.ReadyChanged -= OnReadyChanged;
            player.IndexChanged -= OnIndexChanged;
            player.LeftGame -= PlayerLeft;
        }

        void OnIndexChanged(int _, int index)
        {
            transform.SetSiblingIndex(index);
        }

        void OnReadyChanged(bool ready)
        {
            readyLED.texture = (ready ? ledOn : ledOff); ;
        }
    }
}