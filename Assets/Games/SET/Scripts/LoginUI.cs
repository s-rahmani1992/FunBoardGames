using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginUI : MonoBehaviour
{
    [SerializeField]
    InputField nameField;
    [SerializeField]
    Button loginBtn;
    [SerializeField]
    Text logTxt;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateBtn()
    {
        loginBtn.interactable = nameField.text.Length > 2;
    }

    public void AtemptLogin()
    {
        (SETNetworkManager.singleton.authenticator as SimpleNameAuthenticator).reqName = nameField.text;
        SETNetworkManager.singleton.StartClient();
    }

    public void LogMsg(string msg)
    {
        logTxt.text = msg;
    }
}
