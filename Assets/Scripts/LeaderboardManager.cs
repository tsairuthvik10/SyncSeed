using Firebase.Extensions;
using Firebase.Firestore;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance { get; private set; }

    public Transform leaderboardContainer;
    public GameObject leaderboardEntryPrefab;
    FirebaseFirestore db;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        db = FirebaseFirestore.DefaultInstance;
    }

    public void SubmitScore(string playerName, int score)
    {
        if (string.IsNullOrEmpty(playerName)) return;

        var entry = new Dictionary<string, object>
        {
            {"name", playerName},
            {"score", score},
            {"timestamp", Timestamp.GetCurrentTimestamp()}
        };

        db.Collection("leaderboard").AddAsync(entry).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Score submitted to leaderboard");
            }
        });
    }

    public void FetchLeaderboard()
    {
        db.Collection("leaderboard")
          .OrderBy("score")
          .Limit(10)
          .GetSnapshotAsync().ContinueWithOnMainThread(task =>
          {
              if (task.IsCompleted)
              {
                  foreach (Transform child in leaderboardContainer)
                      Destroy(child.gameObject);

                  List<DocumentSnapshot> documents = new List<DocumentSnapshot>(task.Result.Documents);
                  documents.Reverse();

                  foreach (var doc in documents)
                  {
                      string name = "Unknown";
                      int score = 0;

                      doc.TryGetValue("name", out name);
                      doc.TryGetValue("score", out score);

                      GameObject entry = Instantiate(leaderboardEntryPrefab, leaderboardContainer);
                      entry.GetComponentInChildren<TMP_Text>().text = name + ": " + score;
                  }
              }
          });
    }

}