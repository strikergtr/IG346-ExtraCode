using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GUIControl : MonoBehaviour, INetworkRunnerCallbacks
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

    private NetworkRunner runner;

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
            api.myplayer = await api.LoadPlayerData(api.uuid);
            GoOnline();
            //GameObject go = Instantiate(playerPrefab, api.myplayer.position, Quaternion.identity);
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
    async private void GoOnline()
    {
        runner = gameObject.AddComponent<NetworkRunner>();
        runner.ProvideInput = true;
        var sceneInfo = new NetworkSceneInfo();
        sceneInfo.AddSceneRef(SceneRef.FromIndex(0));
        await runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.AutoHostOrClient,
            SessionName = "Room01",
            Scene = sceneInfo,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }
    private void SetStatus(string msg)
    {
        if (debug) debug.text = msg;
    }
    public void QuitApplicatio()
    {
        //Application.Quit();
        runner.Shutdown();
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        throw new NotImplementedException();
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        throw new NotImplementedException();
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        NetworkObject playerObj =  runner.Spawn(playerPrefab, new Vector3(0, 1, 0), Quaternion.identity, player);
        runner.SetPlayerObject(player, playerObj);
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            if (runner.TryGetPlayerObject(player, out var networkObject))
            {
                runner.Despawn(networkObject);
            }
        }
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        ReturnToTitle();
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        ReturnToTitle();
    }

    private void ReturnToTitle()
    {
        StartCoroutine(ShutdownAndLoad());
    }

    private IEnumerator ShutdownAndLoad()
    {
        if (NetworkRunner.Instances.Count > 0)
        {
            if (runner != null && runner.IsRunning)
            {
                yield return runner.Shutdown();
            }
        }

        SceneManager.LoadScene(0);
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        throw new NotImplementedException();
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        throw new NotImplementedException();
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        throw new NotImplementedException();
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
        throw new NotImplementedException();
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
        throw new NotImplementedException();
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();

        if (Keyboard.current != null)
        {
            float move = 0;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                move = -1;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                move = 1;

            data.horizontal = move;
            data.jump = Keyboard.current.spaceKey.isPressed;
        }

        input.Set(data);
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        throw new NotImplementedException();
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        throw new NotImplementedException();
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        throw new NotImplementedException();
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        throw new NotImplementedException();
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        throw new NotImplementedException();
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        throw new NotImplementedException();
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        throw new NotImplementedException();
    }

    //public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    //{
    //    runner.Spawn(playerPrefab, new Vector3(0, 1, 0), Quaternion.identity, player);
    //}

    //// ที่เหลือของ INetworkRunnerCallbacks ใส่ว่างไว้ก่อน
    //public void OnInput(NetworkRunner runner, NetworkInput input)
    //{
    //    var data = new NetworkInputData();

    //    if (Keyboard.current != null)
    //    {
    //        float move = 0;
    //        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
    //            move = -1;
    //        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
    //            move = 1;

    //        data.horizontal = move;
    //        data.jump = Keyboard.current.spaceKey.isPressed;
    //    }

    //    input.Set(data);
    //}

    //public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    //{
    //    throw new NotImplementedException();
    //}

    //public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    //{
    //    throw new NotImplementedException();
    //}

    //public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    //{
    //    throw new NotImplementedException();
    //}

    //public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    //{
    //    throw new NotImplementedException();
    //}

    //public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    //{
    //    throw new NotImplementedException();
    //}

    //public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    //{
    //    throw new NotImplementedException();
    //}

    //public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    //{
    //    throw new NotImplementedException();
    //}

    //public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    //{
    //    throw new NotImplementedException();
    //}

    //public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    //{
    //    throw new NotImplementedException();
    //}

    //public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    //{
    //    throw new NotImplementedException();
    //}


    //public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    //{
    //    throw new NotImplementedException();
    //}

    //public void OnConnectedToServer(NetworkRunner runner)
    //{
    //    throw new NotImplementedException();
    //}

    //public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    //{
    //    throw new NotImplementedException();
    //}

    //public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    //{
    //    throw new NotImplementedException();
    //}

    //public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    //{
    //    throw new NotImplementedException();
    //}

    //public void OnSceneLoadDone(NetworkRunner runner)
    //{
    //    throw new NotImplementedException();
    //}

    //public void OnSceneLoadStart(NetworkRunner runner)
    //{
    //    throw new NotImplementedException();
    //}
}
[Serializable]
public struct NetworkInputData : INetworkInput
{
    public float horizontal;
    public bool jump;
}