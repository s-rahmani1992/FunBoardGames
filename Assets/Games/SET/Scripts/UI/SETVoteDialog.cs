using DG.Tweening;
using FunBoardGames.Network;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FunBoardGames.SET
{
    public class SETVoteDialog : BaseDialog, IDataDialog<(ISETGameHandler gameHandler, ISETPlayer starterPlayer, List<ISETPlayer> players)>
    {
        [SerializeField] Button yesBtn, noBtn;
        [SerializeField] PlayerVoteUI[] voteUIPlayers;

        Dictionary<ISETPlayer, PlayerVoteUI> voteUIMap = new();

        ISETGameHandler setGameHandler;
        List<ISETPlayer> players;
        ISETPlayer selfPlayer;

        public override void Show()
        {
            base.Show();
            setGameHandler.VoteResultReceived += OnVoteResultReceived;
            setGameHandler.PlayerVoteReceived += OnPlayerVoteReceived;
            int index = 0;

            foreach(var player in players)
            {
                voteUIMap.Add(player, voteUIPlayers[index]);
                voteUIPlayers[index].SetPlayer(player);

                if (player.IsMe)
                {
                    selfPlayer = player;
                    OnLocalPlayerVoteChanged(player.Vote);
                }

                index++;
            }
        }

        private void OnPlayerVoteReceived(ISETPlayer player, bool positive)
        {
            voteUIMap[player].UpdateVoteUI(positive);
        }

        private void OnVoteResultReceived(bool passed)
        {
            DOVirtual.DelayedCall(0.5f, Close);
        }

        private void OnLocalPlayerVoteChanged(bool? vote)
        {
            noBtn.interactable = yesBtn.interactable = (!vote.HasValue);
        }

        public override void Close()
        {
            setGameHandler.VoteResultReceived -= OnVoteResultReceived;
            setGameHandler.PlayerVoteReceived -= OnPlayerVoteReceived;
            base.Close();
        }

        public void SendVote(bool yes)
        {
            setGameHandler.VoteCard(yes);
        }

        public void Initialize((ISETGameHandler gameHandler, ISETPlayer starterPlayer, List<ISETPlayer> players) data)
        {
            setGameHandler = data.gameHandler;
            players = new(data.players);
        }
    }
}
