using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace FunBoardGames.CantStop
{
    public enum DiceVisualState
    {
        None,
        Selected,
        Partner,
    }

    public class DiceItem : MonoBehaviour
    {
        // Rotation that turns the face of the model with the given value toward the camera (index 0 is the resting face)
        static readonly Quaternion[] FaceRotations =
        {
            Quaternion.identity,
            Quaternion.Euler(0f, 0f, 0f),
            Quaternion.Euler(0f, 90f, 0f),
            Quaternion.Euler(270f, 180f, 0f),
            Quaternion.Euler(90f, 180f, 0f),
            Quaternion.Euler(0f, 270f, 0f),
            Quaternion.Euler(0f, 180f, 0f),
        };

        static readonly int ColorId = Shader.PropertyToID("_Color");

        [field: SerializeField] public int Index { get; private set; }
        [SerializeField] Transform holder;
        [SerializeField] Transform die;
        [SerializeField] MeshRenderer dieRenderer;
        [SerializeField] Image glow;
        [SerializeField] Color selectedGlowColor = new(1f, 0.78f, 0.25f, 1f);
        [SerializeField] Color partnerGlowColor = new(0.18f, 0.83f, 0.77f, 1f);
        [SerializeField] Color waitingTint = new(0.55f, 0.55f, 0.55f, 1f);
        [SerializeField] float liftHeight = 18f;
        [SerializeField] float selectedScale = 1.08f;
        [SerializeField] float spinSpeed = 720f;
        [SerializeField] float minSpinTime = 0.02f;
        [SerializeField] float settleTime = 0.7f;
        [SerializeField] float hopHeight = 30f;

        public int Value { get; private set; }

        public event Action<DiceItem> Clicked;

        DiceVisualState state;
        float hopOffset;
        Vector3 holderRestPosition;
        MaterialPropertyBlock propertyBlock;
        Coroutine rollRoutine;
        Sequence turnSequence;

        void Awake()
        {
            holderRestPosition = holder.localPosition;
            propertyBlock = new MaterialPropertyBlock();
            SetGlowColor(new Color(selectedGlowColor.r, selectedGlowColor.g, selectedGlowColor.b, 0f));
            SetTint(waitingTint);
        }

        void Update()
        {
            bool lifted = state == DiceVisualState.Selected;
            var targetPos = holderRestPosition + Vector3.up * ((lifted ? liftHeight : 0f) + hopOffset);
            var targetScale = Vector3.one * (lifted ? selectedScale : 1f);
            float k = 1f - Mathf.Exp(-14f * Time.deltaTime);
            holder.localPosition = Vector3.Lerp(holder.localPosition, targetPos, k);
            holder.localScale = Vector3.Lerp(holder.localScale, targetScale, k);

            var current = glow.color;
            var glowTarget = state == DiceVisualState.Selected ? selectedGlowColor : state == DiceVisualState.Partner ? partnerGlowColor : new Color(current.r, current.g, current.b, 0f);
            SetGlowColor(Color.Lerp(current, glowTarget, k));
        }

        void OnDisable() => StopRoll();

        public void StartRoll()
        {
            StopRoll();
            Value = 0;
            SetTint(Color.white);
            rollRoutine = StartCoroutine(RollRoutine());
        }

        IEnumerator RollRoutine()
        {
            var spinAxis = UnityEngine.Random.onUnitSphere;
            float remaining = minSpinTime;

            while(remaining > 0f || Value == 0)
            {
                die.Rotate(spinAxis, spinSpeed * Time.deltaTime, Space.Self);
                remaining -= Time.deltaTime;
                yield return null;
            }

            rollRoutine = null;
            TurnToValue();
        }

        public void SetValue(int value)
        {
            if (value <= 0)
            {
                StopRoll();
                Value = 0;
                SetTint(waitingTint);
                return;
            }

            Value = value;
            SetTint(Color.white);

            if (rollRoutine == null)
                TurnToValue();
        }

        void TurnToValue()
        {
            turnSequence?.Kill();

            var start = die.localRotation;
            var target = FaceRotations[Mathf.Clamp(Value, 1, 6)];
            var axis = UnityEngine.Random.onUnitSphere;

            turnSequence = DOTween.Sequence()
                .Append(DOVirtual.Float(0f, 1f, settleTime, p =>
                {
                    float ease = 1f - Mathf.Pow(1f - p, 3f);
                    die.localRotation = Quaternion.AngleAxis(360f * (1f - ease), axis) * Quaternion.Slerp(start, target, ease);
                }))
                .Join(DOVirtual.Float(0f, 1f, settleTime, p => hopOffset = Mathf.Sin(p * Mathf.PI) * hopHeight))
                .OnKill(() => hopOffset = 0f);
        }

        void StopRoll()
        {
            if (rollRoutine != null)
                StopCoroutine(rollRoutine);

            rollRoutine = null;
            turnSequence?.Kill();
            turnSequence = null;
        }

        public void OnClick() => Clicked?.Invoke(this);

        public void Highlight(bool highlight) => SetState(highlight ? DiceVisualState.Selected : DiceVisualState.None);

        public void SetState(DiceVisualState newState) => state = newState;

        void SetGlowColor(Color color) => glow.color = color;

        void SetTint(Color color)
        {
            dieRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(ColorId, color);
            dieRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}
