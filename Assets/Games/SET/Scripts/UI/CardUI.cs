using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace FunBoardGames.SET
{
    public class CardUI : MonoBehaviour, IPoolable
    {
        [SerializeField] Image[] shapes;

        public string ObjectTag { get; set; }
        public CardData info { get; private set; }

        SETGameUIManager uiManager;
        Transform cardHolder;

        public event Action<CardUI> Selected;
        public event Action<CardUI> UnSelected;

        bool isSelected = false;

        static float[][] yPos = new float[][] { new float[] { 0 }, new float[] { -45f, 45f }, new float[] { -90f, 0, 90f } };

        private void Awake()
        {
            uiManager = FindObjectOfType<SETGameUIManager>();
        }

        public void OnCardClicked()
        {
            if (isSelected == false)
                Selected?.Invoke(this);
            else
                UnSelected?.Invoke(this);

            isSelected = !isSelected;
            Mark(isSelected);
        }

        public void InitializeUI(CardData cardData, Transform cardHolder)
        {
            info = cardData;
            this.cardHolder = cardHolder;

            foreach (var item in shapes) 
            { 
                item.gameObject.SetActive(false);
            }

            for (var a = 0; a <= info.CountIndex; a++)
            {
                shapes[a].transform.localPosition = new Vector3(yPos[info.CountIndex][a], 0, 0);
                shapes[a].color = uiManager.colors[info.Color];
                shapes[a].sprite = uiManager.cardShapes[3 * info.Shape + info.Shading];
                shapes[a].gameObject.SetActive(true);
            }
        }

        public void OnPull(params object[] parameters)
        {
            info = (CardData)(parameters[0]);
            transform.SetParent(parameters[3] as Transform);
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one;
            gameObject.SetActive(true);
            Debug.Log("pull  " + (float)parameters[2]);
            for (var a = 0; a <= info.CountIndex; a++){
                shapes[a].transform.localPosition = new Vector3(0, yPos[info.CountIndex][a], 0);
                shapes[a].color = uiManager.colors[info.Color];
                shapes[a].sprite = uiManager.cardShapes[3 * info.Shape + info.Shading];
                shapes[a].gameObject.SetActive(true);
            }

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

        public void MoveBack(float delay = 0.0f)
        {
            transform.SetParent(cardHolder);
            transform.DOLocalMove(new Vector2(0, 0), 0.2f).SetDelay(delay);
        }

        public void Reset()
        {
            isSelected = false;
            Mark(false);
        }
    }
}
