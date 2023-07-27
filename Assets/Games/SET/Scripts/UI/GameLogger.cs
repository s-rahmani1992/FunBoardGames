using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace OnlineBoardGames.SET{
    public class GameLogger : MonoBehaviour
    {
        [SerializeField] Text statTxt;

        public void SetText(string text) => statTxt.text = text;
    }
}
