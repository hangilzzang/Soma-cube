using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraToUI : MonoBehaviour
{
    public Camera sourceCamera;  // 카메라를 할당합니다.
    public RawImage targetRawImage;  // RawImage UI를 할당합니다.

    void Start()
    {
        targetRawImage.texture = sourceCamera.targetTexture;
    }
}
