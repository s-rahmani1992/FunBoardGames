using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    [SerializeField]
    float timeToCountdown;
    float t = 0;
    [SerializeField]
    Text timeTxt;

    // Start is called before the first frame update
    void Start()
    {
        t = timeToCountdown;
    }

    // Update is called once per frame
    void Update()
    {
        t -= Time.deltaTime;
        if(t < 0)
        {
            t = 0;
            enabled = false;
        }
        timeTxt.text = Mathf.FloorToInt(t).ToString();
    }
}
