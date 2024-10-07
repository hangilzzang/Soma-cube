using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    void Start()
    {
        // 부모 오브젝트의 중점을 찾습니다.
        Vector3 centerPoint = FindCenterPoint();
        Debug.Log("Center Point: " + centerPoint);
    }

    Vector3 FindCenterPoint()
    {
        // 모든 자식 오브젝트의 Renderer 컴포넌트를 가져옵니다.
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return Vector3.zero; // Renderer가 없으면 중점이 없음을 나타냅니다.

        // 첫 번째 Renderer의 bounds로 시작합니다.
        Bounds bounds = renderers[0].bounds;
        foreach (Renderer renderer in renderers)
        {
            // 모든 Renderer의 bounds를 포함하는 최대 bounds를 계산합니다.
            bounds.Encapsulate(renderer.bounds);
        }

        // 계산된 bounds의 중심을 반환합니다.
        return bounds.center;
        // ㅋㅋ
    }
}
