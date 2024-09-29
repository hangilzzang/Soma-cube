using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    // public bool isBlockOnField = false;
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
}
