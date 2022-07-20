using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoardGames.SET
{
    public class SETGameUIManager : MonoBehaviour
    {
        public Transform playerPanel;
        public Timer timer;
        public ObjectPoolManager pool;

        public Color[] colors;
        public Sprite[] cardShapes;

        public void PlaceCards(byte[] cardInfos, byte[] cardPoses){
            for (int i = 0; i < cardInfos.Length; i++)
            {
                pool.PullFromList(0, this, cardInfos[i], cardPoses[i], 0.2f * i);
            }
        }
    }
}
