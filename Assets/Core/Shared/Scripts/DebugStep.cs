using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugStep
{
    static int step = 1;

    public static void Log(string msg){
        Debug.Log($"Step {step} ---- {msg}");
        step++;
    }
}
