using UnityEngine;
using Mirror;

namespace OnlineBoardGames.SET
{
    public struct AttempSETGuess : NetworkMessage { }

    public struct GuessSETMessage : NetworkMessage
    {
        public byte card1, card2, card3;
    }
}
