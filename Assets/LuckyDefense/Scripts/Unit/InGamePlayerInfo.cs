using Data;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.Tilemaps;

public class UnitGroup
{
    public List<UnitLogic> listUnit = new List<UnitLogic>();
    public int groupUnitID;

    public GameObject parentObj;
    public Vector3Int placePos;

    public bool IsMaxUnit()
    {
        var maxCount = Managers.Data.GetConstValue(Define.ConstDefType.GroupUnitCount);
        return listUnit.Count >= maxCount;
    }

    public int GetUnitCount()
    {
        return listUnit.Count;
    }

    public void Entry()
    {

    }

    public UnitLogic unitLogic
    {
        get
        {
            return listUnit.FirstOrDefault();
        }
    }
}


public class InGamePlayerInfo : MonoBehaviour
{
    [HideIf("@IsMonster")]
    public List<Vector3> groupPivots = new List<Vector3>();

    [ShowInInspector]
    public List<UnitGroup> listUnitGroup { get; private set; } = new List<UnitGroup>();
    [ShowInInspector]
    public List<UnitLogic> listUnit { get; private set; } = new List<UnitLogic>();

    public IGameData GameData { get; set; }
    public static Subject<InGamePlayerInfo> OnInitPlayer = new Subject<InGamePlayerInfo>();

    public List<ProjectileLogic> listLogicObject = new List<ProjectileLogic>();
    private List<ProjectileLogic> addLogicObject = new List<ProjectileLogic>();
    private List<ProjectileLogic> removeLogicObject = new List<ProjectileLogic>();
    private Dictionary<Vector3Int, UnitGroup> unitPositions = new Dictionary<Vector3Int, UnitGroup>();
    public Tilemap tilemap;
    public UnitGroup selectGroup { get; private set; }
    public Subject<UnitGroup> onSelectGroup = new Subject<UnitGroup>();

    public bool IsMonster;
    public int playerIndex;

    public int UnitCount => listUnit.Count;

    private GamePlayInfoScript Info;
    private List<Vector3Int> pathTiles = new List<Vector3Int>();
    public Vector3Int startPath => pathTiles.IsNullOrEmpty() ? Vector3Int.zero : pathTiles[0];
    public List<Vector3Int> GetTiles => pathTiles;

    public void Init(GamePlayInfoScript _info,Tilemap _tilemap, int _index = 0, bool _isMonster = false)
    {
        playerIndex = _index;
        IsMonster = _isMonster;


        Info = _info;
        if (Info == null)
            return;

        tilemap = _tilemap;

        
        if(_isMonster)
        {
            FindMonsterPathTiles();
        }
        else
        {
            InitMapInfo();
        }
    }

    public void InitListUnit(IEnumerable<UnitLogic> _listMyUnit)
    {
        listUnit.Clear();
        if (_listMyUnit != null)
        {
            foreach (var mit in _listMyUnit)
            {
                if (mit == null)
                    continue;

                mit.Reset();
                if (!mit.gameObject.activeSelf)
                    mit.gameObject.SetActive(true);
            }

            listUnit.AddRange(_listMyUnit);
        }
    }

    public UnitGroup AddUnit(UnitLogic _unit)
    {
        UnitGroup ret = null;
        if (_unit == null)
            return null;

        _unit.Reset();
        if (!_unit.gameObject.activeSelf)
            _unit.gameObject.SetActive(true);

        if (listUnit != null && !listUnit.Contains(_unit))
        {
            listUnit.Add(_unit);
        }
        
        if(_unit.Rarity != Define.EUNIT_RARITY.Myth)
        {
            var groups = listUnitGroup.FindAll(_ => _.groupUnitID == _unit.UnitID);
            foreach (var group in groups)
            {
                if (group != null && !group.IsMaxUnit())
                {
                    if(!group.listUnit.Contains(_unit))
                        group.listUnit.Add(_unit);

                    ret = group;
                    _unit.transform.SetParent(ret.parentObj.transform);
                    _unit.groupOwner = group;
                    Debug.Log("기존 그룹에 유닛 추가");
                    return ret;
                }
            }
        }

        ret = AddUnitGroup(_unit);


        _unit.transform.SetParent(ret.parentObj.transform);

        return ret;
    }

    public UnitGroup AddUnitGroup(UnitLogic _unit)
    {
        var newGroup = new UnitGroup();
        newGroup.groupUnitID = _unit.UnitID;
        newGroup.listUnit.Add(_unit);
        listUnitGroup.Add(newGroup);
        var obj = new GameObject { name = $"UnitGroup_{_unit.name}" };
        newGroup.parentObj = obj;
        newGroup.parentObj.transform.SetParent(gameObject.transform);
        _unit.groupOwner = newGroup;
        return newGroup;
    }

