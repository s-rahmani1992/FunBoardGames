using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OnlineBoardGames.SET
{
    public class VotePanelUI : MonoBehaviour
    {
        [SerializeField]
        Button yesBtn, noBtn;

        void Start(){
            var eventHandler = SingletonUIHandler.GetInstance<SETUIEventHandler>();
            eventHandler.OnGameStateChanged += OnStateChange;
            eventHandler.OnCommonOrLocalStateEvent += OnLocalVote;
            gameObject.SetActive(false);
        }

        private void OnLocalVote(UIStates state){
            if (state == UIStates.PlaceVote || state == UIStates.BeginVote)
                noBtn.interactable = yesBtn.interactable = false;
            else if (state == UIStates.Clear)
                noBtn.interactable = yesBtn.interactable = true;
        }

        private void OnStateChange(SETGameState state){
            gameObject.SetActive(state == SETGameState.Request);
        }

        public void SendVote(bool yes){
            Mirror.NetworkClient.Send(new VoteMessage { isYes = yes });
        }
    }
}
