using System;

[Serializable]
public class LeaderboardEntry
{
    public string playerName;
    public int score;
    public int levelReached;
    public bool completedGame;
    public float timeSeconds;
    public long createdAt;

    public LeaderboardEntry(
        string playerName,
        int score,
        int levelReached,
        bool completedGame,
        float timeSeconds,
        long createdAt
    )
    {
        this.playerName = playerName;
        this.score = score;
        this.levelReached = levelReached;
        this.completedGame = completedGame;
        this.timeSeconds = timeSeconds;
        this.createdAt = createdAt;
    }
}
