using FunBoardGames.Network;
using UnityEngine;

[CreateAssetMenu(fileName = "UserResourceHolder", menuName = "Scriptable Objects/UserResourceHolder")]
public class UserBalanceResourceHolder : ScriptableObject
{
    [field: SerializeField] public ResourceName Name { get; private set; }

    public int Balance { get; private set; }

    public event System.Action<int, int> BalanceChanged;

    public void Reset()
    {
        Balance = 0;
    }

    public void UpdateBalance(UserResourceData resourceData)
    {
        int newBalance = resourceData.Balance ?? 0;
        if (newBalance == Balance)
            return;

        int oldBalance = Balance;
        Balance = newBalance;
        BalanceChanged?.Invoke(oldBalance, Balance);
    }
}
