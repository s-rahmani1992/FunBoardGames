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
        Start,
        Normal,
        Destribute,
        Guess,
        ProcessGuess,
        CardVote
    }

    [Serializable]
    public class RoomData
    {
        public float guessTime;
        public int roundCount;
    }

    public class SETRoomNetworkManager : RoomManager
    {
        public bool CanSelect { get => (state == SETGameState.Guess) && (guessingPlayer != null) && (guessingPlayer.hasAuthority); }
        public event Action<SETGameState, SETGameState> StateChanged;
        public event Action<CardData[], int[]> NewCardsReceived;
        public event Action<SETNetworkPlayer> GuessBegin;
        public event Action<SETNetworkPlayer, CardData[], byte> PlayerGuessReceived;
        public event Action<SETNetworkPlayer> PlayerStartedVote;

        #region Server Only
        byte[] deck;
        List<CardData> placedCards = new(18);
        int placedCardCount;
        int cursor = 0;
        int voteYesCount, voteNoCount = 0;
        Coroutine guessProcess;
        List<CardData> hintCards = new(3);

        [Server]
        void UpdateHintCards()
        {
            hintCards.Clear();
            List<CardData> cards = new List<CardData>();

            foreach(var card in placedCards)
            {
                if (card != null)
                    cards.Add(card);
            }

            for (int i = 0; i < cards.Count; i++)
            {
                for(int j = i + 1; j < cards.Count; j++)
                {
                    CardData thirdCard = new CardData((byte)((2 * (cards[i].Color + cards[j].Color)) % 3)
                                                    , (byte)((2 * (cards[i].Shape + cards[j].Shape)) % 3)
                                                    , (byte)((2 * (cards[i].CountIndex + cards[j].CountIndex)) % 3)
                                                    , (byte)((2 * (cards[i].Shading + cards[j].Shading)) % 3));

                    for (int k = j + 1; k < cards.Count; k++)
                    {
                        if (cards[k].Equals(thirdCard))
                        {
                            hintCards.Add(cards[i]);
                            hintCards.Add(cards[j]);
                            hintCards.Add(cards[k]);
                            return;
                        }
                    }
                }
            }
        }

        [Server]
        bool ValidateGuess(CardData c1, CardData c2, CardData c3)
        {
            if (MyUtils.CompareItems(c1, c2, c3) != TripleComparisonResult.ALL_DIFFERRENT) return false;
            if (!placedCards.Contains(c1) || !placedCards.Contains(c2) || !placedCards.Contains(c3)) return false;
            return true;
        }

        [Server]
        public IEnumerator DelaySendBegin()
        {
            yield return new WaitForSeconds(0.5f);
            state = SETGameState.Start;
            yield return new WaitForSeconds(4);
            byte[] temp = new byte[12];

            placedCardCount = 12;
            for (int i = 0; i < 12; i++){
                temp[i] = (byte)i;
                placedCards[i] = BoardGameCardDataHolder.Instance.GetCard(deck[i]);
            }

            UpdateHintCards();
            state = SETGameState.Destribute;
            RPCPlaceCards(deck.ToList().GetRange(cursor, 12).Select(c => BoardGameCardDataHolder.Instance.GetCard(c)).ToArray(), temp);
            cursor = 12;
            yield return new WaitForSeconds(2.5f);
            state = SETGameState.Normal;
        }

        [Server]
        IEnumerator ProcessGuess()
        {
            yield return new WaitForSeconds(roomInfo.guessTime);
            state = SETGameState.ProcessGuess;
            RPCPopResult(guessingPlayer, null, 0);
            yield return new WaitForSeconds(2);
            guessingPlayer.wrongs++;
            guessingPlayer.isGuessing = false;
            state = SETGameState.Normal;
            guessProcess = null;
            guessingPlayer = null;
        }

        [Server]
        IEnumerator ResultProcess(byte result, CardData[] cards)
        {
            RPCPopResult(guessingPlayer, cards, result);
            yield return new WaitForSeconds(6.8f);
            guessingPlayer.GetComponent<SETNetworkPlayer>().isGuessing = false;
            
            if (CardData.IsSET(result))
            {
                guessingPlayer.GetComponent<SETNetworkPlayer>().corrects++;
                
                if (cursor < 81 && placedCardCount == 12)
                {
                    placedCards[placedCards.IndexOf(cards[0])] = null;
                    placedCards[placedCards.IndexOf(cards[1])] = null;
                    placedCards[placedCards.IndexOf(cards[2])] = null;
                    byte[] places2Add = new byte[3];
                    int place = 0;

                    for (int i = 0; i < placedCards.Count; i++)
                    {
                        if (placedCards[i] == null)
                        {
                            places2Add[place] = (byte)i;
                            placedCards[i] = BoardGameCardDataHolder.Instance.GetCard(deck[cursor + place]);
                            place++;

                            if (place == 3)
                                break;
                        }
                    }

                    UpdateHintCards();
                    state = SETGameState.Destribute;
                    RPCPlaceCards(deck.ToList().GetRange(cursor, 3).Select(c => BoardGameCardDataHolder.Instance.GetCard(c)).ToArray(), places2Add);
                    cursor += 3;
                    yield return new WaitForSeconds(0.7f);
                }
                else
                {
                    int temp = placedCards.IndexOf(cards[0]);
                    placedCards[temp] = null;
                    temp = placedCards.IndexOf(cards[1]);
                    placedCards[temp] = null;
                    temp = placedCards.IndexOf(cards[2]);
                    placedCards[temp] = null;
                    placedCardCount -= 3;
                    UpdateHintCards();
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
            voteStarter = null;
            yield return new WaitForSeconds(1);
            if (v){
                state = SETGameState.Destribute;
                if (cursor < 81){
                    byte[] placed2Add = new byte[3];
                    int place = 0;
                    for (int i = 0; i < placedCards.Count; i++){
                        if (placedCards[i] == null){
                            placed2Add[place] = (byte)i;
                            placedCards[i] = BoardGameCardDataHolder.Instance.GetCard(deck[cursor + place]);
                            place++;
                            if (place == 3)
                                break;
                        }
                    }
                    placedCardCount += 3;
                    state = SETGameState.Destribute;
                    RPCPlaceCards(deck.ToList().GetRange(cursor, 3).Select(c => BoardGameCardDataHolder.Instance.GetCard(c)).ToArray(), placed2Add);
                    cursor += 3;
                    foreach (var p in Players)
                        (p as SETNetworkPlayer).voteState = VoteStat.NULL;
                    
                    yield return new WaitForSeconds(0.7f);
                }
                else
                    yield return new WaitForSeconds(0.2f);

                foreach (var p in Players)
                    (p as SETNetworkPlayer).voteState = VoteStat.NULL;
                state = SETGameState.Normal;
            }
            else{
                state = SETGameState.Normal;
                foreach (var p in Players)
                    (p as SETNetworkPlayer).voteState = VoteStat.NULL;
            }
        }
        #endregion

        #region Syncvars
        [SyncVar(hook = nameof(GameStateChanged))]
        public SETGameState state = SETGameState.Normal;

        [SyncVar]
        public RoomData roomInfo;

        [SyncVar(hook = nameof(PlayerStartGuess))]
        public SETNetworkPlayer guessingPlayer;

        [SyncVar(hook = nameof(OnPlayerStartVote))]
        public SETNetworkPlayer voteStarter;
        #endregion

        #region Syncvar Hooks
        void GameStateChanged(SETGameState oldVal, SETGameState newVal)
        {
            StateChanged?.Invoke(oldVal, newVal);
        }

        void PlayerStartGuess(SETNetworkPlayer _, SETNetworkPlayer newPlayer)
        {
            if(newPlayer != null)
                GuessBegin?.Invoke(newPlayer);
        }

        void OnPlayerStartVote(SETNetworkPlayer _, SETNetworkPlayer newPlayer)
        {
            if (newPlayer != null)
                PlayerStartedVote?.Invoke(newPlayer);
        }
        #endregion

        #region Client RPCs
        [ClientRpc]
        private void RPCPlaceCards(CardData[] cards, byte[] cardPoses)
        {
            NewCardsReceived?.Invoke(cards, cardPoses.Select(x => (int)x).ToArray());
        }

        [ClientRpc]
        void RPCCheat(string cheat) {
            Debug.LogError("Cheat Detected!- " + cheat);
        }

        [ClientRpc]
        void RPCPopResult(SETNetworkPlayer guessPlayer, CardData[] cards, byte result)
        {
            PlayerGuessReceived?.Invoke(guessPlayer, cards, result);
        }

        #endregion

        #region Commands
        [Command(requiresAuthority = false)]
        internal void CmdAttemptGuess(SETNetworkPlayer identity)
        {
            if (guessingPlayer == null && state == SETGameState.Normal && guessProcess == null)
            {
                guessingPlayer = identity;
                identity.isGuessing = true;
                state = SETGameState.Guess;
                guessProcess = StartCoroutine(ProcessGuess());
            }
        }

        [Command(requiresAuthority = false)]
        internal void CmdRequestMoreCards(SETNetworkPlayer player)
        {
            if (state == SETGameState.Normal && cursor < 81)
            {
                voteStarter = player;
                state = SETGameState.CardVote;
                player.voteState = VoteStat.YES;
                voteYesCount++;
            }
        }

        [Command(requiresAuthority = false)]
        internal void CmdHintRequest(NetworkIdentity identity)
        {
            TargetGetHint(identity.connectionToClient, hintCards.ToArray());
        }

        [Command(requiresAuthority = false)]
        internal void CmdGuessCards(SETNetworkPlayer player, CardData[] cards)
        {
            if (state == SETGameState.Guess && guessingPlayer == player && ValidateGuess(cards[0], cards[1], cards[2]))
            {
                byte r = CardData.CheckSET(cards[0], cards[1], cards[2]);
                state = SETGameState.ProcessGuess;
                StopCoroutine(guessProcess);
                StartCoroutine(ResultProcess(r, cards));
            }
        }

        [Command(requiresAuthority = false)]
        internal void CmdSendVote(SETNetworkPlayer player, bool isYes)
        {
            if (state == SETGameState.CardVote && player.voteState == VoteStat.NULL)
            {
                if (isYes)
                {
                    player.voteState = VoteStat.YES;
                    voteYesCount++;
                }
                else
                {
                    player.voteState = VoteStat.NO;
                    voteNoCount++;
                }

                if (voteNoCount + voteYesCount == PlayerCount)
                    StartCoroutine(ProcessVote(voteYesCount >= voteNoCount));
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
            deck = MyUtils.GetRandomByteList(81);
            cursor = 0;
            roomInfo = new RoomData { guessTime = 5, roundCount = 1 };
            for (int i = 0; i < 18; i++)
                placedCards.Insert(i, null);
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
        public override void OnStartClient() 
        {
            base.OnStartClient();
        }

        [TargetRpc]
        private void TargetGetHint(NetworkConnection conn, CardData[] cards)
        {
            SETGameUIManager.Instance.MarkHints(cards);
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
