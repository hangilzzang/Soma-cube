using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Lean.Touch;
using DG.Tweening;

public class BlockTouch : MonoBehaviour
{
    public float leftRightThreshold = 3f;
    public Transform blockTransform; // 핸드 블록 트랜스폼 컴포넌트
    public float rotateDuration = 0.5f; // 회전 애니메이션 지속 시간
    public GameObject blockPrefab; // 필드배치 블록 프리팹
    public bool placement = false; // 배치중
    public bool reposition = false; // 재배치중
    public int offset = 2; //  프리팹 배치위치 조절 
    GameObject fieldBlock = null; // 필드에 실제로 배치되는 블록
    public RawImage blockUI; // 배치하면 ui 비활성화 이펙트 주기위함
    public bool placeAble;

    void Start()
    {
        // LeanTouch.OnFingerOld += HandleFingerHeld1;
        LeanTouch.OnFingerUpdate += HandleFingerUpdate;
        LeanTouch.OnFingerUp += HandleFingerUp;

        // LeanTouch.OnFingerOld += HandleFingerHeld2; 
        GameManager.Instance.OnBlockSwipe += HandleBlockSwipe;
        GameManager.Instance.OnBlockUIOld += HandleBlockUIOld;
        GameManager.Instance.OnBlockOld += HandleBlockOld;
    }

    void OnDisable()
    {
        // LeanTouch.OnFingerOld -= HandleFingerHeld1;
        LeanTouch.OnFingerUpdate -= HandleFingerUpdate;
        LeanTouch.OnFingerUp -= HandleFingerUp;

        // LeanTouch.OnFingerOld -= HandleFingerHeld2; 
        GameManager.Instance.OnBlockSwipe -= HandleBlockSwipe;
        GameManager.Instance.OnBlockUIOld -= HandleBlockUIOld;
        GameManager.Instance.OnBlockOld -= HandleBlockOld;
    }

    
    List<RaycastResult> GetUIRaycastResults(Vector2 startPos)
    {
        // PointerEventData 생성
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = startPos
        };

        // Raycast 결과를 저장할 리스트
        var results = new List<RaycastResult>();

        // Raycast 수행
        EventSystem.current.RaycastAll(pointerData, results);

