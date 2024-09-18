using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuarterView : MonoBehaviour
{
    float offset = 10;

    void LateUpdate()
    {
        // 카메라의 위치를 Vector3.zero에서 offset만큼 떨어진 위치로 설정
        transform.position = new Vector3(offset, offset, offset);
        // 카메라가 항상 (0, 0, 0)을 바라보도록 설정
        transform.LookAt(Vector3.zero);
    }

}
