using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Lean.Touch;
using DG.Tweening;
using System;
using System.Threading;

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
    // public RawImage blockUI; // 배치하면 ui 비활성화 이펙트 주기위함 // 테스트
    public Camera blockCamera; // 카메라 백그라운드 컬러 바꿔서 비활성화 이펙트 주기위함 
    public bool placeAble;
    public Transform blocks; // 필드에 배치된 블록들의 부모 게임 오브젝트
    public Transform blockRotation; // 필드에 배치되는 프리팹의 회전정보를 저장한다

    
    void Start()
    {
        LeanTouch.OnFingerUpdate += HandleFingerUpdate;
        LeanTouch.OnFingerUp += HandleFingerUp;
        GameManager.Instance.OnBlockSwipe += HandleBlockSwipe;
        GameManager.Instance.OnBlockUIOld += HandleBlockUIOld;
        GameManager.Instance.OnBlockOld += HandleBlockOld;
    }

    void OnDisable()
    {
        LeanTouch.OnFingerUpdate -= HandleFingerUpdate;
        LeanTouch.OnFingerUp -= HandleFingerUp;
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
            fieldBlock = Instantiate(blockPrefab, new Vector3 (0, -5, 0) ,blockRotation.rotation); // 회전정보는 핸드블록의 rotation을 이용하지않음

            // 1. 피봇이 되기에 적합한 블록찾기
            // 1-1. fieldBlock에서 가장 낮은 y값을 구하기
            int lowestY = int.MaxValue;
            foreach (Transform child in fieldBlock.transform)
            {
                if (child.position.y < lowestY)
                {
                    lowestY = (int)child.position.y; // == 연산자의 정확성을 위해 정수로 변환해줌
                }
            }
            // 1-2. lowestY를 기준으로 가장 아래층에 있는 큐브 따로 리스트에 담기
            List<Transform> lowestCubes = new List<Transform>();
            foreach (Transform child in fieldBlock.transform)
            {
                if ((int)child.position.y == lowestY)
                {
                    lowestCubes.Add(child);
                }      
            }


            // 1-3. 가장 아래층에 있는 큐브중 가장 위에 큐브가 많은 큐브 찾기
            Transform pivotCube = null;
            int maxCount = -1;
            foreach (Transform cube in lowestCubes)
            {
                int countAbove = 0;
                foreach (Transform compareCube in fieldBlock.transform)
                {
                    // 위에 블럭이 있을때마다 countAbove 1씩 증가 시키기
                    if ((int)cube.position.x  == (int)compareCube.position.x 
                        && (int)cube.position.z == (int)compareCube.position.z)
                        {
                            countAbove++;
                        }
                }
                
                if (countAbove > maxCount)
                {
                    maxCount = countAbove;
                    pivotCube = cube; 
                }
            }
            // Debug.Log(pivotCube);
            // 2. 피봇큐브를 중심으로 포지션 재정렬
            Vector3 diff = Vector3.zero - pivotCube.localPosition;
            foreach (Transform child in fieldBlock.transform)
            {
                child.localPosition += diff;
            }


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


    // 블럭의 이동
    void HandleFingerUpdate(LeanFinger finger)
    {
        if (finger.Index == GameManager.Instance.dragingIndex) // 꾹터치 손가락을 대상으로만 드래깅
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
                
                // 배치된 블럭 blocks에 몰아넣기
                fieldBlock.transform.parent = blocks;
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
    }


    void HandleFingerUp(LeanFinger finger)
    {
        if (finger.Index == GameManager.Instance.dragingIndex) // 기존에 드래깅하던 손가락에만 반응한다
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

                    // ui 비활성화 이펙트
                    blockCamera.backgroundColor = new Color(181f / 255f, 167f / 255f, 145f / 255f);
                    // blockUI.color = new Color(200f / 255f, 200f / 255f, 200f / 255f, 128f / 255f);
                    Canvas.ForceUpdateCanvases(); // 실험용

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
                        blockCamera.backgroundColor = new Color(211f / 255f, 183f / 255f, 133f / 255f);
                        // blockUI.color = new Color(255f / 255f, 255f / 255f, 255f / 255f, 255f / 255f);
                        // 이벤트 구독 시작(스와이프 입력, 꾹터치 입력 다시 받음)
                        GameManager.Instance.OnBlockSwipe += HandleBlockSwipe;
                        GameManager.Instance.OnBlockUIOld += HandleBlockUIOld;
                    }
                }

                // 필드 카메라 위치 조정
                GameManager.Instance.GetNewCameraTarget(); // 배치된 블럭들의 위치를 바탕으로 새로운 카메라 위치 계산
                GameManager.Instance.TriggerOnBlockPlaced(); // 이벤트 실행

                placement = false;
                reposition = false;
                GameManager.Instance.dragingIndex = -1; // 다시 초기값 설정
            }
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
                    // blockRotation.eulerAngles += new Vector3(0, -90, 0);
                    blockTransform.DORotate(new Vector3(0, -90, 0), rotateDuration, RotateMode.WorldAxisAdd);
                    blockRotation.DORotate(new Vector3(0, -90, 0), rotateDuration, RotateMode.WorldAxisAdd);
                }
                else
                {
                    // blockRotation.eulerAngles += new Vector3(0, 90, 0);
                    blockTransform.DORotate(new Vector3(0, 90, 0), rotateDuration, RotateMode.WorldAxisAdd);
                    blockRotation.DORotate(new Vector3(0, 90, 0), rotateDuration, RotateMode.WorldAxisAdd);
                }
            }
            else
            {
                // x축, z축
                if (swipeDelta.x > 0 && swipeDelta.y > 0)
                {
                    // blockRotation.eulerAngles += new Vector3(0, 0, 90);
                    blockTransform.DORotate(new Vector3(0, 0, 90), rotateDuration, RotateMode.WorldAxisAdd);
                    blockRotation.DORotate(GetBlockSwipeAngle(new Vector3(0, 0, 90)), rotateDuration, RotateMode.WorldAxisAdd);
                }
                else if (swipeDelta.x < 0 && swipeDelta.y > 0)
                {
                    // blockRotation.eulerAngles += new Vector3(-90, 0, 0);
                    blockTransform.DORotate(new Vector3(-90, 0, 0), rotateDuration, RotateMode.WorldAxisAdd);
                    blockRotation.DORotate(GetBlockSwipeAngle(new Vector3(-90, 0, 0)), rotateDuration, RotateMode.WorldAxisAdd);
                }
                else if (swipeDelta.x < 0 && swipeDelta.y < 0)
                {
                    // blockRotation.eulerAngles += new Vector3(0, 0, -90);
                    blockTransform.DORotate(new Vector3(0, 0, -90), rotateDuration, RotateMode.WorldAxisAdd);
                    blockRotation.DORotate(GetBlockSwipeAngle(new Vector3(0, 0, -90)), rotateDuration, RotateMode.WorldAxisAdd);
                }
                else if (swipeDelta.x > 0 && swipeDelta.y < 0)
                {
                    // blockRotation.eulerAngles += new Vector3(90, 0, 0);
                    blockTransform.DORotate(new Vector3(90, 0, 0), rotateDuration, RotateMode.WorldAxisAdd);
                    blockRotation.DORotate(GetBlockSwipeAngle(new Vector3(90, 0, 0)), rotateDuration, RotateMode.WorldAxisAdd);
                }
            }
        }
    }

    // 실제 블럭이 배치되는 회전상태와 핸들 블록의 현재 회전상태에는 차이가 있다 그것을 보정해주는 메서드
    Vector3 GetBlockSwipeAngle(Vector3 vector3)
    {
        if (GameManager.Instance.fieldRotateAngle == 0) 
        {
            return vector3;
        }
        else if (GameManager.Instance.fieldRotateAngle == 90) 
        {
            return new Vector3(vector3.z, vector3.y, -vector3.x);
        }
        else if (GameManager.Instance.fieldRotateAngle == 180)
        {
            return new Vector3(-vector3.x, vector3.y, -vector3.z);
        }
        else
        {
            return new Vector3(-vector3.z, vector3.y, vector3.x);
        }
    }

}