        // Raycast 결과 반환
        return results;
    }
    
    // UI 꾹터치 반응함(배치)
    void HandleBlockUIOld(GameObject heldUI1, GameObject heldUI2)
    {
        if (heldUI1 == gameObject && heldUI2 == gameObject) // 꾹터치 만족 조건
        {
            // 안보이는곳에 소환
            fieldBlock = Instantiate(blockPrefab, new Vector3 (0, -5, 0) ,blockTransform.rotation);
            // offset값 찾기(특정좌표위에 올라가도록 블럭을 배치하고싶을경우 계산해야하는 offset값)
            int lowestY = -5;
            foreach (Transform child in fieldBlock.transform)
            {
                if ((int)child.position.x == 0 && (int)child.position.z == 0)
                {
                    if (child.position.y < lowestY)
                    {
                        lowestY = (int)child.position.y; // 갱신됨
                    }
                }
            }
            // Debug.Log(lowestY);
            offset = (-5 - lowestY) + 2;
            placement = true;
        }
    }

    // 게임오브젝트 꾹터치 반응(위치옮기거나 삭제)
    void HandleBlockOld(Transform startBlock, Transform endBlock)
    {
        if (startBlock == fieldBlock?.transform && endBlock == fieldBlock?.transform)
        {
            // 콜라이더 비활성화
            foreach (Collider childCollider in fieldBlock.GetComponentsInChildren<Collider>())
            {
                childCollider.enabled = false; 
            } 
            // 배치된 블럭 딕셔너리에서 위치 정보를 제거함
            GameManager.Instance.RemoveParentObjectPositions(fieldBlock);
            // 리포지션 시작
            reposition = true;
        }
    }


    void HandleFingerUpdate(LeanFinger finger)
    {
        if (placement || reposition)
        {
            // 게임 오브젝트 감지
            Vector2 pos = finger.ScreenPosition;
            Ray ray = Camera.main.ScreenPointToRay(pos);
            RaycastHit hit; 
            if (!(Physics.Raycast(ray, out hit) && hit.transform.gameObject.tag == "BlockCube")) // 블럭 게임오브젝트를 감지하지못함
            {
                goto NotPlaceable;
            }
            
            GameObject touchedObject = hit.transform.gameObject; // 현재 터치중인 게임오브젝트 

            // Debug.Log("다른거임");
            // ui 오브젝트 감지
            List<RaycastResult> results = GetUIRaycastResults(pos);
            if (results.Count > 0) // ui오브젝트가 존재함
            {
                goto NotPlaceable;
            }

            // 블럭의 포지션을 바꾼다(확정x)
            fieldBlock.transform.position = touchedObject.transform.position + new Vector3(0f, offset, 0f); // 블럭 게임 오브젝트 포인터쪽으로 위치 변경
            
            // 겹치는 오브젝트가 있는지 확인
            if (GameManager.Instance.ArePositionsUnique(fieldBlock) == false) // 다른블럭과 겹쳐서 배치할수없는 경우
            {
                // Debug.Log("배치할수없어요");
                goto NotPlaceable;
            }

            // 블럭 배치 가능 판정
            // Debug.Log("배치 가능합니다");
            placeAble = true;
            return;

        NotPlaceable: // 블럭 배치 불가능 판정
            placeAble = false; 
            fieldBlock.transform.position = new Vector3 (0, -5, 0);
            return;       

        }
    }


    void HandleFingerUp(LeanFinger finger)
    {
        if (placement || reposition)
        {
            if (placeAble) // 배치가능
            {
                // 블럭의 자식 큐브 콜라이더 활성화
                foreach (Collider childCollider in fieldBlock.GetComponentsInChildren<Collider>())
                {
                    childCollider.enabled = true; 
                }

                // 배치된 블럭 딕셔너리에 위치 정보를 추가함
                GameManager.Instance.AddParentObjectPositions(fieldBlock);
                // Debug.Log(GameManager.Instance.positionSet);

                // ui 비활성화 이펙트
                blockUI.color = new Color(200f / 255f, 200f / 255f, 200f / 255f, 128f / 255f);
                // 이벤트 구독 종료(스와이프 입력, 꾹터치 입력 못받음)
                GameManager.Instance.OnBlockSwipe -= HandleBlockSwipe;
                GameManager.Instance.OnBlockUIOld -= HandleBlockUIOld;
            }
            else // 제거
            {
                // 배치된 블럭 딕셔너리에서 위치 정보를 제거함
                GameManager.Instance.RemoveParentObjectPositions(fieldBlock);
                
                Destroy(fieldBlock); // 기존 프리팹 파괴
                fieldBlock = null;
 
                if (reposition) // UI재활성화
                {
                    // ui 활성화 이펙트
                    blockUI.color = new Color(255f / 255f, 255f / 255f, 255f / 255f, 255f / 255f);
                    // 이벤트 구독 시작(스와이프 입력, 꾹터치 입력 다시 받음)
                    GameManager.Instance.OnBlockSwipe += HandleBlockSwipe;
                    GameManager.Instance.OnBlockUIOld += HandleBlockUIOld;
                }
            }
            
            placement = false;
            reposition = false;
        }
    }

    void HandleBlockSwipe(GameObject swipedUI, Vector2 swipeDelta) // 블록스와이프 이벤트 핸들러
    {
        if (swipedUI == gameObject && !DOTween.IsTweening(blockTransform))
        {
            // y축
            if (Mathf.Abs(swipeDelta.x) > leftRightThreshold * Mathf.Abs(swipeDelta.y))
            {
                if (swipeDelta.x > 0)
                {
                    blockTransform.DORotate(new Vector3(0, -90, 0), rotateDuration, RotateMode.WorldAxisAdd);
                }
                else
                {
                    blockTransform.DORotate(new Vector3(0, 90, 0), rotateDuration, RotateMode.WorldAxisAdd);
                }
            }
            else
            {
                // x축, z축
                if (swipeDelta.x > 0 && swipeDelta.y > 0)
                {
                    blockTransform.DORotate(new Vector3(0, 0, 90), rotateDuration, RotateMode.WorldAxisAdd);
                }
                else if (swipeDelta.x < 0 && swipeDelta.y > 0)
                {
                    blockTransform.DORotate(new Vector3(-90, 0, 0), rotateDuration, RotateMode.WorldAxisAdd);
                }
                else if (swipeDelta.x < 0 && swipeDelta.y < 0)
                {
                    blockTransform.DORotate(new Vector3(0, 0, -90), rotateDuration, RotateMode.WorldAxisAdd);
                }
                else if (swipeDelta.x > 0 && swipeDelta.y < 0)
                {
                    blockTransform.DORotate(new Vector3(90, 0, 0), rotateDuration, RotateMode.WorldAxisAdd);
                }
            }
        }
    }
}
