using UnityEngine;
using UnityEngine.Tilemaps;

public class TestUnitMove : MonoBehaviour
{
    public Tilemap tilemap;        // �̵� ������ Ÿ�ϸ�
    public GameObject arrowPrefab; // �̵� ��� UI ������
    private GameObject arrowInstance; // ������ UI ������Ʈ
    private bool isDragging = false;

    void Update()
    {
        if (isDragging)
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;

            // ���콺 ��ġ�� Ÿ�ϸ��� �׸��� ��ǥ�� ��ȯ
            Vector3Int tilePos = tilemap.WorldToCell(mouseWorldPos);

            // �̵� ���� Ÿ������ Ȯ��
            if (tilemap.HasTile(tilePos))
            {
                // UI�� Ÿ�� �߽� ��ġ�� �̵�
                arrowInstance.transform.position = tilemap.GetCellCenterWorld(tilePos);
            }
        }

        // �巡�� ���� (���콺 ��ư ����)
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            MoveToTargetTile();
        }
    }

    void OnMouseDown()
    {
        isDragging = true;

        // ȭ��ǥ UI ���� (ó�� �� ���� ����)
        if (arrowInstance == null)
        {
            arrowInstance = Instantiate(arrowPrefab);
        }
        arrowInstance.SetActive(true);
    }

    void MoveToTargetTile()
    {
        Vector3Int tilePos = tilemap.WorldToCell(arrowInstance.transform.position);

        // �ش� Ÿ�Ͽ� ������ �̵��� �� �ִ��� Ȯ��
        if (tilemap.HasTile(tilePos))
        {
            StartCoroutine(MoveSmoothly(tilemap.GetCellCenterWorld(tilePos)));
        }

        // ȭ��ǥ UI �����
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
