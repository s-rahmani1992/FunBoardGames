using UnityEngine;
using Mirror;

namespace OnlineBoardGames.SET
{
    public struct GuessSETMessage : NetworkMessage
    {
        public CardData card1, card2, card3;
    }

    public struct VoteMessage : NetworkMessage
    {
        public bool isYes;
    }
}
