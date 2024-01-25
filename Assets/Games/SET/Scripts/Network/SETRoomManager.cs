using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using FishNet.Object.Synchronizing;
using FishNet.Object;
using FishNet.Connection;

namespace FunBoardGames.SET 
{
    public class SETRoomManager : RoomManager
    {
        public event Action<SETGameState, SETGameState> StateChanged;
        public event Action<CardData[], int[]> NewCardsReceived;
        public event Action<SETPlayer> GuessBegin;
        public event Action<SETPlayer, CardData[], byte> PlayerGuessReceived;
        public event Action<SETPlayer> PlayerStartedVote;

        public bool CanSelect { get => (State == SETGameState.Guess) && (GuessingPlayer != null) && (GuessingPlayer.IsOwner); }
        public const int endCursor = 81;

        byte[] deck;
        List<CardData> placedCards = new(18);
        int placedCardCount;
        int cursor = 0;
        int voteYesCount, voteNoCount = 0;
        Coroutine guessProcess;
        List<CardData> hintCards = new(3);
        
        #region Syncvars

        [field: SyncVar(OnChange = nameof(GameStateChanged))]
        public SETGameState State { get; private set; } = SETGameState.Normal;

        [field: SyncVar]
        public SETRoomMetaData MetaData { get; private set; }

        [field: SyncVar(OnChange = nameof(PlayerStartGuess))]
        public SETPlayer GuessingPlayer { get; private set; }

        [field: SyncVar(OnChange = nameof(OnPlayerStartVote))]
        public SETPlayer VoteStarter { get; private set; }

        void GameStateChanged(SETGameState oldVal, SETGameState newVal, bool _)
        {
            StateChanged?.Invoke(oldVal, newVal);
        }

        void PlayerStartGuess(SETPlayer _, SETPlayer newPlayer, bool __)
        {
            if (newPlayer != null)
                GuessBegin?.Invoke(newPlayer);
        }

        void OnPlayerStartVote(SETPlayer _, SETPlayer newPlayer, bool ___)
        {
            if (newPlayer != null)
                PlayerStartedVote?.Invoke(newPlayer);
        }

        #endregion

        #region Server Part

        /// <summary>
        /// This is invoked for NetworkBehaviour objects when they become active on the server.
        /// <para>This could be triggered by NetworkServer.Listen() for objects in the scene, or by NetworkServer.Spawn() for objects that are dynamically created.</para>
        /// <para>This will be called for objects on a "host" as well as for object on a dedicated server.</para>
        /// </summary>
        public override void OnStartServer()
        {
            base.OnStartServer();
            State = SETGameState.Destribute;
            deck = MyUtils.GetRandomByteList(81);
            cursor = 0;
            MetaData = new SETRoomMetaData(7, 1);

            for (int i = 0; i < 18; i++)
                placedCards.Insert(i, null);
        }

        [ServerRpc(RequireOwnership = false)]
        internal void CmdAttemptGuess(SETPlayer player)
        {
            if (GuessingPlayer == null && State == SETGameState.Normal && guessProcess == null)
            {
                GuessingPlayer = player;
                player.SetGuess(true);
                State = SETGameState.Guess;
                guessProcess = StartCoroutine(ProcessGuess());
            }
        }

