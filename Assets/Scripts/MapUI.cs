using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapUI : MonoBehaviour
{
    public Image mapUIImage;
    public Sprite puzzleSprite;  // 토글이 켜졌을 때 이미지
    public Sprite mapSprite; // 토글이 꺼졌을 때 이미지
    public Transform blocks; // 필드에 배치된 블록들의 부모 게임 오브젝트
    
    public void HandleMapUITouch() // Map의 버튼 컴포넌트와 연결되어있다
    {
        // mapUI 이미지 변경
        GameManager.Instance.isMapUIToggled = !GameManager.Instance.isMapUIToggled;
        mapUIImage.sprite = GameManager.Instance.isMapUIToggled ? puzzleSprite : mapSprite;
    
        // 모든 블록의 레이어 변경
        int targetLayer = GameManager.Instance.isMapUIToggled ? 
            LayerMask.NameToLayer("HiddenBlocks") : 
            LayerMask.NameToLayer("Default");

        foreach (Transform block in blocks)
        {
            foreach (Transform cube in block)
            {
                cube.gameObject.layer = targetLayer;
            }
        }
    }
}
