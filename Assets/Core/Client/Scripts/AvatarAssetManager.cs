using System.Collections.Generic;

using UnityEngine;

namespace FunBoardGames.Client
{
    [CreateAssetMenu(fileName = "AvatarAssetManager", menuName = "Scriptable Objects/AvatarAssetManager")]
    public class AvatarAssetManager : ScriptableObject
    {
        [SerializeField] List<Sprite> avatars = new();

        public int Count => avatars.Count;

        public Sprite GetAvatar(int index)
        {
            if (avatars.Count == 0) return null;
            if (index < 0 || index >= avatars.Count) index = 0;
            return avatars[index];
        }
    }
}
