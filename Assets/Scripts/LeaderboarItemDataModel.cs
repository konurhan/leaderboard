using System.Collections.Generic;

[System.Serializable]
public class PlayerData
{
    public string id;
    public string nickname;
    public int score;
}

[System.Serializable]
public class PlayerDataList
{
    public List<PlayerData> players;
}

