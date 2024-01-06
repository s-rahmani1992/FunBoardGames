using UnityEngine;
using Mirror;

namespace OnlineBoardGames {
    public class BoardGameCardDataHolder : NetworkBehaviour
    {
        [field: SerializeField] public CantStop.GameBoard Board { get; private set; }

        public static BoardGameCardDataHolder Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }
    }
}