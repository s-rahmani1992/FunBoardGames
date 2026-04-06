using FunBoardGames.Network;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FunBoardGames.CantStop
{
    public class CantStopResultDialog : BaseDialog, IDataDialog<IEnumerable<ICantStopPlayer>>
    {
        [SerializeField] CantStopPlayerResultUI[] playerUIs;
        List<ICantStopPlayer> players;

        public override void Show()
        {
            base.Show();

            for (int i = 0; i < players.Count; i++)
            {
                playerUIs[i].SETUI(players[i], i + 1);
            }

            for (int i = players.Count; i < playerUIs.Length; i++)
            {
                playerUIs[i].gameObject.SetActive(false);
            }
        }

        public void Initialize(IEnumerable<ICantStopPlayer> data)
        {
            players = data.ToList();
        }

        public void OnMenuClicked()
        {
            SceneManager.LoadScene("Menu");
        }
    }
}
