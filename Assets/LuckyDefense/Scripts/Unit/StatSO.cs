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
    [Title("���� ����")]
    [InfoBox("�켱������ ���� ���� �ش� ��ų�� ����")]
    [LabelText("�̵��ӵ�")]
    public float speed = 0;

    [LabelText("ġ��Ÿ ���� ����")]
    public int critDmgRate = 2;

    [LabelText("ü�¹� ��ġ")]
    public float hpBarPosY = 1.5f;

    [LabelText("ü�¹� ��ġ(z)")]
    public float hpBarPosZ = -1.2f;

    [LabelText("Ž������")]
    public float detectRange = 10;

    [LabelText("Ž����������")]
    public float explorationReleaseRange = 2;

    [Title("���°�����")]
    public StateManagerGraph stateManager;

    public StateGraph deathState;

}