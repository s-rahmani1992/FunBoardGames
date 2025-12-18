using TMPro;
using UnityEngine;

namespace FunBoardGames.Client
{
    public class MenuUIManager : MonoBehaviour
    {
        [SerializeField] UserProfile userProfile;
        [SerializeField] TMP_Text playerNameText;

        // Start is called before the first frame update
        void Start()
        {
            playerNameText.text = userProfile.PlayerName;
        }
    }
}