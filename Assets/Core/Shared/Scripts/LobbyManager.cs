using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;

using FishNet.Object;
using FishNet.Transporting;
using FishNet.Connection;
using FishNet.Managing.Server;
using FishNet.Component.Observing;

using UnityEngine;

namespace FunBoardGames
{
    public class LobbyManager : NetworkBehaviour
    {
        [SerializeField] MatchCondition matchCondition;
        [SerializeField] GamePrefabs prefabs;

        public event Action<RoomData[]> RoomListReceived;
        public event Action<RoomManager> JoinedRoom;

        ServerManager serverManager;
        Dictionary<BoardGame, ConcurrentDictionary<int, RoomManager>> rooms = new();
        int lastId = 1;

        #region Server Part

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
            serverManager.OnRemoteConnectionState += OnClientConnectionStateChanged;
            var directGameContainer = DirectGameContainer.Instance;

            if (directGameContainer.IsDirectGameActive)
            {
                var TestRoom = Instantiate(prefabs[directGameContainer.Game].room.GetComponent<RoomManager>());
                MatchCondition.AddToMatch(directGameContainer.TestRoomId, TestRoom.NetworkObject);
                rooms[directGameContainer.Game].TryAdd(directGameContainer.TestRoomId, TestRoom);
                serverManager.Spawn(TestRoom.gameObject);
            }
        }

        private void OnClientConnectionStateChanged(NetworkConnection conn, RemoteConnectionStateArgs e)
        {
            if(e.ConnectionState == RemoteConnectionState.Stopped)
            {
                LeavePlayer(conn);
            }
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
            LeavePlayer(conn);
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

        [Server]
        void LeavePlayer(NetworkConnection conn)
        {
            var data = conn.CustomData as AuthData;

            if (data.roomID != -1)
            {
                RoomManager r = rooms[data.gameType][data.roomID];
                r.Remove(conn.FirstObject.GetComponent<BoardGamePlayer>());
                MatchCondition.RemoveFromMatch(r.Id, conn);

                if (r.PlayerCount == 0)
                {
                    rooms[data.gameType].TryRemove(data.roomID, out r);
                    serverManager.Despawn(r.NetworkObject);
                    (conn.CustomData as AuthData).roomID = -1;
                }
            }
        }

        #endregion

        #region Client

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

        #endregion
    }
}