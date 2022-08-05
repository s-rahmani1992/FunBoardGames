using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OnlineBoardGames
{
    public enum TripleComparisonResult : byte
    {
        ALL_SAME = 0,
        ALL_DIFFERRENT = 1,
        NONE = 2
    }

    public class MyUtils
    {
        public static TripleComparisonResult CompareItems<T>(T item1, T item2, T item3){
            if (item1.Equals(item2)){
                if (item3.Equals(item2)) return TripleComparisonResult.ALL_SAME;
                else return TripleComparisonResult.NONE;
            }
            else{
                if (item3.Equals(item1) || item3.Equals(item2)) return TripleComparisonResult.NONE;
                else return TripleComparisonResult.ALL_DIFFERRENT;
            }
        }

        public static void DelayAction(System.Action func, float delay, MonoBehaviour mono){
            mono.StartCoroutine(DelayAct(func, delay));
        }

        static IEnumerator DelayAct(System.Action func, float delay){
            yield return new WaitForSeconds(delay);
            func?.Invoke();
        }
    }
}
