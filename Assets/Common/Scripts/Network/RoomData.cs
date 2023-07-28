using Mirror;
using System;

namespace OnlineBoardGames
{
    [Serializable]
    public class RoomData
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public int PlayerCount { get; private set; }

        public RoomData() { }

        public RoomData(BoardGameRoomManager room, Guid roomId)
        {
            Id = roomId;
            Name = room.roomName;
            PlayerCount = room.playerCount;
        }

        public RoomData(NetworkReader reader)
        {
            Id = reader.ReadGuid();
            Name = reader.ReadString();
            PlayerCount = reader.ReadByte();
        }
    }

    public static class RoomDataSerializer
    {
        public static RoomData ReadRoomData(this NetworkReader reader)
        {
            return new RoomData(reader);
        }

        public static void WriteRoomData(this NetworkWriter writer, RoomData roomData)
        {
            writer.WriteGuid(roomData.Id);
            writer.WriteString(roomData.Name);
            writer.WriteByte((byte)roomData.PlayerCount);
        }
    }
}
