using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapUI : MonoBehaviour
{
    bool isToggled = false;
    public Image mapUIImage;
    public Sprite puzzleSprite;  // 토글이 켜졌을 때 이미지
    public Sprite mapSprite; // 토글이 꺼졌을 때 이미지
    public void MapUIImageChanger() // Map의 버튼 컴포넌트와 연결되어있다
    {
        isToggled = !isToggled;
        mapUIImage.sprite = isToggled ? puzzleSprite : mapSprite;
    }
}
