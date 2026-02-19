using System;
using System.Collections;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

public class FirebaseUserDataManager : MonoBehaviour
{
    public static FirebaseUserDataManager Instance;

    [Header("Firebase")]
    public string databaseURL = "https://babyuapogame-default-rtdb.europe-west1.firebasedatabase.app/";

    FirebaseAuth auth;
    DatabaseReference dbRef;

    public int currentCoins;
    public event Action<int> OnCoinsUpdated;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        StartCoroutine(InitFirebase());
    }

    IEnumerator InitFirebase()
    {
        var depTask = FirebaseApp.CheckAndFixDependenciesAsync();
        yield return new WaitUntil(() => depTask.IsCompleted);

        if (depTask.Result != DependencyStatus.Available)
        {
            Debug.LogError("Firebase dependency error: " + depTask.Result);
            yield break;
        }

        FirebaseApp app = FirebaseApp.DefaultInstance;
        app.Options.DatabaseUrl = new Uri(databaseURL);

        auth = FirebaseAuth.DefaultInstance;
        dbRef = FirebaseDatabase.GetInstance(app).RootReference;

        if (auth.CurrentUser != null)
            LoadUserCoins();
    }

    public void LoadUserCoins()
    {
        string uid = auth.CurrentUser.UserId;

        dbRef.Child("Users").Child(uid).Child("coins").GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (!task.IsCompleted || task.Result == null || !task.Result.Exists)
                {
                    currentCoins = 0;
                    SaveCoins(0);
                }
                else
                {
                    currentCoins = int.Parse(task.Result.Value.ToString());
                }

                Debug.Log("Coins Loaded: " + currentCoins);
                OnCoinsUpdated?.Invoke(currentCoins);
            });
    }

    public void SaveCoins(int amount)
    {
        currentCoins = amount;
        string uid = auth.CurrentUser.UserId;
        dbRef.Child("Users").Child(uid).Child("coins").SetValueAsync(currentCoins);
        OnCoinsUpdated?.Invoke(currentCoins);
    }

    public void AddCoins(int amount)
    {
        SaveCoins(currentCoins + amount);
    }
}
