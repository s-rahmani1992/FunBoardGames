using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FunBoardGames.Network
{
    public interface IResourceHandler
    {
        IEnumerable<ResourceData> Resources{ get; }

        event Action<IEnumerable<UserResourceData>> UserResourceReceived;
        event Action<UserResourceData> ResourceUpdated;
    }
}
