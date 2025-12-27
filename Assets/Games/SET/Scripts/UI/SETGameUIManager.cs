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
        [SerializeField] GameLogger gameLogger;
        [SerializeField] UserGameHolder userGameHolder;
        [SerializeField] PlayerUI[] playerUIs;

        Dictionary<ISETPlayer, PlayerUI> playerUIMap = new();

        public static SETGameUIManager Instance { get; private set; }

        public Transform playerPanel;
        public Timer timer;
        public Color[] colors;
        public Sprite[] cardShapes;

        int cardCount = SETRoomManager.endCursor;
        SETRoomManager sessionManager;
        List<ISETPlayer> players = new();
        SETPlayer localPlayer;
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

        void Subscribe()
        {
            setGameHandler.NewCardsReceived += PlaceCards;
            setGameHandler.PlayerStartedGuess += OnPlayerStartedGuess;
            setGameHandler.PlayerGuessTimeout += OnPlayerGuessTimeout;
            setGameHandler.PlayerGuessReceived += OnPlayerGuessReceived;
            //sessionManager.StateChanged += OnStateChanged;
            //sessionManager.PlayerStartedVote += OnPlayerStartedVote;
        }

        private void OnPlayerGuessReceived(ISETPlayer player, IEnumerable<CardData> enumerable, bool isCorrect)
        {
            gameLogger.SetText("");
            timer.Stop();
            playerUIMap[player].UpdateScores(); 
            playerUIMap[player].ToggleGuess(false);
            StartCoroutine(DisplayResult(isCorrect, player));
            cardDeckManager.CardDestributionEnded += OnCardDestributionEnded;
        }

        private void OnPlayerGuessTimeout(ISETPlayer player)
        {
            gameLogger.Toast(player.IsMe ? "You didn't guess in time. You lost 1 point" : $"{player.Name} didn't guess in time. {player.Name} lost 1 point");
            playerUIMap[player].ToggleGuess(false);
            playerUIMap[player].UpdateScores();
            RefreshBtns(SETGameState.Normal);
        }

        private void UpdatePlayerUIs()
        {
            players = new(setGameHandler.Players);

            int index = 0;

            foreach (var player in players)
            {
                playerUIs[index].SetPlayer(player);
                player.LeftGame += () => players.Remove(player);
                playerUIMap.Add(player, playerUIs[index]);

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

        private void OnPlayerStartedGuess(ISETPlayer player)
        {
            RefreshBtns(SETGameState.Guess);
            playerUIMap[player].ToggleGuess(true);
            timer.StartCountdown(7); //TODO sync with game data in future
            gameLogger.SetText(player.IsMe ? "Your are guessing. Guess quickly" : $"{player.Name} is guessing");
        }

        void UnSubscribe()
        {
            setGameHandler.NewCardsReceived -= PlaceCards;
            setGameHandler.PlayerStartedGuess -= OnPlayerStartedGuess;
            setGameHandler.PlayerGuessTimeout -= OnPlayerGuessTimeout;
            setGameHandler.PlayerGuessReceived -= OnPlayerGuessReceived;
            //sessionManager.StateChanged -= OnStateChanged;
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

        void PlaceCards(IEnumerable<CardData> cardInfos)
        {
            gameLogger.SetText("Destributing Cards");
            RefreshBtns(SETGameState.Destribute);
            cardDeckManager.CardDestributionEnded += OnCardDestributionEnded;
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
        }

        public void AttemptGuess()
        {
            setGameHandler.StartGuess();
        }

        IEnumerator DisplayResult(bool isSet, ISETPlayer player)
        {
            yield return new WaitForSeconds(0.5f);

            if (isSet)
                gameLogger.SetText(player.IsMe ? "You Guessed Right! You Got 1 point." : $"{player.Name} Guessed Right! He Got 1 point.");
            else
                gameLogger.SetText(player.IsMe ? "Your Guess was Wrong! You lost 1 point." : $"{player.Name}'s Guess was Wrong! He lost 1 point.");

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
