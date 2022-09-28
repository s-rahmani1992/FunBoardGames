using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseDialog : MonoBehaviour
{
    public Animator DialogAnimator { get; private set; }
    public Canvas canvas { get; private set; }
    public GraphicRaycaster rayCast { get; private set; }

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
        if (DialogAnimator != null) DialogAnimator.SetTrigger("show");
    }

    public virtual void Close()
    {
        if (DialogAnimator != null) DialogAnimator.SetTrigger("close");
    }
}
