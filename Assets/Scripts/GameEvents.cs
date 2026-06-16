using System;

public static class GameEvents
{
    public static Action<GameManager.VictimType> OnMinigameStarted;
    public static Action<GameManager.VictimType> OnMinigameCompleted;
}
