using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using CustomNode.State;
using CustomNode.StateManager;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Data;


[Serializable]
public class UnitStatSO
{
    [Title("유닛 스탯")]
    [InfoBox("우선순위는 높을 수록 해당 스킬을 쓴다")]
    [LabelText("이동속도")]
    public float speed = 0;

    [LabelText("치명타 피해 배율")]
    public int critDmgRate = 2;

    [LabelText("체력바 위치")]
    public float hpBarPosY = 1.5f;

    [LabelText("체력바 위치(z)")]
    public float hpBarPosZ = -1.2f;

    [LabelText("탐색범위")]
    public float detectRange = 10;

    [LabelText("탐색해제범위")]
    public float explorationReleaseRange = 2;

    [Title("상태관리자")]
    public StateManagerGraph stateManager;

    public StateGraph deathState;

}