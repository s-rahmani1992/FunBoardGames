using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using System.Linq;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/guides/networkbehaviour
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

// NOTE: Do not put objects in DontDestroyOnLoad (DDOL) in Awake.  You can do that in Start instead.



namespace OnlineBoardGames.SET {
    [Serializable]
    public enum GameState : byte
    {
        Normal,
        Guess,
        Destribute,
        Process
    }

    [Serializable]
    public class RoomData
    {
        public float guessTime;
        public int roundCount;
    }

    public class SETSessionNetworkManager : NetworkBehaviour
    {
        [SerializeField]
        SETGameUIManager gameUIManager;

        public bool CanSelect { get => (state == GameState.Guess) && (guessingPlayer != null) && (guessingPlayer.hasAuthority); }

        #region Server Only
        List<byte> deck;
        List<byte> placedCards = new List<byte>(18);
        int cursor = 0;
        Coroutine guessProcess;

        [Server]
        bool ValidateGuess(byte c1, byte c2, byte c3){
            if (MyUtils.CompareItems(c1, c2, c3) != TripleComparisonResult.ALL_DIFFERRENT) return false;
            if (!placedCards.Contains(c1) || !placedCards.Contains(c2) || !placedCards.Contains(c3)) return false;
            return true;
        }

        [Server]
        public IEnumerator DelaySendBegin(){
            yield return new WaitForSeconds(0.5f);
            RPCBeginGame();
            yield return new WaitForSeconds(roomInfo.guessTime);
            byte[] temp = new byte[12];
            for (int i = 0; i < 12; i++){
                temp[i] = (byte)i;
                placedCards[i] = deck[i];
            }
            RPCPlaceCards(deck.GetRange(cursor, 12).ToArray(), temp);
            cursor = 12;
            yield return new WaitForSeconds(2.5f);
            state = GameState.Normal;
        }

        [Server]
        IEnumerator ProcessGuess(){
            yield return new WaitForSeconds(roomInfo.guessTime);
            state = GameState.Process;
            RPCPopTimeout(guessingPlayer);
            yield return new WaitForSeconds(2);
            guessingPlayer.GetComponent<SETNetworkPlayer>().wrongs++;
            guessingPlayer.GetComponent<SETNetworkPlayer>().isGuessing = false;
            state = GameState.Normal;
            guessProcess = null;
            guessingPlayer = null;
        }

        [Server]
        IEnumerator ResultProcess(byte result, GuessSETMessage msg){
            RPCPopResult(msg, result, guessingPlayer);
            yield return new WaitForSeconds(6.8f);
            if (MyUtils.IsSET(result)){
                guessingPlayer.GetComponent<SETNetworkPlayer>().corrects++;
                byte[] temp = new byte[3];
                temp[0] = (byte)placedCards.IndexOf(msg.card1);
                temp[1] = (byte)placedCards.IndexOf(msg.card2);
                temp[2] = (byte)placedCards.IndexOf(msg.card3);
                placedCards[temp[0]] = deck[cursor];
                placedCards[temp[1]] = deck[cursor + 1];
                placedCards[temp[2]] = deck[cursor + 2];

                RPCPlaceCards(deck.GetRange(cursor, 3).ToArray(), temp);
                cursor += 3;
                yield return new WaitForSeconds(0.7f);
            }
            else
                guessingPlayer.GetComponent<SETNetworkPlayer>().wrongs++;

            guessingPlayer.GetComponent<SETNetworkPlayer>().isGuessing = false;
            guessingPlayer = null;
            state = GameState.Normal;
            guessProcess = null;
        }

        #endregion

        #region Syncvars
        [SyncVar(hook = nameof(GameStateChanged))]
        public GameState state;

        [SyncVar]
        public RoomData roomInfo;

        [SyncVar]
        public NetworkIdentity guessingPlayer;
        #endregion

        #region Syncvar Hooks
        void GameStateChanged(GameState oldVal, GameState newVal){
            DebugStep.Log($"State {oldVal}, {newVal}");
            gameUIManager?.RefreshBtns(newVal);
        }
        #endregion

        #region Client RPCs
        [ClientRpc]
        private void RPCBeginGame(){
            gameUIManager.timer.StartCountdown(roomInfo.guessTime);
        }

        [ClientRpc]
        private void RPCPlaceCards(byte[] cards, byte[] cardPoses){
            gameUIManager.PlaceCards(cards, cardPoses);
        }

        [ClientRpc]
        void RPCCheat(string cheat) {
            Debug.LogError("Cheat Detected!- " + cheat);
        }

        [ClientRpc]
        void RPCPopTimeout(NetworkIdentity guessPlayer){
            gameUIManager.PopTimeout(guessPlayer.hasAuthority ? "You did not guess in time!\nYou lost one point!" : $"{guessPlayer.GetComponent<SETNetworkPlayer>().playerName} did not guess in time!\nHe lost one point!");
        }

