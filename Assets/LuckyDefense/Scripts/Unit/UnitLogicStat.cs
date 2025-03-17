using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Data;
#if UNITY_EDITOR
using Sirenix.OdinInspector;
#endif
[System.Serializable]
public class UnitLogicStat
{
    /*
        NONE = 0,
        ATK = 1,
        DEF = 2,
        LIFE = 3,
        ATK_SPEED = 4,
        MOVE_SPEED = 5,
        CRITICAL = 6,
     */

    //Ω∫≈»
    public long atk;
    public long life;
    public long def;
    public long atk_speed;
    public long speed;
    public long critical;

    public Define.EUnitType unitType;

    public void Clear()
    {
        atk = 0;
        life = 0;
        def = 0;
        atk_speed = 1;
        speed = 1;
        critical = 0;
        unitType = Define.EUnitType.None;
    }
    public void Set(UnitInfoScript info, UnitData unitData)
    {
        Clear();
        if (info == null)
            return;

        //unitType = info.unitType;

        var statList = Managers.Data.GetUnitStatData(unitData.StatGrade);
        foreach (var stat in statList)
        {
            SetUnitStatData(stat);
        }
    }

    public void SetUnitStatData(StatData _stat)
    {
        switch (_stat.stat)
        {
            case Define.EStatType.NONE:
                break;
            case Define.EStatType.ATK:
                this.atk += _stat.val;
                break;
            case Define.EStatType.DEF:
                this.def += _stat.val;
                break;
            case Define.EStatType.LIFE:
                this.life += _stat.val;
                break;
            case Define.EStatType.ATK_SPEED:
                this.atk_speed += _stat.val;
                break;
            case Define.EStatType.MOVE_SPEED:
                this.speed += _stat.val;
                break;
            case Define.EStatType.CRITICAL:
                this.critical += _stat.val;
                break;
            default:
                break;
        }
    }

}
