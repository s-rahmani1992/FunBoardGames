using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace OnlineBoardGames.SET{
    public class GameLogger : MonoBehaviour
    {
        [SerializeField]
        Text statTxt;

        private void Awake(){
            GameUIEventManager.OnCommonOrLocalStateEvent.AddListener(OnGenericOrLocalNewEvent);
            GameUIEventManager.OnOtherStateEvent.AddListener(OnOtherPlayerNewEvent);
        }

        private void OnDestroy(){
            GameUIEventManager.OnCommonOrLocalStateEvent.RemoveListener(OnGenericOrLocalNewEvent);
            GameUIEventManager.OnOtherStateEvent.RemoveListener(OnOtherPlayerNewEvent);
        }

        private void OnOtherPlayerNewEvent(UIStates state, string playerName){
            switch (state){
                case UIStates.GuessBegin:
                    statTxt.text = $"{playerName} is Guessing";
                    break;
                case UIStates.GuessTimeout:
                    statTxt.text = $"{playerName} did not guess in time!\nHe lost one point!";
                    break;
                case UIStates.GuessRight:
                    statTxt.text = $"{playerName} Guessed Right! He Got 1 point.";
                    break;
                case UIStates.GuessWrong:
                    statTxt.text = $"{playerName}'s Guess was Wrong! He lost 1 point.";
                    break;
                case UIStates.BeginVote:
                    statTxt.text = $"{playerName} Voted For destribute. place your vote.";
                    break;

            }
        }

        private void OnGenericOrLocalNewEvent(UIStates state){
            switch (state){
                case UIStates.Clear:
                    statTxt.text = "";
                    break;
                case UIStates.StartGame:
                    statTxt.text = "Wait For The Game To Start.";
                    break;
                case UIStates.CardDestribute:
                    statTxt.text = "Destributing Cards";
                    break;
                case UIStates.GuessBegin:
                    statTxt.text = "You Are Guessing. Guess Quickly.";
                    break;
                case UIStates.GuessTimeout:
                    statTxt.text = "You did not guess in time!\nYou lost one point!";
                    break;
                case UIStates.GuessRight:
                    statTxt.text = "You Guessed Right! You Got 1 point.";
                    break;
                case UIStates.GuessWrong:
                    statTxt.text = "Your Guess was Wrong! You lost 1 point.";
                    break;
                case UIStates.PlaceVote:
                    statTxt.text = "Wait For Others to vote.";
                    break;
            };
        }
    }
}
