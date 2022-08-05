using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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

    public class GameUIEventManager
    {
        public static UnityEvent<GameState> OnGameStateChanged = new UnityEvent<GameState>();
        public static UnityEvent<bool> OnPlayerVote = new UnityEvent<bool>();
        public static UnityEvent<UIStates> OnCommonOrLocalStateEvent = new UnityEvent<UIStates>();
        public static UnityEvent<UIStates, string> OnOtherStateEvent = new UnityEvent<UIStates, string>();
    }
}
