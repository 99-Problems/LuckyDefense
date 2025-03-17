using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TestCode : MonoBehaviour
{
    public Tilemap tilemap; // 타일맵 연결
    public Vector3 pivot;

    void Start()
    {
        FitCameraToTilemap();
    }
    [Button]
    void FitCameraToTilemap()
    {
        if (tilemap == null) return;

        // 타일맵 크기 가져오기
        Bounds bounds = tilemap.localBounds;

        // 화면 비율 계산
        float aspectRatio = (float)Screen.width / Screen.height;
        float mapWidth = bounds.size.x;
        float mapHeight = bounds.size.y;

        // 카메라 크기 조정
        Camera cam = Camera.main;
        float sizeByHeight = mapHeight / 2f;
        float sizeByWidth = (mapWidth / 2f) / aspectRatio;
        cam.orthographicSize = Mathf.Max(sizeByHeight, sizeByWidth);

        // 카메라 위치를 타일맵 중앙으로 이동
        cam.transform.position = new Vector3(bounds.center.x, bounds.center.y, cam.transform.position.z)+ pivot;

        // 게임 해상도를 타일맵 비율에 맞게 자동 조정
        int targetWidth = Mathf.RoundToInt(mapWidth * 100);
        int targetHeight = Mathf.RoundToInt(mapHeight * 100);
        Screen.SetResolution(targetWidth, targetHeight, false);
    }
}
