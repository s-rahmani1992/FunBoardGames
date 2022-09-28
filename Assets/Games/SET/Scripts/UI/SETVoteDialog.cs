using System;
using System.Collections;
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

        public override void Show()
        {
            var eventHandler = SingletonUIHandler.GetInstance<SETUIEventHandler>();
            eventHandler.OnCommonOrLocalStateEvent += OnLocalVote;
            DialogManager.OnSETVoteDialogSpawned?.Invoke(this);
            base.Show();
        }

        public override void Close()
        {
            var eventHandler = SingletonUIHandler.GetInstance<SETUIEventHandler>();
            eventHandler.OnCommonOrLocalStateEvent -= OnLocalVote;
            base.Close();
        }

        private void OnLocalVote(UIStates state)
        {
            if (state == UIStates.PlaceVote || state == UIStates.BeginVote)
                noBtn.interactable = yesBtn.interactable = false;
            else if (state == UIStates.Clear)
                noBtn.interactable = yesBtn.interactable = true;
        }

        public void SendVote(bool yes)
        {
            Mirror.NetworkClient.Send(new VoteMessage { isYes = yes });
        }

        public PlayerVoteUI GetVoteUI(int index)
        {
            return voteUIPlayers.GetChild(index).GetComponent<PlayerVoteUI>();
        }
    }
}
