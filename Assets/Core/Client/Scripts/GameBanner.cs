using FunBoardGames.Network;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FunBoardGames.Client
{
    public abstract class GameBanner<T> : MonoBehaviour where T : GameData
    {
        [Header("Common UI Elements")]
        [SerializeField] TMP_Text titleText;
        [SerializeField] TMP_Text playerCountText;
        [SerializeField] Button joinRandomButton;
        [SerializeField] Button howToPlayButton;
        [SerializeField] protected UserGameHolder userGameHolder;
        [SerializeField] protected UserProfile userProfile;
        [SerializeField] protected SceneAssetMap sceneMap;

        protected T GameData { get; private set; }

        protected void InitializeBase(T gameData)
        {
            GameData = gameData;

            titleText.text = string.IsNullOrEmpty(gameData.Name) ? gameData.Type.ToString() : gameData.Name;
            playerCountText.text = gameData.NumberofPlayers.ToString();
            joinRandomButton.onClick.AddListener(OnJoinRandomClicked);
            howToPlayButton.onClick.AddListener(OnHowToPlayClicked);
        }

        public abstract void Initialize(T gameData);

        protected abstract void OnHowToPlayClicked();

        protected abstract void OnJoinRandomClicked();
    }
}
