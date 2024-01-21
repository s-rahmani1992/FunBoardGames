using UnityEngine;
using UnityEngine.UI;

namespace OnlineBoardGames
{
    [RequireComponent(typeof(GraphicRaycaster))]
    public class BaseDialog : MonoBehaviour
    {
        public Animator DialogAnimator { get; private set; }
        public Canvas canvas { get; private set; }
        public GraphicRaycaster rayCast { get; private set; }

        public System.Action<BaseDialog> OnSpawned;

        public float showTime, closeTime;

        protected virtual void Awake()
        {
            DialogAnimator = GetComponent<Animator>();
            rayCast = GetComponent<GraphicRaycaster>();
            canvas = GetComponent<Canvas>();
            rayCast.enabled = false;
        }

        public virtual void Show()
        {
            canvas.sortingLayerName = "Dialog";
            OnSpawned?.Invoke(this);
            if (DialogAnimator != null) DialogAnimator.SetTrigger("show");
        }

        public virtual void Close()
        {
            DialogManager.Instance.CloseDialog(this);
        }

        public virtual void OnClose() 
        {
            if (DialogAnimator != null) DialogAnimator.SetTrigger("close");
        }
    }
}
