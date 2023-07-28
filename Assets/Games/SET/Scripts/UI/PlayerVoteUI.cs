using OnlineBoardGames.SET;
using UnityEngine;
using UnityEngine.UI;

public class PlayerVoteUI : MonoBehaviour
{
    [SerializeField]
    RawImage correct, wrong;
    [SerializeField]
    Text playerTxt;
    [SerializeField]
    Texture2D[] stateTextures;

    SETNetworkPlayer networkPlayer;

    internal void UpdateUI(VoteStat newVal)
    {
        switch (newVal){
            case VoteStat.NULL:
                correct.texture = wrong.texture = stateTextures[0];
                correct.color = wrong.color = new Color(0.11f, 0.11f, 0.11f);
                break;
            case VoteStat.NO:
                correct.texture = stateTextures[0];
                correct.color = new Color(0.11f, 0.11f, 0.11f);
                wrong.texture = stateTextures[1];
                wrong.color = Color.white;
                break;
            case VoteStat.YES:
                wrong.texture = stateTextures[0];
                wrong.color = new Color(0.11f, 0.11f, 0.11f);
                correct.texture = stateTextures[2];
                correct.color = Color.white;
                break;
        }
    }

    internal void UpdateText(string str)
    {
        gameObject.SetActive(true);
        playerTxt.text = str;
    }

    internal void RefreshVote(VoteStat newVal)
    {
        UpdateUI(newVal);
    }

    public void SetPlayer(SETNetworkPlayer player)
    {
        if (networkPlayer != null)
            UnSubscribe();

        networkPlayer = player; 
        playerTxt.text = networkPlayer.Name;
        gameObject.SetActive(true);
        UpdateUI(networkPlayer.voteState);
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
        networkPlayer.VoteChanged += OnVoteChanged;
        networkPlayer.LeftGame += PlayerLeft;
    }

    public void PlayerLeft()
    {
        gameObject.SetActive(false);
    }

    private void OnVoteChanged(VoteStat _, VoteStat vote)
    {
        UpdateUI(vote);
    }

    void UnSubscribe()
    {
        networkPlayer.VoteChanged -= OnVoteChanged;
        networkPlayer.LeftGame -= PlayerLeft;
    }
}
