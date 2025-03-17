using System;
using System.Collections;
using System.Collections.Generic;
using Data;
using CustomNode.State;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using UnityEngine.AI;
using Random = UnityEngine.Random;
using UnityEngine.Tilemaps;

public interface IUnitLogicMovement
{
    void Init(UnitLogic _logic, Tilemap _tilemap, List<Vector3Int> _tiles);

    void FrameMove(float _delta);
}

public class UnitLogicMovement : IUnitLogicMovement
{
    private UnitLogic unitLogic;
    private Tilemap tilemap;
    private List<Vector3Int> pathTiles = new List<Vector3Int>(); // 경로 저장
    private int currentPathIndex = 0;



    public void Init(UnitLogic _logic, Tilemap _tilemap, List<Vector3Int> _tiles)
    {
        unitLogic = _logic;
        tilemap = _tilemap;
        pathTiles = _tiles;

        if (!pathTiles.IsNullOrEmpty())
        {
            unitLogic.transform.position = tilemap.GetCellCenterWorld(pathTiles[0]); // 시작 위치
        }
    }

    public void FrameMove(float _delta)
    {
        unitLogic.nodeGraph?.FrameMove(_delta * unitLogic.GetAtkSpeedPer);
        if (unitLogic.unitType == Define.EUnitType.Monster)
            UnitMonsterMove(_delta);
    }

    private void UnitMonsterMove(float _delta)
    {
        if (tilemap == null || pathTiles.IsNullOrEmpty())
            return;

        Vector3 targetPosition = tilemap.GetCellCenterWorld(pathTiles[currentPathIndex]);
        if(Vector3.Distance(unitLogic.transform.position, targetPosition) > 0.1f)
        {
            unitLogic.transform.position = Vector3.MoveTowards(unitLogic.transform.position, targetPosition, unitLogic.statSo.speed * _delta);
        }
        else
        {
            currentPathIndex++;
            if (currentPathIndex >= pathTiles.Count)
            {
                currentPathIndex = 0;
            }
        }
    }
}
