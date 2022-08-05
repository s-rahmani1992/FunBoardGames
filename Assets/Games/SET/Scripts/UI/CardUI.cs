using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace OnlineBoardGames.SET
{
    public class CardUI : MonoBehaviour, IPoolable
    {
        public string ObjectTag { get; set; }

        public CardData info { get; private set; }
        Vector2Int position;
        [SerializeField]
        LineMove move;
        SETGameUIManager uiManager;
        //SETSessionNetworkManager session;


        static float[][] yPos = new float[][] { new float[] { 0 }, new float[] { -1.6f, 1.6f }, new float[] { -3.3f, 0, 3.3f } };

        private void Awake(){
            uiManager = FindObjectOfType<SETGameUIManager>();
            //session = FindObjectOfType<SETSessionNetworkManager>();
        }

        private void OnMouseDown(){
            if(uiManager.UpdateSelected(this)) Mark(true);
        }

        public void OnPull(params object[] parameters){
            info = new CardData((byte)parameters[0]);
            position = new Vector2Int((byte)parameters[1] % 3, (byte)parameters[1] / 3);
            transform.position = new Vector3(0, 3.0f, 0);
            GetComponent<SortingGroup>().sortingOrder = 0;
            GetComponent<SortingGroup>().sortingLayerName = "card";
            gameObject.SetActive(true);
            Debug.Log("pull  " + (float)parameters[2]);
            for(var a = 0; a < info.count; a++){
                transform.GetChild(a).localPosition = new Vector3(0, yPos[info.count - 1][a], 0);
                transform.GetChild(a).GetComponent<SpriteRenderer>().color = uiManager.colors[info.color];
                transform.GetChild(a).GetComponent<SpriteRenderer>().sprite = uiManager.cardShapes[3 * info.shape + info.shading];
                transform.GetChild(a).gameObject.SetActive(true);
                GetComponent<SortingGroup>().sortingOrder = 1;
                move.MoveInLine(new Vector2(-1.6f + 1.6f * position.x, 1.8f - 1.1f * position.y), MoveMode.FixedTime, 0.2f, (b) => b.GetComponent<SortingGroup>().sortingOrder = 0, (float)parameters[2]);
            }

            for(var a = info.count; a < 3; a++)
                transform.GetChild(a).gameObject.SetActive(false);
            //gameObject.SetActive(true);
        }

        public void OnPush(params object[] parameters){
            gameObject.SetActive(false);
            Mark(false);
        }

        public void Mark(bool isSelected) {
            GetComponent<SpriteRenderer>().color = (isSelected ? Color.yellow : Color.white);
        }

        public void MoveBack(){
            move.MoveInLine(new Vector2(-1.6f + 1.6f * position.x, 1.8f - 1.1f * position.y), MoveMode.FixedTime, 0.2f, 
                (b) => { 
                    b.GetComponent<SortingGroup>().sortingOrder = 0;
                    b.GetComponent<SortingGroup>().sortingLayerName = "card";
                });
        }
    }
}
