using Cysharp.Threading.Tasks;
using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public partial class DataManager
{
    public void Init()
    {

    }

    public async UniTask LoadScript()
    {
        await LoadAllParser();
    }

    public UnitInfoScript GetUnitInfo(int _unitID)
    {
        return GetUnitInfoScript(_ => _.unitID == _unitID);
    }

    public Int64 GetConstValue(Define.ConstDefType constDef)
    {
        var ret = GetConstValueScript(_ => _.type == constDef);
        if (ret == null)
            return 0;

        return ret.value;
    }

    public List<UnitInfoScript> GetUnitInfoRarityList(Define.EUNIT_RARITY _rarity, Define.EUnitType _type)
    {
        return GetUnitInfoScriptList.Where(_ => _.unitRarity == _rarity && _.unitType == _type).ToList();
    }
}
