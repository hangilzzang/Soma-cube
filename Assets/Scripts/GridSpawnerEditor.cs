// Unity 에디터에서만 코드를 실행하기 위해 사용.
#if UNITY_EDITOR 
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode] // 에디터 모드에서 실행
public class GridSpawnerEditor : MonoBehaviour
{
    public GameObject cubePrefab;
    public int rows = 17;
    public int cols = 17;
    public int spacing = 2;

    // 에디터에서 실행되는 함수
    [ContextMenu("Spawn Grid")] // Spawn Grid를 Unity 에디터의 우클릭 메뉴에 추가
    void SpawnGrid()
    {
        // 큐브를 에디터에서 미리 생성하고 배치
        int offset = spacing * (rows / 2);
        for (int x = 0; x < rows; x++)
        {
            for (int z = 0; z < cols; z++)
            {
                Vector3 position = new Vector3((x * spacing) - offset, 0, (z * spacing) - offset);
                GameObject cube = PrefabUtility.InstantiatePrefab(cubePrefab) as GameObject;
                cube.transform.position = position;
                cube.transform.SetParent(this.transform);
            }
        }
    }
}
#endif
