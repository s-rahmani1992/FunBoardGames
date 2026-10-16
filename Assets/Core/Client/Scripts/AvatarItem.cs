using System;

using UnityEngine;
using UnityEngine.UI;

namespace FunBoardGames.Client
{
    public class AvatarItem : MonoBehaviour
    {
        [SerializeField] Image avatarImage;
        [SerializeField] Toggle toggle;

        public int Index { get; private set; }

        public event Action<AvatarItem> Selected;

        public void Initialize(int index, Sprite sprite, ToggleGroup group)
        {
            Index = index;
            avatarImage.sprite = sprite;
            toggle.group = group;
            toggle.onValueChanged.AddListener(OnToggleChanged);
        }

        public void Select()
        {
            toggle.isOn = true;
        }

        private void OnToggleChanged(bool isOn)
        {
            if (isOn) Selected?.Invoke(this);
        }
    }
}
