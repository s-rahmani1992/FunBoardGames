using System;
using DG.Tweening;
using UnityEngine;

namespace FunBoardGames.CantStop
{
    public class ConeMover : MonoBehaviour
    {
        public const float Duration = 0.25f;

        Tween tween;
        Action onDetach;

        public static ConeMover Of(GameObject cone) => cone.TryGetComponent(out ConeMover mover) ? mover : cone.AddComponent<ConeMover>();

        public void MoveTo(Transform target, float scale, Action onDetach = null, bool animate = true)
        {
            tween?.Kill();

            var canvas = GetComponentInParent<Canvas>().rootCanvas.transform;
            transform.SetParent(canvas, true);
            transform.SetAsLastSibling();

            Detach();
            this.onDetach = onDetach;

            if (!animate)
            {
                Arrive(target, scale);
                return;
            }

            var rect = (RectTransform)transform;
            rect.anchoredPosition3D = new Vector3(rect.anchoredPosition.x, rect.anchoredPosition.y, 0f);
            Vector3 fromPosition = transform.position;
            Vector3 fromScale = transform.localScale;
            Vector3 toScale = Vector3.one * (scale * target.lossyScale.x / canvas.lossyScale.x);

            tween = DOVirtual.Float(0f, 1f, Duration, t =>
            {
                if (target == null)
                    return;

                transform.position = Vector3.LerpUnclamped(fromPosition, target.position, t);
                transform.localScale = Vector3.LerpUnclamped(fromScale, toScale, t);
            })
            .SetEase(Ease.OutQuad)
            .SetLink(gameObject)
            .OnComplete(() => Arrive(target, scale));
        }

        public void Release()
        {
            tween?.Kill();
            Detach();
        }

        void Arrive(Transform target, float scale)
        {
            if (target == null)
                return;

            transform.SetParent(target, false);

            var rect = (RectTransform)transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition3D = Vector3.zero; // the flight is in world space, which leaves a stray z behind
            rect.localScale = Vector3.one * scale;
        }

        void Detach()
        {
            var callback = onDetach;
            onDetach = null;
            callback?.Invoke();
        }
    }
}
