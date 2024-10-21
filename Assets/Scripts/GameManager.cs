using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using Lean.Touch;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Dictionary<GameObject, List<Vector3>> positionSet = new Dictionary<GameObject, List<Vector3>>(); // 배치된 블럭들의 정보를 담은 딕셔너리
    public event Action<GameObject, Vector2> OnBlockSwipe; // 블록스와이프 이벤트
    public event Action<GameObject, GameObject> OnBlockUIOld; // 블록UI 꾹터치 이벤트
    public event Action<Transform, Transform> OnBlockOld; // 블록UI 꾹터치 이벤트
    public bool placement;
    public bool reposition;
    // public Vector3 blocksCenter;
    public event Action<Vector2> OnFieldSwipe;
    public bool swipeAble; // 줌과 스와이프가 동시에 실행되지않도록 조정하는 변수
    public int dragingIndex = -1;
    public event Action OnBlockPlaced; // 블럭이 배치될때 실행되는 이벤트
    public Vector3 CameraTarget;
    public int fieldRotateAngle = 0;
    
    public void TriggerOnBlockPlaced()
    {
        OnBlockPlaced?.Invoke();
    }
    
    void OnEnable()
    {
        LeanTouch.OnFingerSwipe += HandleFingerSwipe;
        LeanTouch.OnFingerOld += HandleFingerOld1;
        LeanTouch.OnFingerOld += HandleFingerOld2; 
    }

    void OnDisable()
    {
        LeanTouch.OnFingerSwipe -= HandleFingerSwipe;
        LeanTouch.OnFingerOld -= HandleFingerOld1;
        LeanTouch.OnFingerOld -= HandleFingerOld2; 
    }
    
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 파괴되지 않도록 설정
        }
        else
        {
            // 이미 인스턴스가 존재하면 새로 생성된 오브젝트는 파괴
            Destroy(gameObject);
        }
        Application.targetFrameRate = 120; // fps 60으로 설정
    }   
     // 부모 오브젝트의 자식들의 위치를 추가하는 함수
    public void AddParentObjectPositions(GameObject parentObject) 
    {
        // 모든 블록 게임 오브젝트의 부모 오브젝트
        Transform blocks = parentObject.transform.parent; 
        // 부모 오브젝트의 자식들의 위치를 저장할 리스트
        List<Vector3> childPositions = new List<Vector3>();

        // 모든 자식 오브젝트 순회
        foreach (Transform child in parentObject.transform)
        {
            childPositions.Add(blocks.InverseTransformPoint(child.position)); // 자식의 위치를 리스트에 추가
            // Debug.Log(blocks.InverseTransformPoint(child.position)); // 테스트용
        }

        // 딕셔너리에 부모 오브젝트와 그 자식들의 위치 리스트 추가
        if (!positionSet.ContainsKey(parentObject))
        {
            positionSet.Add(parentObject, childPositions);
        }
    }
    // 부모 오브젝트의 자식들의 위치를 제거하는 함수
    public void RemoveParentObjectPositions(GameObject parentObject)
    {
        if (positionSet.ContainsKey(parentObject))
        {
            positionSet.Remove(parentObject); // 부모 오브젝트에 대한 위치 정보 제거
        }
    }

    public bool ArePositionsUnique(GameObject parentObject)
    {
        // Debug.Log("-----업데이트주기-----");
        // 모든 블록 게임 오브젝트의 부모 오브젝트
        Transform blocks = parentObject.transform.parent; 
        // 부모 오브젝트의 자식들의 포지션을 리스트로 가져오기
        List<Vector3> childPositions = new List<Vector3>();
        foreach (Transform child in parentObject.transform)
        {
            // 블럭의 로컬좌표계(Blocks)를 기준으로한 위치 정보 리스트에 삽입
            childPositions.Add(blocks.InverseTransformPoint(child.position));
            // Debug.Log(blocks.InverseTransformPoint(child.position)); // 테스트용
        }

        // Debug.Log("-----원래있던애 포지션-----");
        // positionSet의 모든 값과 비교하여 겹치는지 확인
        foreach (var entry in positionSet)
        {
            List<Vector3> storedPositions = entry.Value;
            // 자식 포지션 중 하나라도 겹치면 false 반환
            foreach (Vector3 storedPos in storedPositions)
            {
                // Debug.Log(storedPos);
                foreach (Vector3 childPos in childPositions)
                {
                    // Debug.Log("--------");
                    // Debug.Log("배치할 " + childPos);
                    // Debug.Log("배치된 " + storedPos);
                    // Debug.Log(storedPos == childPos);
                    if (Vector3.Distance(storedPos, childPos) < 0.01f) // 변경함 이제 겹치는 부분에 대해 제대로 작동
                    {
                        return false; // 겹치는 포지션이 있으면 false
                    }
                }
            }
        }

        // 겹치는 포지션이 없다면 true 반환
        return true;
    }

    public List<RaycastResult> GetUIRaycastResults(Vector2 pos)
    {
        // PointerEventData 생성
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = pos
        };

        // Raycast 결과를 저장할 리스트
        var results = new List<RaycastResult>();

        // Raycast 수행
        EventSystem.current.RaycastAll(pointerData, results);

        // Raycast 결과 반환
        return results;
    }

    void HandleFingerSwipe(LeanFinger finger) // 스와이프 이벤트 핸들러
    {
        Vector2 startPos = finger.StartScreenPosition;
        // 레이케스트 결과
        List<RaycastResult> startResults = GetUIRaycastResults(startPos);
        
        if (swipeAble) // 오직 한손가락 스와이프만 허용
        {
            if (startResults.Count > 0)  // UI 스와이프 한경우
            {
                GameObject swipedUI = startResults[0].gameObject;
                if (swipedUI.tag == "BlockUI") // 만약 블록 UI 감지됐으면 블록스와이프 이벤트 발동
                {
                    Vector2 swipeDelta = finger.SwipeScreenDelta;
                    OnBlockSwipe?.Invoke(swipedUI, swipeDelta);
                }
            }
            else // 아닌경우(필드 스와이프)
            {

                Vector2 swipeDelta = finger.SwipeScreenDelta;
                OnFieldSwipe?.Invoke(swipeDelta);

            }
        }
    }

    void HandleFingerOld1(LeanFinger finger)
    {
        Vector2 startPos = finger.StartScreenPosition;
        Vector2 EndPos = finger.LastScreenPosition;

        List<RaycastResult> startResults = GetUIRaycastResults(startPos);
        List<RaycastResult> endResults = GetUIRaycastResults(EndPos);
        
        if (startResults.Count > 0 && endResults.Count > 0) // 
        {
            GameObject heldUI1 = startResults[0].gameObject;
            GameObject heldUI2 = endResults[0].gameObject;
            if (heldUI1 == heldUI2 && heldUI1.tag == "BlockUI" && heldUI2.tag == "BlockUI") // 두 터치 모두 같은 블록 UI를 터치
            {
                if (dragingIndex == -1) // 배치동작은 중복해서 실행할수없음
                {
                    dragingIndex = finger.Index;
                    OnBlockUIOld?.Invoke(heldUI1, heldUI2);
                }
            }
        }
    }

    
    void HandleFingerOld2(LeanFinger finger)
    {
        Vector2 startPos = finger.StartScreenPosition;
        Vector2 EndPos = finger.LastScreenPosition;

        Ray rayStart = Camera.main.ScreenPointToRay(startPos);
        RaycastHit hitStart; 
        Ray rayEnd = Camera.main.ScreenPointToRay(EndPos);
        RaycastHit hitEnd; 

        // 레이케스트 결과: 모두 게임오브젝트에 충돌
        if (Physics.Raycast(rayStart, out hitStart) && Physics.Raycast(rayEnd, out hitEnd)) 
        {
            Transform startBlock = hitStart.transform.parent;
            Transform endBlock = hitEnd.transform.parent;
            // 동일한 블록을 가르키는지 확인,  태그 확인
            if (startBlock == endBlock && startBlock.CompareTag("Block"))
            {
                if (dragingIndex == -1) // 배치동작은 중복해서 실행할수없음
                {
                    dragingIndex = finger.Index;
                    OnBlockOld?.Invoke(startBlock, endBlock);
                }
            }
        }
    }

    public void GetNewCameraTarget() // 필드카메라가 찍을 지점
    {
        // 만약 배치된 블럭이 없을경우
        if (positionSet.Count == 0)
        {
            CameraTarget = Vector3.zero;
            return;
        }

        // 매우 큰 초기값과 매우 작은 초기값 설정
        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float minZ = float.MaxValue;

        float maxX = float.MinValue;
        float maxY = float.MinValue;
        float maxZ = float.MinValue;

        // 딕셔너리의 모든 Vector3 값들을 순회하며 가장 작은 값과 가장 큰 값을 찾음
        foreach (var key in positionSet)
        {
            foreach (var pos in key.Value)
            {
                // 각 축에 대해 가장 작은 값과 가장 큰 값 갱신
                if (pos.x < minX) minX = pos.x;
                if (pos.y < minY) minY = pos.y;
                if (pos.z < minZ) minZ = pos.z;

                if (pos.x > maxX) maxX = pos.x;
                if (pos.y > maxY) maxY = pos.y;
                if (pos.z > maxZ) maxZ = pos.z;
            }
        }

        // 중간 지점 계산
        float centerX = (minX + maxX) / 2;
        float centerZ = (minZ + maxZ) / 2;

        // 필드값에 할당
        CameraTarget = new Vector3(centerX, maxY, centerZ);
    }
}
