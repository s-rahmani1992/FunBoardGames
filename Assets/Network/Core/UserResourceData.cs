using System;

namespace FunBoardGames.Network
{
    public class UserResourceData
    {
        public ResourceData Resource { get; internal set; }
        public int? Balance { get; internal set; }
        public DateTimeOffset? ExpireDate { get; internal set; }
    }
}
