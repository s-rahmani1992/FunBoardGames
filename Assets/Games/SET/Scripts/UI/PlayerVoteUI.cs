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

    SETPlayer networkPlayer;

    internal void UpdateUI(VoteAnswer newVal)
    {
        switch (newVal){
            case VoteAnswer.None:
                correct.texture = wrong.texture = stateTextures[0];
                correct.color = wrong.color = new Color(0.11f, 0.11f, 0.11f);
                break;
            case VoteAnswer.NO:
                correct.texture = stateTextures[0];
                correct.color = new Color(0.11f, 0.11f, 0.11f);
                wrong.texture = stateTextures[1];
                wrong.color = Color.white;
                break;
            case VoteAnswer.YES:
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

    internal void RefreshVote(VoteAnswer newVal)
    {
        UpdateUI(newVal);
    }

    public void SetPlayer(SETPlayer player)
    {
        if (networkPlayer != null)
            UnSubscribe();

        networkPlayer = player; 
        playerTxt.text = networkPlayer.Name;
        gameObject.SetActive(true);
        UpdateUI(networkPlayer.VoteAnswer);
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

    private void OnVoteChanged(VoteAnswer _, VoteAnswer vote)
    {
        UpdateUI(vote);
    }

    void UnSubscribe()
    {
        networkPlayer.VoteChanged -= OnVoteChanged;
        networkPlayer.LeftGame -= PlayerLeft;
    }
}
