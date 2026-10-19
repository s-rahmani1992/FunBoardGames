using FunBoardGames.Network;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FunBoardGames.Client
{
    public class UserProfileItem : MonoBehaviour
    {
        [SerializeField] AvatarAssetManager avatarAssets;
        [SerializeField] Image avatarImage;
        [SerializeField] TMP_Text nameText;
        [SerializeField] Sprite anonymousAvatar;

        public void SetProfile(Profile profile)
        {
            avatarImage.sprite = avatarAssets.GetAvatar(profile.Avatar);
            nameText.text = profile.PlayerName;
        }

        public void SetSearching()
        {
            avatarImage.sprite = anonymousAvatar;
            nameText.text = "Searching....";
        }
    }
}
