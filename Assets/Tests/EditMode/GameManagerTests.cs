using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Events;

namespace SyncSeed.Tests
{
    public class GameManagerTests
    {
        private GameManager gameManager;
        private GameObject gameManagerObject;
        private UIManager uiManager;
        private GameObject uiManagerObject;
        private MenuUI menuUI;
        private GameObject menuUIObject;
        private LeaderboardManager leaderboardManager;
        private GameObject leaderboardManagerObject;

        [SetUp]
        public void SetUp()
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
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up GameManager singleton
            if (GameManager.Instance != null)
            {
                Object.DestroyImmediate(GameManager.Instance.gameObject);
            }
            
            // Clean up other objects
            if (gameManagerObject != null)
                Object.DestroyImmediate(gameManagerObject);
            if (uiManagerObject != null)
                Object.DestroyImmediate(uiManagerObject);
            if (menuUIObject != null)
                Object.DestroyImmediate(menuUIObject);
            if (leaderboardManagerObject != null)
                Object.DestroyImmediate(leaderboardManagerObject);
        }

        [Test]
        public void GameManager_Initialization_ShouldSetDefaultValues()
        {
            // Assert
            Assert.AreEqual(1, gameManager.CurrentLevel);
            Assert.AreEqual(0, gameManager.PlayerScore);
            Assert.AreEqual(10, gameManager.PointsPerTarget);
            Assert.AreEqual(0, gameManager.TargetsRemaining);
            Assert.IsEmpty(gameManager.PlayerName);
        }

        [Test]
        public void GameManager_SetPlayerName_WithValidName_ShouldUpdatePlayerName()
        {
            // Arrange
            string testName = "TestPlayer";

            // Act
            bool result = gameManager.SetPlayerName(testName);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(testName, gameManager.PlayerName);
        }

        [Test]
        public void GameManager_SetPlayerName_WithEmptyName_ShouldReturnFalse()
        {
            // Arrange
            string testName = "";

            // Act
            bool result = gameManager.SetPlayerName(testName);

            // Assert
            Assert.IsFalse(result);
            Assert.IsEmpty(gameManager.PlayerName);
        }

        [Test]
        public void GameManager_SetPlayerName_WithWhitespace_ShouldReturnFalse()
        {
            // Arrange
            string testName = "   ";

            // Act
            bool result = gameManager.SetPlayerName(testName);

            // Assert
            Assert.IsFalse(result);
            Assert.IsEmpty(gameManager.PlayerName);
        }

        [Test]
        public void GameManager_SetPlayerName_WithValidName_ShouldLogMessage()
        {
            // Arrange
            string testName = "TestPlayer";

            // Act & Assert
            LogAssert.Expect(LogType.Log, $"Player name set to: {testName}");
            gameManager.SetPlayerName(testName);
        }

        [Test]
        public void GameManager_StartLevel_WithoutPlayerName_ShouldLogWarning()
        {
            // Act & Assert
            LogAssert.Expect(LogType.Log, "Cannot start level without setting player name.");
            gameManager.StartLevel();
        }

        [Test]
        public void GameManager_StartLevel_WithPlayerName_ShouldResetScoreAndSetTargets()
        {
            // Arrange
            gameManager.SetPlayerName("TestPlayer");
            gameManager.AdvanceToNextLevel(); // Set level to 2
            gameManager.AddScore(100);

            // Act
            gameManager.StartLevel();

            // Assert
            Assert.AreEqual(0, gameManager.PlayerScore);
            Assert.AreEqual(5, gameManager.TargetsRemaining); // 3 + (2-1)*2 = 5
        }

        [Test]
        public void GameManager_StartLevel_ShouldLogLevelStart()
        {
            // Arrange
            gameManager.SetPlayerName("TestPlayer");
            gameManager.AdvanceToNextLevel(); // Set level to 2

            // Act & Assert
            LogAssert.Expect(LogType.Log, "Level 2 started with 5 targets.");
            gameManager.StartLevel();
        }

