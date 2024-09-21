using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CustonScrollInput : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private bool isDragging = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        // 터치 시작 시 처리
        isDragging = false;
        Debug.Log("Touch");
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 드래그 시 처리
        isDragging = true;
        Debug.Log("Drag");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // 터치 끝 시 처리
        if (!isDragging)
        {
            // 드래그가 아닌 터치였을 경우의 로직
        }
    }
}
