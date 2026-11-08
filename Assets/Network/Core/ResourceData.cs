using UnityEngine;

namespace FunBoardGames.Network
{
    public enum ResourceType
    {
        Amount,
        Subscription,
    }

    public enum ResourceName
    {
        xp,
        gem,
        setCardHint,
        cantStopResetRound,
        cantStopBustSaver,
    }

    public class ResourceData
    {
        public int Id { get; internal set; }
        public ResourceType Type { get; internal set; }
        public ResourceName Name { get; internal set; }
    }
}
