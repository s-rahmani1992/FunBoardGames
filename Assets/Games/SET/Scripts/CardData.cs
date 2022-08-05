using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OnlineBoardGames.SET
{
    [System.Serializable]
    public class CardData
    {
        public byte color;
        public byte shape;
        public byte shading;
        public byte count;
        public byte RawByte { get; private set; }

        public CardData(byte byteIn){
            RefreshData(byteIn);
        }

        public void RefreshData(byte cardByte){
            RawByte = cardByte;
            byte mask = 3;
            color = (byte)(cardByte & mask);

            mask = 12;
            shape = (byte)((cardByte & mask) >> 2);

            mask = 48;
            shading = (byte)((cardByte & mask) >> 4);

            mask = 192;
            count = (byte)((cardByte & mask) >> 6);
        }

        public static bool isValid(byte cardByte){
            byte mask = 3;
            if ((cardByte & mask) == 3)
                return false;

            mask = 12;
            if ((cardByte & mask) == 12)
                return false;

            mask = 48;
            if ((cardByte & mask) == 48)
                return false;

            mask = 192;
            if ((cardByte & mask) == 0)
                return false;
            return true;
        }

        public override string ToString(){
            return $"({color}, {shape}, {shading}, {count})";
        }

        public static byte CheckSET(byte card1, byte card2, byte card3){
            byte result = 0, mask = 3;
            result = (byte)(result | (byte)(MyUtils.CompareItems(card1 & mask, card2 & mask, card3 & mask)));
            mask = 12;
            result = (byte)(result | (byte)(MyUtils.CompareItems(card1 & mask, card2 & mask, card3 & mask)) << 2);
            mask = 48;
            result = (byte)(result | (byte)(MyUtils.CompareItems(card1 & mask, card2 & mask, card3 & mask)) << 4);
            mask = 192;
            result = (byte)(result | (byte)(MyUtils.CompareItems(card1 & mask, card2 & mask, card3 & mask)) << 6);
            return result;
        }

        public static TripleComparisonResult[] Byte2Result(byte result){
            TripleComparisonResult[] g = new TripleComparisonResult[4];
            byte mask = 3;
            g[0] = (TripleComparisonResult)(result & mask);

            mask = 12;
            g[1] = (TripleComparisonResult)((result & mask) >> 2);

            mask = 48;
            g[2] = (TripleComparisonResult)((result & mask) >> 4);

            mask = 192;
            g[3] = (TripleComparisonResult)((result & mask) >> 6);
            return g;
        }

        public static bool IsSET(byte result){
            byte mask = 3;
            if ((TripleComparisonResult)(result & mask) == TripleComparisonResult.NONE) return false;

            mask = 12;
            if ((TripleComparisonResult)((result & mask) >> 2) == TripleComparisonResult.NONE) return false;

            mask = 48;
            if ((TripleComparisonResult)((result & mask) >> 4) == TripleComparisonResult.NONE) return false;

            mask = 192;
            if ((TripleComparisonResult)((result & mask) >> 4) == TripleComparisonResult.NONE) return false;
            return true;
        }
    }
}