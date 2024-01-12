using UnityEngine;

namespace OnlineBoardGames
{
    [CreateAssetMenu]
    public class DirectGameContainer : ScriptableObject
    {
        public static DirectGameContainer Instance { get; private set; }

        [field: SerializeField] public bool IsDirectGameActive { get; private set; }
        [field: SerializeField] public BoardGame Game { get; private set; }
        [field: SerializeField] public int PlayerCount { get; private set; }
        [field: SerializeField] public int TestRoomId { get; private set; }

        private void OnEnable()
        {
            Instance = this;
        }
    }
}
