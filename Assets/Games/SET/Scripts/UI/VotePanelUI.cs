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

        void Awake(){
            GameUIEventManager.OnGameStateChanged.AddListener(OnStateChange);
            GameUIEventManager.OnCommonOrLocalStateEvent.AddListener(OnLocalVote);
            gameObject.SetActive(false);
        }

        private void OnDestroy(){
            GameUIEventManager.OnGameStateChanged.RemoveListener(OnStateChange);
            GameUIEventManager.OnCommonOrLocalStateEvent.RemoveListener(OnLocalVote);
        }

        private void OnLocalVote(UIStates state){
            if (state == UIStates.PlaceVote || state == UIStates.BeginVote)
                noBtn.interactable = yesBtn.interactable = false;
            else if (state == UIStates.Clear)
                noBtn.interactable = yesBtn.interactable = true;
        }

        private void OnStateChange(GameState state){
            gameObject.SetActive(state == GameState.Request);
        }

        public void SendVote(bool yes){
            Mirror.NetworkClient.Send(new VoteMessage { isYes = yes });
        }
    }
}
