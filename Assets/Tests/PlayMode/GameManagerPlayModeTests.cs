using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Events;

namespace SyncSeed.Tests
{
    public class GameManagerPlayModeTests
    {
        private GameManager gameManager;
        private GameObject gameManagerObject;
        private UIManager uiManager;
        private GameObject uiManagerObject;
        private MenuUI menuUI;
        private GameObject menuUIObject;
        private LeaderboardManager leaderboardManager;
        private GameObject leaderboardManagerObject;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            // Create GameManager
            gameManagerObject = new GameObject("GameManager");
            gameManager = gameManagerObject.AddComponent<GameManager>();
            
            // Create UIManager
            uiManagerObject = new GameObject("UIManager");
            uiManager = uiManagerObject.AddComponent<UIManager>();
            
            // Create MenuUI
            menuUIObject = new GameObject("MenuUI");
            menuUI = menuUIObject.AddComponent<MenuUI>();
            
            // Create LeaderboardManager
            leaderboardManagerObject = new GameObject("LeaderboardManager");
            leaderboardManager = leaderboardManagerObject.AddComponent<LeaderboardManager>();
            
            // Set up references using reflection since fields are now private
            var uiManagerField = typeof(GameManager).GetField("_uiManager", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var menuUIField = typeof(GameManager).GetField("_menuUI", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var leaderboardManagerField = typeof(GameManager).GetField("_leaderboardManager", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            uiManagerField?.SetValue(gameManager, uiManager);
            menuUIField?.SetValue(gameManager, menuUI);
            leaderboardManagerField?.SetValue(gameManager, leaderboardManager);
            
            // Wait for one frame to ensure all components are properly initialized
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            // Clean up GameManager singleton
            if (GameManager.Instance != null)
            {
                Object.Destroy(GameManager.Instance.gameObject);
            }
            
            // Clean up other objects
            if (gameManagerObject != null)
                Object.Destroy(gameManagerObject);
            if (uiManagerObject != null)
                Object.Destroy(uiManagerObject);
            if (menuUIObject != null)
                Object.Destroy(menuUIObject);
            if (leaderboardManagerObject != null)
                Object.Destroy(leaderboardManagerObject);
            
            // Wait for destruction to complete
            yield return null;
        }

        [UnityTest]
        public IEnumerator GameManager_Awake_ShouldSetSingletonInstance()
        {
            // Wait for Awake to be called
            yield return null;
            
            // Assert
            Assert.IsNotNull(GameManager.Instance);
            Assert.AreEqual(gameManager, GameManager.Instance);
        }

        [UnityTest]
        public IEnumerator GameManager_Start_ShouldInitializeGame()
        {
            // Act
            gameManager.Start();
            yield return null;
            
            // Assert
            // The Start method should not throw any exceptions
            Assert.Pass("Start method executed successfully");
        }

        [UnityTest]
        public IEnumerator GameManager_StartLevel_ShouldResetGameState()
        {
            // Arrange
            gameManager.SetPlayerName("TestPlayer");
            gameManager.AdvanceToNextLevel(); // Set level to 2
            gameManager.AddScore(100);
            
            // Act
            gameManager.StartLevel();
            yield return null;
            
            // Assert
            Assert.AreEqual(0, gameManager.PlayerScore);
            Assert.AreEqual(5, gameManager.TargetsRemaining); // 3 + (2-1)*2 = 5
        }

        [UnityTest]
        public IEnumerator GameManager_RhythmTargetHit_ShouldUpdateGameState()
        {
            // Arrange
            gameManager.SetPlayerName("TestPlayer");
            gameManager.StartLevel();
            
            // Act
            gameManager.RhythmTargetHit();
            yield return null;
            
            // Assert
            Assert.AreEqual(10, gameManager.PlayerScore);
            Assert.AreEqual(2, gameManager.TargetsRemaining); // 3 - 1 = 2
        }

        [UnityTest]
        public IEnumerator GameManager_CompleteLevel_ShouldTriggerEndLevel()
        {
            // Arrange
            gameManager.SetPlayerName("TestPlayer");
            gameManager.StartLevel();
            
            // Act - Complete the level
            gameManager.RhythmTargetHit(); // 2 remaining
            gameManager.RhythmTargetHit(); // 1 remaining
            gameManager.RhythmTargetHit(); // 0 remaining, should trigger EndLevel
            yield return null;
            
            // Assert
            Assert.AreEqual(0, gameManager.TargetsRemaining);
            Assert.AreEqual(30, gameManager.PlayerScore);
            Assert.IsTrue(gameManager.IsLevelComplete);
        }

        [UnityTest]
        public IEnumerator GameManager_AdvanceToNextLevel_ShouldProgressCorrectly()
        {
            // Arrange
            gameManager.SetPlayerName("TestPlayer");
            gameManager.StartLevel();
            gameManager.AddScore(50);
            
            // Act
            gameManager.AdvanceToNextLevel();
            yield return null;
            
            // Assert
            Assert.AreEqual(2, gameManager.CurrentLevel);
            Assert.AreEqual(0, gameManager.PlayerScore);
        }

        [UnityTest]
        public IEnumerator GameManager_RestartLevel_ShouldKeepLevelButResetScore()
        {
            // Arrange
            gameManager.SetPlayerName("TestPlayer");
            gameManager.AdvanceToNextLevel(); // Level 2
            gameManager.AdvanceToNextLevel(); // Level 3
            gameManager.StartLevel();
            gameManager.AddScore(100);
            
            // Act
            gameManager.RestartLevel();
            yield return null;
            
            // Assert
            Assert.AreEqual(3, gameManager.CurrentLevel);
            Assert.AreEqual(0, gameManager.PlayerScore);
        }

        [UnityTest]
        public IEnumerator GameManager_MultipleTargetHits_ShouldAccumulateScore()
        {
            // Arrange
            gameManager.SetPlayerName("TestPlayer");
            gameManager.StartLevel();
            
            // Act
            gameManager.RhythmTargetHit();
            yield return null;
            gameManager.RhythmTargetHit();
            yield return null;
            
            // Assert
            Assert.AreEqual(20, gameManager.PlayerScore);
            Assert.AreEqual(1, gameManager.TargetsRemaining);
        }

        [UnityTest]
        public IEnumerator GameManager_LevelProgression_ShouldIncreaseDifficulty()
        {
            // Arrange
            gameManager.SetPlayerName("TestPlayer");
            gameManager.StartLevel();
            int level1Targets = gameManager.TargetsRemaining;
            yield return null;
            
            // Act
            gameManager.AdvanceToNextLevel();
            int level2Targets = gameManager.TargetsRemaining;
            yield return null;
            
            // Assert
            Assert.Greater(level2Targets, level1Targets);
        }

        [UnityTest]
        public IEnumerator GameManager_AddScore_ShouldIncreaseScore()
        {
            // Arrange
            gameManager.SetPlayerName("TestPlayer");
            gameManager.StartLevel();
            int scoreToAdd = 15;
            
            // Act
            gameManager.AddScore(scoreToAdd);
            yield return null;
            
            // Assert
            Assert.AreEqual(15, gameManager.PlayerScore);
        }

        [UnityTest]
        public IEnumerator GameManager_SetPlayerName_ShouldUpdateName()
        {
            // Arrange
            string testName = "PlayModeTestPlayer";
            
            // Act
            bool result = gameManager.SetPlayerName(testName);
            yield return null;
            
            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(testName, gameManager.PlayerName);
        }

        [UnityTest]
        public IEnumerator GameManager_EndLevel_ShouldHandleNullDependencies()
        {
            // Arrange
            gameManager.SetPlayerName("TestPlayer");
            gameManager.StartLevel();
            gameManager.RhythmTargetHit(); // 2 remaining
            gameManager.RhythmTargetHit(); // 1 remaining
            gameManager.RhythmTargetHit(); // 0 remaining, level complete
            
            // Clear dependencies
            var uiManagerField = typeof(GameManager).GetField("_uiManager", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var menuUIField = typeof(GameManager).GetField("_menuUI", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var leaderboardManagerField = typeof(GameManager).GetField("_leaderboardManager", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            uiManagerField?.SetValue(gameManager, null);
            menuUIField?.SetValue(gameManager, null);
            leaderboardManagerField?.SetValue(gameManager, null);
            
            // Act & Assert
            Assert.DoesNotThrow(() => gameManager.EndLevel());
            yield return null;
        }

        [UnityTest]
        public IEnumerator GameManager_GenerateTargetsForLevel_ShouldReturnValidValues()
        {
            // Test various level values
            Assert.AreEqual(3, gameManager.GenerateTargetsForLevel(1));
            Assert.AreEqual(11, gameManager.GenerateTargetsForLevel(5));
            Assert.AreEqual(30, gameManager.GenerateTargetsForLevel(15)); // Clamped to max
            Assert.AreEqual(3, gameManager.GenerateTargetsForLevel(-5)); // Clamped to min
            
            yield return null;
        }

        [UnityTest]
        public IEnumerator GameManager_PointsPerTarget_ShouldBeConfigurable()
        {
            // Arrange
            int newPointsPerTarget = 25;
            gameManager.PointsPerTarget = newPointsPerTarget;
            gameManager.SetPlayerName("TestPlayer");
            gameManager.StartLevel();
            
            // Act
            gameManager.RhythmTargetHit();
            yield return null;
            
            // Assert
            Assert.AreEqual(newPointsPerTarget, gameManager.PlayerScore);
        }

        [UnityTest]
        public IEnumerator GameManager_CompleteGameSession_ShouldWorkEndToEnd()
        {
            // Arrange
            gameManager.SetPlayerName("EndToEndTestPlayer");
            gameManager.StartLevel();
            yield return null;
            
            // Act - Complete level 1
            while (gameManager.TargetsRemaining > 0)
            {
                gameManager.RhythmTargetHit();
                yield return null;
            }
            
            // Assert - Level should be completed
            Assert.AreEqual(0, gameManager.TargetsRemaining);
            Assert.Greater(gameManager.PlayerScore, 0);
            Assert.IsTrue(gameManager.IsLevelComplete);
            
            // Act - Advance to next level
            gameManager.AdvanceToNextLevel();
            yield return null;
            
            // Assert - Should be on level 2 with reset score
            Assert.AreEqual(2, gameManager.CurrentLevel);
            Assert.AreEqual(0, gameManager.PlayerScore);
            Assert.Greater(gameManager.TargetsRemaining, 0);
        }

        [UnityTest]
        public IEnumerator GameManager_Events_ShouldFireCorrectly()
        {
            // Arrange
            bool scoreChangedFired = false;
            bool levelChangedFired = false;
            bool targetsRemainingChangedFired = false;
            bool levelCompletedFired = false;
            bool gameStartedFired = false;
            bool playerNameChangedFired = false;
            
            gameManager.OnScoreChanged.AddListener((score) => scoreChangedFired = true);
            gameManager.OnLevelChanged.AddListener((level) => levelChangedFired = true);
            gameManager.OnTargetsRemainingChanged.AddListener((targets) => targetsRemainingChangedFired = true);
            gameManager.OnLevelCompleted.AddListener(() => levelCompletedFired = true);
            gameManager.OnGameStarted.AddListener(() => gameStartedFired = true);
            gameManager.OnPlayerNameChanged.AddListener((name) => playerNameChangedFired = true);
            
            // Act - Set player name
            gameManager.SetPlayerName("EventTestPlayer");
            yield return null;
            
            // Assert - Player name event should fire
            Assert.IsTrue(playerNameChangedFired);
            
            // Act - Start level
            gameManager.StartLevel();
            yield return null;
            
            // Assert - Score and targets events should fire
            Assert.IsTrue(scoreChangedFired);
            Assert.IsTrue(targetsRemainingChangedFired);
            Assert.IsTrue(gameStartedFired);
            
            // Act - Hit target
            gameManager.RhythmTargetHit();
            yield return null;
            
            // Assert - Score and targets events should fire again
            Assert.IsTrue(scoreChangedFired);
            Assert.IsTrue(targetsRemainingChangedFired);
            
            // Act - Advance level
            gameManager.AdvanceToNextLevel();
            yield return null;
            
            // Assert - Level event should fire
            Assert.IsTrue(levelChangedFired);
        }

        [UnityTest]
        public IEnumerator GameManager_IsGameActive_ShouldUpdateCorrectly()
        {
            // Initially not active
            Assert.IsFalse(gameManager.IsGameActive);
            
            // After setting player name
            gameManager.SetPlayerName("TestPlayer");
            yield return null;
            Assert.IsTrue(gameManager.IsGameActive);
            
            // After reset
            gameManager.ResetGame();
            yield return null;
            Assert.IsFalse(gameManager.IsGameActive);
        }

        [UnityTest]
        public IEnumerator GameManager_ResetGame_ShouldResetAllValues()
        {
            // Arrange
            gameManager.SetPlayerName("TestPlayer");
            gameManager.AdvanceToNextLevel(); // Level 2
            gameManager.StartLevel();
            gameManager.AddScore(100);
            
            // Act
            gameManager.ResetGame();
            yield return null;
            
            // Assert
            Assert.AreEqual(1, gameManager.CurrentLevel);
            Assert.AreEqual(0, gameManager.PlayerScore);
            Assert.AreEqual(0, gameManager.TargetsRemaining);
            Assert.IsEmpty(gameManager.PlayerName);
            Assert.IsFalse(gameManager.IsGameActive);
        }
    }
} 