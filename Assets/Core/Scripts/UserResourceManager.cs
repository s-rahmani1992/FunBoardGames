using FunBoardGames.Network;
using System.Collections.Generic;
using UnityEngine;

public class UserResourceManager : ScriptableObject
{
    [SerializeField] UserBalanceResourceHolder[] _balanceResourceHolders;

    readonly Dictionary<ResourceName, UserBalanceResourceHolder> _balanceResourceHoldersByName = new();
    IResourceHandler _resourceHandler;

    public void Register(IResourceHandler resourceHandler)
    {
        Unregister();

        _balanceResourceHoldersByName.Clear();
        foreach (var resourceHolder in _balanceResourceHolders)
        {
            resourceHolder.Reset();
            _balanceResourceHoldersByName[resourceHolder.Name] = resourceHolder;
        }

        _resourceHandler = resourceHandler;
        _resourceHandler.UserResourceReceived += OnUserResourceReceived;
        _resourceHandler.ResourceUpdated += OnResourceUpdated;
    }

    void Unregister()
    {
        if (_resourceHandler == null)
            return;

        _resourceHandler.UserResourceReceived -= OnUserResourceReceived;
        _resourceHandler.ResourceUpdated -= OnResourceUpdated;
        _resourceHandler = null;
    }

    void OnUserResourceReceived(IEnumerable<UserResourceData> userResources)
    {
        foreach (var resourceHolder in _balanceResourceHolders)
            resourceHolder.Reset();

        foreach (var userResource in userResources)
            OnResourceUpdated(userResource);
    }

    void OnResourceUpdated(UserResourceData userResource)
    {
        if (_balanceResourceHoldersByName.TryGetValue(userResource.Resource.Name, out var resourceHolder))
            resourceHolder.UpdateBalance(userResource);
    }

    void OnDisable()
    {
        Unregister();
    }
}
