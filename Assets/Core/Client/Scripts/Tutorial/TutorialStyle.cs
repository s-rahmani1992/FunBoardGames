using TMPro;
using UnityEngine;

namespace FunBoardGames.Tutorial
{
    [CreateAssetMenu(fileName = "TutorialStyle", menuName = "Scriptable Objects/Tutorial Style")]
    public class TutorialStyle : ScriptableObject
    {
        [Header("Art")]
        [Tooltip("A hand pointing up, its fingertip is at the tip pivot")]
        public Sprite handSprite;
        [Tooltip("9-sliced background of the message panel")]
        public Sprite panelSprite;
        [Tooltip("9-sliced background of the skip button")]
        public Sprite skipButtonSprite;
        public TMP_FontAsset font;

        [Header("Colors")]
        public Color dimColor = new(0f, 0f, 0f, 0.5f);
        public Color panelColor = new(0.3f, 0.21f, 0.16f, 0.98f);
        public Color textColor = new(1f, 0.91f, 0.66f);
        public Color hintTextColor = new(1f, 0.91f, 0.66f, 0.6f);

        [Header("Layout (canvas units, the overlay uses a 1080x1920 reference)")]
        public float messageFontSize = 42f;
        public float panelWidth = 960f;
        public float panelMargin = 70f;
        public float holePadding = 14f;
        public float handSize = 140f;
        [Tooltip("Pivot of the hand sprite that sits on the pointed spot, the fingertip of a hand pointing up")]
        public Vector2 handTipPivot = new(0.359375f, 0.96484375f);
    }
}
