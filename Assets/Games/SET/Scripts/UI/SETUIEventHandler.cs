using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace OnlineBoardGames.SET
{
    public enum UIStates : byte
    {
        Clear,
        StartGame,
        CardDestribute,
        GuessBegin,
        GuessTimeout,
        GuessRight,
        GuessWrong,
        BeginVote,
        PlaceVote
    }

    public class SETUIEventHandler : MonoBehaviour
    {
        public Action<SETGameState> OnGameStateChanged;
        public Action<bool> OnPlayerVote;
        public Action<UIStates> OnCommonOrLocalStateEvent;
        public Action<UIStates, string> OnOtherStateEvent;
        public Action<SETNetworkPlayer, bool> OnPlayerBeginVote;
        //public Action<PlayerUI, bool> OnPlayerVote;

        void Awake(){
            SingletonUIHandler.SetInstance(this);
        }

        private void OnDestroy(){
            OnGameStateChanged = null;
            OnPlayerBeginVote = null;
            OnPlayerVote = null;
            OnCommonOrLocalStateEvent = null;
            OnOtherStateEvent = null;
            SingletonUIHandler.SetInstance<MonoBehaviour>(null);
        }
    }
}
