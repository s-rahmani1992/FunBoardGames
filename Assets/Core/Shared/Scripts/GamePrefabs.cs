using FishNet.Managing.Object;
using RotaryHeart.Lib.SerializableDictionary;
using System;
using UnityEngine;

namespace FunBoardGames 
{
    [CreateAssetMenu]
    public class GamePrefabs : ScriptableObject
    {
        [Serializable]
        public class GamePrefabData
        {
            public RoomManager room;
            public BoardGamePlayer player;
        }

        [SerializeField] SerializableDictionaryBase<BoardGame, GamePrefabData> prefabs;

        public GamePrefabData this[BoardGame game] => prefabs[game];

        public void Register(PrefabObjects prefabObjects)
        {
            foreach(var g in prefabs.Values)
            {
                prefabObjects.AddObject(g.room.NetworkObject, true);
                prefabObjects.AddObject(g.player.NetworkObject, true);
            }
        }
    }
}
