using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Data;

public static class UnitLogicExtension
{
    public static async void AddDamageArea(this UnitLogic _unit
        , int _skillID
       , Vector3 _pos
       , float moveTime
       , Vector3 _size, float _tick,
       float _delayTime, float _duration, long _damage, Define.EDAMAGE_TYPE _damageType, bool _isMove,Vector3 _dir, float _power = 0)
    {
        ProjectileLogic collider = Managers.Pool.PopBoxCollider();
        if (collider == null)
            collider = await Managers.Pool.CreateBoxCollider();
       
        


        if (_size != Vector3.zero)
        {
            collider.transform.localScale = new Vector3(1 * _size.x, 1 * _size.y, 1 * _size.z);
        }

#if UNITY_EDITOR
        collider.name = $"{Managers.String.GetString(_unit.Info.nameID)}_{_skillID}";
#endif
        collider.transform.position = _pos;

        if (_skillID > 0)
        {
            var skillInfo = Managers.Data.GetUnitSkillInfoScript(_ => _.skillID == _skillID);
            if (skillInfo == null)
            {
                Debug.LogError("스킬 정보 없음");
            }
            else
            {
                var effectObject = Managers.Pool.PopEffectGameObject(_skillID);
                if (effectObject == null)
                    effectObject = await Managers.Pool.CreateEffectGameObjectPool(_skillID);

                effectObject.transform.SetParent(collider.transform);
                effectObject.transform.localPosition = Vector3.zero;
                effectObject.gameObject.SetActive(true);
            }
        }


        collider.Init(_unit, _skillID, _tick, _delayTime, _duration, _damage, _damageType, _isMove, _dir,_power);
        _unit.Owner.AddObject(collider);
    }

    public static Dictionary<Define.EStatType, long> GetAllStatInfo(int _statGrade)
    {
        Dictionary<Define.EStatType, long> statValue = new Dictionary<Define.EStatType, long>();
        var statList = Managers.Data.GetUnitStatData(_statGrade);
        for (int i = 0; i < statList.Count; i++)
        {
            statValue[statList[i].stat] = statList[i].val;
        }

        return statValue;
    }

    public static List<Vector3> GetUnitGroupTempFormaition()
    {
        var _groupPivots = new List<Vector3>
        {
            new Vector3(-0.5f,0,0),
            new Vector3(0.5f,0,0),
            new Vector3(0,0.5f,0),
        };

        return _groupPivots;
    }
}

public class UnitData
{
    public int UnitID { get; set; }
    public int StatGrade { get; set; }
}

public class StatData
{
    public Define.EStatType stat;
    public long val;
}

