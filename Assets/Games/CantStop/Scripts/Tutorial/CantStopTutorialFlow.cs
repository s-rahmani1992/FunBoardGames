using FunBoardGames.Network;
using FunBoardGames.Tutorial;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FunBoardGames.CantStop
{
    public class CantStopTutorialFlow : MonoBehaviour
    {
        const string MenuScene = "Menu";

        [SerializeField] UserGameHolder userGameHolder;
        [SerializeField] TutorialStyle style;
        [SerializeField] TutorialOverlay overlayPrefab;
        [SerializeField] DiceController diceController;
        [SerializeField] Button rollButton;
        [SerializeField] Button playButton;
        [SerializeField] Button resetRoundButton;
        [SerializeField] Button bustSaverButton;
        [SerializeField] RectTransform boardRect;
        [SerializeField] RectTransform diceAreaRect;
        [SerializeField] RectTransform markerRect;
        [SerializeField] RectTransform playerPanelRect;

        TutorialOverlay overlay;
        TutorialRunner runner;
        CantStopTutorialGameHandler handler;

        void Awake()
        {
            if (userGameHolder.IsTutorial == false)
                Destroy(gameObject);
        }

        void Start()
        {
            handler = userGameHolder.GetGameHandler<CantStopTutorialGameHandler>();

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
            userGameHolder.StartTutorial(CantStopTutorialGameHandler.CreateGameData(), new CantStopTutorialGameHandler(profile));
            SceneManager.LoadScene(gameScene);
        }

        IEnumerable<TutorialStep> CreateSteps()
        {
            bool TableReady() => rollButton.interactable;
            RectTransform RollRect() => (RectTransform)rollButton.transform;
            RectTransform PlayRect() => (RectTransform)playButton.transform;
            RectTransform ResetRect() => (RectTransform)resetRoundButton.transform;
            RectTransform BustSaverRect() => (RectTransform)bustSaverButton.transform;
            bool DiceReady(int set) => handler.DiceSetsDelivered > set;

            bool Picked(int a, int b)
            {
                var selected = diceController.SelectedIndices.ToList();
                return selected.Count == 2 && selected.Contains(a) && selected.Contains(b);
            }

            RectTransform NextDiceToPick(int a, int b)
            {
                bool hasFirst = diceController.SelectedIndices.Contains(a);
                return diceController.GetDiceRect(hasFirst ? b : a);
            }

            string PairText(int set, int a, int b)
            {
                var dice = CantStopTutorialGameHandler.DiceSets[set].Dice;
                var rest = Enumerable.Range(0, dice.Length).Where(i => i != a && i != b).ToArray();
                return $"{dice[a]} + {dice[b]} = {dice[a] + dice[b]}  and  {dice[rest[0]]} + {dice[rest[1]]} = {dice[rest[0]] + dice[rest[1]]}";
            }

            // The player taps Roll and the dice of the given set arrive
            TutorialStep RollStep(int set, string message) => TutorialStep.Say(message)
                .WaitFor(TableReady)
                .Highlight(RollRect)
                .PointAt(RollRect)
                .Until(() => DiceReady(set));

            // The player taps exactly the two dice that make the pairs of the given set
            TutorialStep PickStep(int set, int a, int b, string message) => TutorialStep.Say(message)
                .WaitFor(() => DiceReady(set))
                .HighlightAll(() => new[] { diceController.GetDiceRect(a), diceController.GetDiceRect(b) })
                .PointAt(() => NextDiceToPick(a, b))
                .Until(() => Picked(a, b));

            // The player taps Roll again: the markers are placed and the next set arrives
            TutorialStep RollAgainStep(int set, string message) => TutorialStep.Say(message)
                .HighlightAll(() => new[] { RollRect(), boardRect })
                .PointAt(RollRect)
                .Until(() => DiceReady(set + 1));

            TutorialStep PlayStep(int round, string message) => TutorialStep.Say(message)
                .Highlight(PlayRect)
                .PointAt(PlayRect)
                .Until(() => handler.RoundsPlayed >= round);

            yield return TutorialStep.Say("Welcome to Can't Stop!\nPush your luck and climb to the top of 3 columns before anyone else.")
                .WaitFor(TableReady);

            yield return TutorialStep.Say("Every column is a number from 2 to 12, the sum of two dice.\nThe middle columns are the longest because those sums come up most often.")
                .Highlight(() => boardRect);

            yield return TutorialStep.Say("Reach the top of a column to finish it.\nEvery finished column lights up a cone here. Light up 3 and you win!")
                .Highlight(() => playerPanelRect);

            yield return TutorialStep.Say("On your turn you climb with 3 temporary markers.\nEvery new column you climb in uses one of them.")
                .Highlight(() => markerRect);

            // Turn 1: the normal flow
            yield return RollStep(0, "Let's play a turn!\nTap Roll to throw the 4 dice.");

            yield return PickStep(0, 0, 1, "Split the dice in two pairs.\nTap these two dice, the other two become the second pair.");

            yield return RollAgainStep(0, $"{PairText(0, 0, 1)}\nYour markers will climb in both of those columns.\nTap Roll to place them and roll again.");

            yield return PickStep(1, 1, 2, $"New dice! Pick these two:\n{PairText(1, 1, 2)}\nYou have only 3 markers, so this one fills the last of them.");

            yield return PlayStep(1, "Tap Play to lock in your climb and end your turn.\nRolling again is risky: if no pair fits your markers, you bust and lose the whole turn!");

            yield return TutorialStep.Say("Your markers turned into permanent cones.\nNext turn you continue from where you stopped.")
                .WaitFor(TableReady)
                .Highlight(() => boardRect);

            // Turn 2: the reset round power-up
            yield return RollStep(2, "Another turn. Roll the dice!");

            yield return PickStep(2, 0, 1, "Pick these two dice for now.");

            yield return RollAgainStep(2, $"{PairText(2, 0, 1)}\nTap Roll to place the markers and roll again.");

            yield return TutorialStep.Say("These dice won't get you far.\nUse Reset Round: it removes the markers you placed this turn so you can start over.\nYou can use it once per turn.")
                .WaitFor(() => DiceReady(3) && resetRoundButton.interactable)
                .Highlight(ResetRect)
                .PointAt(ResetRect)
                .Until(() => handler.ResetCount >= 1);

            yield return RollStep(4, "The markers are gone and the turn starts fresh.\nRoll the dice again!");

            yield return PickStep(4, 0, 3, $"Pick these two dice:\n{PairText(4, 0, 3)}");

            yield return PlayStep(2, "That's a fine climb.\nTap Play to keep it.");

            // Turn 3: the bust and the bust saver power-up
            yield return TutorialStep.Say("One more turn, to see what happens when luck runs out.")
                .WaitFor(TableReady);

            yield return RollStep(5, "Roll the dice!");

            yield return PickStep(5, 0, 1, $"Pick these two dice:\n{PairText(5, 0, 1)}");

            yield return RollAgainStep(5, "Tap Roll to place the markers and roll again.");

            yield return PickStep(6, 0, 1, $"Pick these two dice:\n{PairText(6, 0, 1)}\nThat uses your third marker.");

            yield return RollAgainStep(6, "All 3 markers are placed now, so only those 3 columns can still move.\nTap Roll to take the risk.");

            yield return TutorialStep.Say("Busted! None of these sums fit your 3 markers.\nNormally you would lose everything you climbed this turn.")
                .WaitFor(() => DiceReady(7))
                .Highlight(() => diceAreaRect);

            yield return TutorialStep.Say("But you have a Bust Saver!\nIt cancels the busted roll and keeps your markers where they are.")
                .WaitFor(() => bustSaverButton.interactable)
                .Highlight(BustSaverRect)
                .PointAt(BustSaverRect)
                .Until(() => handler.BustSaverCount >= 1);

            yield return RollStep(8, "You are safe! The bad roll does not count.\nRoll the dice again.");

            yield return PickStep(8, 0, 1, $"Pick these two dice:\n{PairText(8, 0, 1)}\nOnly one of them can move, and that is fine.");

            yield return PlayStep(3, "Tap Play to lock in your climb.");

            yield return TutorialStep.Say("Great job! You know how to play Can't Stop.\nNow go and challenge other players!")
                .WaitFor(TableReady);
        }
    }
}
