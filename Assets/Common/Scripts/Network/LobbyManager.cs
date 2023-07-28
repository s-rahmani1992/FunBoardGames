using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using OnlineBoardGames.SET;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/guides/networkbehaviour
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

// NOTE: Do not put objects in DontDestroyOnLoad (DDOL) in Awake.  You can do that in Start instead.

namespace OnlineBoardGames
{
    public class LobbyManager : NetworkBehaviour
    {
        Dictionary<Guid, RoomManager> rooms = new();
        public event Action<RoomManager, NetworkConnectionToClient> RoomRequested;

        [Server]
        Guid GenerateRoomID()
        {
            Guid newID = Guid.NewGuid();
            while(rooms.ContainsKey(newID))
                newID = Guid.NewGuid();
            return newID;
        }

        /// <summary>
        /// This is invoked for NetworkBehaviour objects when they become active on the server.
        /// <para>This could be triggered by NetworkServer.Listen() for objects in the scene, or by NetworkServer.Spawn() for objects that are dynamically created.</para>
        /// <para>This will be called for objects on a "host" as well as for object on a dedicated server.</para>
        /// </summary>
        public override void OnStartServer() 
        {
            NetworkServer.RegisterHandler<CreateRoomMessage>(OnCreateRoomRequest);
            NetworkServer.RegisterHandler<GetRoomListMessage>(OnRoomListRequest);
            NetworkServer.RegisterHandler<JoinMatchMessage>(OnJoinRoomRequest);
            NetworkServer.RegisterHandler<PlayerReadyMessage>((conn, msg) => { rooms[(conn.authenticationData as AuthData).roomID].OnPlayerReady(conn, msg); }, false);
            NetworkServer.RegisterHandler<LeaveRoomMessage>(OnLeaveRoomRequest);
        }

        public void OnLeaveRoomRequest(NetworkConnectionToClient conn, LeaveRoomMessage msg)
        {
            var data = conn.authenticationData as AuthData;

            if(data.roomID != Guid.Empty)
            {
                RoomManager r = rooms[data.roomID];
                r.Remove(conn.identity.GetComponent<BoardGamePlayer>());
                conn.Send(new SceneMessage { sceneName = "Menu" });

                if(r.PlayerCount == 0)
                {
                    rooms.Remove(data.roomID);
                    NetworkServer.Destroy((r as NetworkBehaviour).gameObject);
                    (conn.authenticationData as AuthData).roomID = Guid.Empty;
                }
            }
        }

        void OnJoinRoomRequest(NetworkConnectionToClient conn, JoinMatchMessage msg)
        {
            if (rooms.TryGetValue(msg.matchID, out RoomManager room))
            {
                (conn.authenticationData as AuthData).roomID = msg.matchID;
                AddPlayerForMatch(conn, room);
            }
        }

        void OnRoomListRequest(NetworkConnectionToClient conn, GetRoomListMessage msg)
        {
            List<RoomData> roomList = new();

            foreach(var room in rooms)
            {
                if (room.Value.IsAcceptingPlayer && room.Value.PlayerCount < 4)
                    roomList.Add(new RoomData(room.Value, room.Key));
            }

            conn.Send(new RoomListResponse { rooms = roomList.ToArray() });
        }

        void AddPlayerForMatch(NetworkConnectionToClient conn, RoomManager room)
        {
            NetworkMatch match = room.GetComponent<NetworkMatch>();
            var player = Instantiate(GameNetworkManager.singleton.spawnPrefabs[2 * (byte)room.GameType + 3]).GetComponent<BoardGamePlayer>();
            player.GetComponent<NetworkMatch>().matchId = match.matchId;
            NetworkServer.AddPlayerForConnection(conn, player.gameObject);
            RoomRequested?.Invoke(room, conn);
            room.AddPlayer(player);
        }

        void OnCreateRoomRequest(NetworkConnectionToClient conn, CreateRoomMessage msg)
        {
            var newMatchID = GenerateRoomID();
            var room = Instantiate(GameNetworkManager.singleton.spawnPrefabs[2 * (byte)msg.gameType + 2]).GetComponent<RoomManager>();
            room.SetName(msg.reqName);
            room.GetComponent<NetworkMatch>().matchId = newMatchID;
            (conn.authenticationData as AuthData).roomID = newMatchID;
            rooms.Add(newMatchID, room);
            NetworkServer.Spawn(room.gameObject);
            AddPlayerForMatch(conn, room);
        }
    }
}