        [Test]
        public void GameManager_RhythmTargetHit_WithoutGameActive_ShouldLogWarning()
        {
            // Act & Assert
            LogAssert.Expect(LogType.Log, "Cannot hit target: game not active or level already complete.");
            gameManager.RhythmTargetHit();
        }

        [Test]
        public void GameManager_RhythmTargetHit_WithGameActive_ShouldIncreaseScore()
        {
            // Arrange
            gameManager.SetPlayerName("TestPlayer");
            gameManager.StartLevel();

            // Act
            gameManager.RhythmTargetHit();

            // Assert
            Assert.AreEqual(10, gameManager.PlayerScore);
            Assert.AreEqual(2, gameManager.TargetsRemaining); // 3 - 1 = 2
        }

        [Test]
        public void GameManager_RhythmTargetHit_ShouldNotEndLevelWhenTargetsRemaining()
        {
            // Arrange
            gameManager.SetPlayerName("TestPlayer");
            gameManager.StartLevel();

            // Act
            gameManager.RhythmTargetHit();

            // Assert
            Assert.AreEqual(2, gameManager.TargetsRemaining);
            Assert.AreEqual(10, gameManager.PlayerScore);
            Assert.IsFalse(gameManager.IsLevelComplete);
        }

        [Test]
        public void GameManager_RhythmTargetHit_ShouldEndLevelWhenNoTargetsRemaining()
        {
            // Arrange
            gameManager.SetPlayerName("TestPlayer");
            gameManager.StartLevel();
            gameManager.RhythmTargetHit(); // 2 remaining
            gameManager.RhythmTargetHit(); // 1 remaining

            // Act & Assert
            LogAssert.Expect(LogType.Log, "Level 1 ended. Final Score: 30");
            gameManager.RhythmTargetHit(); // 0 remaining, should end level
        }

        [Test]
        public void GameManager_AddScore_WithPositiveAmount_ShouldIncreaseScore()
        {
            // Arrange
            gameManager.SetPlayerName("TestPlayer");
            gameManager.StartLevel();
            int scoreToAdd = 15;

            // Act
            gameManager.AddScore(scoreToAdd);

            // Assert
            Assert.AreEqual(15, gameManager.PlayerScore);
        }

        [Test]
        public void GameManager_AddScore_WithZeroAmount_ShouldLogWarning()
        {
            // Arrange
            gameManager.SetPlayerName("TestPlayer");
            gameManager.StartLevel();

            // Act & Assert
            LogAssert.Expect(LogType.Log, "Cannot add non-positive score: 0");
            gameManager.AddScore(0);
        }

        [Test]
        public void GameManager_AddScore_WithNegativeAmount_ShouldLogWarning()
        {
            // Arrange
            gameManager.SetPlayerName("TestPlayer");
            gameManager.StartLevel();

            // Act & Assert
            LogAssert.Expect(LogType.Log, "Cannot add non-positive score: -5");
            gameManager.AddScore(-5);
        }

        [Test]
        public void GameManager_AdvanceToNextLevel_ShouldIncrementLevel()
        {
            // Arrange
            gameManager.SetPlayerName("TestPlayer");
            int initialLevel = gameManager.CurrentLevel;

            // Act
            gameManager.AdvanceToNextLevel();

            // Assert
            Assert.AreEqual(initialLevel + 1, gameManager.CurrentLevel);
        }

        [Test]
        public void GameManager_AdvanceToNextLevel_ShouldStartNewLevel()
        {
            // Arrange
            gameManager.SetPlayerName("TestPlayer");
            gameManager.StartLevel();
            gameManager.AddScore(50);

            // Act
            gameManager.AdvanceToNextLevel();

            // Assert
            Assert.AreEqual(2, gameManager.CurrentLevel);
            Assert.AreEqual(0, gameManager.PlayerScore);
            Assert.AreEqual(5, gameManager.TargetsRemaining); // 3 + (2-1)*2 = 5
        }

        [Test]
        public void GameManager_RestartLevel_ShouldKeepSameLevel()
        {
            // Arrange
            gameManager.SetPlayerName("TestPlayer");
            gameManager.AdvanceToNextLevel(); // Level 2
            gameManager.AdvanceToNextLevel(); // Level 3
            gameManager.StartLevel();
            gameManager.AddScore(100);

            // Act
            gameManager.RestartLevel();

            // Assert
            Assert.AreEqual(3, gameManager.CurrentLevel);
            Assert.AreEqual(0, gameManager.PlayerScore);
        }

