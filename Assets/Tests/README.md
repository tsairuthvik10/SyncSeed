# SyncSeed Test Suite

This directory contains comprehensive unit tests for the SyncSeed game using Unity Test Framework and NUnit.

## Test Structure

### EditMode Tests (`EditMode/`)
- **Location**: `Assets/Tests/EditMode/GameManagerTests.cs`
- **Purpose**: Fast unit tests that run in the Unity Editor without entering Play Mode
- **Use Cases**: Testing individual methods, logic validation, edge cases
- **Execution**: Runs in the Unity Editor's test runner

### PlayMode Tests (`PlayMode/`)
- **Location**: `Assets/Tests/PlayMode/GameManagerPlayModeTests.cs`
- **Purpose**: Integration tests that run in the actual Unity game loop
- **Use Cases**: Testing runtime behavior, component interactions, end-to-end scenarios
- **Execution**: Runs in Play Mode through the Unity Test Runner

## Running the Tests

### Method 1: Unity Test Runner Window
1. Open Unity Editor
2. Go to `Window > General > Test Runner`
3. Select the appropriate test mode:
   - **EditMode**: For fast unit tests
   - **PlayMode**: For runtime integration tests
4. Click "Run All" or run individual tests

### Method 2: Command Line
```bash
# Run EditMode tests
Unity.exe -batchmode -quit -projectPath "path/to/SyncSeed" -runTests -testPlatform EditMode -testResults "EditModeResults.xml"

# Run PlayMode tests
Unity.exe -batchmode -quit -projectPath "path/to/SyncSeed" -runTests -testPlatform PlayMode -testResults "PlayModeResults.xml"
```

## Test Coverage

### GameManager Tests (Refactored)

#### Core Functionality
- ✅ Initialization and default values
- ✅ Player name management with validation
- ✅ Score tracking and accumulation
- ✅ Level progression and restart
- ✅ Target generation and completion
- ✅ Event system validation

#### Properties and Validation
- ✅ Property getters/setters with validation
- ✅ Value clamping and bounds checking
- ✅ State validation (IsLevelComplete, IsGameActive)
- ✅ Input validation and error handling

#### Edge Cases
- ✅ Null reference handling
- ✅ Empty/whitespace player names
- ✅ Singleton pattern validation
- ✅ Boundary conditions for target generation
- ✅ Invalid score additions

#### Integration
- ✅ UI Manager interactions
- ✅ Level completion flow
- ✅ Score submission to leaderboard
- ✅ End-to-end game session
- ✅ Event system integration

## Test Categories

### Unit Tests (EditMode)
- `GameManager_Initialization_ShouldSetDefaultValues`
- `GameManager_SetPlayerName_WithValidName_ShouldUpdatePlayerName`
- `GameManager_SetPlayerName_WithEmptyName_ShouldReturnFalse`
- `GameManager_StartLevel_WithPlayerName_ShouldResetScoreAndSetTargets`
- `GameManager_RhythmTargetHit_WithGameActive_ShouldIncreaseScore`
- `GameManager_GenerateTargetsForLevel_ShouldReturnCorrectValues`
- `GameManager_Properties_ShouldValidateValues`

### Integration Tests (PlayMode)
- `GameManager_Awake_ShouldSetSingletonInstance`
- `GameManager_CompleteLevel_ShouldTriggerEndLevel`
- `GameManager_CompleteGameSession_ShouldWorkEndToEnd`
- `GameManager_LevelProgression_ShouldIncreaseDifficulty`
- `GameManager_Events_ShouldFireCorrectly`

### Error Handling Tests
- `GameManager_SetPlayerName_WithWhitespace_ShouldReturnFalse`
- `GameManager_StartLevel_WithoutPlayerName_ShouldLogWarning`
- `GameManager_RhythmTargetHit_WithoutGameActive_ShouldLogWarning`
- `GameManager_AddScore_WithNegativeAmount_ShouldLogWarning`
- `GameManager_EndLevel_WithoutLevelComplete_ShouldLogWarning`

### Event System Tests
- `GameManager_Events_ShouldFireCorrectly`
- `GameManager_IsLevelComplete_ShouldReturnCorrectValue`
- `GameManager_IsGameActive_ShouldUpdateCorrectly`

## Refactored GameManager Features

### New Features
- **Event System**: UnityEvents for score, level, and game state changes
- **Property Validation**: Automatic bounds checking and value clamping
- **Input Validation**: Proper validation for player names and scores
- **State Management**: Clear game state tracking with boolean properties
- **Configuration**: Inspector-configurable level difficulty settings
- **Error Handling**: Comprehensive error checking and logging

### Improvements
- **Encapsulation**: Private fields with public properties
- **Singleton Pattern**: Proper implementation with DontDestroyOnLoad
- **Dependency Injection**: Serialized field references instead of FindObjectOfType
- **Documentation**: XML documentation for all public methods
- **Validation**: OnValidate method for inspector validation
- **Reset Functionality**: Complete game state reset capability

## Best Practices Used

1. **Arrange-Act-Assert Pattern**: Clear test structure
2. **Descriptive Test Names**: Easy to understand what each test validates
3. **Proper Setup/Teardown**: Clean test environment with reflection for private fields
4. **Isolation**: Each test is independent
5. **Edge Case Coverage**: Tests boundary conditions and error scenarios
6. **Log Assertions**: Validates debug output where appropriate
7. **Event Testing**: Validates UnityEvent system integration
8. **Property Testing**: Tests getter/setter validation logic

## Adding New Tests

### For EditMode Tests:
```csharp
[Test]
public void GameManager_NewFeature_ShouldBehaveCorrectly()
{
    // Arrange
    // Act
    // Assert
}
```

### For PlayMode Tests:
```csharp
[UnityTest]
public IEnumerator GameManager_NewFeature_ShouldWorkInRuntime()
{
    // Arrange
    // Act
    yield return null; // Wait for frame if needed
    // Assert
}
```

### For Event Tests:
```csharp
[UnityTest]
public IEnumerator GameManager_NewEvent_ShouldFireCorrectly()
{
    // Arrange
    bool eventFired = false;
    gameManager.OnNewEvent.AddListener(() => eventFired = true);
    
    // Act
    gameManager.TriggerNewEvent();
    yield return null;
    
    // Assert
    Assert.IsTrue(eventFired);
}
```

## Dependencies

- Unity Test Framework (1.4.6)
- NUnit Framework
- Unity 2022.3 LTS or later

## Notes

- Tests use `Object.DestroyImmediate()` in EditMode for immediate cleanup
- Tests use `Object.Destroy()` in PlayMode to respect Unity's destruction cycle
- Singleton pattern is properly tested and cleaned up between tests
- All tests include proper null checking and error handling validation
- Reflection is used to access private fields for testing purposes
- Event system is thoroughly tested for proper integration
- Property validation is tested for bounds checking and value clamping 