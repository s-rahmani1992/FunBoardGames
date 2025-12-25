using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FunBoardGames.Network;

namespace FunBoardGames.SET
{
    public class SETGameUIManager : MonoBehaviour
    {
        [SerializeField] CardDeckManager cardDeckManager;
        [SerializeField] ObjectPoolManager pool;
        [SerializeField] Button guessBtn, cardBtn, hintBtn;
        [SerializeField] Transform cardHolder;
        [SerializeField] Text remainTxt;
        [SerializeField] GraphicRaycaster cardRaycaster;
        [SerializeField] GameLogger gameLogger;
        [SerializeField] UserGameHolder userGameHolder;
        [SerializeField] PlayerUI[] playerUIs;

        public static SETGameUIManager Instance { get; private set; }

        public Transform playerPanel;
        public Timer timer;
        public Color[] colors;
        public Sprite[] cardShapes;

        int cardCount = SETRoomManager.endCursor;
        SETRoomManager sessionManager;
        List<ISETPlayer> players = new();
        SETPlayer localPlayer;
        List<CardUI> selected = new(3);
        List<CardUI> hints = new(3);
        List<CardUI> placedCardUIs = new(18);

        ISETGameHandler setGameHandler;
        ISETPlayer selfPlayer;

        private void Awake()
        {
            Instance = this;
            setGameHandler = userGameHolder.GetGameHandler<ISETGameHandler>();
            cardDeckManager.RegisterGameHandler(setGameHandler);
            setGameHandler.SignalGameLoaded();
            Subscribe();
            UpdatePlayerUIs();
            BeginGameCountDown();
        }

        void BeginGameCountDown()
        {
            timer.StartCountdown(4);
            gameLogger.SetText("Wait For The Game To Start.");
        }

        public bool UpdateSelected(CardUI card)
        {
            if (!sessionManager.CanSelect || selected.Contains(card)) return false;

            if (selected.Count < 3)
            {
                selected.Add(card);

                if (selected.Count == 3 && sessionManager.State == SETGameState.Guess)
                    sessionManager.CmdGuessCards(localPlayer, selected.Select((cardUI) => cardUI.info).ToArray());

                return true;
            }
            else
                return false;
        }

        void Subscribe()
        {
            setGameHandler.NewCardsReceived += PlaceCards;
            //sessionManager.StateChanged += OnStateChanged;
            //sessionManager.GuessBegin += OnPlayerStartedGuess;
            //sessionManager.PlayerGuessReceived += OnPlayerGuessReceived;
            //sessionManager.PlayerStartedVote += OnPlayerStartedVote;
        }

        private void UpdatePlayerUIs()
        {
            players = new(setGameHandler.Players);

            int index = 0;

            foreach (var player in players)
            {
                playerUIs[index].SetPlayer(player);
                player.LeftGame += () => players.Remove(player);

                if (player.IsMe)
                    selfPlayer = player;

                index++;
            }

            //localPlayer.VoteChanged += OnLocalPlayerVoteChanged;
        }

        private void OnPlayerStartedVote(SETPlayer player)
        {
            gameLogger.SetText(player.IsOwner ? "Wait For Others to vote." : $"{player.Name} Started Vote destribute. place your vote.");
        }

        private void OnLocalPlayerVoteChanged(VoteAnswer _, VoteAnswer vote)
        {
            if(vote != VoteAnswer.None)
                gameLogger.SetText("Wait For Others to vote.");
        }

        private void OnPlayerGuessReceived(SETPlayer player, CardData[] cards, byte result)
        {
            if(cards == null)
            {
                for (int i = 0; i < selected.Count; i++)
                    selected[i].Mark(false);
                selected.Clear();
                gameLogger.SetText(player.IsOwner ? "You didn't guess in time. You lost 1 point" : $"{player.Name} didn't guess in time. {player.Name} lost 1 point");
                return;
            }

            PopResult(result, cards, player);
        }

        private void OnPlayerStartedGuess(SETPlayer player)
        {
            timer.StartCountdown(sessionManager.MetaData.GuessTime);
            gameLogger.SetText(player.IsOwner ? "Your are guessing. Guess quickly" : $"{player.Name} is guessing");
        }

        void UnSubscribe()
        {
            //sessionManager.StateChanged -= OnStateChanged;
            setGameHandler.NewCardsReceived -= PlaceCards;
            //sessionManager.GuessBegin -= OnPlayerStartedGuess;
            //sessionManager.PlayerGuessReceived -= OnPlayerGuessReceived;
            //sessionManager.PlayerStartedVote -= OnPlayerStartedVote;
            //localPlayer.VoteChanged -= OnLocalPlayerVoteChanged;
        }

