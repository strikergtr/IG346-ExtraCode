using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp.Server;

public class GUIControl : MonoBehaviour
{
    GoogleAPI api;
    public string googleDesktopClientId;
    public string googleDesktopClientSecret;

    public Button signInbutton;
    public TMP_Text debug;

    private bool _busy;
    private bool _pass;

    public GameObject playerPrefab;
    public Vector3 pos;
    void Start()
    {
        api = GoogleAPI.instance;
    }
    public async void OnClickSignIn()
    {
        debug.text = "Process Google...";
        if (_busy) return;
        _busy = true;
        api.stage = 0;
        SetStatus("Opening browser…");
        if (signInbutton) signInbutton.interactable = false;
        try
        {
            var oauth = new GoogleOAuthLoopback(
                googleDesktopClientId,
                googleDesktopClientSecret,
                "openid", "email", "profile"
            );
            var tokens = await oauth.SignInAsync();
            SetStatus("Exchanging token with Firebase…");
            var user = await FirebaseGoogleSignIn.SignInWithIdTokenAsync(tokens.id_token);
            SetStatus($"Signed in: {user.DisplayName} ({user.Email})");
            Debug.Log($"Firebase UID: {user.UserId}");
            api.uuid = user.UserId;
            api.stage = 10;
            api.myplayer =  await api.LoadPlayerData(api.uuid);
            GameObject go = Instantiate(playerPrefab, api.myplayer.position, Quaternion.identity);
            _pass = true;
        }
        catch (Exception ex)
        {
            Debug.LogError("Google sign-in failed: " + ex);
            SetStatus("Sign-in failed. Check Console.");
            api.stage = 1;
            _pass = false;
        }
        finally
        {
            if (signInbutton && _pass) signInbutton.gameObject.SetActive(false);
            else if (signInbutton) signInbutton.interactable = true;
            _busy = false;
        }
    }
    private void SetStatus(string msg)
    {
        if (debug) debug.text = msg;
    }
    public void QuitApplicatio() 
    {
        Application.Quit();
    }
}