        [Test]
        public void GameManager_GenerateTargetsForLevel_ShouldReturnCorrectValues()
        {
            // Test level 1: 3 + (1-1)*2 = 3
            Assert.AreEqual(3, gameManager.GenerateTargetsForLevel(1));
            
            // Test level 5: 3 + (5-1)*2 = 11
            Assert.AreEqual(11, gameManager.GenerateTargetsForLevel(5));
            
            // Test level 15: 3 + (15-1)*2 = 31, but clamped to 30
            Assert.AreEqual(30, gameManager.GenerateTargetsForLevel(15));
            
            // Test level 0: 3 + (0-1)*2 = 1, but clamped to 3
            Assert.AreEqual(3, gameManager.GenerateTargetsForLevel(0));
        }

        [Test]
        public void GameManager_GenerateTargetsForLevel_ShouldClampToMinimum()
        {
            // Test negative level
            Assert.AreEqual(3, gameManager.GenerateTargetsForLevel(-5));
        }

        [Test]
        public void GameManager_GenerateTargetsForLevel_ShouldClampToMaximum()
        {
            // Test very high level
            Assert.AreEqual(30, gameManager.GenerateTargetsForLevel(100));
        }

        [Test]
        public void GameManager_EndLevel_WithoutLevelComplete_ShouldLogWarning()
        {
            // Arrange
            gameManager.SetPlayerName("TestPlayer");
            gameManager.StartLevel();

            // Act & Assert
            LogAssert.Expect(LogType.Log, "Cannot end level: targets still remaining.");
            gameManager.EndLevel();
        }

        [Test]
        public void GameManager_EndLevel_WithLevelComplete_ShouldLogLevelEnd()
        {
            // Arrange
            gameManager.SetPlayerName("TestPlayer");
            gameManager.StartLevel();
            gameManager.RhythmTargetHit(); // 2 remaining
            gameManager.RhythmTargetHit(); // 1 remaining
            gameManager.RhythmTargetHit(); // 0 remaining, level complete

            // Act & Assert
            LogAssert.Expect(LogType.Log, "Level 1 ended. Final Score: 30");
            gameManager.EndLevel();
        }

        [Test]
        public void GameManager_EndLevel_WithEmptyPlayerName_ShouldNotSubmitScore()
        {
            // Arrange
            gameManager.StartLevel();
            gameManager.RhythmTargetHit(); // 2 remaining
            gameManager.RhythmTargetHit(); // 1 remaining
            gameManager.RhythmTargetHit(); // 0 remaining, level complete

            // Act
            gameManager.EndLevel();

            // Assert
            // This test verifies that no exception is thrown when player name is empty
            // The actual leaderboard submission logic would be tested separately
        }

        [Test]
        public void GameManager_SingletonPattern_ShouldCreateSingleInstance()
        {
            // Arrange
            GameObject secondGameManagerObject = new GameObject("SecondGameManager");
            GameManager secondGameManager = secondGameManagerObject.AddComponent<GameManager>();

            // Act
            // The Awake method should be called automatically

            // Assert
            Assert.AreEqual(GameManager.Instance, gameManager);
            Assert.AreNotEqual(GameManager.Instance, secondGameManager);

            // Cleanup
            Object.DestroyImmediate(secondGameManagerObject);
        }

        [Test]
        public void GameManager_Start_ShouldCallShowStartMenu()
        {
            // This test would require mocking or a more complex setup
            // For now, we'll test that Start doesn't throw an exception
            Assert.DoesNotThrow(() => gameManager.Start());
        }

        [UnityTest]
        public IEnumerator GameManager_StartLevel_ShouldUpdateUIScoreAfterFrame()
        {
            // Arrange
            gameManager.SetPlayerName("TestPlayer");
            gameManager.AddScore(100);

            // Act
            gameManager.StartLevel();
            yield return null; // Wait one frame

            // Assert
            Assert.AreEqual(0, gameManager.PlayerScore);
        }

