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
    public float zoomSpeed = 50f;
    public float maxOffset = 45;
    public float minOffset = 4;
    public float rotationSpeed = 0.1f;
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
    public Vector3 newTarget; // 카메라 타겟(월드좌표)
    public Transform CameraRotator;


    void Start()
    {
        // 타겟을 기준으로 카메라 로테이터 위치 설정
        CameraRotator.position = target + new Vector3(0f, offset, 0f);
        // 카메라 로테이터를 기준으로 카메라 위치설정
        transform.position = CameraRotator.position + new Vector3(offset, 0f, offset);
        // 카메라가 항상 (0, 0, 0)을 바라보도록 설정
        transform.LookAt(target);
        // 줌인/줌아웃 구현용 이벤트 구독
        LeanTouch.OnGesture += HandleGesture; // 손가락 하나이상터치시 매프레임마다 터치중인 손가락정보 리스트로 가져옴
        // 필드 회전 구현용 이벤트
        GameManager.Instance.OnFieldSwipe += HandleFieldSwipe; // 필드 스와이프 이벤트 발생시 호출
        // 블록배치 이벤트 구독
        GameManager.Instance.OnBlockPlaced += HandleBlockPlaced; 

    }

    void OnDisable()
    {
        LeanTouch.OnGesture -= HandleGesture;
        GameManager.Instance.OnFieldSwipe -= HandleFieldSwipe;
        GameManager.Instance.OnBlockPlaced -= HandleBlockPlaced; 

    }

    void HandleBlockPlaced()
    {
        // 새로운 타겟 월드좌표계로 변환
        newTarget = blocks.TransformPoint(GameManager.Instance.CameraTarget);
        // offset을 바탕으로 새 포지션 계산하기
        Vector3 newPosition = newTarget + new Vector3(0f, offset, 0f);
        // 카메라 로테이터 이동(카메라는 로테이터의 자식이라 카메라도 이동됨)
        CameraRotator.DOMove(newPosition, rotateDuration).SetEase(Ease.InOutQuad); 
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
        // 핸드블록이 회전중이 아니라면
        if (!DOTween.IsTweening(blockV)
            && !DOTween.IsTweening(blockL)
            && !DOTween.IsTweening(blockT)
            && !DOTween.IsTweening(blockZ)
            && !DOTween.IsTweening(blockA)
            && !DOTween.IsTweening(blockB)
            && !DOTween.IsTweening(blockP)) 
        {
            if (swipeDelta.x > 0)
            {
                GameManager.Instance.fieldRotateAngle = (GameManager.Instance.fieldRotateAngle + 90 + 360) % 360; // 현재 필드각도 조절
                // Debug.Log(GameManager.Instance.fieldRotateAngle);

                Sequence sequence = DOTween.Sequence();

                // 카메라 회전 및 타겟 바라보기
                sequence.Join(CameraRotator.DORotate(new Vector3(0, 90, 0), rotateDuration, RotateMode.WorldAxisAdd));
                // sequence.Join(transform.DOLookAt(newTarget, rotateDuration));

                // 블록들 회전
                sequence.Join(blockV.DORotate(new Vector3(0, -90, 0), rotateDuration, RotateMode.WorldAxisAdd));
                sequence.Join(blockL.DORotate(new Vector3(0, -90, 0), rotateDuration, RotateMode.WorldAxisAdd));
                sequence.Join(blockT.DORotate(new Vector3(0, -90, 0), rotateDuration, RotateMode.WorldAxisAdd));
                sequence.Join(blockZ.DORotate(new Vector3(0, -90, 0), rotateDuration, RotateMode.WorldAxisAdd));
                sequence.Join(blockA.DORotate(new Vector3(0, -90, 0), rotateDuration, RotateMode.WorldAxisAdd));
                sequence.Join(blockB.DORotate(new Vector3(0, -90, 0), rotateDuration, RotateMode.WorldAxisAdd));
                sequence.Join(blockP.DORotate(new Vector3(0, -90, 0), rotateDuration, RotateMode.WorldAxisAdd));

                // 시퀀스 실행
                sequence.Play();
            }
            else
            {
                GameManager.Instance.fieldRotateAngle = (GameManager.Instance.fieldRotateAngle - 90 + 360) % 360; // 현재 필드각도 조절
                // Debug.Log(GameManager.Instance.fieldRotateAngle);

                Sequence sequence = DOTween.Sequence();

                // 카메라 회전 및 타겟 바라보기
                sequence.Join(CameraRotator.DORotate(new Vector3(0, -90, 0), rotateDuration, RotateMode.WorldAxisAdd));
                // sequence.Join(transform.DOLookAt(newTarget, rotateDuration));

                // 블록들 회전
                sequence.Join(blockV.DORotate(new Vector3(0, 90, 0), rotateDuration, RotateMode.WorldAxisAdd));
                sequence.Join(blockL.DORotate(new Vector3(0, 90, 0), rotateDuration, RotateMode.WorldAxisAdd));
                sequence.Join(blockT.DORotate(new Vector3(0, 90, 0), rotateDuration, RotateMode.WorldAxisAdd));
                sequence.Join(blockZ.DORotate(new Vector3(0, 90, 0), rotateDuration, RotateMode.WorldAxisAdd));
                sequence.Join(blockA.DORotate(new Vector3(0, 90, 0), rotateDuration, RotateMode.WorldAxisAdd));
                sequence.Join(blockB.DORotate(new Vector3(0, 90, 0), rotateDuration, RotateMode.WorldAxisAdd));
                sequence.Join(blockP.DORotate(new Vector3(0, 90, 0), rotateDuration, RotateMode.WorldAxisAdd));

                // 시퀀스 실행
                sequence.Play();
            }
        }
    }
}
