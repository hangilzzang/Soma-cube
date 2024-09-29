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
    public Transform blockTransform; // 핸드 블록 트랜스폼 컴포넌트
    public float rotateDuration = 0.5f; // 회전 애니메이션 지속 시간
    public GameObject blockPrefab; // 필드배치 블록 프리팹
    public bool draging = false;
    public int offset = 2; // 배치위치 조절 프리팹
    public GameObject fieldBlock; // 필드에 실제로 배치되는 블록

    void OnEnable()
    {
        // 스와이프 이벤트 구독
        LeanTouch.OnFingerSwipe += HandleFingerSwipe;
        LeanTouch.OnFingerOld += HandleFingerHeld;
        LeanTouch.OnFingerUpdate += HandleFingerUpdate;
        LeanTouch.OnFingerUp += HandleFingerUp;
    }

    void OnDisable()
    {
        // 스와이프 이벤트 구독 해제
        LeanTouch.OnFingerSwipe -= HandleFingerSwipe;
        LeanTouch.OnFingerOld -= HandleFingerHeld;
        LeanTouch.OnFingerUpdate -= HandleFingerUpdate;
        LeanTouch.OnFingerUp -= HandleFingerUp;
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

    // 특정좌표(평면)에 블럭을 올리려할때 위치 계산
        public Vector3 CalculateBlockPlacement(Transform blockTransform, Vector3 planePosition)
    {
        // 블럭의 크기와 중심을 계산
        // Vector3 blockCenter = blockTransform.GetComponent<Renderer>().bounds.center;
        Vector3 blockExtents = blockTransform.GetComponent<Renderer>().bounds.extents;

        // 블럭의 중심과 최상단 간의 거리 계산
        float topDistance = blockExtents.y;

        // 평면 위치에 블럭을 배치할 때, 중심이 평면 위로 오도록 위치 조정
        Vector3 placementPosition = new Vector3(planePosition.x, planePosition.y + topDistance, planePosition.z);

        return placementPosition;
    }


    
    void HandleFingerHeld(LeanFinger finger)
    {
        Vector2 startPos = finger.StartScreenPosition;
        Vector2 EndPos = finger.LastScreenPosition;

        List<RaycastResult> startResults = GetUIRaycastResults(startPos);
        List<RaycastResult> endResults = GetUIRaycastResults(EndPos);
        
        if (startResults.Count > 0 && endResults.Count > 0)
        {
            GameObject heldUI1 = startResults[0].gameObject;
            GameObject heldUI2 = endResults[0].gameObject;
            if (heldUI1 == gameObject && heldUI2 == gameObject) // 꾹터치 만족 조건
            {
                // 안보이는곳에 소환
                fieldBlock = Instantiate(blockPrefab, new Vector3 (0, -5, 0) ,blockTransform.rotation);
                // offset값 찾기(특정좌표위에 올라가도록 블럭을 배치하고싶을경우 계산해야하는 offset값)
                int lowestY = -5;
                foreach (Transform child in fieldBlock.transform)
                {
                    if ((int)child.position.x == 0 && (int)child.position.z == 0)
                    {
                        if (child.position.y < lowestY)
                        {
                            lowestY = (int)child.position.y; // 갱신됨
                        }
                    }
                }
                // Debug.Log(lowestY);
                offset = (-5 - lowestY) + 2;
                draging = true;
                Debug.Log(offset);
            }
        }
    }

    void HandleFingerUpdate(LeanFinger finger)
    {
        if (draging)
        {
            // Debug.Log(finger.ScreenPosition);
        }
    }

    void HandleFingerUp(LeanFinger finger)
    {
        if (draging)
        {
            Debug.Log("drag finished");
            draging = false;
        }
    }


    
    
    void HandleFingerSwipe(LeanFinger finger)
    {
        // Get the screen position where the swipe started
        Vector2 startPos = finger.StartScreenPosition;
        // Raycast results
        List<RaycastResult> startResults = GetUIRaycastResults(startPos);

        if (startResults.Count > 0)
        {
            GameObject swipedUI = startResults[0].gameObject;
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
