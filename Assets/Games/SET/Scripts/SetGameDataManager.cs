using System.Collections.Generic;
using UnityEngine;

namespace FunBoardGames.SET
{
    public class SetGameDataManager
    {
        private static List<CardData> SETCards = new(81);

#if UNITY_EDITOR || UNITY_SERVER
        [RuntimeInitializeOnLoadMethod]
#endif
        static void Initialize()
        {
            SETCards = new List<CardData>(81);
            for (byte i = 0; i < 3; i++)
            {
                for (byte j = 0; j < 3; j++)
                {
                    for (byte k = 0; k < 3; k++)
                    {
                        for (byte l = 0; l < 3; l++)
                            SETCards.Add(new CardData(i, j, k, l));
                    }
                }
            }
        }

        public static CardData GetCard(int index)
        {
            return SETCards[index];
        }
    }
}
