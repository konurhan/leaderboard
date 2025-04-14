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
    
    public static LeaderboardManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
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
    
    public void UpdateScrollController()
    {
        if (leaderboardScrollController.IsMoving)
        {
            return;
        }
        PlayerData myPlayerData;
        if (putUserInRank)
        {
            myPlayerData = UpdateLeaderboardPredefinedRank();
        }
        else
        {
            myPlayerData = UpdateLeaderboardRandom();
        }

        if (myPlayerData != null)
        {
            leaderboardScrollController.UpdateViewAndScroll(_playerDataList, myPlayerData, _myDataIndex);
        }
    }

    public void InsertSortAndCache(PlayerData playerData = null)
    {
        if (playerData != null)
        {
            _playerDataList.players.Add(playerData);
        }
        SortPlayersByScoreDescending();
        CacheMyPlayerDataIndex();
    }
    
    private bool CacheMyPlayerDataIndex()
    {
        for (int i = 0; i < _playerDataList.players.Count; i++)
        {
            if (_playerDataList.players[i].id == MyId)
            {
                bool myIndexChanged = _myDataIndex != i;
                _myDataIndex = i;
                return myIndexChanged;
            }
        }
        return false;
    }

    private void SortPlayersByScoreDescending()
    {
        _playerDataList.players.Sort((a, b) => b.score.CompareTo(a.score));
    }

    public PlayerData UpdateLeaderboardRandom()
    {
        LeaderboardUtils.RandomizeScores(_playerDataList);
        SortPlayersByScoreDescending();
        bool indexChanged = CacheMyPlayerDataIndex();
        if (indexChanged)
        {
            PlayerData myPlayerData = LeaderboardUtils.ExtractMeFromList(_playerDataList);
            SortPlayersByScoreDescending();
            return myPlayerData;
        }
        else
        {
            return null;
        }
    }
    
    public PlayerData UpdateLeaderboardPredefinedRank()
    {
        LeaderboardUtils.RandomizeScoresWithMeAtRankAndSort(_playerDataList, desiredRank);
        bool indexChanged = CacheMyPlayerDataIndex();
        if (indexChanged)
        {
            return LeaderboardUtils.ExtractMeFromList(_playerDataList);
        }
        else
        {
            return null;
        }
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
