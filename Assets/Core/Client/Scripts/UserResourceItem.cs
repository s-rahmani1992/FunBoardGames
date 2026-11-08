using TMPro;
using UnityEngine;

namespace FunBoardGames.Client
{
    public class UserResourceItem : MonoBehaviour
    {
        [SerializeField] UserBalanceResourceHolder resourceHolder;
        [SerializeField] TMP_Text balanceText;

        void OnEnable()
        {
            resourceHolder.BalanceChanged += OnBalanceChanged;
            Refresh();
        }

        void OnDisable()
        {
            resourceHolder.BalanceChanged -= OnBalanceChanged;
        }

        void OnBalanceChanged(int oldBalance, int newBalance)
        {
            Refresh();
        }

        void Refresh()
        {
            balanceText.text = resourceHolder.Balance.ToString();
        }
    }
}
