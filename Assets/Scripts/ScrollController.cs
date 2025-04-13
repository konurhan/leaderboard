using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class ScrollController : MonoBehaviour
{
    [SerializeField] private SpriteMask viewPortAreaPlaceholder;
    [SerializeField] private int visibleItemCount = 5;
    [SerializeField] private int totalItemCount = 7;
    [SerializeField] private float verticalDistance = 1;
    [SerializeField] private float scrollContentUpdateDistance = 1;
    //[SerializeField] private float scrollStepDuration = 0.1f;
    [SerializeField] private float scrollTotalDuration = 3f;
    [SerializeField] private GameObject scrollItemPrefab;
    [SerializeField] private int maxStepCount = 20;
    [SerializeField] private float scrollStepDurationMax = 0.3f;
    
    [SerializeField] private Transform scrollableItemContainer;
    [SerializeField] private Transform detachedItemContainer;
    
    private PlayerDataList _playerDataList;
    // int _myIndex;
    private int _currentCenteredIndex;
    private PlayerData _myPlayerData;
    private List<PlayerItemController> _playerItemViewList = new List<PlayerItemController>();//TODO: should be generic

    [SerializeField] private TextMeshProUGUI centeredText;
    [SerializeField] private TextMeshProUGUI scrollPos;

    private void Update()
    {
        scrollPos.text = "LocalPos.y: " + scrollableItemContainer.transform.localPosition.y.ToString();
    }

    public void Initialize(PlayerDataList playerDataList, int myIndex)
    {
        _playerDataList = playerDataList;
        float topPos = (int)(totalItemCount / 2) * verticalDistance;
        for (int i = 0; i < totalItemCount; i++)
        {
            GameObject item = Instantiate(scrollItemPrefab, scrollableItemContainer);
            item.transform.localPosition = Vector3.up * (topPos - verticalDistance * i);
            _playerItemViewList.Add(item.GetComponent<PlayerItemController>());
        }
        SetItemViews(myIndex);
    }
    
    private void SetItemViews(int centeredIndex)
    {
        centeredText.text = "Centered: "+centeredIndex.ToString();
        _currentCenteredIndex = centeredIndex;
        //int startIndex = Math.Max(centeredIndex - totalItemCount / 2, 0);
        //int endIndex = Math.Min(startIndex + totalItemCount - 1, _playerDataList.players.Count - 1);
        int startIndex = centeredIndex - totalItemCount / 2;
        int endIndex = startIndex + totalItemCount - 1;
        for (int i = 0; i < endIndex - startIndex + 1; i++)
        {
            if (startIndex + i < 0 || startIndex + i >= _playerDataList.players.Count)
            {
                _playerItemViewList[i].gameObject.SetActive(false);
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
    
    private void UpdateItemViews(int centeredIndex, bool rankUp)
    {
        centeredText.text = "Centered: "+centeredIndex.ToString();
        _currentCenteredIndex = centeredIndex;
        int startIndex = centeredIndex - totalItemCount / 2;
        int endIndex = startIndex + totalItemCount - 1;
        
        for (int i = 0; i < endIndex - startIndex + 1; i++)
        {
            if (startIndex + i < 0 || startIndex + i >= _playerDataList.players.Count)
            {
                _playerItemViewList[i].gameObject.SetActive(false);
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
        
        PlayerData myDataNew = playerDataListNew.players[myIndexNew];
        PlayerItemController myPlayerItemView = _playerItemViewList.Find(p => p.IsMe);
        GameObject detachedCopy = Instantiate(myPlayerItemView.gameObject, detachedItemContainer);//TODO:tween myplayeritemview to localpos = 0 if its on the way
        
        int verticalDisplacementSteps = Math.Abs(targetCenteredIndex - _currentCenteredIndex);
        StartCoroutine(ScrollRoutine(_currentCenteredIndex, verticalDisplacementSteps, targetCenteredIndex - _currentCenteredIndex > 0));
        _currentCenteredIndex = targetCenteredIndex;
    }

    private IEnumerator ScrollRoutine(int centeredIndex, int stepCount, bool moveScrollUp)
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
            UpdateItemViews(centeredIndex + totalSteps * direction, !moveScrollUp);
        }
        
        yield break;
    }
    
}
