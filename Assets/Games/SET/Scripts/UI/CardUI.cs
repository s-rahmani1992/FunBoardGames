using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OnlineBoardGames.SET
{
    public class CardUI : MonoBehaviour, IPoolable
    {
        public string ObjectTag { get; set; }
        public CardData info { get; private set; }
        Vector2Int position;
        [SerializeField] LineMove move;
        [SerializeField] Image[] shapes;
        [SerializeField] Canvas canvas;
        SETGameUIManager uiManager;

        static float[][] yPos = new float[][] { new float[] { 0 }, new float[] { -45f, 45f }, new float[] { -90f, 0, 90f } };

        private void Awake()
        {
            uiManager = FindObjectOfType<SETGameUIManager>();
        }

        public void OnCardClicked()
        {
            if (uiManager.UpdateSelected(this)) Mark(true);
        }

        public void OnPull(params object[] parameters)
        {
            info = (CardData)(parameters[0]);
            position = new Vector2Int((byte)parameters[1] % 3, (byte)parameters[1] / 3);
            transform.SetParent(parameters[3] as Transform);
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one;
            canvas.sortingOrder = 0;
            canvas.sortingLayerName = "card";
            gameObject.SetActive(true);
            Debug.Log("pull  " + (float)parameters[2]);
            for (var a = 0; a <= info.CountIndex; a++){
                shapes[a].transform.localPosition = new Vector3(0, yPos[info.CountIndex][a], 0);
                shapes[a].color = uiManager.colors[info.Color];
                shapes[a].sprite = uiManager.cardShapes[3 * info.Shape + info.Shading];
                shapes[a].gameObject.SetActive(true);
                canvas.sortingOrder = 1;
                move.MoveInLine(new Vector2(-320 + 320 * position.x, -270 - 220f * position.y), MoveMode.FixedTime, 0.2f, (b) => b.GetComponent<Canvas>().sortingOrder = 0, (float)parameters[2]);
            }

            MyUtils.DelayAction(() => { uiManager.UpdateCardMeter(); }, (float)parameters[2], uiManager);
            for (var a = info.CountIndex + 1; a < 3; a++)
                shapes[a].gameObject.SetActive(false);
        }

        public void OnPush(params object[] parameters)
        {
            gameObject.SetActive(false);
            Mark(false);
        }

        public void Mark(bool isSelected) 
        {
            GetComponent<Image>().color = (isSelected ? Color.yellow : Color.white);
        }

        public void MarkHint()
        {
            GetComponent<Image>().color = new Color(0, 0.6135f, 1);
        }

        public void MoveBack()
        {
            move.MoveInLine(new Vector2(-320 + 320 * position.x, -270 - 220f * position.y), MoveMode.FixedTime, 0.2f, 
                (b) => { 
                    b.GetComponent<Canvas>().sortingOrder = 0;
                    b.GetComponent<Canvas>().sortingLayerName = "card";
                });
        }
    }
}
