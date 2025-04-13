using System.Collections.Generic;
using UnityEngine;

public class LeaderboardUtils
{
    public static void RandomlyIncreaseScores(PlayerDataList playerDataList)
    {
        foreach (var player in playerDataList.players)
        {
            int increase = Random.Range(0, 21); // Range is [0, 21) → inclusive 0 to 20
            player.score += increase;
        }
    }
    
    public static void RandomizeScores(PlayerDataList playerDataList, int minScore = 500, int maxScore = 3000)
    {
        foreach (var player in playerDataList.players)
        {
            player.score = Random.Range(minScore, maxScore + 1); // Inclusive upper bound
        }
    }
    
    public static void RandomizeScoresWithMeAtRankAndSort(PlayerDataList playerDataList, int desiredRank)
    {
        // Clamp the desired rank within valid range
        desiredRank = Mathf.Clamp(desiredRank, 1, playerDataList.players.Count);

        // Separate "me" from the list
        PlayerData me = playerDataList.players.Find(p => p.id == "me");
        playerDataList.players.Remove(me);

        // Randomize scores for everyone else
        System.Random rand = new System.Random();
        foreach (var player in playerDataList.players)
        {
            player.score = rand.Next(100, 1000);
        }

        // Sort others by score descending
        playerDataList.players.Sort((a, b) => b.score.CompareTo(a.score));

        // Determine what score "me" needs to be at the desired rank
        int meScore;
        if (desiredRank == 1)
        {
            meScore = playerDataList.players[0].score + 1;
        }
        else if (desiredRank > playerDataList.players.Count)
        {
            meScore = playerDataList.players[playerDataList.players.Count - 1].score - 1;
        }
        else
        {
            int scoreAbove = playerDataList.players[desiredRank - 2].score;
            int scoreBelow = playerDataList.players[desiredRank - 1].score;
            meScore = (scoreAbove + scoreBelow) / 2;
        }

        me.score = meScore;

        // Insert "me" back
        playerDataList.players.Add(me);

        // Final sort including "me"
        playerDataList.players.Sort((a, b) => b.score.CompareTo(a.score));
    }
    
    public static PlayerData ExtractMeFromList(PlayerDataList playerDataList)
    {
        PlayerData me = playerDataList.players.Find(p => p.id == "me");
        if (me != null)
        {
            playerDataList.players.Remove(me);
        }
        return me;
    }

}