namespace PushTheBox.Core
{
    /// <summary>
    /// Represents the high-level states of the game lifecycle.
    /// </summary>
    public enum GameState
    {
        MainMenu,
        LevelSelect,
        Playing,
        Paused,
        LevelCompleted,
        GameOver
    }
}
