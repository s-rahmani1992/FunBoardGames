using FishNet.Serializing;
using System;

namespace FunBoardGames
{
    [Serializable]
    public class RoomData
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public int PlayerCount { get; private set; }
        public BoardGame GameType { get; private set; }

        public RoomData() { }

        public RoomData(RoomManager room, int roomId)
        {
            Id = roomId;
            Name = room.Name;
            PlayerCount = room.PlayerCount;
            GameType = room.GameType;
        }

        public RoomData(Reader reader)
        {
            Id = reader.ReadInt32();
            Name = reader.ReadString();
            PlayerCount = reader.ReadByte();
            GameType = (BoardGame)reader.ReadByte();
        }
    }

    public static class RoomDataSerializer
    {
        public static RoomData ReadRoomData(this Reader reader)
        {
            return new RoomData(reader);
        }

        public static void WriteRoomData(this Writer writer, RoomData roomData)
        {
            writer.WriteInt32(roomData.Id);
            writer.WriteString(roomData.Name);
            writer.WriteByte((byte)roomData.PlayerCount);
            writer.WriteByte((byte)roomData.GameType);
        }
    }
}
