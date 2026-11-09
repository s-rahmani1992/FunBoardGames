using FunBoardGames.Network;
using FunBoardGames.SET;
using TMPro;
using UnityEngine;

namespace FunBoardGames.Client
{
    public class SETGameBanner : GameBanner<SETGame>
    {
        [Header("SET Game UI Elements")]
        [SerializeField] TMP_Text cardCountText;
        [SerializeField] TMP_Text wrongLimitText;
        [SerializeField] TMP_Text roundTimeText;

        public override void Initialize(SETGame game)
        {
            InitializeBase(game);
            int cardCount = 1;

            for (int i = 0; i < game.AttributeCount; i++)
                cardCount *= 3;
            cardCountText.text = cardCount.ToString();
            wrongLimitText.text = game.WrongLimit.ToString();
            roundTimeText.text = $"{game.RoundTime}s";
        }

        protected override void OnHowToPlayClicked()
        {
            SETTutorialFlow.Launch(userGameHolder, userProfile.Profile, sceneMap.GetScene(BoardGame.SET));
        }
    }
}
