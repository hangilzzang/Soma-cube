using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuarterView : MonoBehaviour
{
    private float offset = 6;
    // public Vector3 target;
    private Vector3 target = new Vector3(0, 0, 0);

    void Start()
    { 
        // 카메라의 위치를 Vector3.zero에서 offset만큼 떨어진 위치로 설정
        transform.position = new Vector3(target.x + offset, target.y + offset, target.z + offset);
        // 카메라가 항상 target을 바라보도록 설정
        transform.LookAt(target);
    }

}
