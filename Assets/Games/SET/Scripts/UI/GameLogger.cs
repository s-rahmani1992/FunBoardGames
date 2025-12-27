using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace FunBoardGames.SET{
    public class GameLogger : MonoBehaviour
    {
        [SerializeField] Text statTxt;

        Tween currentLogTween;

        public void SetText(string text) => statTxt.text = text;

        public void Toast(string text, float duration = 3)
        {
            currentLogTween?.Kill();
            statTxt.text = text;
            currentLogTween = DOVirtual.DelayedCall(duration, () => statTxt.text = "");
        }
    }
}
