using Microsoft.AspNetCore.SignalR.Client;
using FunBoardGames.Network.SignalR.Shared;
using System;
using System.Threading;

namespace FunBoardGames.Network.SignalR
{
    public class SignalRAuthHandler : IAuthHandler
    {
        public event Action<SignInResponse> LoginSuccess;
        public event Action<AuthenticationError> LoginFailed;

        HubConnection _connection; 
        SynchronizationContext unityContext;

        public SignalRAuthHandler(HubConnection connection)
        {
            unityContext = SynchronizationContext.Current;
            _connection = connection;
            _connection.On<AuthenticationResponseMessage>(AuthenticationMessageNames.SignIn, (loginMsg) =>
            {
                unityContext.Post(_ => OnLogin(loginMsg), null);
            });
        }

        private void OnLogin(AuthenticationResponseMessage msg)
        {
            if (msg.ErrorCode == AuthenticationErrorCode.None)
            {
                LoginSuccess?.Invoke(new SignInResponse()
                {
                    Profile = new ProfileData()
                    {
                        UserId = msg.Profile.UserId,
                        PlayerName = msg.Profile.PlayerName,
                        Avatar = msg.Profile.Avatar,
                    },
                    Token = msg.AuthToken,
                });
            }
            else 
            {
                AuthenticationError status = msg.ErrorCode switch
                {
                    AuthenticationErrorCode.InvalidCredentials => AuthenticationError.InvalidCredentials,
                    AuthenticationErrorCode.UserNotFound => AuthenticationError.InvalidCredentials,
                    AuthenticationErrorCode.UserAlreadyExists => AuthenticationError.TooManyAccounts,
                    _ => AuthenticationError.Unknown,
                };
                LoginFailed?.Invoke(status);
            }
        }

        public void SignUp(SignUpData signUpData)
        {
            _connection.InvokeAsync(AuthenticationMessageNames.SignUp, new SignUpRequestMessage()
            {
                PlayerName = signUpData.PlayerName,
                DeviceId = signUpData.DeviceId,
            });
        }

        public void SignIn(SignInData signInData)
        {
            _connection.InvokeAsync(AuthenticationMessageNames.SignIn, new SignInRequestMessage()
            {
                UserId = signInData.UserId,
                AuthToken = signInData.Password,
                DeviceId = signInData.DeviceId,
            });
        }
    }
}