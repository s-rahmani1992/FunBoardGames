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
    public enum SETGameState : byte
    {
        Normal,
        Guess,
        Destribute,
        Process,
        Request
    }

    [Serializable]
    public class RoomData
    {
        public float guessTime;
        public int roundCount;
    }

    public class SETRoomNetworkManager : BoardGameRoomManager<SETNetworkPlayer>
    {
        public bool CanSelect { get => (state == SETGameState.Guess) && (guessingPlayer != null) && (guessingPlayer.hasAuthority); }

        #region Server Only
        List<byte> deck;
        List<byte> placedCards = new List<byte>(18);
        int placedCardCount;
        int cursor = 0;
        int voteYesCount, voteNoCount = 0;
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
            placedCardCount = 12;
            for (int i = 0; i < 12; i++){
                temp[i] = (byte)i;
                placedCards[i] = deck[i];
            }
            RPCPlaceCards(deck.GetRange(cursor, 12).ToArray(), temp);
            cursor = 12;
            yield return new WaitForSeconds(2.5f);
            state = SETGameState.Normal;
        }

        [Server]
        IEnumerator ProcessGuess(){
            yield return new WaitForSeconds(roomInfo.guessTime);
            state = SETGameState.Process;
            RPCPopTimeout(guessingPlayer);
            yield return new WaitForSeconds(2);
            guessingPlayer.GetComponent<SETNetworkPlayer>().wrongs++;
            guessingPlayer.GetComponent<SETNetworkPlayer>().isGuessing = false;
            state = SETGameState.Normal;
            guessProcess = null;
            guessingPlayer = null;
        }

        [Server]
        IEnumerator ResultProcess(byte result, GuessSETMessage msg){
            RPCPopResult(msg, result, guessingPlayer);
            yield return new WaitForSeconds(6.8f);
            guessingPlayer.GetComponent<SETNetworkPlayer>().isGuessing = false;
            if (CardData.IsSET(result)){
                guessingPlayer.GetComponent<SETNetworkPlayer>().corrects++;
                if (cursor < 81 && placedCardCount == 12){
                    placedCards[placedCards.IndexOf(msg.card1)] = 0;
                    placedCards[placedCards.IndexOf(msg.card2)] = 0;
                    placedCards[placedCards.IndexOf(msg.card3)] = 0;
                    byte[] places2Add = new byte[3];
                    int place = 0;
                    for (int i = 0; i < placedCards.Count; i++){
                        if (placedCards[i] == 0){
                            places2Add[place] = (byte)i;
                            placedCards[i] = deck[cursor + place];
                            place++;
                            if (place == 3)
                                break;
                        }
                    }

                    RPCPlaceCards(deck.GetRange(cursor, 3).ToArray(), places2Add);
                    cursor += 3;
                    yield return new WaitForSeconds(0.7f);
                }
                else{
                    int temp = placedCards.IndexOf(msg.card1);
                    placedCards[temp] = 0;
                    temp = placedCards.IndexOf(msg.card2);
                    placedCards[temp] = 0;
                    temp = placedCards.IndexOf(msg.card3);
                    placedCards[temp] = 0;
                    placedCardCount -= 3;
                    yield return new WaitForSeconds(0.2f);
                }
            }
            else
                guessingPlayer.GetComponent<SETNetworkPlayer>().wrongs++;

            guessingPlayer = null;
            state = SETGameState.Normal;
            guessProcess = null;
        }

        [Server]
        IEnumerator ProcessVote(bool v){
            voteNoCount = voteYesCount = 0;
            yield return new WaitForSeconds(1);
            if (v){
                state = SETGameState.Destribute;
                if (cursor < 81){
                    byte[] placed2Add = new byte[3];
                    int place = 0;
                    for (int i = 0; i < placedCards.Count; i++){
                        if (placedCards[i] == 0){
                            placed2Add[place] = (byte)i;
                            placedCards[i] = deck[cursor + place];
                            place++;
                            if (place == 3)
                                break;
                        }
                    }
                    placedCardCount += 3;
                    RPCPlaceCards(deck.GetRange(cursor, 3).ToArray(), placed2Add);
                    cursor += 3;
                    foreach (var p in roomPlayers)
                        p.voteState = VoteStat.NULL;
                    yield return new WaitForSeconds(0.7f);
                }
                else{
                    yield return new WaitForSeconds(0.2f);
                }
                foreach (var p in roomPlayers)
                    p.voteState = VoteStat.NULL;
                state = SETGameState.Normal;
            }
            else{
                state = SETGameState.Normal;
                foreach (var p in roomPlayers)
                    p.voteState = VoteStat.NULL;
            }
        }
        #endregion

        #region Syncvars
        [SyncVar(hook = nameof(GameStateChanged))]
        public SETGameState state;

        [SyncVar]
        public RoomData roomInfo;

        [SyncVar]
        public NetworkIdentity guessingPlayer;
        #endregion

        #region Syncvar Hooks
        void GameStateChanged(SETGameState oldVal, SETGameState newVal){
            SingletonUIHandler.GetInstance<SETUIEventHandler>()?.OnGameStateChanged?.Invoke(newVal);
            if (newVal == SETGameState.Normal)
                SingletonUIHandler.GetInstance<SETUIEventHandler>()?.OnCommonOrLocalStateEvent?.Invoke(UIStates.Clear);
        }
        #endregion

        #region Client RPCs
        [ClientRpc(includeOwner = false)]
        protected override void RPCPlayerJoin(NetworkIdentity identity){
            base.RPCPlayerJoin(identity);
        }

        [ClientRpc]
        protected override void RPCAllPlayersReady(){
            base.RPCAllPlayersReady();
        }

        [ClientRpc(includeOwner = false)]
        protected override void RPCPlayerLeave(NetworkIdentity identity){
            base.RPCPlayerLeave(identity);
        }

        [ClientRpc]
        private void RPCBeginGame() {
            SingletonUIHandler.GetInstance<SETUIEventHandler>()?.OnCommonOrLocalStateEvent?.Invoke(UIStates.StartGame);
            SETGameUIManager.Instance.timer.StartCountdown(roomInfo.guessTime, true,  () => { SingletonUIHandler.GetInstance<SETUIEventHandler>().OnCommonOrLocalStateEvent?.Invoke(UIStates.CardDestribute); });
        }

        [ClientRpc]
        private void RPCPlaceCards(byte[] cards, byte[] cardPoses){
            SETGameUIManager.Instance.PlaceCards(cards, cardPoses);
            SingletonUIHandler.GetInstance<SETUIEventHandler>()?.OnCommonOrLocalStateEvent?.Invoke(UIStates.CardDestribute);
        }

        [ClientRpc]
        void RPCCheat(string cheat) {
            Debug.LogError("Cheat Detected!- " + cheat);
        }

        [ClientRpc]
        void RPCPopTimeout(NetworkIdentity guessPlayer){
            SETGameUIManager.Instance.PopTimeout(guessPlayer.hasAuthority ? "You did not guess in time!\nYou lost one point!" : $"{guessPlayer.GetComponent<SETNetworkPlayer>().playerName} did not guess in time!\nHe lost one point!");
            if (guessPlayer.hasAuthority)
                SingletonUIHandler.GetInstance<SETUIEventHandler>()?.OnCommonOrLocalStateEvent?.Invoke(UIStates.GuessTimeout);
            else
                SingletonUIHandler.GetInstance<SETUIEventHandler>()?.OnOtherStateEvent?.Invoke(UIStates.GuessTimeout, guessPlayer.GetComponent<SETNetworkPlayer>().playerName);
        }

        [ClientRpc]
        void RPCPopResult(GuessSETMessage msg, byte result, NetworkIdentity guessPlayer){
            SETGameUIManager.Instance.PopResult(result, msg, guessPlayer);
        }

        [ClientRpc]
        private void RPCBeginVote(NetworkIdentity identity){
            if (identity.hasAuthority)
                SingletonUIHandler.GetInstance<SETUIEventHandler>()?.OnCommonOrLocalStateEvent?.Invoke(UIStates.BeginVote);
            else
                SingletonUIHandler.GetInstance<SETUIEventHandler>()?.OnOtherStateEvent?.Invoke(UIStates.BeginVote, identity.GetComponent<SETNetworkPlayer>().playerName);  
        }

        [ClientRpc]
        void RPCPlayerVoted(NetworkIdentity identity){
            if (identity.hasAuthority)
                SingletonUIHandler.GetInstance<SETUIEventHandler>()?.OnCommonOrLocalStateEvent?.Invoke(UIStates.PlaceVote);
        }
        #endregion

        #region MessageHandlers
        internal void OnAttemptGuess(NetworkConnectionToClient conn, AttempSETGuess msg){
            if (guessingPlayer == null && state == SETGameState.Normal && guessProcess == null){
                guessingPlayer = conn.identity;
                conn.identity.GetComponent<SETNetworkPlayer>().isGuessing = true;
                state = SETGameState.Guess;
                guessProcess = StartCoroutine(ProcessGuess());
            }
        }

        internal void OnSETGuess(NetworkConnectionToClient conn, GuessSETMessage msg){
            if (state == SETGameState.Guess && guessingPlayer != null && conn.identity == guessingPlayer && ValidateGuess(msg.card1, msg.card2, msg.card3)){
                byte r = CardData.CheckSET(msg.card1, msg.card2, msg.card3);
                state = SETGameState.Process;
                StopCoroutine(guessProcess);
                StartCoroutine(ResultProcess(r, msg));
            }
        }

        internal void OnRequestDestribute(NetworkConnectionToClient conn, DestributeRequest msg){
            if(state == SETGameState.Normal && cursor < 81){
                state = SETGameState.Request;
                conn.identity.GetComponent<SETNetworkPlayer>().voteState = VoteStat.YES;
                voteYesCount++;
                RPCPlayerVoted(conn.identity);
                RPCBeginVote(conn.identity);
            }
        }

        internal void OnPlayerVote(NetworkConnectionToClient conn, VoteMessage msg){
            if(state == SETGameState.Request && conn.identity.GetComponent<SETNetworkPlayer>().voteState == VoteStat.NULL){
                if (msg.isYes){
                    conn.identity.GetComponent<SETNetworkPlayer>().voteState = VoteStat.YES;
                    voteYesCount++;
                }
                else{
                    conn.identity.GetComponent<SETNetworkPlayer>().voteState = VoteStat.NO;
                    voteNoCount++;
                }
                RPCPlayerVoted(conn.identity);
                if (voteNoCount + voteYesCount == playerCount){
                    StartCoroutine(ProcessVote(voteYesCount >= voteNoCount));
                }
            }
        }
        #endregion

        #region Unity Callbacks
        protected override void Awake(){
            base.Awake();
            //BoardGameNetworkManager.singleton.session = this;
        }
        #endregion

        #region Start & Stop Callbacks

        /// <summary>
        /// This is invoked for NetworkBehaviour objects when they become active on the server.
        /// <para>This could be triggered by NetworkServer.Listen() for objects in the scene, or by NetworkServer.Spawn() for objects that are dynamically created.</para>
        /// <para>This will be called for objects on a "host" as well as for object on a dedicated server.</para>
        /// </summary>
        public override void OnStartServer() {
            base.OnStartServer();
            state = SETGameState.Destribute;
            deck = BoardGameNetworkManager.singleton.GetShuffleDeck();
            cursor = 0;
            roomInfo = new RoomData { guessTime = 5, roundCount = 1 };
            for (int i = 0; i < 18; i++)
                placedCards.Insert(i, 0);
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
        public override void OnStartClient() {
            base.OnStartClient();
        }

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

        protected override void BeginGame()
        {
            StartCoroutine(DelaySendBegin());
        }
        #endregion
    } 
}
