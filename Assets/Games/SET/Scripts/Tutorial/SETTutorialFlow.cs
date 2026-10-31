using FunBoardGames.Network;
using FunBoardGames.Tutorial;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FunBoardGames.SET
{
    public class SETTutorialFlow : MonoBehaviour
    {
        const string MenuScene = "Menu";

        [SerializeField] SETGameUIManager ui;
        [SerializeField] CardDeckManager deck;
        [SerializeField] UserGameHolder userGameHolder;
        [SerializeField] TutorialStyle style;
        [SerializeField] TutorialOverlay overlayPrefab;
        [SerializeField] RectTransform cardAreaRect;
        [SerializeField] RectTransform guessButtonRect;
        [SerializeField] Button hintButton;

        RectTransform hintButtonRect => (RectTransform)hintButton.transform;

        TutorialOverlay overlay;
        TutorialRunner runner;
        SETTutorialGameHandler handler;

        void Awake()
        {
            if (userGameHolder.IsTutorial == false)
                Destroy(gameObject);
        }

        void Start()
        {
            handler = userGameHolder.GetGameHandler<SETTutorialGameHandler>();

            ui.HideRoundTimer();

            overlay = Instantiate(overlayPrefab);
            overlay.Initialize(style, Camera.main);

            runner = TutorialRunner.Create(overlay);
            runner.Completed += Leave;
            runner.Skipped += Leave;
            runner.Run(CreateSteps());
        }

        void Leave()
        {
            handler.Dispose();
            userGameHolder.EndTutorial();
            Destroy(runner.gameObject);
            Destroy(overlay.gameObject);
            SceneManager.LoadScene(MenuScene);
        }

        public static void Launch(UserGameHolder userGameHolder, ProfileData profile, string gameScene)
        {
            userGameHolder.StartTutorial(SETTutorialGameHandler.CreateGameData(), new SETTutorialGameHandler(profile));
            SceneManager.LoadScene(gameScene);
        }

        IEnumerable<TutorialStep> CreateSteps()
        {
            IReadOnlyList<CardData> set = null;
            IReadOnlyList<CardData> CurrentSet() => set ??= handler.FindSet() ?? new List<CardData>();
            IEnumerable<RectTransform> SetCards() => CurrentSet().Select(deck.GetCardUI).Where(card => card != null).Select(card => (RectTransform)card.transform);
            RectTransform NextCardToSelect() => CurrentSet().Where(card => deck.IsSelected(card) == false).Select(deck.GetCardUI).FirstOrDefault(card => card != null)?.transform as RectTransform;
            bool TableReady() => ui.IsReadyForInput;

            yield return TutorialStep.Say("Welcome to SET!\nLet's learn how to play it in a minute.")
                .WaitFor(TableReady);

            yield return TutorialStep.Say("These are the cards on the table.\nEvery card has 4 features: color, shape, number and shading.")
                .Highlight(() => cardAreaRect);

            yield return TutorialStep.Say("A SET is 3 cards where, for each feature, the cards are either all the same or all different.")
                .HighlightAll(SetCards);

            yield return TutorialStep.Say("Look at these 3 cards: the colors are all different, the shape is the same, the numbers are all different and the shading is the same.\nThat's a SET!")
                .HighlightAll(SetCards);

            yield return TutorialStep.Say("When you spot a SET, tap Guess first to claim your turn.")
                .Highlight(() => guessButtonRect)
                .ClickTarget();

            yield return TutorialStep.Say("Now select the 3 cards that make the SET.")
                .HighlightAll(SetCards)
                .PointAt(NextCardToSelect)
                .Until(() => handler.Me.CorrectScore >= 1)
                .OnExit(() => set = null);

            yield return TutorialStep.Say("You found a SET and scored a point!\nYour score is shown on your panel.")
                .WaitFor(TableReady)
                .Highlight(() => ui.GetPlayerPanel(handler.Me));

            yield return TutorialStep.Say("Be careful: a wrong guess lights up one of these lamps and costs you a point.\nToo many wrong guesses and you are out of the game!")
                .Highlight(() => ui.GetPlayerPanel(handler.Me));

            yield return TutorialStep.Say("With other players, the first one to tap Guess has only a few seconds to pick the cards.\nBe quick!")
                .Highlight(() => guessButtonRect);

            yield return TutorialStep.Say("Stuck? Ask for a hint.\nIt marks one card of a SET. You have a limited number of hints.")
                .WaitFor(() => TableReady() && hintButton.interactable)
                .Highlight(() => hintButtonRect)
                .ClickTarget();

            yield return TutorialStep.Say("The marked card is part of a SET.\nTap Guess, then select it and the 2 cards that complete the SET.")
                .NoDim()
                .At(TutorialMessagePosition.Top)
                .PointAt(() => ui.IsReadyForInput ? guessButtonRect : null)
                .Until(() => handler.Me.CorrectScore >= 2);

            yield return TutorialStep.Say("Great job! You know how to play SET.\nNow go and challenge other players!")
                .WaitFor(TableReady);
        }
    }
}
