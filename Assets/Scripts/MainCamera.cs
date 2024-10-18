using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Lean.Touch;
using UnityEngine.EventSystems;


public class MainCamera : MonoBehaviour
{
    public float offset = 20;
    public Vector3 target;
    // public float duration = 1f;
    public float zoomSpeed = 1f;
    public float maxOffset = 40;
    public float minOffset = 5;
    public float rotationSpeed = 0.01f;
    public Transform blocks;
    public Transform field;
    // public bool lotationAvailable = false; // 회전동작이 가능한지 여부 판별
    // 회전시킬 핸드블록들
    public Transform blockV;
    public Transform blockL;
    public Transform blockT;
    public Transform blockZ;
    public Transform blockA;
    public Transform blockB;
    public Transform blockP;
    public float rotateDuration = 0.5f; // 회전 애니메이션 지속 시간


    void Start()
    {
        // 카메라의 위치를 Vector3.zero에서 offset만큼 떨어진 위치로 설정
        transform.position = target + new Vector3(offset, offset, offset);
        // 카메라가 항상 (0, 0, 0)을 바라보도록 설정
        transform.LookAt(target);
        // 줌인/줌아웃 구현용 이벤트 구독
        LeanTouch.OnGesture += HandleGesture; // 손가락 하나이상터치시 매프레임마다 터치중인 손가락정보 리스트로 가져옴
        // 필드 회전 구현용 이벤트
        GameManager.Instance.OnFieldSwipe += HandleFieldSwipe; // 필드 스와이프 이벤트 발생시 호출

    }

    void OnDisable()
    {
        LeanTouch.OnGesture -= HandleGesture;
        GameManager.Instance.OnFieldSwipe -= HandleFieldSwipe;

    }

     // 레이트 업데이트 사용으로 줌동작이후 손가락이 모두 떨어진뒤 스와이프 이벤트가 발동할때 swipeable = false 상태를 유지할수있다
    void LateUpdate()
    {
        // 현재 터치 중인 손가락이 없다면 swipeAble
        if (LeanTouch.Fingers.Count == 0)
        {
            GameManager.Instance.swipeAble = true;
        }
    }

    void HandleGesture(List<LeanFinger> fingers)
    {
        // 손가락이 두개이상 일때만 실행
        if (fingers.Count >= 2)
        {
            // 현재 줌잉중인 손가락이 모두 떨어질때까지 swipe이벤트 팝업을 막는다
            GameManager.Instance.swipeAble = false;
        }
    }

    void HandleFieldSwipe(Vector2 swipeDelta)
    {
        if (!DOTween.IsTweening(field)) // 회전중이 아닐경우
        {
            if (swipeDelta.x > 0)
            {
                blocks.DORotate(new Vector3(0, -90, 0), rotateDuration, RotateMode.WorldAxisAdd);
                field.DORotate(new Vector3(0, -90, 0), rotateDuration, RotateMode.WorldAxisAdd);
                blockV.DORotate(new Vector3(0, -90, 0), rotateDuration, RotateMode.WorldAxisAdd);
                blockL.DORotate(new Vector3(0, -90, 0), rotateDuration, RotateMode.WorldAxisAdd);
                blockT.DORotate(new Vector3(0, -90, 0), rotateDuration, RotateMode.WorldAxisAdd);
                blockZ.DORotate(new Vector3(0, -90, 0), rotateDuration, RotateMode.WorldAxisAdd);
                blockA.DORotate(new Vector3(0, -90, 0), rotateDuration, RotateMode.WorldAxisAdd);
                blockB.DORotate(new Vector3(0, -90, 0), rotateDuration, RotateMode.WorldAxisAdd);
                blockP.DORotate(new Vector3(0, -90, 0), rotateDuration, RotateMode.WorldAxisAdd);
            }
            else
            {
                blocks.DORotate(new Vector3(0, 90, 0), rotateDuration, RotateMode.WorldAxisAdd);
                field.DORotate(new Vector3(0, 90, 0), rotateDuration, RotateMode.WorldAxisAdd);
                blockV.DORotate(new Vector3(0, 90, 0), rotateDuration, RotateMode.WorldAxisAdd);
                blockL.DORotate(new Vector3(0, 90, 0), rotateDuration, RotateMode.WorldAxisAdd);
                blockT.DORotate(new Vector3(0, 90, 0), rotateDuration, RotateMode.WorldAxisAdd);
                blockZ.DORotate(new Vector3(0, 90, 0), rotateDuration, RotateMode.WorldAxisAdd);
                blockA.DORotate(new Vector3(0, 90, 0), rotateDuration, RotateMode.WorldAxisAdd);
                blockB.DORotate(new Vector3(0, 90, 0), rotateDuration, RotateMode.WorldAxisAdd);
                blockP.DORotate(new Vector3(0, 90, 0), rotateDuration, RotateMode.WorldAxisAdd);
                
            }
        }
    }
}
