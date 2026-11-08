using FunBoardGames.Network.SignalR.Shared;
using FunBoardGames.SignalR.Shared;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SharedResource = FunBoardGames.Network.SignalR.Shared.Resource;
using SharedResourceType = FunBoardGames.Network.SignalR.Shared.ResourceType;

namespace FunBoardGames.Network.SignalR
{
    public class SignalRUserHandler : IUserHandler, IResourceHandler
    {
        HubConnection _connection;
        SynchronizationContext unityContext;

        // The server identifies a resource by its permanent key, the client by this enum
        static readonly Dictionary<string, ResourceName> ResourceNamesByKey = new()
        {
            ["xp"] = ResourceName.xp,
            ["gem"] = ResourceName.gem,
            ["set_hint_card"] = ResourceName.setCardHint,
            ["cantstop_reset_round"] = ResourceName.cantStopResetRound,
            ["cantstop_bust_saver"] = ResourceName.cantStopBustSaver,
        };

        readonly Dictionary<int, ResourceData> _resourcesById = new();

        public IEnumerable<ResourceData> Resources => _resourcesById.Values;
        public event Action<IEnumerable<UserResourceData>> UserResourceReceived;
        public event Action<UserResourceData> ResourceUpdated;

        public SignalRUserHandler(HubConnection connection)
        {
            unityContext = SynchronizationContext.Current;
            _connection = connection;

            _connection.On<GetUserDataResponseMessage>(UserMessageNames.GetUserData, (response) =>
            {
                unityContext.Post(_ => OnUserDataReceived(response), null);
            });

            _connection.On<UserResourcesUpdateMessage>(UserMessageNames.UserResourcesUpdate, (message) =>
            {
                unityContext.Post(_ => OnUserResourcesUpdated(message), null);
            });

            _connection.On<Profile>(UserMessageNames.ProfileUpdate, (profile) =>
            {
                unityContext.Post(_ => ProfileUpdated?.Invoke(new ProfileData()
                {
                    UserId = profile.UserId,
                    PlayerName = profile.PlayerName,
                    Avatar = profile.Avatar,
                }), null);
            });
        }

        private ResourceData ConvertResource(SharedResource resource)
        {
            if (_resourcesById.TryGetValue(resource.Id, out var known))
                return known;

            // A resource added on the server that this client version does not know is ignored
            if (ResourceNamesByKey.TryGetValue(resource.Key, out var name) == false)
            {
                UnityEngine.Debug.LogWarning($"Unknown resource key '{resource.Key}' was ignored.");
                return null;
            }

            var data = new ResourceData
            {
                Id = resource.Id,
                Type = resource.Type == SharedResourceType.Subscription ? ResourceType.Subscription : ResourceType.Amount,
                Name = name,
            };
            _resourcesById[resource.Id] = data;
            return data;
        }

        private List<UserResourceData> ConvertUserResources(IEnumerable<UserResource> userResources)
        {
            var converted = new List<UserResourceData>();
            foreach (var userResource in userResources)
            {
                var resource = ConvertResource(userResource.Resource);
                if (resource == null)
                    continue;

                converted.Add(new UserResourceData
                {
                    Resource = resource,
                    Balance = userResource.Balance,
                    ExpireDate = userResource.ExpireDate,
                });
            }
            return converted;
        }

        private void OnUserResourcesUpdated(UserResourcesUpdateMessage message)
        {
            foreach (var userResource in ConvertUserResources(message.UserResources))
                ResourceUpdated?.Invoke(userResource);
        }

        private void OnUserDataReceived(GetUserDataResponseMessage response)
        {
            foreach (var resource in response.Resources)
                ConvertResource(resource);

            // Resources go out first, so the balances are ready by the time the menu loads
            UserResourceReceived?.Invoke(ConvertUserResources(response.UserResources));

            var games = response.Games
                .Select(ConvertGameDto)
                .ToList();

            UserGameDataReceived?.Invoke(new UserGameData()
            {
                Games = games,
            });
        }

        private static GameData ConvertGameDto(BoardGameData gameDto)
        {
            return gameDto switch
            {
                SETGameData setGameDto => new SETGame()
                {
                    Id = gameDto.Id,
                    Name = gameDto.Name,
                    Type = BoardGame.SET,
                    NumberofPlayers = gameDto.PlayerCount,
                    StartCardCount = setGameDto.VisibleCardCount,
                    HintLimit = setGameDto.HintLimit,
                    RoundTime = setGameDto.RoundTime,
                    WrongLimit = setGameDto.WrongLimit,
                    AttributeCount = (setGameDto.ShapeAttribute != null ? 0 : 1) +
                                     (setGameDto.ColorAttribute != null ? 0 : 1) +
                                     (setGameDto.CountAttribute != null ? 0 : 1) +
                                     (setGameDto.ShadingAttribute != null ? 0 : 1)
                },
                CantStopGameData cantStopGameDto => new CantStopGame()
                {
                    Id = gameDto.Id,
                    Name = gameDto.Name,
                    Type = BoardGame.CantStop,
                    NumberofPlayers = gameDto.PlayerCount,
                    BoardData = new CantStopBoardData(cantStopGameDto.BoardData),
                    MarkerConeCount = cantStopGameDto.MarkerConeCount,
                    RoundTime = cantStopGameDto.RoundTime,
                    TimeoutLimit = cantStopGameDto.TimeoutLimit,
                    WinConeCount = cantStopGameDto.WinConeCount,
                },
                _ => new GameData(),
            };
        }

        public event Action<UserGameData> UserGameDataReceived;
        public event Action<ProfileData> ProfileUpdated;

        public void GetUserData()
        {
            _connection.InvokeAsync(UserMessageNames.GetUserData);
        }

        public void ChangePlayerName(string newName)
        {
            _connection.InvokeAsync(UserMessageNames.ChangePlayerName, newName);
        }

        public void ChangeAvatar(int newAvatarId)
        {
            _connection.InvokeAsync(UserMessageNames.ChangePlayerAvatar, newAvatarId);
        }
    }
}
