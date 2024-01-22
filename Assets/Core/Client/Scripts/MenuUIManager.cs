using FishNet.Managing;
using FishNet.Managing.Client;
using System.Linq;
using TMPro;
using UnityEngine;

namespace OnlineBoardGames.Client
{
    public class MenuUIManager : MonoBehaviour
    {
        [SerializeField] TMP_Text playerNameText;

        ClientManager clientManager;

        // Start is called before the first frame update
        void Start()
        {
            clientManager = NetworkManager.Instances.ElementAt(0).ClientManager;
            playerNameText.text = (clientManager.Connection.CustomData as AuthData).playerName;
        }
    }
}