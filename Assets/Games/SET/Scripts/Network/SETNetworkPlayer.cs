using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using UnityEngine.UI;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/guides/networkbehaviour
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/



// NOTE: Do not put objects in DontDestroyOnLoad (DDOL) in Awake.  You can do that in Start instead.
namespace OnlineBoardGames.SET
{
    public struct PlayerData
    {
        public string name;
        public int wrong;
        public int correct;
        public bool isGuessing;
    }

    [Serializable]
    public enum VoteStat
    {
        NULL = 0,
        NO = 1,
        YES = 2
    }

    public class SETNetworkPlayer : NetworkBehaviour
    {
        #region Syncvars
        [SyncVar]
        public string playerName;

        [SyncVar( hook = nameof(PlayerDataChanged))]
        public byte wrongs;

        [SyncVar(hook = nameof(PlayerDataChanged))]
        public byte corrects;

        [SyncVar(hook = nameof(OnGuessChanged))]
        public bool isGuessing;

        [SyncVar(hook = nameof(OnVoteChanged))]
        public VoteStat voteState;

        [SyncVar(hook = nameof(IndexChanged))]
        public int playerNumber;
        #endregion

        #region Syncvar Hooks
        void IndexChanged(int oldVal, int newVal){
            Debug.Log($"IndexChanged({oldVal}, {newVal})");
            if (newVal < 1)
                return;
            RefreshUI();
        }

        void PlayerDataChanged(byte oldVal, byte newVal){
            RefreshUI();
        }

        void OnGuessChanged(bool oldVal, bool newVal){
            if (!oldVal && newVal){
                UIManager.AlertGuess();
                if (hasAuthority)
                    GameUIEventManager.OnCommonOrLocalStateEvent?.Invoke(UIStates.GuessBegin);
                else
                    GameUIEventManager.OnOtherStateEvent?.Invoke(UIStates.GuessBegin, playerName);
            }
            RefreshUI();
        }

        void OnVoteChanged(VoteStat oldVal, VoteStat newVal){
            playerUI?.RefreshVote(newVal);
        }

        #endregion

        [SerializeField]
        PlayerUI playerUI = null;

        [SerializeField]
        SETGameUIManager UIManager;

        private void Awake(){
            UIManager = FindObjectOfType<SETGameUIManager>();
        }

        

        #region Start & Stop Callbacks

        /// <summary>
        /// This is invoked for NetworkBehaviour objects when they become active on the server.
        /// <para>This could be triggered by NetworkServer.Listen() for objects in the scene, or by NetworkServer.Spawn() for objects that are dynamically created.</para>
        /// <para>This will be called for objects on a "host" as well as for object on a dedicated server.</para>
        /// </summary>
        public override void OnStartServer(){
            DebugStep.Log($"NetworkBehaviour<{connectionToClient.connectionId}>.OnstartServer()");
            playerName = connectionToClient.authenticationData as string;
            SETNetworkManager.singleton.session.AddPlayer(this);
            corrects = wrongs = 0;
            voteState = VoteStat.NULL;
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
        public override void OnStartClient() {
        }

        [Client]
        public void RefreshUI(){
            playerUI = UIManager.playerPanel.GetChild(playerNumber - 1).GetComponent<PlayerUI>();
            playerUI?.RefreshUI(new PlayerData { name = (hasAuthority ? "You" : playerName), correct = corrects, wrong = wrongs, isGuessing = isGuessing} );
        }

        /// <summary>
        /// This is invoked on clients when the server has caused this object to be destroyed.
        /// <para>This can be used as a hook to invoke effects or do client specific cleanup.</para>
        /// </summary>
        public override void OnStopClient(){
            playerUI.PlayerLeft();
        }

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