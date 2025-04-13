using System;
using UnityEngine;

public class SpriteButton : MonoBehaviour
{
    private void OnMouseDown()
    {
        Debug.Log("Button clicked!");
        // Call your custom button logic here
        OnButtonClicked();
    }

    private void OnButtonClicked()
    {
        // Your button's function
        LeaderboardManager.Instance.UpdateScrollController();
    }
}