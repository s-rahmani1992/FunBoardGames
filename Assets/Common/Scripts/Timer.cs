using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OnlineBoardGames
{
    public class Timer : MonoBehaviour
    {
        [SerializeField]
        Text timeTxt;
        Coroutine countDown = null;

        public void StartCountdown(float seconds, bool force = true){
            if (force){
                Stop();
                countDown = StartCoroutine(CountDown(seconds));
            }
            else if (countDown == null)
                countDown = StartCoroutine(CountDown(seconds));
        }

        public void Stop(){
            if (countDown != null){
                StopCoroutine(countDown);
                countDown = null;
            }
            timeTxt.text = "";
        }

        IEnumerator CountDown(float seconds){
            float t = seconds;
            while (t > 0){
                t -= Time.deltaTime;
                timeTxt.text = Mathf.FloorToInt(t).ToString();
                yield return null;
            }
            t = 0;
            timeTxt.text = "";
            countDown = null;
        }
    }
}