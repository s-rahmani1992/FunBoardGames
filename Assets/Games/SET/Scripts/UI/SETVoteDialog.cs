using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FunBoardGames.SET
{
    public class SETVoteDialog : BaseDialog
    {
        [SerializeField]
        Button yesBtn, noBtn;
        [SerializeField] RectTransform voteUIPlayers;

        SETRoomManager manager;
        List<SETPlayer> players;
        SETPlayer localPlayer;

        public override void Show()
        {
            base.Show();

            manager.StateChanged += OnStateChanged;

            foreach(var player in players)
            {
                GetVoteUI(player.Index).SetPlayer(player);

                if (player.IsOwner)
                {
                    localPlayer = player;
                    OnLocalPlayerVoteChanged(VoteAnswer.None, localPlayer.VoteAnswer);
                    localPlayer.VoteChanged += OnLocalPlayerVoteChanged; 
                } 
            }
        }

        private void OnLocalPlayerVoteChanged(VoteAnswer _, VoteAnswer state)
        {
            noBtn.interactable = yesBtn.interactable = (state == VoteAnswer.None);
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

        public void Init(SETRoomManager manager, List<SETPlayer> players)
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
