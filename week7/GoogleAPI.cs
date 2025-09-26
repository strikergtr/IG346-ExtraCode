using Firebase;
using Firebase.Auth;
using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GoogleAPI : MonoBehaviour
{
    public static GoogleAPI instance;
    public string uuid = "";
    public int stage = 0;
    public DatabaseReference dbRef;
    public PlayerSaveData myplayer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                dbRef = FirebaseDatabase.DefaultInstance.RootReference;
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }
    // Update is called once per frame
    void Update()
    {

    }
    public void SavePlayerData(string userId, string name, int score, float x, float y, float z)
    {
        Dictionary<string, object> playerData = new Dictionary<string, object>
        {
            { "name", name },
            { "score", score },
            { "position", new Dictionary<string, object>
                {
                    { "x", x },
                    { "y", y },
                    { "z", z }
                }
            }
        };

        dbRef.Child("players").Child(userId).SetValueAsync(playerData);
    }

    public void SaveGame()
    {
        SavePlayerData(uuid, uuid, myplayer.score, myplayer.position.x, myplayer.position.y, myplayer.position.z);
    }

    public async Task<PlayerSaveData> LoadPlayerData(string userId)
    {
        DataSnapshot snapshot = await dbRef.Child("players").Child(userId).GetValueAsync();

        if (!snapshot.Exists)
            return new PlayerSaveData();

        PlayerSaveData data = new PlayerSaveData
        {
            name = snapshot.Child("name").Value?.ToString() ?? "Unknown",
            score = snapshot.Child("score").Value != null ? int.Parse(snapshot.Child("score").Value.ToString()) : 0,
            position =
            new Vector3(
                snapshot.Child("position").Child("x").Value != null ? float.Parse(snapshot.Child("position").Child("x").Value.ToString()) : 0f,
                snapshot.Child("position").Child("y").Value != null ? float.Parse(snapshot.Child("position").Child("y").Value.ToString()) : 0f,
                snapshot.Child("position").Child("z").Value != null ? float.Parse(snapshot.Child("position").Child("z").Value.ToString()) : 0f
            )
        };

        return data;
    }
}


[Serializable]
public class PlayerSaveData
{
    public string name;
    public int score;
    public Vector3 position;

}