        private void OnStateChanged(SETGameState _, SETGameState state)
        {
            RefreshBtns(state);

            if (state == SETGameState.Normal)
                gameLogger.SetText("");
            
            if(state == SETGameState.CardVote)
            {
                //DialogManager.Instance.SpawnDialog<SETVoteDialog>(DialogShowOptions.OverAll, (dialog) => {
                //    (dialog as SETVoteDialog).Init(sessionManager, players);
                //});
            }

            if(state == SETGameState.Start)
            {
                timer.StartCountdown(4);
                gameLogger.SetText("Wait For The Game To Start.");
            }

            if(state == SETGameState.Finish)
            {
                gameLogger.SetText("Game Finished!");
                DialogManager.Instance.SpawnDialog<SETResultDialog>(DialogShowOptions.OverAll, (dialog) =>
                {
                    (dialog as SETResultDialog).Initialize(sessionManager);
                });
            }
        }

        private void OnDestroy()
        {
            UnSubscribe();
            Instance = null;
        }

        public void UpdateCardMeter()
        {
            cardCount--;
            remainTxt.text = cardCount.ToString();
        }

        void PlaceCards(IEnumerable<CardData> cardInfos)
        {
            gameLogger.SetText("Destributing Cards");
            RefreshBtns(SETGameState.Destribute);
            cardDeckManager.CardDestributionEnded += OnCardDestributionEnded;
            //for (int i = 0; i < cardInfos.Length; i++)
            //    placedCardUIs.Add(pool.PullFromList(0, cardInfos[i], cardPoses[i], 0.2f * i, cardHolder).GetComponent<CardUI>());
        }

        private void OnCardDestributionEnded()
        {
            cardDeckManager.CardDestributionEnded -= OnCardDestributionEnded;
            gameLogger.SetText("");
            RefreshBtns(SETGameState.Normal);
        }

        void RefreshBtns(SETGameState state)
        {
            guessBtn.interactable = hintBtn.interactable = (state == SETGameState.Normal);
            cardBtn.interactable = (cardCount > 0 && state == SETGameState.Normal);
            for (int i = 0; i < hints.Count; i++)
                hints[i].Mark(false);
            hints.Clear();
            cardRaycaster.enabled = (state == SETGameState.Guess);
        }

        public void AttemptGuess()
        {
            sessionManager.CmdAttemptGuess(localPlayer);
        }

        public void PopResult(byte result, CardData[] cards, SETPlayer player)
        {
            gameLogger.SetText("");
            bool isCorrect = CardData.IsSET(result);
            string p = null;
            if (!player.IsOwner)
            {
                p = player.Name;
                selected.Clear();
                foreach (var c in placedCardUIs)
                {
                    if (c.info.Equals(cards[0]) || c.info.Equals(cards[1]) || c.info.Equals(cards[2]))
                    {
                        c.Mark(true);
                        selected.Add(c);
                    }
                    else
                        c.Mark(false);
                }
            }

            GuessResultDialog.Show(selected.ToArray(), result);
            timer.Stop();
            StartCoroutine(DisplayResult(isCorrect, player));
        }

        IEnumerator DisplayResult(bool isSet, SETPlayer player)
        {
            yield return new WaitForSeconds(0.5f);

            if (isSet)
                gameLogger.SetText(player.IsOwner ? "You Guessed Right! You Got 1 point." : $"{player.Name} Guessed Right! He Got 1 point.");
            else
                gameLogger.SetText(player.IsOwner ? "Your Guess was Wrong! You lost 1 point." : $"{player.Name}'s Guess was Wrong! He lost 1 point.");

            yield return new WaitForSeconds(6);
            DialogManager.Instance.CloseDialog<GuessResultDialog>();
        }

        public void SendCardRequest()
        {
            sessionManager.CmdRequestMoreCards(localPlayer);
        }

        public void SendHint()
        {
            sessionManager.CmdHintRequest();
        }

        public void RemoveSelected()
        {
            foreach (var s in selected)
            {
                pool.Push2List(s.gameObject);
                placedCardUIs.Remove(s);
            }
            selected.Clear();
        }

        public void ClearSelected()
        {
            selected.Clear();
        }

        public void MarkHints(CardData[] cards)
        {
            hints.Clear();
            if(cards.Length == 0) return;
            foreach (var c in placedCardUIs)
            {
                if (c.info.Equals(cards[0]) || c.info.Equals(cards[1]) || c.info.Equals(cards[2]))
                {
                    c.MarkHint();
                    hints.Add(c);
                }
            }
        }
    }
}