        [ClientRpc]
        void RPCPopResult(GuessSETMessage msg, byte result, NetworkIdentity guessPlayer){
            gameUIManager.PopResult(result, msg, guessPlayer);
        }
        #endregion

        #region MessageHandlers
        private void OnAttemptGuess(NetworkConnectionToClient conn, AttempSETGuess msg){
            if (guessingPlayer == null && state == GameState.Normal && guessProcess == null){
                guessingPlayer = conn.identity;
                conn.identity.GetComponent<SETNetworkPlayer>().isGuessing = true;
                state = GameState.Guess;
                guessProcess = StartCoroutine(ProcessGuess());
            }
        }

        private void OnSETGuess(NetworkConnectionToClient conn, GuessSETMessage msg){
            if (state == GameState.Guess && guessingPlayer != null && conn.identity == guessingPlayer && ValidateGuess(msg.card1, msg.card2, msg.card3)){
                byte r = CardData.CheckSET(msg.card1, msg.card2, msg.card3);
                state = GameState.Process;
                StopCoroutine(guessProcess);
                StartCoroutine(ResultProcess(r, msg));
            }
        }

        #endregion

        #region Unity Callbacks

        private void Awake(){
            SETNetworkManager.singleton.session = this;
            //gameUIManager = FindObjectOfType<SETGameUIManager>();
        }

        #endregion

        #region Start & Stop Callbacks

        /// <summary>
        /// This is invoked for NetworkBehaviour objects when they become active on the server.
        /// <para>This could be triggered by NetworkServer.Listen() for objects in the scene, or by NetworkServer.Spawn() for objects that are dynamically created.</para>
        /// <para>This will be called for objects on a "host" as well as for object on a dedicated server.</para>
        /// </summary>
        public override void OnStartServer() {
            state = GameState.Destribute;
            deck = SETNetworkManager.singleton.GetShuffleDeck();
            cursor = 0;
            roomInfo = new RoomData { guessTime = 5, roundCount = 1 };
            for (int i = 0; i < 18; i++)
                placedCards.Insert(i, 0);
            NetworkServer.RegisterHandler<AttempSETGuess>(OnAttemptGuess, false);
            NetworkServer.RegisterHandler<GuessSETMessage>(OnSETGuess, false);
        }

        /// <summary>
        /// Invoked on the server when the object is unspawned
        /// <para>Useful for saving object data in persistent storage</para>
        /// </summary>
        public override void OnStopServer() { }

        /// <summary>
        /// Called on every NetworkBehaviour when it is activated on a client.
        /// <para>Objects on the host have this function called, as there is a local client on the host. The values of SyncVars on object are guaranteed to be initialized correctly with the latest state from the server when this function is called on the client.</para>
        /// </summary>
        public override void OnStartClient() { }

        /// <summary>
        /// This is invoked on clients when the server has caused this object to be destroyed.
        /// <para>This can be used as a hook to invoke effects or do client specific cleanup.</para>
        /// </summary>
        public override void OnStopClient() { }

        /// <summary>
        /// Called when the local player object has been set up.
        /// <para>This happens after OnStartClient(), as it is triggered by an ownership message from the server. This is an appropriate place to activate components or functionality that should only be active for the local player, such as cameras and input.</para>
        /// </summary>
        public override void OnStartLocalPlayer() { }

        /// <summary>
        /// Called when the local player object is being stopped.
        /// <para>This happens before OnStopClient(), as it may be triggered by an ownership message from the server, or because the player object is being destroyed. This is an appropriate place to deactivate components or functionality that should only be active for the local player, such as cameras and input.</para>
        /// </summary>
        public override void OnStopLocalPlayer() { }

        /// <summary>
        /// This is invoked on behaviours that have authority, based on context and <see cref="NetworkIdentity.hasAuthority">NetworkIdentity.hasAuthority</see>.
        /// <para>This is called after <see cref="OnStartServer">OnStartServer</see> and before <see cref="OnStartClient">OnStartClient.</see></para>
        /// <para>When <see cref="NetworkIdentity.AssignClientAuthority">AssignClientAuthority</see> is called on the server, this will be called on the client that owns the object. When an object is spawned with <see cref="NetworkServer.Spawn">NetworkServer.Spawn</see> with a NetworkConnectionToClient parameter included, this will be called on the client that owns the object.</para>
        /// </summary>
        public override void OnStartAuthority() { }

        /// <summary>
        /// This is invoked on behaviours when authority is removed.
        /// <para>When NetworkIdentity.RemoveClientAuthority is called on the server, this will be called on the client that owns the object.</para>
        /// </summary>
        public override void OnStopAuthority() { }

        #endregion
    } 
}
