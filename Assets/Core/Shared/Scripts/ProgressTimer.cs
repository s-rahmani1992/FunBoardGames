using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FunBoardGames
{
    /// <summary>
    /// Shows the remaining time of a countdown as a filled progress bar, updated every frame.
    /// </summary>
    public class ProgressTimer : MonoBehaviour
    {
        [SerializeField] Image fillImage;
        [SerializeField] TMP_Text timeTxt;

        float duration;
        float remaining;
        bool isRunning = false;

        public bool IsRunning => isRunning;

        /// <param name="duration">Full length of the countdown in seconds.</param>
        /// <param name="elapsed">Seconds that already passed, e.g. while the round was running on the server.</param>
        public void StartTimer(float duration, float elapsed = 0f)
        {
            this.duration = Mathf.Max(duration, 0.01f);
            remaining = Mathf.Clamp(duration - elapsed, 0f, duration);
            isRunning = remaining > 0f;
            UpdateUI();
        }

        public void Clear()
        {
            isRunning = false;
            remaining = 0f;
            UpdateUI();
        }

        private void Update()
        {
            if (isRunning == false)
                return;

            remaining = Mathf.Max(remaining - Time.deltaTime, 0f);
            isRunning = remaining > 0f;
            UpdateUI();
        }

        void UpdateUI()
        {
            fillImage.fillAmount = duration > 0f ? remaining / duration : 0f;

            if (timeTxt != null)
                timeTxt.text = Mathf.CeilToInt(remaining).ToString();
        }
    }
}
