using UnityEngine;

namespace FunBoardGames
{
    [System.Serializable]
    public struct SceneData
    {
        [field: Scene]
        [field: SerializeField]
        public string Scene { get; private set; }

        [field: SerializeField]
        public BoardGame gameType { get; private set; }
    }

    [CreateAssetMenu(fileName = "SceneAssetMap", menuName = "Scriptable Objects/SceneAssetMap")]
    public class SceneAssetMap : ScriptableObject
    {
        [SerializeField] SceneData[] scenes;

        public string GetScene(BoardGame gameType)
        {
            foreach (var s in scenes)
            {
                if (s.gameType == gameType)
                    return s.Scene;
            }

            return null;
        }
    }
}
