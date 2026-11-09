using System.Linq;
using FunBoardGames.Network;
using UnityEngine;

namespace FunBoardGames.Client
{
    /// <summary>
    /// Spawns a <see cref="SETGameBanner"/> for every tournament SET game.
    /// </summary>
    public class SETBannerManager : MonoBehaviour
    {
        [SerializeField] SETGameBanner gameBannerPrefab;
        [SerializeField] TournamentGameHolder tournamentGameHolder;
        [SerializeField] RectTransform bannerHolder;

        void Start()
        {
            foreach (var game in tournamentGameHolder.Games.OfType<SETGame>())
                Instantiate(gameBannerPrefab, bannerHolder).Initialize(game);
        }
    }
}
