
#if UNITY_EDITOR
using ParrelSync;
#endif

using UnityEngine;

namespace FunBoardGames
{
    public class Utilities
    {
        public static string DeviceId
        {
            get
            {
#if UNITY_EDITOR
                bool isClone = ClonesManager.IsClone();
                var argument = ClonesManager.GetArgument();
                return !isClone ? SystemInfo.deviceUniqueIdentifier : SystemInfo.deviceUniqueIdentifier + "_" + argument;
#else
                return SystemInfo.deviceUniqueIdentifier;
#endif
            }
        }
    }
}