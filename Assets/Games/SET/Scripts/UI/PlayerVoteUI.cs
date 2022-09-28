using OnlineBoardGames.SET;
using System;
using System.Collections;
using System.Collections.Generic;
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

    internal void UpdateUI(VoteStat newVal){
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

    internal void UpdateText(string str){
        gameObject.SetActive(true);
        playerTxt.text = str;
    }

    internal void RefreshVote(VoteStat newVal){
        UpdateUI(newVal);
    }
}
