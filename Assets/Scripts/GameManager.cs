using Unity.VisualScripting;
using UnityEngine;

public static class GameManager
{
    public enum GameState
    {
        Title,
        InGame,
        GameOver,
    };

    public static GameState gameState =  GameState.Title;

    public static GameStateChangeLog ChangeGameState(GameState nextState)
    {
        GameStateChangeLog returnState = new GameStateChangeLog { beforeState = gameState, afterState = nextState};
        gameState = nextState;
        return returnState;
    }
}

public struct GameStateChangeLog
{
    public GameManager.GameState beforeState;
    public GameManager.GameState afterState;
}