    public void AddUnit(IEnumerable<UnitLogic> _listMyUnit)
    {
        if (_listMyUnit != null)
        {
            foreach (var mit in _listMyUnit)
            {
                if (mit == null)
                    continue;

                mit.Reset();
                if (!mit.gameObject.activeSelf)
                    mit.gameObject.SetActive(true);
            }

            listUnit.AddRange(_listMyUnit);
            listUnit = listUnit.Distinct().ToList();
        }
    }

    public Vector3Int RemoveUnit(UnitGroup _group)
    {
        var logic = _group.listUnit.LastOrDefault();
        listUnit.Remove(logic);
        _group.listUnit.Remove(logic);
        Managers.Pool.PushUnit(logic.assetPath, logic.prefabName, logic);
        if(_group.GetUnitCount() == 0)
        {
            var pos = GetUnitTilePosition(_group);
            if(unitPositions.ContainsKey(pos))
            {
                unitPositions[pos] = null;
            }
            listUnitGroup.Remove(_group);
            GameObject.Destroy(_group.parentObj);
            selectGroup = null;
            return pos;
        }

        return Vector3Int.zero;
    }

    public (Define.ItemType, int) SellUnit(UnitGroup _group)
    {
        var sellInfo = Managers.Data.GetUnitSellInfoScript(_ => _.rarity == _group.unitLogic.Rarity);
        var pos = RemoveUnit(_group);
        if(_group.unitLogic != null)
        {
            PlaceUnit(_group,pos);
        }

        return (sellInfo.itemType, sellInfo.itemCount);
    }


    public UnitLogic GetUnitFromID(int _mitUnitID)
    {
        for (var index = 0; index < listUnit.Count; index++)
        {
            var mit = listUnit[index];
            if (mit == null)
                continue;
            if (mit.UnitID == _mitUnitID)
                return mit;
        }

        return null;
    }

    public void FrameMove(float _deltaTime)
    {
        var deadUnits = new List<UnitLogic>();
        for (int index = 0; index < listUnit.Count; index++)
        {
            var mit = listUnit[index];
            if (mit == null)
                continue;
            if (!mit.gameObject.activeInHierarchy)
            {
                if (mit.state == Define.EUNIT_STATE.DESTROY)
                    deadUnits.Add(mit);
                continue;
            }

            mit.FrameMove(_deltaTime);
        }

        listUnit.RemoveAll(_=> deadUnits.Contains(_));


        foreach (var mit in listLogicObject.Distinct())
        {
            if (mit)
            {
                mit.FrameMove(_deltaTime);
            }
        }

        listLogicObject.AddRange(addLogicObject);
        addLogicObject.Clear();

        foreach (var mit in removeLogicObject)
        {
            if (mit == null)
                continue;
            Managers.Pool.PushCollider(mit);
            listLogicObject.Remove(mit);
        }

        removeLogicObject.Clear();
    }

    public void Entry()
    {

    }

    public void AddObject(ProjectileLogic _logic)
    {
        addLogicObject.Add(_logic);
    }

    public void RemoveObject(ProjectileLogic _logic)
    {
        removeLogicObject.Add(_logic);
    }

    public float GetRandom(float min, float max)
    {
        return UnityEngine.Random.Range(min, max);
    }

    public int GetRandom(int min, int max)
    {
        return UnityEngine.Random.Range(min, max);
    }

