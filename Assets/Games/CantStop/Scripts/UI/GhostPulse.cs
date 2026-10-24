using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace FunBoardGames.CantStop
{
    public enum GhostStyle
    {
        /// <summary>A result the player only needs to see: calm, still and translucent.</summary>
        Preview,

        /// <summary>A choice the player must make: gold, bobbing and pulsing so it reads as tappable.</summary>
        Choice,

        /// <summary>The choice the player made: solid and still.</summary>
        Chosen,
    }

    /// <summary>Draws a ghost of a move that has not been made yet, in the style of the situation it is shown in.</summary>
    [RequireComponent(typeof(Image))]
    public class GhostPulse : MonoBehaviour
    {
        [SerializeField] GhostStyle defaultStyle = GhostStyle.Preview;
        [SerializeField] Color previewTint = new(1f, 1f, 1f, 0.5f);
        [SerializeField] Color choiceTint = new(1f, 1f, 0.92f, 1f);
        [SerializeField] Color chosenTint = Color.white;
        [SerializeField] float choiceMinAlpha = 0.75f;
        [SerializeField] float choiceMaxAlpha = 1f;
        [SerializeField] float bobPixels = 10f;
        [SerializeField] float duration = 0.6f;

        GhostStyle style;
        bool hasStyle;
        Image image;
        Tween alphaTween;
        Tween bobTween;
        Vector3 basePosition;

        void OnEnable() => Apply();

        void OnDisable() => StopMotion(true);

        public void SetStyle(GhostStyle newStyle)
        {
            style = newStyle;
            hasStyle = true;

            if (isActiveAndEnabled)
                Apply();
        }

        void Apply()
        {
            image = image != null ? image : GetComponent<Image>();
            var current = hasStyle ? style : defaultStyle;
            StopMotion(current != GhostStyle.Choice);

            switch (current)
            {
                case GhostStyle.Preview:
                    image.color = previewTint;
                    break;

                case GhostStyle.Chosen:
                    image.color = chosenTint;
                    break;

                case GhostStyle.Choice:
                    var color = choiceTint;
                    color.a = choiceMinAlpha;
                    image.color = color;
                    alphaTween = image.DOFade(choiceMaxAlpha, duration).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);

                    if (bobPixels > 0f)
                    {
                        // The position was set before the style, so it is the resting point of the bobbing
                        basePosition = transform.position;
                        float bob = 0f;
                        bobTween = DOTween.To(() => bob, v =>
                        {
                            bob = v;
                            transform.position = basePosition + Vector3.up * (v * transform.lossyScale.y);
                        }, bobPixels, duration).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
                    }

                    break;
            }
        }

        void StopMotion(bool restorePosition)
        {
            alphaTween?.Kill();
            alphaTween = null;

            if (bobTween != null)
            {
                bobTween.Kill();
                bobTween = null;

                if (restorePosition)
                    transform.position = basePosition;
            }
        }
    }
}
