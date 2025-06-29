using Firebase;
using Firebase.Firestore;
using UnityEngine;
using System.Collections.Generic;

public class FirebaseManager : MonoBehaviour
{
    FirebaseFirestore db;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
    }

    public void SubmitScore(string playerId, int score)
    {
        DocumentReference docRef = db.Collection("leaderboard").Document(playerId);
        docRef.SetAsync(new Dictionary<string, object> {
            { "score", score }
        });
    }
}