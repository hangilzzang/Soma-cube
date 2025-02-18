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
    public bool isBlockTweening = false; // 핸드블록중 하나라도 회전중이면 true
    public bool isMapUIToggled = false; // MapUI 토글 상태를 저장하는 변수
    public GameObject MainUI; // MainUI 게임오브젝트 참조 추가
    
    public void TriggerOnBlockPlaced()
    {
        OnBlockPlaced?.Invoke();
    }
    
    void OnEnable()
    {
        LeanTouch.OnFingerTap += HandleFingerTap; // 탭 이벤트 핸들러 추가
        LeanTouch.OnFingerSwipe += HandleFingerSwipe;
        LeanTouch.OnFingerOld += HandleFingerOld1;
        LeanTouch.OnFingerOld += HandleFingerOld2; 
    }

    void OnDisable()
    {
        LeanTouch.OnFingerTap -= HandleFingerTap; // 탭 이벤트 핸들러 제거
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
    
    // 터치지점에 게임오브젝트가 있으면 게임오브젝트 반환 아니면 null
    public GameObject GetGameObjectRaycastResult(Vector2 pos)
    {
        Ray ray = Camera.main.ScreenPointToRay(pos);
        RaycastHit hit; 
        return Physics.Raycast(ray, out hit) ? hit.transform.gameObject : null;
    }

    // 터치지점에 UI가 있으면 게임오브젝트 반환 아니면 null
    public GameObject GetUIRaycastResult(Vector2 pos)
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

        // 가장 앞쪽 UI 게임오브젝트만 반환 (없으면 null)
        return results.Count > 0 ? results[0].gameObject : null;
    }

    void HandleFingerSwipe(LeanFinger finger) // 스와이프 이벤트 핸들러
    {
        Vector2 startPos = finger.StartScreenPosition;
        // 레이케스트 결과
        GameObject startResult = GetUIRaycastResult(startPos);
        
        if (swipeAble) // 오직 한손가락 스와이프만 허용
        {
            if (startResult != null)  // UI 스와이프 한경우
            {
                if (startResult.tag == "BlockUI") // 만약 블록 UI 감지됐으면 블록스와이프 이벤트 발동
                {
                    Vector2 swipeDelta = finger.SwipeScreenDelta;
                    OnBlockSwipe?.Invoke(startResult, swipeDelta);
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

        GameObject heldUI1 = GetUIRaycastResult(startPos);
        GameObject heldUI2 = GetUIRaycastResult(EndPos);
        
        if (heldUI1 != null && heldUI2 != null)
        {
            if (heldUI1 == heldUI2 && heldUI1.tag == "BlockUI") // 두 터치 모두 같은 블록 UI를 터치
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

        // HiddenBlocks 레이어를 제외한 레이어마스크 설정
        int layerMask = ~(1 << LayerMask.NameToLayer("HiddenBlocks"));

        // 레이케스트 결과: 모두 게임오브젝트에 충돌
        if (Physics.Raycast(rayStart, out hitStart, Mathf.Infinity, layerMask) && Physics.Raycast(rayEnd, out hitEnd, Mathf.Infinity, layerMask)) 
        {
            // 다른 "블록큐브"를 터치하더라도 같은 부모(블록)이면 ok
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

    void HandleFingerTap(LeanFinger finger)
    {
        // 터치 시작과 끝 위치 가져오기
        Vector2 startPos = finger.StartScreenPosition;
        Vector2 endPos = finger.LastScreenPosition;
        
        // 두 위치 모두에서 게임오브젝트와 UI 레이캐스트 체크
        GameObject startGameObj = GetGameObjectRaycastResult(startPos);
        GameObject startUI = GetUIRaycastResult(startPos);
        GameObject endGameObj = GetGameObjectRaycastResult(endPos);
        GameObject endUI = GetUIRaycastResult(endPos);
        
        // 모든 결과가 null이면 (빈 공간 터치) 이벤트 발동
        if (startGameObj == null && startUI == null && 
            endGameObj == null && endUI == null)
        {
            // MainUI가 할당되어 있는지 확인
            if (MainUI != null)
            {
                // MainUI의 활성화 상태를 토글
                MainUI.SetActive(!MainUI.activeSelf);
            }
        }
    }

    // positionSet을 allCubePositions 형식으로 변환하는 메서드
    public List<int[]> ConvertPositionSetToCubePositions()
    {
        List<int[]> cubePositions = new List<int[]>();
        
        foreach (var positions in positionSet.Values)
        {
            foreach (Vector3 pos in positions)
            {
                cubePositions.Add(new int[] {
                    Mathf.RoundToInt(pos.x),
                    Mathf.RoundToInt(pos.y),
                    Mathf.RoundToInt(pos.z)
                });
            }
        }
        
        return cubePositions;
    }

    public void GetNewCameraTarget(List<int[]> cubePositions) 
    {
        // 만약 배치된 블럭이 없을경우
        if (cubePositions.Count == 0)
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

        // 모든 큐브 위치를 순회하며 가장 작은 값과 가장 큰 값을 찾음
        foreach (var pos in cubePositions)
        {
            // 각 축에 대해 가장 작은 값과 가장 큰 값 갱신
            if (pos[0] < minX) minX = pos[0];
            if (pos[1] < minY) minY = pos[1];
            if (pos[2] < minZ) minZ = pos[2];

            if (pos[0] > maxX) maxX = pos[0];
            if (pos[1] > maxY) maxY = pos[1];
            if (pos[2] > maxZ) maxZ = pos[2];
        }

        // 중간 지점 계산
        float centerX = (minX + maxX) / 2;
        float centerZ = (minZ + maxZ) / 2;

        // 필드값에 할당
        CameraTarget = new Vector3(centerX, maxY + 1, centerZ);
    }
}
