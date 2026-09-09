using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FunBoardGames.Client
{
    public class MessageDialogButton : MonoBehaviour
    {
        [SerializeField] TMP_Text label;
        [SerializeField] Button button;

        public event Action Clicked;

        public void Setup(ButtonData data)
        {
            label.text = data.Text;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                data.Action?.Invoke();
                Clicked?.Invoke();
            });
        }
    }
}
