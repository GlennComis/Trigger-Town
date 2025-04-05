using System.Collections.Generic;
using UnityEngine;

public class RewardSystemController : MonoBehaviour
{
    private const int BountyReward = 6;
    private const int FlawlessBonus = 3;
    private const int FastDrawBonus = 4;

    private List<Reward> rewards = new List<Reward>();

    public void CalculateRewards(bool flawless, float reactionTime, int streak)
    {
        rewards.Clear(); // Reset previous rewards

        // Base Bounty (Always Given)
        rewards.Add(new Reward(RewardType.Bounty, BountyReward));

        // Flawless Victory
        if (flawless) rewards.Add(new Reward(RewardType.Flawless, FlawlessBonus));

        // Fast Draw (Reaction Time Bonus)
        if (reactionTime < 0.5f) rewards.Add(new Reward(RewardType.FastDraw, FastDrawBonus));

        // Streak Bonus (Scaling with Streak)
        if (streak > 2) rewards.Add(new Reward(RewardType.Streak, streak));
        
    }
    
    public List<Reward> GetRewards()
    {
        return rewards;
    }
}

[System.Serializable]
public class Reward
{
    public RewardType type;
    public int amount;

    public Reward(RewardType type, int amount)
    {
        this.type = type;
        this.amount = amount;
    }
}

public enum RewardType
{
    Bounty,
    Flawless,
    FastDraw,
    Streak
}