using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Dictionary<GameObject, List<Vector3>> positionSet = new Dictionary<GameObject, List<Vector3>>(); // 배치된 블럭들의 정보를 담은 딕셔너리
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
    }   
     // 부모 오브젝트의 자식들의 위치를 추가하는 함수
    public void AddParentObjectPositions(GameObject parentObject) 
    {
        // 부모 오브젝트의 자식들의 위치를 저장할 리스트
        List<Vector3> childPositions = new List<Vector3>();

        // 모든 자식 오브젝트 순회
        foreach (Transform child in parentObject.transform)
        {
            childPositions.Add(child.position); // 자식의 위치를 리스트에 추가
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
        // 부모 오브젝트의 자식들의 포지션을 리스트로 가져오기
        List<Vector3> childPositions = new List<Vector3>();
        foreach (Transform child in parentObject.transform)
        {
            childPositions.Add(child.position);
        }

        // positionSet의 모든 값과 비교하여 겹치는지 확인
        foreach (var entry in positionSet)
        {
            List<Vector3> storedPositions = entry.Value;

            // 자식 포지션 중 하나라도 겹치면 false 반환
            foreach (Vector3 storedPos in storedPositions)
            {
                foreach (Vector3 childPos in childPositions)
                {
                    if (storedPos == childPos)
                    {
                        return false; // 겹치는 포지션이 있으면 false
                    }
                }
            }
        }

        // 겹치는 포지션이 없다면 true 반환
        return true;
    }

    
}
