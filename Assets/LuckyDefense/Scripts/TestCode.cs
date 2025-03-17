using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TestCode : MonoBehaviour
{
    public Tilemap tilemap; // Ÿ�ϸ� ����
    public Vector3 pivot;

    void Start()
    {
        FitCameraToTilemap();
    }
    [Button]
    void FitCameraToTilemap()
    {
        if (tilemap == null) return;

        // Ÿ�ϸ� ũ�� ��������
        Bounds bounds = tilemap.localBounds;

        // ȭ�� ���� ���
        float aspectRatio = (float)Screen.width / Screen.height;
        float mapWidth = bounds.size.x;
        float mapHeight = bounds.size.y;

        // ī�޶� ũ�� ����
        Camera cam = Camera.main;
        float sizeByHeight = mapHeight / 2f;
        float sizeByWidth = (mapWidth / 2f) / aspectRatio;
        cam.orthographicSize = Mathf.Max(sizeByHeight, sizeByWidth);

        // ī�޶� ��ġ�� Ÿ�ϸ� �߾����� �̵�
        cam.transform.position = new Vector3(bounds.center.x, bounds.center.y, cam.transform.position.z)+ pivot;

        // ���� �ػ󵵸� Ÿ�ϸ� ������ �°� �ڵ� ����
        int targetWidth = Mathf.RoundToInt(mapWidth * 100);
        int targetHeight = Mathf.RoundToInt(mapHeight * 100);
        Screen.SetResolution(targetWidth, targetHeight, false);
    }
}
