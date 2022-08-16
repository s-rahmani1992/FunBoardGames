using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/guides/networkbehaviour
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

// NOTE: Do not put objects in DontDestroyOnLoad (DDOL) in Awake.  You can do that in Start instead.
namespace OnlineBoardGames {
    public interface IRoom
    {
        byte playerCount { get; }
        bool IsAcceptingPlayer { get; }
        string RoomName { get; set; }
        void AddPlayer(BoardGamePlayer player);
        void Remove(BoardGamePlayer player, bool canRefreshIndices = true);
        void OnPlayerReady(NetworkConnectionToClient conn, PlayerReadyMessage msg);
    }

    public abstract class BoardGameRoomManager<T> : NetworkBehaviour, IRoom where T: BoardGamePlayer
    {
        protected T host;
        protected List<T> roomPlayers = new List<T>();
        int readyCount;

        [SyncVar]
        public string roomName;

        bool _acceptPlayer;

        protected virtual void Awake(){
            DontDestroyOnLoad(gameObject);
        }


        #region Virtual RPCClients
        protected virtual void RPCAllPlayersReady(){
            SingletonUIHandler.GetInstance<RoomUIEventHandler>()?.OnAllPlayersReady();
        }

        protected virtual void RPCPlayerJoin(NetworkIdentity identity){
            if(!identity.hasAuthority)
                SingletonUIHandler.GetInstance<RoomUIEventHandler>()?.OnOtherPlayerJoined?.Invoke(identity.GetComponent<BoardGamePlayer>().playerName);
        }

        protected virtual void RPCPlayerLeave(NetworkIdentity identity){
            if (!identity.hasAuthority)
                SingletonUIHandler.GetInstance<RoomUIEventHandler>()?.OnOtherPlayerLeft?.Invoke(identity.GetComponent<BoardGamePlayer>().playerName);
        }
        #endregion

        #region Message Handlers
        void IRoom.OnPlayerReady(NetworkConnectionToClient conn, PlayerReadyMessage msg){
            conn.identity.GetComponent<BoardGamePlayer>().isReady = true;
            readyCount++;
            if (playerCount >= 2 && readyCount == playerCount){
                RPCAllPlayersReady();
                _acceptPlayer = false;
                MyUtils.DelayAction(() => {
                    NetworkServer.SendToReadyObservers(conn.identity, new SceneMessage { sceneName = "Game" });
                }, 1, this);
                MyUtils.DelayAction(() => {
                    BeginGame();
                }, 1.2f, this);
            }
        }

        #endregion

        #region Start & Stop Callbacks

        /// <summary>
        /// This is invoked for NetworkBehaviour objects when they become active on the server.
        /// <para>This could be triggered by NetworkServer.Listen() for objects in the scene, or by NetworkServer.Spawn() for objects that are dynamically created.</para>
        /// <para>This will be called for objects on a "host" as well as for object on a dedicated server.</para>
        /// </summary>
        public override void OnStartServer() {
            _acceptPlayer = true;
        }

        protected abstract void BeginGame();

        /// <summary>
        /// Invoked on the server when the object is unspawned
        /// <para>Useful for saving object data in persistent storage</para>
        /// </summary>
        public override void OnStopServer() { }

        /// <summary>
        /// Called on every NetworkBehaviour when it is activated on a client.
        /// <para>Objects on the host have this function called, as there is a local client on the host. The values of SyncVars on object are guaranteed to be initialized correctly with the latest state from the server when this function is called on the client.</para>
        /// </summary>
        public override void OnStartClient() {

            DebugStep.Log("NetworkRoomManager.OnStartClient()");
            //BoardGameNetworkManager.singleton.session = this;
        }

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

        #region interface
        public byte playerCount => (byte)roomPlayers.Count;
        public bool IsAcceptingPlayer => _acceptPlayer;
        public string RoomName { get => roomName; set { roomName = value; } }

        [Server]
        public void AddPlayer(BoardGamePlayer player){
            roomPlayers.Add((T)player);
            RPCPlayerJoin(player.netIdentity);
            for (int i = 0; i < roomPlayers.Count; i++)
                roomPlayers[i].playerIndex = (byte)(i + 1);
        }

        [Server]
        public void Remove(BoardGamePlayer player, bool canRefreshIndices = true){
            if (roomPlayers.Remove((T)player)){
                RPCPlayerLeave(player.netIdentity);
                if (player.isReady)
                    readyCount--;
                if (canRefreshIndices){
                    for (int i = 0; i < playerCount; i++)
                        roomPlayers[i].playerIndex = (byte)(i + 1);
                }
                NetworkServer.RemovePlayerForConnection(player.connectionToClient, true);
            }
        }
        #endregion
    }
}