    void InitMapInfo()
    {
        BoundsInt bounds = tilemap.cellBounds;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (tilemap.HasTile(pos))
            {
                unitPositions[pos] = null; 
            }
        }
    }

    public void PlaceUnit(UnitGroup _group, Vector3Int tilePos)
    {
        if(unitPositions.ContainsValue(_group))
        {
            for (int i = 0; i < _group.listUnit.Count; i++)
            {
                var addPos = Vector3.zero;
                if(groupPivots.Count > i)
                    addPos = groupPivots[i];
                _group.listUnit[i].transform.position = _group.parentObj.transform.position + addPos;
            }
            
            return;
        }

        if (unitPositions.ContainsKey(tilePos) && unitPositions[tilePos] == null)
        {
            unitPositions[tilePos] = _group;
            _group.parentObj.transform.position = tilemap.GetCellCenterWorld(tilePos);
            _group.unitLogic.transform.localPosition = Vector3.zero;
            Debug.Log($"유닛 배치 완료: {tilePos}");
            _group.placePos = tilePos;
        }
        else
        {
            Debug.Log("타일 꽉참");
        }
    }

    // 유닛이 특정 타일에서 이동할 때 호출
    public void MoveTile(UnitGroup _group, Vector3Int newTilePos)
    {
        Vector3Int oldTilePos = GetUnitTilePosition(_group);

        if (!unitPositions.ContainsKey(newTilePos) || oldTilePos == newTilePos)
        {
            Debug.Log("이동할 수 없는 타일입니다.");
            return;
        }

        // 이동할 타일이 비어 있다면 그냥 이동
        if (unitPositions[newTilePos] == null)
        {
            SwapUnitPositions(_group, oldTilePos, newTilePos);
        }
        //else
        //{
        //    // 이동할 위치에 유닛이 있을 경우 자리 교체
        //    UnitGroup otherUnit = unitPositions[newTilePos];
        //    SwapUnitPositions(_group, oldTilePos, newTilePos);
        //    SwapUnitPositions(otherUnit, newTilePos, oldTilePos);
        //}
    }

    public Vector3Int GetUnitTilePosition(UnitGroup _group)
    {
        foreach (var entry in unitPositions)
        {
            if (entry.Value == _group)
            {
                return entry.Key;
            }
        }
        return Vector3Int.zero; 
    }

    public bool IsTileOccupied(Vector3Int tilePos)
    {
        return unitPositions.ContainsKey(tilePos) && unitPositions[tilePos] != null;
    }

    public UnitGroup GetTileGroup(Vector3Int tilePos)
    {
        if (!IsTileOccupied(tilePos))
        {
            selectGroup = null;
            onSelectGroup.OnNext(selectGroup);
            return null;
        }

        selectGroup = unitPositions[tilePos];
        onSelectGroup.OnNext(selectGroup);
        return unitPositions[tilePos];
    }

    private void SwapUnitPositions(UnitGroup _group, Vector3Int prevPos, Vector3Int newPos)
    {
        if (unitPositions.ContainsKey(prevPos))
        {
            unitPositions[prevPos] = null; // 기존 위치 비우기
        }

        unitPositions[newPos] = _group;
        _group.parentObj.transform.position = tilemap.GetCellCenterWorld(newPos);

        Debug.Log($"유닛 이동: {prevPos} → {newPos}");
    }

    public Vector3Int FindEmptyTile()
    {
        BoundsInt bounds = tilemap.cellBounds;

        for (int x = bounds.xMin; x <= bounds.xMax; x++) // 열(왼쪽 → 오른쪽)
        {
            for (int y = bounds.yMax; y >= bounds.yMin; y--) // 행(위쪽 → 아래쪽)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0);

                if (tilemap.HasTile(tilePos) && unitPositions.ContainsKey(tilePos))
                {
                    if(unitPositions[tilePos] == null)
                        return tilePos;
                }
            }
        }
        return Vector3Int.zero; 
    }

    public void FindMonsterPathTiles()
    {
        pathTiles.Clear();
        BoundsInt bounds = tilemap.cellBounds;
        var tempTiles = new List<Vector3Int>();
        List<Vector3Int> leftColumn = new List<Vector3Int>();
        List<Vector3Int> topRow = new List<Vector3Int>();
        List<Vector3Int> rightColumn = new List<Vector3Int>();
        List<Vector3Int> bottomRow = new List<Vector3Int>();
        for (int y = bounds.yMin; y < bounds.yMax; y++)
        {
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                var tilePos = new Vector3Int(x, y, 0);
                if (tilemap.HasTile(tilePos))
                    tempTiles.Add(tilePos);
            }
        }
        var minXY = new Vector2(tempTiles.Min(_ => _.x), tempTiles.Min(_ => _.y));
        var maxXY = new Vector2(tempTiles.Max(_ => _.x), tempTiles.Max(_ => _.y));
        foreach (var _tile in tempTiles)
        {
            if (_tile.y == minXY.y)
                bottomRow.Add(_tile);

            if (_tile.y == maxXY.y)
                topRow.Add(_tile);

            if (_tile.x == minXY.x)
                leftColumn.Add(_tile);

            if (_tile.y == maxXY.x)
                rightColumn.Add(_tile);
        }
        if(playerIndex == 1)
        {
            rightColumn.Reverse();
            bottomRow.Reverse();
            pathTiles.AddRange(leftColumn);
            pathTiles.AddRange(topRow);
            pathTiles.AddRange(rightColumn);
            pathTiles.AddRange(bottomRow);
        }
        else if(playerIndex == 2)
        {
            leftColumn.Reverse();
            topRow.Reverse();
            pathTiles.AddRange(leftColumn);
            pathTiles.AddRange(bottomRow);
            pathTiles.AddRange(rightColumn);
            pathTiles.AddRange(topRow);
        }
        
        pathTiles.Distinct();
    }
}
