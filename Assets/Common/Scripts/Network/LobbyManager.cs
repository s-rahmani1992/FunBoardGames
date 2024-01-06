using System.Collections.Generic;
using System;
using System.Collections.Concurrent;
using FishNet.Object;
using FishNet.Connection;
using FishNet.Managing.Server;
using UnityEngine;
using FishNet.Component.Observing;
using System.Threading;

namespace OnlineBoardGames
{
    public class LobbyManager : NetworkBehaviour
    {
        ServerManager serverManager;
        Dictionary<BoardGame, ConcurrentDictionary<int, RoomManager>> rooms = new();
        [SerializeField] MatchCondition matchCondition;
        [SerializeField] GamePrefabs prefabs;
        
        int lastId = 1;
        public event Action<RoomData[]> RoomListReceived; 
        public event Action<RoomManager> JoinedRoom;

        [Server]
        int GenerateRoomID()
        {
            int newID = lastId;
            Interlocked.Increment(ref lastId);
            return newID;
        }

        public override void OnStartServer() 
        {
            serverManager = NetworkManager.ServerManager;
            rooms.Add(BoardGame.SET, new());
            rooms.Add(BoardGame.CantStop, new());

            //if (GameNetworkManager.singleton.OverrideGame)
            //{
            //    GameNetworkManager m = GameNetworkManager.singleton;
            //    var newMatchID = GenerateRoomID();
            //    var TestRoom = Instantiate(m.spawnPrefabs[2 * (byte)m.GameType + 2]).GetComponent<RoomManager>();
            //    TestRoom.SetName("Test");
            //    TestRoom.GetComponent<NetworkMatch>().matchId = GameNetworkManager.TestGuid;
            //    rooms[m.GameType].TryAdd(GameNetworkManager.TestGuid, TestRoom);
            //    NetworkServer.Spawn(TestRoom.gameObject);
            //}
        }

        [ServerRpc(RequireOwnership = false)]
        public void CmdCreateRoom(BoardGame gameType, string name, NetworkConnection conn = null)
        {
            var newMatchID = GenerateRoomID();
            (conn.CustomData as AuthData).roomID = newMatchID;
            (conn.CustomData as AuthData).gameType = gameType;
            var room = Instantiate(prefabs[gameType].room.GetComponent<RoomManager>());
            room.SetParams(newMatchID, name);
            MatchCondition.AddToMatch(newMatchID, conn);
            MatchCondition.AddToMatch(newMatchID, room.GetComponent<NetworkObject>());
            rooms[gameType].TryAdd(newMatchID, room);
            serverManager.Spawn(room.gameObject);
            AddPlayerForMatch(conn, room);
        }

        [ServerRpc(RequireOwnership = false)]
        public void CmdJoinRoom(BoardGame gameType, int matchId, NetworkConnection conn = null)
        {
            if (rooms[gameType].TryGetValue(matchId, out RoomManager room))
            {
                (conn.CustomData as AuthData).roomID = matchId;
                (conn.CustomData as AuthData).gameType = gameType;
                MatchCondition.AddToMatch(room.Id, conn);
                AddPlayerForMatch(conn, room);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void CmdLeaveRoom(NetworkConnection conn = null)
        {
            var data = conn.CustomData as AuthData;

            if(data.roomID != -1)
            {
                RoomManager r = rooms[data.gameType][data.roomID];
                r.Remove(conn.FirstObject.GetComponent<BoardGamePlayer>());
                MatchCondition.RemoveFromMatch(r.Id, conn);

                if(r.PlayerCount == 0)
                {
                    rooms[data.gameType].TryRemove(data.roomID, out r);
                    serverManager.Despawn(r.NetworkObject);
                    (conn.CustomData as AuthData).roomID = -1;
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void CmdGetRoomList(BoardGame gameType, NetworkConnection conn = null)
        {
            List<RoomData> roomList = new();

            foreach(var room in rooms[gameType])
            {
                if (room.Value.IsAcceptingPlayer && room.Value.PlayerCount < 4)
                    roomList.Add(new RoomData(room.Value, room.Key));
            }

            RpcSendRoomList(conn, roomList.ToArray());
        }

        [Server]
        void AddPlayerForMatch(NetworkConnection conn, RoomManager room)
        {
            var player = Instantiate(prefabs[room.GameType].player);
            MatchCondition.AddToMatch(room.Id, conn);
            conn.Broadcast(new AuthSyncMessage { authData = conn.CustomData as AuthData });
            serverManager.Spawn(player.NetworkObject, conn);
            room.AddPlayer(player);
            RpcSendRoom(conn, room);
        }

        [TargetRpc]
        void RpcSendRoomList(NetworkConnection _, RoomData[] rooms)
        {
            RoomListReceived?.Invoke(rooms);
        }

        [TargetRpc]
        void RpcSendRoom(NetworkConnection _, RoomManager roomManager)
        {
            JoinedRoom?.Invoke(roomManager);
        }
    }
}