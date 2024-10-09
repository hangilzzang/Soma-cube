using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Lean.Touch;

public class MainCamera : MonoBehaviour
{
    public float offset = 20;
    public Vector3 target;
    public float duration = 1f;
    public float zoomSpeed = 1f;
    public float maxOffset = 40;
    public float minOffset = 5;


    void Start()
    {
        // 카메라의 위치를 Vector3.zero에서 offset만큼 떨어진 위치로 설정
        transform.position = target + new Vector3(offset, offset, offset);
        // 카메라가 항상 (0, 0, 0)을 바라보도록 설정
        transform.LookAt(target);
        // 블록배치 이벤트 구독
        GameManager.Instance.OnBlockPlaced += HandleBlockPlaced;  
        // 줌인/줌아웃 구현용 이벤트 구독
        LeanTouch.OnGesture += HandleGesture; // 손가락 하나이상터치시 매프레임마다 터치중인 손가락정보 리스트로 가져옴
    }

    void OnDisable()
    {
        GameManager.Instance.OnBlockPlaced -= HandleBlockPlaced;  
        LeanTouch.OnGesture -= HandleGesture;
    }
    
    // 카메라 위치 이동
    void HandleBlockPlaced()
    {
        // 새로운 타겟 불러오기
        Vector3 newTarget = GameManager.Instance.blocksCenter;
        // offset을 바탕으로 새 포지션 계산하기
        Vector3 newPosition = newTarget + new Vector3(offset, offset, offset);
        // 이동
        transform.DOMove(newPosition, duration).SetEase(Ease.InOutQuad); 
    }

    void HandleGesture(List<LeanFinger> fingers)
    {
        // 손가락이 딱 두개 일때만 실행
        if (fingers.Count == 2)
        {
            // 두 손가락의 핀치 스케일을 계산
            float pinchScale = LeanGesture.GetPinchScale(fingers);
            if (pinchScale != 1.0f)
            {
                // 줌 오프셋 계산
                float zoomOffset = -(pinchScale - 1.0f) * zoomSpeed;
                // 줌오프셋이 적용된 새로운 포지션 계산
                Vector3 newPosition = transform.position + new Vector3(zoomOffset, zoomOffset, zoomOffset);
                // 새로운 포지션과 원래 타겟과의 거리 계산
                float diff = (newPosition - GameManager.Instance.blocksCenter).x;
                
                if (diff < minOffset) // 만약 최저 offset보다 더 가깝다면 최저 offset만큼만 떨어지도록 함
                {
                    transform.position = GameManager.Instance.blocksCenter + new Vector3(minOffset, minOffset, minOffset);
                    offset = minOffset;
                }
                else if (diff > maxOffset) // 최대 offset보다 더 멀다면 최대 offset만큼만 떨어지도록함
                {
                    transform.position = GameManager.Instance.blocksCenter + new Vector3(maxOffset, maxOffset, maxOffset);
                    offset = maxOffset;
                }
                else
                {
                    transform.position = newPosition;
                    offset += zoomOffset;
                }
            }
        }
    }
}
