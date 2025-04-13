using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{
    [SerializeField] private ScrollController leaderboardScrollController;
    [SerializeField] private bool putUserInRank = false;
    [SerializeField] private int desiredRank = 1;

    private PlayerDataList _playerDataList;
    private int _myDataIndex;

    public const string MyId = "me";

    private void Awake()
    {
        Application.targetFrameRate = 60;
        TextAsset jsonText = Resources.Load<TextAsset>("sample_leaderboard");
        _playerDataList = JsonUtility.FromJson<PlayerDataList>(jsonText.text);
    }

    private void Start()
    {
        InitializeScrollController();
    }
    
    private void InitializeScrollController()
    {
        SortPlayersByScoreDescending();
        CacheMyPlayerDataIndex();
        leaderboardScrollController.Initialize(_playerDataList, _myDataIndex);
    }
    
    private void UpdateScrollController()
    {
        PlayerData myPlayerData;
        if (putUserInRank)
        {
            myPlayerData = UpdateLeaderboardPredefinedRank();
        }
        else
        {
            myPlayerData = UpdateLeaderboardRandom();
        }
        
        leaderboardScrollController.UpdateViewAndScroll(_playerDataList, myPlayerData, _myDataIndex);
    }

    private void CacheMyPlayerDataIndex()
    {
        for (int i = 0; i < _playerDataList.players.Count; i++)
        {
            if (_playerDataList.players[i].id == MyId)
            {
                _myDataIndex = i;
            }
        }
    }

    private void SortPlayersByScoreDescending()
    {
        _playerDataList.players.Sort((a, b) => b.score.CompareTo(a.score));
    }

    public PlayerData UpdateLeaderboardRandom()
    {
        LeaderboardUtils.RandomizeScores(_playerDataList);
        SortPlayersByScoreDescending();
        CacheMyPlayerDataIndex();
        PlayerData myPlayerData = LeaderboardUtils.ExtractMeFromList(_playerDataList);
        SortPlayersByScoreDescending();
        return myPlayerData;
    }
    
    public PlayerData UpdateLeaderboardPredefinedRank()
    {
        LeaderboardUtils.RandomizeScoresWithMeAtRankAndSort(_playerDataList, desiredRank);
        CacheMyPlayerDataIndex();
        PlayerData myPlayerData = LeaderboardUtils.ExtractMeFromList(_playerDataList);
        return myPlayerData;
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            UpdateScrollController();
        }
    }
#endif
}
