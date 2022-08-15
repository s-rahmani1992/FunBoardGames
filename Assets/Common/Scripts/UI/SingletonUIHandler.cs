using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonUIHandler
{
    static MonoBehaviour _instance = null;
    public static T GetInstance<T>() where T : MonoBehaviour { return _instance as T; }
    public static void SetInstance<T>(T handler) where T : MonoBehaviour { _instance = handler; }
}
