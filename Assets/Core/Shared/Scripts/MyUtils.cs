using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace FunBoardGames
{
    public enum TripleComparisonResult : byte
    {
        ALL_SAME = 0,
        ALL_DIFFERRENT = 1,
        NONE = 2
    }

    public class MyUtils
    {
        public static byte[] GetRandomByteList(int n)
        {
            byte[] bytes = new byte[n];
            for (int i = 0; i < n; i++)
                bytes[i] = (byte)i;
            System.Random r = new System.Random();
            return bytes.OrderBy(x => r.Next()).ToArray();
        }

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
    }
}
