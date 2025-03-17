using Cysharp.Threading.Tasks;
using Data;
using Sirenix.OdinInspector;
using System;
using UniRx;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class InputManager : MonoBehaviour
{
    public Action KeyAction = null;
    public Action<MouseEvent> MouseAction = null;
    public enum MouseEvent
    {
        Press = 0,
        PointerDown = 1,
        PointerUp = 2,
        Click = 3,
    }
    public enum TouchType
    {
        Stopped = 0,
        Moved = 1,
        SizeUp = 2,
        SizeDown = 3,
    }

    public Subject<Vector3> dragDir = new Subject<Vector3>();
    public Subject<bool> dragSubject = new Subject<bool>();

    bool _pressed = false;
    float _pressedTime = 0;


#if UNITY_EDITOR
    public static int pointerID = -1; //����Ƽ �󿡼��� -1
#else
        public static int pointerID = 0;  
#endif

    private Vector3 startPosition;
    private Vector3 lastPosition; // ������ ��ġ
    private bool isDragging = false; // �巡�� ������ ����

    public bool isInteractable;
    public float minDistance = 1f;

    private IGameData gameData;
    private Tilemap tilemap;

    public void Init(IGameData _gameData, Tilemap _tilemap)
    {
        gameData = _gameData;
        tilemap = _tilemap;
    }

    public void OnUpdate()
    {
        if (!isInteractable || Managers.Popup.IsWaitPopup())
            return;
#if !UNITY_EDITOR

        if (Managers.Time.IsPause)
            return;

        if (Managers.Popup.IsShowPopupWithoutBlock())
            return;
#endif

        if (EventSystem.current == null)
            return;

        #region Ű �Է� �� �̵�
        // ���콺 �Է� �Ǵ� ��ġ �Է� �� �ϳ��� ó��
        Vector3 inputPosition = Vector3.zero;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            inputPosition = touch.position;
           
            // UI ������ ��ġ�� �߻��ϸ� ����
            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                return;
            }

            if (touch.phase == TouchPhase.Began)
            {
                StartDrag(inputPosition);
                
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                PerformDrag(inputPosition);
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                EndDrag();
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            // UI ������ ���콺 Ŭ���� �߻��ϸ� ����
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            inputPosition = Input.mousePosition;
            StartDrag(inputPosition);
        }
        else if (Input.GetMouseButton(0))
        {
            inputPosition = Input.mousePosition;
            PerformDrag(inputPosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            EndDrag();
        }
        #endregion

    }

    public void Clear()
    {
        KeyAction = null;
        MouseAction = null;
    }

    void StartDrag(Vector3 inputPosition)
    {
        startPosition = inputPosition;
        lastPosition = inputPosition; // �巡�� ���� ������ ��ġ ����
        isDragging = true; // �巡�� ����

        Vector3Int tilePos = tilemap.WorldToCell(Camera.main.ScreenToWorldPoint(inputPosition));
        var group = gameData.PlayInfo.Player.GetTileGroup(tilePos);
        if (group != null)
        {
            Debug.Log("�׷� ����", Color.blue);
        }
        else
        {
            Debug.Log("�׷� ����", Color.red);
        }
    }

    void PerformDrag(Vector3 inputPosition)
    {
        if (!isDragging)
            return;

        var dragDistance = inputPosition - lastPosition;
        if (dragDistance.magnitude < minDistance)
        {
            return;
        }

        dragDir.OnNext(new Vector3(dragDistance.x, 0, dragDistance.y));

        lastPosition = inputPosition; // ���� ��ġ�� ������ ��ġ�� ����
    }

    void EndDrag()
    {
        isDragging = false; // �巡�� ����
        var dragDistance = lastPosition - startPosition;
        dragSubject.OnNext(dragDistance.magnitude > 1f);
    }
}
