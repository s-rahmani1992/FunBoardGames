using System.Linq;
using FunBoardGames.Network;
using UnityEngine;

namespace FunBoardGames.Client
{
    public class CantStopBannerManager : MonoBehaviour
    {
        [SerializeField] CantStopGameBanner gameBannerPrefab;
        [SerializeField] TournamentGameHolder tournamentGameHolder;
        [SerializeField] RectTransform bannerHolder;

        void Start()
        {
            foreach (var game in tournamentGameHolder.Games.OfType<CantStopGame>())
                Instantiate(gameBannerPrefab, bannerHolder).Initialize(game);
        }
    }
}
