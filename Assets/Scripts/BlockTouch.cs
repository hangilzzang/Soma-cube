using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Lean.Touch;

public class BlockTouch : MonoBehaviour
{
    public float leftRightThreshold = 3f;
    public Transform blockTransform;

    void OnEnable()
    {
        // 스와이프 이벤트 구독
        LeanTouch.OnFingerSwipe += HandleFingerSwipe;
    }

    void OnDisable()
    {
        // 스와이프 이벤트 구독 해제
        LeanTouch.OnFingerSwipe -= HandleFingerSwipe;
    }

    void HandleFingerSwipe(LeanFinger finger)
    {
        // Get the screen position where the swipe started
        Vector2 startPos = finger.StartScreenPosition;

        // Create a PointerEventData for raycasting
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = startPos
        };

        // Perform the raycast
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        if (results.Count > 0)
        {
            GameObject swipedUI = results[0].gameObject;
            if (swipedUI == gameObject)
            {
                Vector2 swipeDelta = finger.SwipeScreenDelta;

                // y축
                if (Mathf.Abs(swipeDelta.x) > leftRightThreshold * Mathf.Abs(swipeDelta.y))
                {
                    if (swipeDelta.x > 0)
                    {
                        blockTransform.Rotate(0, -90, 0, Space.World);
                    }
                    else
                    {
                        blockTransform.Rotate(0, 90, 0, Space.World);
                    }
                }
                else
                {
                    // x축, z축
                    if (swipeDelta.x > 0 && swipeDelta.y > 0)
                    {
                        blockTransform.Rotate(0, 0, 90, Space.World);
                    }
                    else if (swipeDelta.x < 0 && swipeDelta.y > 0)
                    {
                        blockTransform.Rotate(-90, 0, 0, Space.World);
                    }
                    else if (swipeDelta.x < 0 && swipeDelta.y < 0)
                    {
                        blockTransform.Rotate(0, 0, -90, Space.World);
                    }
                    else if (swipeDelta.x > 0 && swipeDelta.y < 0)
                    {
                        blockTransform.Rotate(90, 0, 0, Space.World);
                    }
                }
            }
        }
    }
}
