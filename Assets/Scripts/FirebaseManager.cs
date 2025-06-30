using Firebase;
using Firebase.Firestore;
using UnityEngine;
using System.Collections.Generic;

public class FirebaseManager : MonoBehaviour
{
    FirebaseFirestore db;

    void Start()
    {
        InitializeFirebase();
        db = FirebaseFirestore.DefaultInstance;
    }

    public void SubmitScore(string playerId, int score)
    {
        DocumentReference docRef = db.Collection("leaderboard").Document(playerId);
        docRef.SetAsync(new Dictionary<string, object> {
            { "score", score }
        });
    }

    void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                // Optional: Use Firebase features here
                Debug.Log("Firebase is ready!");
            }
            else
            {
                Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
            }
        });
    }
}