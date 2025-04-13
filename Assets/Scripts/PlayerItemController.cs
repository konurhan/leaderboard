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
    
    private Coroutine _scoreDiffRoutine;
    
    public void Initialize(Transform playerItemParent, Transform playerMeParent, PlayerData playerData, int rank)
    {
        _playerItemParent = playerItemParent;
        _playerMeParent = playerMeParent;
        SetPlayerData(playerData, rank);
    }

    public void Uninitialize()
    {
        _playerItemParent = null;
        _playerMeParent = null;
        _playerData = null;
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

    public string GetId()
    {
        if (_playerData == null)
        {
            return "";
        }
        return _playerData.id;
    }

    public void PlayScoreTween(int targetScore, float duration)
    {
        if (_scoreDiffRoutine != null)
        {
            StopCoroutine(_scoreDiffRoutine);
        }
        _scoreDiffRoutine = StartCoroutine(ChangeScoreText(targetScore, duration));
    }

    private IEnumerator ChangeScoreText(int targetScore, float duration, float refreshRate = 30f)
    {
        int startScore = int.Parse(scoreText.text);
        float elapsed = 0f;
        float refreshInterval = 1f / refreshRate;
        float lastUpdate = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            if (elapsed - lastUpdate >= refreshInterval)
            {
                lastUpdate = elapsed;

                float t = Mathf.Clamp01(elapsed / duration);
                int currentDisplayScore = Mathf.RoundToInt(Mathf.Lerp(startScore, targetScore, t));
                scoreText.text = currentDisplayScore.ToString();
            }

            yield return null;
        }
        
        scoreText.text = targetScore.ToString();
        _scoreDiffRoutine = null;
    }


}
