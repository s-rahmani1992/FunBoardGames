using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FunBoardGames.Client
{
    public class MessageDialog : BaseDialog, IDataDialog<MessageData>
    {
        [SerializeField] RectTransform holder;
        [SerializeField] TMP_Text titleText;
        [SerializeField] TMP_Text messageText;
        [SerializeField] RectTransform buttonsPanel;
        [SerializeField] MessageDialogButton buttonTemplate;

        MessageData data;

        public void Initialize(MessageData data)
        {
            this.data = data;
        }

        public override void Show()
        {
            base.Show();

            titleText.text = data.Title;
            messageText.text = data.Message;

            foreach (Transform child in buttonsPanel)
                Destroy(child.gameObject);

            foreach (var buttonData in data.Buttons)
            {
                var button = Instantiate(buttonTemplate, buttonsPanel);
                button.gameObject.SetActive(true);
                button.Setup(buttonData);
                button.Clicked += Close;
            }

            // The holder's VerticalLayoutGroup + ContentSizeFitter grow it to fit the
            // (word-wrapped, fixed font size) message; force an immediate rebuild so the
            // correct size is already in place for the dialog's opening animation.
            LayoutRebuilder.ForceRebuildLayoutImmediate(holder);
        }
    }
}
