using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UniRx;

public partial class DataManager
{
    public int cntLoad = 0;
   public int maxCnt = 12;
    
    public void Complete()
    {
        cntLoad++;
    }
    
    public float LoadProcess()
    {
        return (float)cntLoad / (float)maxCnt;
    }

    public async UniTask LoadAllParser()
    {
        cntLoad = 0;
        ClearConstValue();
        ClearGamePlayInfo();
        ClearInGameSpawnInfo();
        ClearMonsterDropInfo();
        ClearStringKorean();
        ClearUnitGambleInfo();
        ClearUnitInfo();
        ClearUnitMythInfo();
        ClearUnitSellInfo();
        ClearUnitSkillInfo();
        ClearUnitStatInfo();
        ClearUnitSummonInfo();

    await UniTask.WhenAll(
            LoadScriptConstValue(),
            LoadScriptGamePlayInfo(),
            LoadScriptInGameSpawnInfo(),
            LoadScriptMonsterDropInfo(),
            LoadScriptStringKorean(),
            LoadScriptUnitGambleInfo(),
            LoadScriptUnitInfo(),
            LoadScriptUnitMythInfo(),
            LoadScriptUnitSellInfo(),
            LoadScriptUnitSkillInfo());
    await UniTask.WhenAll(
            LoadScriptUnitStatInfo(),
            LoadScriptUnitSummonInfo());
    }
}
