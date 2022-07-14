using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

/*
    Documentation: https://mirror-networking.gitbook.io/docs/components/network-authenticators
    API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkAuthenticator.html
*/

public class PlayerInfo
{
    public string playerName;
}


public class SimpleNameAuthenticator : NetworkAuthenticator
{
    #region Developer

    public string reqName;
    public HashSet<string> playerNames { get; protected set; } = new HashSet<string>();
    public PlayerInfo playerProfile { get; private set; }

    public void RemoveName(string n){
        playerNames.Remove(n);
    }
    #endregion


    #region Messages

    public struct AuthRequestMessage : NetworkMessage {
        public string requestedName;
    }

    public struct AuthResponseMessage : NetworkMessage {
        public byte resultCode;
        public string message;
    }

    #endregion

    #region Server

    /// <summary>
    /// Called on server from StartServer to initialize the Authenticator
    /// <para>Server message handlers should be registered in this method.</para>
    /// </summary>
    public override void OnStartServer(){
        DebugStep.Log("SimpleNameAuthenticator.OnStartServer");
        // register a handler for the authentication request we expect from client
        NetworkServer.RegisterHandler<AuthRequestMessage>(OnAuthRequestMessage, false);
    }

    /// <summary>
    /// Called on server from OnServerAuthenticateInternal when a client needs to authenticate
    /// </summary>
    /// <param name="conn">Connection to client.</param>
    public override void OnServerAuthenticate(NetworkConnectionToClient conn) {
        DebugStep.Log($"SimpleNameAuthenticator.OnServerAuthenticate({conn.connectionId})");
    }

    /// <summary>
    /// Called on server when the client's AuthRequestMessage arrives
    /// </summary>
    /// <param name="conn">Connection to client.</param>
    /// <param name="msg">The message payload</param>
    public void OnAuthRequestMessage(NetworkConnectionToClient conn, AuthRequestMessage msg){
        AuthResponseMessage authResponseMessage = new AuthResponseMessage();
        if (msg.requestedName.Contains("%")){
            authResponseMessage.resultCode = 1;
            authResponseMessage.message = "Player name cannot contain '%' character!";
        }

        else if (playerNames.Add(msg.requestedName)){
            conn.authenticationData = msg.requestedName;
            authResponseMessage.resultCode = 0;
            authResponseMessage.message = "Authentication Success";
            ServerAccept(conn);
        }

        else{
            authResponseMessage.resultCode = 1;
            authResponseMessage.message = $"Player with name '{msg.requestedName}' already exists!";
        }
        conn.Send(authResponseMessage);
    }

    #endregion

    #region Client

    /// <summary>
    /// Called on client from StartClient to initialize the Authenticator
    /// <para>Client message handlers should be registered in this method.</para>
    /// </summary>
    public override void OnStartClient(){
        DebugStep.Log($"SimpleNameAuthenticator.OnStartClient()");
        // register a handler for the authentication response we expect from server
        NetworkClient.RegisterHandler<AuthResponseMessage>(OnAuthResponseMessage, false);
    }

    /// <summary>
    /// Called on client from OnClientAuthenticateInternal when a client needs to authenticate
    /// </summary>
    public override void OnClientAuthenticate(){
        DebugStep.Log($"SimpleNameAuthenticator.OnClientAuthenticate()");
        AuthRequestMessage authRequestMessage = new AuthRequestMessage { requestedName = reqName };

        NetworkClient.Send(authRequestMessage);
    }

    /// <summary>
    /// Called on client when the server's AuthResponseMessage arrives
    /// </summary>
    /// <param name="msg">The message payload</param>
    public void OnAuthResponseMessage(AuthResponseMessage msg){
        // Authentication has been accepted
        if (msg.resultCode != 0)
        {
            ///TODO Refresh UI
            reqName = null;
            playerProfile = null;
            FindObjectOfType<LoginUI>().LogMsg(msg.message);
            ClientReject();
        }
        else
        {
            playerProfile = new PlayerInfo { playerName = reqName };
            ClientAccept();
        }
    }

    #endregion
}
