using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Lean.Touch;
using DG.Tweening;

public class BlockTouch : MonoBehaviour
{
    public float leftRightThreshold = 3f;
    public Transform blockTransform;
    public float rotateDuration = 0.5f; // 회전 애니메이션 지속 시간

    void OnEnable()
    {
        // 스와이프 이벤트 구독
        LeanTouch.OnFingerSwipe += HandleFingerSwipe;
        LeanTouch.OnFingerOld += HandleFingerHeld;
    }

    void OnDisable()
    {
        // 스와이프 이벤트 구독 해제
        LeanTouch.OnFingerSwipe -= HandleFingerSwipe;
        LeanTouch.OnFingerOld -= HandleFingerHeld;
    }

    
    List<RaycastResult> GetUIRaycastResults(Vector2 startPos)
    {
        // PointerEventData 생성
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = startPos
        };

        // Raycast 결과를 저장할 리스트
        var results = new List<RaycastResult>();

        // Raycast 수행
        EventSystem.current.RaycastAll(pointerData, results);

        // Raycast 결과 반환
        return results;
    }

    void HandleFingerHeld(LeanFinger finger)
    {
        Vector2 startPos = finger.StartScreenPosition;
        List<RaycastResult> results = GetUIRaycastResults(startPos);
        if (results.Count > 0)
        {
            GameObject HeldUI = results[0].gameObject;
            if (HeldUI == gameObject)
            {
                Handheld.Vibrate();
                Debug.Log("Held UI Object: " + gameObject.name);
            }
        }
    }
    
    
    void HandleFingerSwipe(LeanFinger finger)
    {
        // Get the screen position where the swipe started
        Vector2 startPos = finger.StartScreenPosition;

        // Raycast results
        List<RaycastResult> results = GetUIRaycastResults(startPos);

        if (results.Count > 0)
        {
            GameObject swipedUI = results[0].gameObject;
            if (swipedUI == gameObject && !DOTween.IsTweening(blockTransform))
            {
                Vector2 swipeDelta = finger.SwipeScreenDelta;

                // y축
                if (Mathf.Abs(swipeDelta.x) > leftRightThreshold * Mathf.Abs(swipeDelta.y))
                {
                    if (swipeDelta.x > 0)
                    {
                        blockTransform.DORotate(new Vector3(0, -90, 0), rotateDuration, RotateMode.WorldAxisAdd);
                    }
                    else
                    {
                        blockTransform.DORotate(new Vector3(0, 90, 0), rotateDuration, RotateMode.WorldAxisAdd);
                    }
                }
                else
                {
                    // x축, z축
                    if (swipeDelta.x > 0 && swipeDelta.y > 0)
                    {
                        blockTransform.DORotate(new Vector3(0, 0, 90), rotateDuration, RotateMode.WorldAxisAdd);
                    }
                    else if (swipeDelta.x < 0 && swipeDelta.y > 0)
                    {
                        blockTransform.DORotate(new Vector3(-90, 0, 0), rotateDuration, RotateMode.WorldAxisAdd);
                    }
                    else if (swipeDelta.x < 0 && swipeDelta.y < 0)
                    {
                        blockTransform.DORotate(new Vector3(0, 0, -90), rotateDuration, RotateMode.WorldAxisAdd);
                    }
                    else if (swipeDelta.x > 0 && swipeDelta.y < 0)
                    {
                        blockTransform.DORotate(new Vector3(90, 0, 0), rotateDuration, RotateMode.WorldAxisAdd);
                    }
                }
            }
        }
    }
}
