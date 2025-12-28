using FunBoardGames.Network;
using UnityEngine;
using UnityEngine.UI;

public class PlayerVoteUI : MonoBehaviour
{
    [SerializeField] RawImage correct, wrong;
    [SerializeField] Text playerTxt;
    [SerializeField] Texture2D[] stateTextures;

    ISETPlayer networkPlayer;

    internal void UpdateVoteUI(bool? newVal)
    {
        if (newVal.HasValue == false)
        {
            correct.texture = wrong.texture = stateTextures[0];
            correct.color = wrong.color = new Color(0.11f, 0.11f, 0.11f);
        }
        else if (newVal.HasValue == true)
        {
            wrong.texture = stateTextures[0];
            wrong.color = new Color(0.11f, 0.11f, 0.11f);
            correct.texture = stateTextures[2];
            correct.color = Color.white;
        }
        else
        {
            correct.texture = stateTextures[0];
            correct.color = new Color(0.11f, 0.11f, 0.11f);
            wrong.texture = stateTextures[1];
            wrong.color = Color.white;
        }
    }

    public void SetPlayer(ISETPlayer player)
    {
        if (networkPlayer != null)
            UnSubscribe();

        networkPlayer = player; 
        playerTxt.text = networkPlayer.Name;
        gameObject.SetActive(true);
        UpdateVoteUI(networkPlayer.Vote);
        Subscribe();
    }

    private void OnDestroy()
    {
        if (networkPlayer == null)
            return;

        UnSubscribe();
    }

    void Subscribe()
    {
        networkPlayer.LeftGame += PlayerLeft;
    }

    public void PlayerLeft()
    {
        gameObject.SetActive(false);
    }

    void UnSubscribe()
    {
        networkPlayer.LeftGame -= PlayerLeft;
    }
}
