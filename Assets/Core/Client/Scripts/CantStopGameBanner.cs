using FunBoardGames.CantStop;
using FunBoardGames.Network;
using TMPro;
using UnityEngine;

namespace FunBoardGames.Client
{
    public class CantStopGameBanner : GameBanner<CantStopGame>
    {
        [Header("Cant Stop UI Elements")]
        [SerializeField] TMP_Text markerConeCountText;
        [SerializeField] TMP_Text winConeCountText;
        [SerializeField] TMP_Text timeoutLimitText;
        [SerializeField] TMP_Text roundTimeText;

        public override void Initialize(CantStopGame game)
        {
            InitializeBase(game);

            markerConeCountText.text = game.MarkerConeCount.ToString();
            winConeCountText.text = game.WinConeCount.ToString();
            timeoutLimitText.text = game.TimeoutLimit.ToString();
            roundTimeText.text = $"{game.RoundTime}s";
        }

        protected override void OnHowToPlayClicked()
        {
            CantStopTutorialFlow.Launch(userGameHolder, userProfile.Profile, sceneMap.GetScene(BoardGame.CantStop));
        }
    }
}
