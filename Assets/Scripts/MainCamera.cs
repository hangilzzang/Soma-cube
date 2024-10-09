using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MainCamera : MonoBehaviour
{
    public float offset = 20;
    public Vector3 target;
    public float duration = 1f;

    void Start()
    {
        // 카메라의 위치를 Vector3.zero에서 offset만큼 떨어진 위치로 설정
        transform.position = new Vector3(target.x + offset, target.y + offset, target.z + offset);
        // 카메라가 항상 (0, 0, 0)을 바라보도록 설정
        transform.LookAt(target);
        // 블록배치 이벤트 구독
        GameManager.Instance.OnBlockPlaced += HandleBlockPlaced;  
    }

    void OnDisable()
    {
        GameManager.Instance.OnBlockPlaced -= HandleBlockPlaced;  
    }
    
    // 카메라 위치 이동
    void HandleBlockPlaced()
    {
        // 새로운 타겟 불러오기
        Vector3 newTarget = GameManager.Instance.blocksCenter;
        // offset을 바탕으로 새 포지션 계산하기
        Vector3 newPosition = new Vector3(newTarget.x + offset, newTarget.y + offset, newTarget.z + offset);
        // 이동
        transform.DOMove(newPosition, duration).SetEase(Ease.InOutQuad); 
    }
}
