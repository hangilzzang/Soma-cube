using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class MakeSomaCubeProblems : MonoBehaviour
{
    public Text LevelName; // UI Text 컴포넌트 참조 추가
    private List<int[]> allCubePositions = new List<int[]>();

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            GetAllCubePositions();
            NormalizeCubePositions();
            SavePositionsToPlayerPrefs();
        }
    }

    private void GetAllCubePositions()
    {
        allCubePositions.Clear(); // 기존 데이터 초기화
        
        // GameManager의 positionSet에서 모든 큐브의 위치를 float 배열 리스트로 변환
        foreach (var positions in GameManager.Instance.positionSet.Values)
        {
            foreach (Vector3 cubePos in positions)
            {
                int[] positionArray = new int[] { 
                    Mathf.RoundToInt(cubePos.x), 
                    Mathf.RoundToInt(cubePos.y), 
                    Mathf.RoundToInt(cubePos.z) 
                };
                allCubePositions.Add(positionArray);
            }
        }
    }

    private void NormalizeCubePositions()
    {
        Vector3 cameraTarget = GameManager.Instance.CameraTarget;
        
        // 모든 큐브 위치에 대해 X와 Z 좌표를 CameraTarget 기준으로 정규화
        for (int i = 0; i < allCubePositions.Count; i++)
        {
            allCubePositions[i] = new int[] {
                allCubePositions[i][0] - Mathf.RoundToInt(cameraTarget.x),
                allCubePositions[i][1],
                allCubePositions[i][2] - Mathf.RoundToInt(cameraTarget.z)
            };
        }
    }

    private void SavePositionsToPlayerPrefs()
    {
        string levelName = LevelName.text;
        
        // 직접 리스트를 JSON으로 변환
        string jsonPositionData = JsonConvert.SerializeObject(allCubePositions);
        
        string problemsJson = PlayerPrefs.GetString("Problems", "{}");
        Dictionary<string, string> problems;
        
        try
        {
            problems = JsonConvert.DeserializeObject<Dictionary<string, string>>(problemsJson);
        }
        catch
        {
            problems = new Dictionary<string, string>();
        }
        
        problems[levelName] = jsonPositionData;
        
        string updatedProblemsJson = JsonConvert.SerializeObject(problems);
        PlayerPrefs.SetString("Problems", updatedProblemsJson);
        PlayerPrefs.Save();

        // 저장된 데이터를 기반으로 로그 출력
        var savedPositions = JsonConvert.DeserializeObject<List<float[]>>(problems[levelName]);
        string positionsStr = string.Join(", ", savedPositions.Select(pos => $"[{pos[0]},{pos[1]},{pos[2]}]"));
        Debug.Log($"{levelName}에 {savedPositions.Count}개의 큐브데이터가 저장되었습니다.\n전체 큐브의 좌표: [{positionsStr}]");
    }
}