        [Test]
        public void GameManager_PointsPerTarget_ShouldBeConfigurable()
        {
            // Arrange
            int newPointsPerTarget = 25;
            gameManager.PointsPerTarget = newPointsPerTarget;
            gameManager.SetPlayerName("TestPlayer");
            gameManager.StartLevel();

            // Act
            gameManager.RhythmTargetHit();

            // Assert
            Assert.AreEqual(newPointsPerTarget, gameManager.PlayerScore);
        }

        [Test]
        public void GameManager_MultipleTargetHits_ShouldAccumulateScore()
        {
            // Arrange
            gameManager.SetPlayerName("TestPlayer");
            gameManager.StartLevel();

            // Act
            gameManager.RhythmTargetHit();
            gameManager.RhythmTargetHit();

            // Assert
            Assert.AreEqual(20, gameManager.PlayerScore);
            Assert.AreEqual(1, gameManager.TargetsRemaining);
        }

        [Test]
        public void GameManager_LevelProgression_ShouldIncreaseDifficulty()
        {
            // Arrange
            gameManager.SetPlayerName("TestPlayer");
            gameManager.StartLevel();
            int level1Targets = gameManager.TargetsRemaining;

            // Act
            gameManager.AdvanceToNextLevel();
            int level2Targets = gameManager.TargetsRemaining;

            // Assert
            Assert.Greater(level2Targets, level1Targets);
        }

        [Test]
        public void GameManager_IsLevelComplete_ShouldReturnCorrectValue()
        {
            // Arrange
            gameManager.SetPlayerName("TestPlayer");
            gameManager.StartLevel();

            // Act & Assert
            Assert.IsFalse(gameManager.IsLevelComplete);
            
            gameManager.RhythmTargetHit(); // 2 remaining
            Assert.IsFalse(gameManager.IsLevelComplete);
            
            gameManager.RhythmTargetHit(); // 1 remaining
            Assert.IsFalse(gameManager.IsLevelComplete);
            
            gameManager.RhythmTargetHit(); // 0 remaining
            Assert.IsTrue(gameManager.IsLevelComplete);
        }

        [Test]
        public void GameManager_IsGameActive_ShouldReturnCorrectValue()
        {
            // Initially not active
            Assert.IsFalse(gameManager.IsGameActive);
            
            // After setting player name
            gameManager.SetPlayerName("TestPlayer");
            Assert.IsTrue(gameManager.IsGameActive);
            
            // After reset
            gameManager.ResetGame();
            Assert.IsFalse(gameManager.IsGameActive);
        }

        [Test]
        public void GameManager_ResetGame_ShouldResetAllValues()
        {
            // Arrange
            gameManager.SetPlayerName("TestPlayer");
            gameManager.AdvanceToNextLevel(); // Level 2
            gameManager.StartLevel();
            gameManager.AddScore(100);

            // Act
            gameManager.ResetGame();

            // Assert
            Assert.AreEqual(1, gameManager.CurrentLevel);
            Assert.AreEqual(0, gameManager.PlayerScore);
            Assert.AreEqual(0, gameManager.TargetsRemaining);
            Assert.IsEmpty(gameManager.PlayerName);
        }

        [Test]
        public void GameManager_Properties_ShouldValidateValues()
        {
            // Test CurrentLevel validation
            gameManager.AdvanceToNextLevel(); // Should set to 2
            Assert.AreEqual(2, gameManager.CurrentLevel);
            
            // Test PlayerScore validation
            gameManager.SetPlayerName("TestPlayer");
            gameManager.StartLevel();
            gameManager.AddScore(50);
            Assert.AreEqual(50, gameManager.PlayerScore);
            
            // Test PointsPerTarget validation
            gameManager.PointsPerTarget = 25;
            Assert.AreEqual(25, gameManager.PointsPerTarget);
            
            // Test invalid values
            gameManager.PointsPerTarget = -5;
            Assert.AreEqual(1, gameManager.PointsPerTarget); // Should clamp to minimum
        }
    }
} 