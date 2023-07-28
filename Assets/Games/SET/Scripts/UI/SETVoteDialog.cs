using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OnlineBoardGames.SET
{
    public class SETVoteDialog : BaseDialog
    {
        [SerializeField]
        Button yesBtn, noBtn;
        [SerializeField] RectTransform voteUIPlayers;

        SETRoomNetworkManager manager;
        List<SETNetworkPlayer> players;
        SETNetworkPlayer localPlayer;

        public override void Show()
        {
            base.Show();

            manager.StateChanged += OnStateChanged;

            foreach(var player in players)
            {
                GetVoteUI(player.Index).SetPlayer(player);

                if (player.hasAuthority)
                {
                    localPlayer = player;
                    OnLocalPlayerVoteChanged(VoteStat.NULL, localPlayer.voteState);
                    localPlayer.VoteChanged += OnLocalPlayerVoteChanged; 
                } 
            }
        }

        private void OnLocalPlayerVoteChanged(VoteStat _, VoteStat state)
        {
            noBtn.interactable = yesBtn.interactable = (state == VoteStat.NULL);
        }

        private void OnStateChanged(SETGameState oldVal, SETGameState _)
        {
            if (oldVal == SETGameState.CardVote)
                Close();
        }

        public override void Close()
        {
            manager.StateChanged -= OnStateChanged;
            localPlayer.VoteChanged -= OnLocalPlayerVoteChanged;
            base.Close();
        }

        public void Init(SETRoomNetworkManager manager, List<SETNetworkPlayer> players)
        {
            this.manager = manager;
            this.players = players;
        }

        public void SendVote(bool yes)
        {
            manager.CmdSendVote(localPlayer, yes);
        }

        public PlayerVoteUI GetVoteUI(int index)
        {
            return voteUIPlayers.GetChild(index).GetComponent<PlayerVoteUI>();
        }
    }
}
