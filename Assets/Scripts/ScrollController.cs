using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class ScrollController : MonoBehaviour
{
    [SerializeField] private Transform scrollableItemContainer;
    [SerializeField] private Transform detachedItemContainer;
    
    [Header("Scroll Settings")]
    [SerializeField] private SpriteMask viewPortAreaPlaceholder;
    [SerializeField] private int visibleItemCount = 5;
    [SerializeField] private int totalItemCount = 7;
    [SerializeField] private float verticalDistance = 1;
    [SerializeField] private float scrollContentUpdateDistance = 1;
    [SerializeField] private float scrollTotalDuration = 3f;
    [SerializeField] private GameObject scrollItemPrefab;
    [SerializeField] private int maxStepCount = 20;
    [SerializeField] private float scrollStepDurationMax = 0.3f;
    [SerializeField] private float meItemPlacementDuration = 0.2f;
    [SerializeField] private float meItemPlacementDelay = 0.8f;
    [SerializeField] private float meItemMovingScale = 1.2f;
    [SerializeField] private float meItemGetNearTargetDuration = 0.5f;
    
    
    private PlayerDataList _playerDataList;
    private int _currentCenteredIndex;
    private int _myIndex;
    private PlayerData _myPlayerData;
    private List<PlayerItemController> _playerItemViewList = new List<PlayerItemController>();//TODO: should be generic

    //[SerializeField] private TextMeshProUGUI centeredText;
    //[SerializeField] private TextMeshProUGUI scrollPos;

    private void Update()
    {
        //scrollPos.text = "LocalPos.y: " + scrollableItemContainer.transform.localPosition.y.ToString();
    }

    public void Initialize(PlayerDataList playerDataList, int startIndex)
    {
        _playerDataList = playerDataList;
        _myIndex = startIndex;
        _currentCenteredIndex = GetInitialCenteredIndex(startIndex);
        float topPos = (int)(totalItemCount / 2) * verticalDistance;
        for (int i = 0; i < totalItemCount; i++)
        {
            GameObject item = Instantiate(scrollItemPrefab, scrollableItemContainer);
            item.transform.localPosition = Vector3.up * (topPos - verticalDistance * i);
            _playerItemViewList.Add(item.GetComponent<PlayerItemController>());
        }
        SetItemViews(_currentCenteredIndex);
    }

    private int GetInitialCenteredIndex(int startIndex)
    {
        if (startIndex < visibleItemCount / 2)
        {
            return visibleItemCount / 2;
        }

        if (startIndex > _playerDataList.players.Count - visibleItemCount/2)
        {
            return _playerDataList.players.Count - visibleItemCount / 2;
        }
         return startIndex;
    }
    
    private void FixItemPositions()
    {
        float topPos = (int)(totalItemCount / 2) * verticalDistance;
        for (int i = 0; i < totalItemCount; i++)
        {
            GameObject item = _playerItemViewList[i].gameObject;
            item.transform.localPosition = Vector3.up * (topPos - verticalDistance * i);
        }
    }
    
    private void SetItemViews(int centeredIndex)
    {
        //centeredText.text = "Centered: "+centeredIndex.ToString();
        _currentCenteredIndex = centeredIndex;
        int startIndex = centeredIndex - totalItemCount / 2;
        int endIndex = startIndex + totalItemCount - 1;
        for (int i = 0; i < endIndex - startIndex + 1; i++)
        {
            if (startIndex + i < 0 || startIndex + i >= _playerDataList.players.Count)
            {
                _playerItemViewList[i].gameObject.SetActive(false);
                _playerItemViewList[i].Uninitialize();
                continue;
            }
            else
            {
                _playerItemViewList[i].gameObject.SetActive(true);
            }
            _playerItemViewList[i].Initialize(scrollableItemContainer, detachedItemContainer,
                                    _playerDataList.players[startIndex + i], startIndex + i);
        }
    }
    
    private void UpdateItemViews(int centeredIndex)
    {
        //centeredText.text = "Centered: "+centeredIndex.ToString();
        _currentCenteredIndex = centeredIndex;
        int startIndex = centeredIndex - totalItemCount / 2;
        int endIndex = startIndex + totalItemCount - 1;
        
        for (int i = 0; i < endIndex - startIndex + 1; i++)
        {
            if (startIndex + i < 0 || startIndex + i >= _playerDataList.players.Count)
            {
                _playerItemViewList[i].gameObject.SetActive(false);
                _playerItemViewList[i].Uninitialize();
                continue;
            }
            else
            {
                _playerItemViewList[i].gameObject.SetActive(true);
            }
            
            _playerItemViewList[i].Initialize(scrollableItemContainer, detachedItemContainer,
                                    _playerDataList.players[startIndex + i], startIndex + i);
        }
    }
    
    public void UpdateViewAndScroll(PlayerDataList playerDataListNew, PlayerData myPlayerDataNew, int myIndexNew)//TODO: test for player count < visibleitemCount
    {
        int targetCenteredIndex = _currentCenteredIndex;
        if (playerDataListNew.players.Count > visibleItemCount)
        {
            if (myIndexNew > playerDataListNew.players.Count - visibleItemCount / 2)
            {
                targetCenteredIndex = playerDataListNew.players.Count - visibleItemCount / 2;
            }
            else if (myIndexNew < visibleItemCount / 2)
            {
                targetCenteredIndex = visibleItemCount / 2;
            }
            else
            {
                if (myIndexNew < _currentCenteredIndex)//center on item below the target while ranking up
                {
                    targetCenteredIndex = myIndexNew + 1;
                }
                else if (myIndexNew > _currentCenteredIndex)//center on item above the target while ranking down
                {
                    targetCenteredIndex = myIndexNew - 1;
                }
            }
        }

        if (_currentCenteredIndex == myIndexNew)
        {
            return;
        }
        
        PlayerItemController myPlayerItemView = _playerItemViewList.Find(p => p.IsMe);
        GameObject detachedCopy = Instantiate(myPlayerItemView.gameObject, detachedItemContainer);

        UpdateItemViews(_currentCenteredIndex);//can update view after detaching my player item
        
        if (Math.Abs(targetCenteredIndex - _currentCenteredIndex) > totalItemCount / 2)
        {
            detachedCopy.transform.DOLocalMove(Vector3.zero, 0.5f);//TODO:tween myplayeritemview to localpos = 0 if its on the way
        }
        
        detachedCopy.transform.DOScale(Vector3.one * meItemMovingScale, 0.5f);//scale up when starting to move
        
        int verticalDisplacementSteps = Math.Abs(targetCenteredIndex - _currentCenteredIndex);
        //bool rankUp = targetCenteredIndex < _currentCenteredIndex;
        bool rankUp = myIndexNew < _myIndex;
        bool canReachNearTarget = Math.Abs(targetCenteredIndex - myIndexNew) == 1; 
        StartCoroutine(ScrollRoutine(_currentCenteredIndex, verticalDisplacementSteps,
            !rankUp, canReachNearTarget,
            detachedCopy.GetComponent<PlayerItemController>(), myPlayerDataNew.score,
            () => InsertDetachedMyPlayerItem(detachedCopy, myPlayerDataNew, myIndexNew, rankUp, canReachNearTarget)));
        //_currentCenteredIndex = targetCenteredIndex;
    }

    private void InsertDetachedMyPlayerItem(GameObject detachedItem, PlayerData myPlayerDataNew, int myIndexNew, bool rankUp, bool canReachNearTarget)
    {
        Action<int> onTweenComplete = (int occupantIndex) =>
        {
            LeaderboardManager.Instance.InsertSortAndCache(myPlayerDataNew);
            UpdateItemViews(_currentCenteredIndex);
            detachedItem.transform.SetParent(scrollableItemContainer.transform, true);
            detachedItem.transform.SetSiblingIndex(occupantIndex);
            FixItemPositions();
            _myIndex = myIndexNew;
        };
        
        if (_playerItemViewList.Count >= totalItemCount)
        {
            if (rankUp)
            {
                PlayerItemController playerItemViewLast = _playerItemViewList[^1];
                _playerItemViewList.RemoveAt(_playerItemViewList.Count - 1);
                Destroy(playerItemViewLast.gameObject);
            }
            else
            {
                PlayerItemController playerItemViewFirst = _playerItemViewList[0];
                _playerItemViewList.RemoveAt(0);
                Destroy(playerItemViewFirst.gameObject);
            }
        }
        int occupantItemIndex = FindOccupantItemIndex(myIndexNew);
        Vector3 occupantItemPos = _playerItemViewList[occupantItemIndex].transform.position;

        detachedItem.transform.DOKill();
        bool applyGetNearDelay = false;
        if (!canReachNearTarget)//first get near target position
        {
            applyGetNearDelay = true;
            Vector3 nearTargetPos = rankUp
                ? new Vector3(0, -verticalDistance * 0.8f, 0)
                : new Vector3(0, verticalDistance * 0.8f, 0);
            detachedItem.transform.DOMove(occupantItemPos + nearTargetPos, meItemGetNearTargetDuration).SetEase(Ease.OutCirc);
        }
        float delay = applyGetNearDelay ? meItemGetNearTargetDuration + meItemPlacementDelay : meItemPlacementDelay;
        
        if (rankUp)
        {
            List<Transform> itemsToMoveDown = new List<Transform>();
            for (int i = occupantItemIndex; i < _playerItemViewList.Count; i++)
            {
                itemsToMoveDown.Add(_playerItemViewList[i].transform);
            }
            _playerItemViewList.Insert(occupantItemIndex, detachedItem.GetComponent<PlayerItemController>());
            
            foreach (var item in itemsToMoveDown)
            {
                item.DOMoveY(item.transform.position.y - verticalDistance, meItemPlacementDuration).SetDelay(delay);
            }
            detachedItem.transform.DOMove(occupantItemPos, meItemPlacementDuration).SetDelay(delay).onComplete +=
                () => onTweenComplete(occupantItemIndex);
            detachedItem.transform.DOScale(Vector3.one, meItemPlacementDuration).SetDelay(delay);
        }
        else
        {
            List<Transform> itemsToMoveUp = new List<Transform>();
            for (int i = occupantItemIndex; i >= 0; i--)
            {
                itemsToMoveUp.Add(_playerItemViewList[i].transform);
            }
            _playerItemViewList.Insert(occupantItemIndex, detachedItem.GetComponent<PlayerItemController>());
            
            foreach (var item in itemsToMoveUp)
            {
                item.DOMoveY(item.transform.position.y + verticalDistance, meItemPlacementDuration).SetDelay(delay);
            }
            detachedItem.transform.DOMove(occupantItemPos, meItemPlacementDuration).SetDelay(delay).onComplete +=
                () => onTweenComplete(occupantItemIndex);
            detachedItem.transform.DOScale(Vector3.one, meItemPlacementDuration).SetDelay(delay);
        }
    }

    private int FindOccupantItemIndex(int playerIndex)
    {
        if (!_playerItemViewList.Find(p => p.GetId() == _playerDataList.players[playerIndex].id))
        {
            Debug.LogError("occupantItem is null");
        }
        return _playerItemViewList.FindIndex(p => p.GetId() == _playerDataList.players[playerIndex].id);
    }
    
    private IEnumerator ScrollRoutine(int centeredIndex, int stepCount, bool moveScrollUp, bool canReachNearTarget,
        PlayerItemController detachedMyItem, int targetScore, Action onComplete = null)
    {
        int totalSteps = 0;
        int direction = moveScrollUp ? 1 : -1;
        Vector3 scrollStartPosition = scrollableItemContainer.localPosition;
        int effectiveStepCount;
        int stepItemCount = 0;
        int stepItemRemainingCount = 0;
        if (stepCount > maxStepCount)
        {
            effectiveStepCount = maxStepCount;
            stepItemCount = stepCount / maxStepCount;
            stepItemRemainingCount = stepCount % maxStepCount;
        }
        else
        {
            effectiveStepCount = stepCount;
        }
        float scrollStepDuration = Mathf.Min(scrollTotalDuration / effectiveStepCount, scrollStepDurationMax);
        
        detachedMyItem.PlayScoreTween(targetScore, scrollStepDuration * effectiveStepCount);
        
        while (totalSteps < stepCount)
        {
            Ease ease = Ease.Linear;
            if (totalSteps == 0)
            {
                ease = Ease.Linear;
            }else if (totalSteps == stepCount - 1)
            {
                ease = Ease.Linear;
            }
            yield return scrollableItemContainer.transform.DOLocalMoveY(scrollStartPosition.y + scrollContentUpdateDistance * direction, scrollStepDuration)
                .SetEase(ease).WaitForCompletion();
                
            if (stepItemCount > 0)//recalculate on tween complete
            {
                totalSteps += stepItemCount;
                if (stepItemRemainingCount > 0)
                {
                    totalSteps++;
                    stepItemRemainingCount--;
                }
            }
            else
            {
                totalSteps++;
            }
            scrollableItemContainer.transform.localPosition = scrollStartPosition;
            UpdateItemViews(centeredIndex + totalSteps * direction);
        }
        
        onComplete?.Invoke();
        yield break;
    }
    
}
