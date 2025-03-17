using UnityEngine;
using UnityEngine.Tilemaps;

public class TestUnitMove : MonoBehaviour
{
    public Tilemap tilemap;        // 이동 가능한 타일맵
    public GameObject arrowPrefab; // 이동 경로 UI 프리팹
    private GameObject arrowInstance; // 생성된 UI 오브젝트
    private bool isDragging = false;

    void Update()
    {
        if (isDragging)
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;

            // 마우스 위치를 타일맵의 그리드 좌표로 변환
            Vector3Int tilePos = tilemap.WorldToCell(mouseWorldPos);

            // 이동 가능 타일인지 확인
            if (tilemap.HasTile(tilePos))
            {
                // UI를 타일 중심 위치로 이동
                arrowInstance.transform.position = tilemap.GetCellCenterWorld(tilePos);
            }
        }

        // 드래그 종료 (마우스 버튼 떼기)
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            MoveToTargetTile();
        }
    }

    void OnMouseDown()
    {
        isDragging = true;

        // 화살표 UI 생성 (처음 한 번만 생성)
        if (arrowInstance == null)
        {
            arrowInstance = Instantiate(arrowPrefab);
        }
        arrowInstance.SetActive(true);
    }

    void MoveToTargetTile()
    {
        Vector3Int tilePos = tilemap.WorldToCell(arrowInstance.transform.position);

        // 해당 타일에 유닛을 이동할 수 있는지 확인
        if (tilemap.HasTile(tilePos))
        {
            StartCoroutine(MoveSmoothly(tilemap.GetCellCenterWorld(tilePos)));
        }

        // 화살표 UI 숨기기
        arrowInstance.SetActive(false);
    }

    System.Collections.IEnumerator MoveSmoothly(Vector3 destination)
    {
        float speed = 5f;
        while ((transform.position - destination).sqrMagnitude > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, destination, speed * Time.deltaTime);
            yield return null;
        }
        transform.position = destination;
    }
}