        [ServerRpc(RequireOwnership = false)]
        internal void CmdRequestMoreCards(SETPlayer player)
        {
            if (State == SETGameState.Normal && cursor < 81)
            {
                VoteStarter = player;
                State = SETGameState.CardVote;
                player.SetVote(VoteAnswer.YES);
                voteYesCount++;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        internal void CmdHintRequest(NetworkConnection conn = null)
        {
            TargetGetHint(conn, hintCards.ToArray());
        }

        [ServerRpc(RequireOwnership = false)]
        internal void CmdGuessCards(SETPlayer player, CardData[] cards)
        {
            if (State == SETGameState.Guess && GuessingPlayer == player && ValidateGuess(cards[0], cards[1], cards[2]))
            {
                byte r = CardData.CheckSET(cards[0], cards[1], cards[2]);
                State = SETGameState.ProcessGuess;
                StopCoroutine(guessProcess);
                StartCoroutine(ResultProcess(r, cards));
            }
        }

        [ServerRpc(RequireOwnership = false)]
        internal void CmdSendVote(SETPlayer player, bool isYes)
        {
            if (State == SETGameState.CardVote && player.VoteAnswer == VoteAnswer.None)
            {
                if (isYes)
                {
                    player.SetVote(VoteAnswer.YES);
                    voteYesCount++;
                }
                else
                {
                    player.SetVote(VoteAnswer.NO);
                    voteNoCount++;
                }

                if (voteNoCount + voteYesCount == PlayerCount)
                    StartCoroutine(ProcessVote(voteYesCount >= voteNoCount));
            }
        }

        [Server]
        void UpdateHintCards()
        {
            hintCards.Clear();
            List<CardData> cards = new();

            foreach (var card in placedCards)
            {
                if (card != null)
                    cards.Add(card);
            }

            for (int i = 0; i < cards.Count; i++)
            {
                for (int j = i + 1; j < cards.Count; j++)
                {
                    CardData thirdCard = new((byte)((2 * (cards[i].Color + cards[j].Color)) % 3)
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
            State = SETGameState.Start;
            yield return new WaitForSeconds(4);
            byte[] temp = new byte[12];
            placedCardCount = 12;

            for (int i = 0; i < 12; i++)
            {
                temp[i] = (byte)i;
                placedCards[i] = SetGameDataManager.GetCard(deck[i]);
            }

            UpdateHintCards();
            State = SETGameState.Destribute;
            RPCPlaceCards(deck.ToList().GetRange(cursor, 12).Select(c => SetGameDataManager.GetCard(c)).ToArray(), temp);
            cursor = 12;
            yield return new WaitForSeconds(2.5f);
            State = SETGameState.Normal;
        }

        [Server]
        IEnumerator ProcessGuess()
        {
            yield return new WaitForSeconds(MetaData.GuessTime);
            State = SETGameState.ProcessGuess;
            RPCPopResult(GuessingPlayer, null, 0);
            yield return new WaitForSeconds(2);
            GuessingPlayer.IncrementWrong();
            GuessingPlayer.SetGuess(false);
            State = SETGameState.Normal;
            guessProcess = null;
            GuessingPlayer = null;
        }

        [Server]
        IEnumerator ResultProcess(byte result, CardData[] cards)
        {
            RPCPopResult(GuessingPlayer, cards, result);
            yield return new WaitForSeconds(6.8f);
            GuessingPlayer.SetGuess(false);

            if (CardData.IsSET(result))
            {
                GuessingPlayer.GetComponent<SETPlayer>().IncrementCorrect();

                if (cursor < endCursor && placedCardCount == 12)
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
                            placedCards[i] = SetGameDataManager.GetCard(deck[cursor + place]);
                            place++;

                            if (place == 3)
                                break;
                        }
                    }

                    UpdateHintCards();
                    State = SETGameState.Destribute;
                    RPCPlaceCards(deck.ToList().GetRange(cursor, 3).Select(c => SetGameDataManager.GetCard(c)).ToArray(), places2Add);
                    cursor += 3;
                    yield return new WaitForSeconds(0.7f);

                    if(cursor >= endCursor && hintCards.Count == 0)
                    {
                        State = SETGameState.Finish;
                        yield break;
                    }
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

                    if (cursor >= endCursor && hintCards.Count == 0)
                    {
                        State = SETGameState.Finish;
                        yield break;
                    }
                }
            }
            else
                GuessingPlayer.GetComponent<SETPlayer>().IncrementWrong();

            GuessingPlayer = null;
            State = SETGameState.Normal;
            guessProcess = null;
        }

        [Server]
        IEnumerator ProcessVote(bool v)
        {
            voteNoCount = voteYesCount = 0;
            VoteStarter = null;
            yield return new WaitForSeconds(1);

            if (v)
            {
                State = SETGameState.Destribute;
                if (cursor < endCursor)
                {
                    byte[] placed2Add = new byte[3];
                    int place = 0;
                    for (int i = 0; i < placedCards.Count; i++)
                    {
                        if (placedCards[i] == null)
                        {
                            placed2Add[place] = (byte)i;
                            placedCards[i] = SetGameDataManager.GetCard(deck[cursor + place]);
                            place++;

                            if (place == 3)
                                break;
                        }
                    }

                    UpdateHintCards();
                    placedCardCount += 3;
                    State = SETGameState.Destribute;
                    RPCPlaceCards(deck.ToList().GetRange(cursor, 3).Select(c => SetGameDataManager.GetCard(c)).ToArray(), placed2Add);
                    cursor += 3;
                    foreach (var p in Players)
                        (p as SETPlayer).SetVote(VoteAnswer.None);

                    yield return new WaitForSeconds(0.7f);

                    if (cursor >= endCursor && hintCards.Count == 0)
                    {
                        State = SETGameState.Finish;
                        yield break;
                    }
                }
                else
                    yield return new WaitForSeconds(0.2f);

                foreach (var p in Players)
                    (p as SETPlayer).SetVote(VoteAnswer.None);
                State = SETGameState.Normal;
            }
            else
            {
                State = SETGameState.Normal;

                foreach (var p in Players)
                    (p as SETPlayer).SetVote(VoteAnswer.None);
            }
        }

        protected override void BeginGame()
        {
            StartCoroutine(DelaySendBegin());
        }

        #endregion

        #region Client Part
        [ObserversRpc]
        private void RPCPlaceCards(CardData[] cards, byte[] cardPoses)
        {
            NewCardsReceived?.Invoke(cards, cardPoses.Select(x => (int)x).ToArray());
        }

        [ObserversRpc]
        void RPCCheat(string cheat) 
        {
            Debug.LogError("Cheat Detected!- " + cheat);
        }

        [ObserversRpc]
        void RPCPopResult(SETPlayer guessPlayer, CardData[] cards, byte result)
        {
            PlayerGuessReceived?.Invoke(guessPlayer, cards, result);
        }

        [TargetRpc]
        private void TargetGetHint(NetworkConnection conn, CardData[] cards)
        {
            SETGameUIManager.Instance.MarkHints(cards);
        }

        #endregion
    } 
}
