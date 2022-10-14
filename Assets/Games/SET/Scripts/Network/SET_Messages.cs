using UnityEngine;
using Mirror;

namespace OnlineBoardGames.SET
{
    public struct AttempSETGuess : NetworkMessage { }

    public struct GuessSETMessage : NetworkMessage
    {
        public CardData card1, card2, card3;
    }

    public struct DestributeRequest : NetworkMessage { }

    public struct VoteMessage : NetworkMessage
    {
        public bool isYes;
    }
}
