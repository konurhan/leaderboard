using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class PlayerItemController : MonoBehaviour
{
    [SerializeField] private TextMeshPro rankText;
    [SerializeField] private TextMeshPro nameText;
    [SerializeField] private TextMeshPro scoreText;

    [SerializeField] private SpriteRenderer bgSprite;
    
    private Transform _playerItemParent;
    private Transform _playerMeParent;
    private PlayerData _playerData;
    public bool IsMe;
    
    public void Initialize(Transform playerItemParent, Transform playerMeParent, PlayerData playerData, int rank)
    {
        _playerItemParent = playerItemParent;
        _playerMeParent = playerMeParent;
        SetPlayerData(playerData, rank);
    }
    
    public void SetPlayerData(PlayerData playerData, int rank)
    {
        _playerData = playerData;
        IsMe = playerData.id == "me";//TODO: fetch from manager const
        rankText.text = (rank + 1).ToString();
        nameText.text = _playerData.nickname;
        scoreText.text = _playerData.score.ToString();
        if (IsMe)
        {
            bgSprite.color = Color.green;
        }
        else
        {
            bgSprite.color = Color.red;
        }
    }
}
