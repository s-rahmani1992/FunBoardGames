using UnityEngine;
using System;
using FishNet.Serializing;
using FishNet.Object;

namespace FunBoardGames.SET
{
    [Serializable]
    public class CardData
    {
        public byte Color { get; private set; }
        public byte Shape { get; private set; }
        public byte CountIndex { get; private set; }
        public byte Shading { get; private set; }
        private byte RawByte { get; set; }

        public CardData()
        {
            RawByte = 0b11111111;
            Color = Shape = CountIndex = Shading = 4;
        }

        public CardData(byte byteIn)
        {
            RawByte = byteIn;
            byte mask = 3;
            Color = (byte)(byteIn & mask);
            mask = 12;
            Shape = (byte)((byteIn & mask) >> 2);
            mask = 48;
            CountIndex = (byte)((byteIn & mask) >> 4);
            mask = 192;
            Shading = (byte)((byteIn & mask) >> 6);
        }

        public CardData(byte color, byte shape, byte count, byte shading)
        {
            Color = color;
            Shape = shape;
            CountIndex = count;
            Shading = shading;
            RawByte = (byte)((Color) | (Shape << 2) | (CountIndex << 4) | (Shading << 6));
        }

        [Server]
        public static bool isValid(byte cardByte)
        {
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
            if ((cardByte & mask) == 192)
                return false;
            return true;
        }

        public override string ToString()
        {
            return $"({Color}, {Shape}, {CountIndex}, {Shading})";
        }

        public override bool Equals(object obj)
        {
            CardData data = obj as CardData;
            return data.RawByte == RawByte;
        }

        public override int GetHashCode()
        {
            return RawByte;
        }

        public static byte CheckSET(CardData card1, CardData card2, CardData card3)
        {
            byte result = 0;
            result = (byte)(result | (byte)(MyUtils.CompareItems(card1.Color, card2.Color, card3.Color)));
            result = (byte)(result | (byte)(MyUtils.CompareItems(card1.Shape, card2.Shape, card3.Shape)) << 2);
            result = (byte)(result | (byte)(MyUtils.CompareItems(card1.CountIndex, card2.CountIndex, card3.CountIndex)) << 4);
            result = (byte)(result | (byte)(MyUtils.CompareItems(card1.Shading, card2.Shading, card3.Shading)) << 6);
            return result;
        }

        public static TripleComparisonResult[] Byte2Result(byte result)
        {
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
            if ((TripleComparisonResult)((result & mask) >> 6) == TripleComparisonResult.NONE) return false;
            return true;
        }
    }

    public static class CardDataSerializer
    {
        public static CardData ReadCardData(this Reader reader)
        {
            byte data = reader.ReadByte();
            byte mask = 3;
            byte color = (byte)(data & mask);
            mask = 12;
            byte shape = (byte)((data & mask) >> 2);
            mask = 48;
            byte countIndex = (byte)((data & mask) >> 4);
            mask = 192;
            byte shading = (byte)((data & mask) >> 6);
            return new CardData(color, shape, countIndex, shading);
        }

        public static void WriteCardData(this Writer writer, CardData cardData)
        {
            writer.WriteByte((byte)((cardData.Color) | (cardData.Shape << 2) | (cardData.CountIndex << 4) | (cardData.Shading << 6)));
        }
    }
}