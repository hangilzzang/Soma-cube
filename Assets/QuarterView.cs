using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuarterView : MonoBehaviour
{
    public Transform target;
    float offset = 10;

    void LateUpdate()
    {
        transform.position = new Vector3(target.position.x + offset, target.position.y + offset, target.position.z + offset); // 쿼터뷰 설정
        transform.LookAt(target); // 타겟을 향하는 카메라
    }

}
