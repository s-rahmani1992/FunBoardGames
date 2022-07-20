using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace BoardGames.SET
{
    public class CardUI : MonoBehaviour, IPoolable
    {
        public string ObjectTag { get; set; }

        CardData info;
        Vector2Int position;
        [SerializeField]
        LineMove move;

        static float[][] yPos = new float[][] { new float[] { 0 }, new float[] { -1.6f, 1.6f }, new float[] { -3.3f, 0, 3.3f } };

        public void OnPull(params object[] parameters){
            SETGameUIManager m = parameters[0] as SETGameUIManager;
            info = new CardData((byte)parameters[1]);
            position = new Vector2Int((byte)parameters[2] % 3, (byte)parameters[2] / 3);
            transform.position = new Vector3(0, 3.8f, 0);
            Debug.Log("pull  " + (float)parameters[3]);
            for(var a = 0; a < info.count; a++)
            {
                transform.GetChild(a).localPosition = new Vector3(0, yPos[info.count - 1][a], 0);
                transform.GetChild(a).GetComponent<SpriteRenderer>().color = m.colors[info.color];
                transform.GetChild(a).GetComponent<SpriteRenderer>().sprite = m.cardShapes[3 * info.shape + info.shading];
                transform.GetChild(a).gameObject.SetActive(true);
                GetComponent<SortingGroup>().sortingOrder = 1;
                move.MoveInLine(new Vector2(-1.6f + 1.6f * position.x, 1.8f - 1.2f * position.y), MoveMode.FixedTime, 0.2f, (b) => b.GetComponent<SortingGroup>().sortingOrder = 0, (float)parameters[3]);
            }

            for(var a = info.count; a < 3; a++)
                transform.GetChild(a).gameObject.SetActive(false);
            //gameObject.SetActive(true);
        }

        public void OnPush(params object[] parameters)
        {
            throw new System.NotImplementedException();
        }
    }
}
