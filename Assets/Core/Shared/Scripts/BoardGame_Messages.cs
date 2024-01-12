using FishNet.Broadcast;

namespace OnlineBoardGames
{
    public struct AuthRequestMessage : IBroadcast
    {
        public string requestedName;
    }

    public struct AuthResponseMessage : IBroadcast
    {
        public byte resultCode;
        public string message;
    }

    public struct AuthSyncMessage : IBroadcast
    {
        public AuthData authData;
    }
}
