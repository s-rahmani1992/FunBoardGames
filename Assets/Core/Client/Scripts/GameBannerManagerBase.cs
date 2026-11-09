using FunBoardGames.Network;
using UnityEngine;

namespace FunBoardGames.Client
{
    /// <summary>
    /// Spawns a <see cref="GameBanner"/> for every tournament game of a single board game type.
    /// </summary>
    public abstract class GameBannerManagerBase : MonoBehaviour
    {
        [SerializeField] GameBanner gameBannerPrefab;
        [SerializeField] TournamentGameHolder tournamentGameHolder;
        [SerializeField] RectTransform bannerHolder;

        protected abstract BoardGame GameType { get; }

        void Start()
        {
            foreach (var gameData in tournamentGameHolder.Games)
            {
                if (gameData.Type != GameType)
                    continue;

                var banner = Instantiate(gameBannerPrefab, bannerHolder);
                banner.Initialize(gameData);
            }
        }
    }
}
