using Firebase.Extensions;
using Firebase.Firestore;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{
    public Transform leaderboardContainer;
    public GameObject leaderboardEntryPrefab;
    FirebaseFirestore db;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        FetchLeaderboard();
    }

    public void FetchLeaderboard()
    {
        db.Collection("leaderboard")
          .OrderBy("score") // Ascending only
          .Limit(10)
          .GetSnapshotAsync().ContinueWithOnMainThread(task =>
          {
              if (task.IsCompleted)
              {
                  foreach (Transform child in leaderboardContainer)
                      Destroy(child.gameObject);

                  List<DocumentSnapshot> documents = new List<DocumentSnapshot>(task.Result.Documents);
                  documents.Reverse(); // flip ascending → descending

                  foreach (var doc in documents)
                  {
                      string player = doc.Id;
                      int score = 0;
                      doc.TryGetValue("score", out score);

                      GameObject entry = Instantiate(leaderboardEntryPrefab, leaderboardContainer);
                      entry.GetComponentInChildren<TMPro.TMP_Text>().text = player + ": " + score;
                  }
              }
          });
    }

}