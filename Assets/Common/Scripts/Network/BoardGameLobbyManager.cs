using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/guides/networkbehaviour
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

// NOTE: Do not put objects in DontDestroyOnLoad (DDOL) in Awake.  You can do that in Start instead.

namespace OnlineBoardGames
{
    [Serializable]
    public enum BoardGameTypes : byte
    {
        SET,
        ALL
    }

    [Serializable]
    public class SerializableRoom
    {
        public Guid id;
        public string roomName;
        public byte playerCount;
        public SerializableRoom() { }
        public SerializableRoom(IRoom room, Guid rID){
            id = rID;
            roomName = room.RoomName;
            playerCount = room.playerCount;
        }
    }

    public class BoardGameLobbyManager : NetworkBehaviour
    {
        Dictionary<Guid, IRoom> rooms = new Dictionary<Guid, IRoom>();

        [Server]
        Guid GenerateRoomID(){
            Guid newID = Guid.NewGuid();
            while(rooms.ContainsKey(newID))
                newID = Guid.NewGuid();
            return newID;
        }

        [Server]
        public T GetRoomManager<T>(Guid id) where T : NetworkBehaviour{
            return (rooms[id] as T);
        }

        #region Start & Stop Callbacks

        /// <summary>
        /// This is invoked for NetworkBehaviour objects when they become active on the server.
        /// <para>This could be triggered by NetworkServer.Listen() for objects in the scene, or by NetworkServer.Spawn() for objects that are dynamically created.</para>
        /// <para>This will be called for objects on a "host" as well as for object on a dedicated server.</para>
        /// </summary>
        public override void OnStartServer() {
            NetworkServer.RegisterHandler<CreateRoomMessage>(OnCreateRoomRequest);
            NetworkServer.RegisterHandler<AddPlayerForMatch>(OnAddPlayerForMatch);
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
                IRoom r = rooms[data.roomID];
                r.Remove(conn.identity.GetComponent<BoardGamePlayer>());
                conn.Send(new SceneMessage { sceneName = "Menu" });
                if(r.playerCount == 0)
                {
                    rooms.Remove(data.roomID);
                    NetworkServer.Destroy((r as NetworkBehaviour).gameObject);
                    (conn.authenticationData as AuthData).roomID = Guid.Empty;
                }
            }
        }

        private void OnJoinRoomRequest(NetworkConnectionToClient conn, JoinMatchMessage msg)
        {
            if (rooms.ContainsKey(msg.matchID))
            {
                (conn.authenticationData as AuthData).roomID = msg.matchID;
                conn.Send(new SceneMessage { sceneName = "Room" });
            }
        }

        private void OnRoomListRequest(NetworkConnectionToClient conn, GetRoomListMessage msg)
        {
            List<SerializableRoom> roomList = new List<SerializableRoom>();
            foreach(var room in rooms){
                if (room.Value.IsAcceptingPlayer && room.Value.playerCount < 4)
                    roomList.Add(new SerializableRoom(room.Value, room.Key));
            }
            conn.Send(new RoomListResponse { rooms = roomList.ToArray() });
        }

        private void OnAddPlayerForMatch(NetworkConnectionToClient conn, AddPlayerForMatch msg)
        {
            var id = (conn.authenticationData as AuthData).roomID;
            if(id != Guid.Empty)
            {
                var player = Instantiate(BoardGameNetworkManager.singleton.spawnPrefabs[2 * (byte)msg.gameType + 2]).GetComponent<BoardGamePlayer>();
                player.GetComponent<NetworkMatch>().matchId = id;
                NetworkServer.AddPlayerForConnection(conn, player.gameObject);
                NetworkServer.Spawn((rooms[id] as MonoBehaviour).gameObject);
                rooms[id].AddPlayer(player);
            }
        }

        private void OnCreateRoomRequest(NetworkConnectionToClient conn, CreateRoomMessage msg){
            {
                var newMatchID = GenerateRoomID();
                var room = Instantiate(BoardGameNetworkManager.singleton.spawnPrefabs[2 * (byte)msg.gameType + 1]).GetComponent<IRoom>();
                room.RoomName = msg.reqName;
                (room as MonoBehaviour).GetComponent<NetworkMatch>().matchId = newMatchID;
                (conn.authenticationData as AuthData).roomID = newMatchID;
                //NetworkServer.Spawn((room as MonoBehaviour).gameObject);
                rooms.Add(newMatchID, room);






                //var player = Instantiate(BoardGameNetworkManager.singleton.spawnPrefabs[2 * (byte)msg.gameType + 2]).GetComponent<BoardGamePlayer>();
                //player.GetComponent<NetworkMatch>().matchId = newMatchID;
                //NetworkServer.AddPlayerForConnection(conn, player.gameObject);
                //NetworkServer.Spawn((room as MonoBehaviour).gameObject);
                conn.Send(new SceneMessage { sceneName = "Room" });
                //NetworkServer.SetClientNotReady(conn);
                //room.AddPlayer(player);
            }
        }

        /// <summary>
        /// Invoked on the server when the object is unspawned
        /// <para>Useful for saving object data in persistent storage</para>
        /// </summary>
        public override void OnStopServer() { }

        /// <summary>
        /// Called on every NetworkBehaviour when it is activated on a client.
        /// <para>Objects on the host have this function called, as there is a local client on the host. The values of SyncVars on object are guaranteed to be initialized correctly with the latest state from the server when this function is called on the client.</para>
        /// </summary>
        public override void OnStartClient() { }

        /// <summary>
        /// This is invoked on clients when the server has caused this object to be destroyed.
        /// <para>This can be used as a hook to invoke effects or do client specific cleanup.</para>
        /// </summary>
        public override void OnStopClient() { }

        /// <summary>
        /// Called when the local player object has been set up.
        /// <para>This happens after OnStartClient(), as it is triggered by an ownership message from the server. This is an appropriate place to activate components or functionality that should only be active for the local player, such as cameras and input.</para>
        /// </summary>
        public override void OnStartLocalPlayer() { }

        /// <summary>
        /// Called when the local player object is being stopped.
        /// <para>This happens before OnStopClient(), as it may be triggered by an ownership message from the server, or because the player object is being destroyed. This is an appropriate place to deactivate components or functionality that should only be active for the local player, such as cameras and input.</para>
        /// </summary>
        public override void OnStopLocalPlayer() { }

        /// <summary>
        /// This is invoked on behaviours that have authority, based on context and <see cref="NetworkIdentity.hasAuthority">NetworkIdentity.hasAuthority</see>.
        /// <para>This is called after <see cref="OnStartServer">OnStartServer</see> and before <see cref="OnStartClient">OnStartClient.</see></para>
        /// <para>When <see cref="NetworkIdentity.AssignClientAuthority">AssignClientAuthority</see> is called on the server, this will be called on the client that owns the object. When an object is spawned with <see cref="NetworkServer.Spawn">NetworkServer.Spawn</see> with a NetworkConnectionToClient parameter included, this will be called on the client that owns the object.</para>
        /// </summary>
        public override void OnStartAuthority() { }

        /// <summary>
        /// This is invoked on behaviours when authority is removed.
        /// <para>When NetworkIdentity.RemoveClientAuthority is called on the server, this will be called on the client that owns the object.</para>
        /// </summary>
        public override void OnStopAuthority() { }

        #endregion
    }
}