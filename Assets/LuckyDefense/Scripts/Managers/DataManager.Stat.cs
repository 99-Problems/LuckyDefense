using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public partial class DataManager 
{
    public List<StatData> GetUnitStatData(int statGrade)
    {
        List<StatData> statDatas = new List<StatData>();

        var statInfos = Managers.Data.GetUnitStatInfoScriptList.Where(_=>_.accountStatGrade == statGrade);
        if (statInfos == null)
        {
            return statDatas;
        }


        foreach (var _stat in statInfos)
        {
            var _statData = new StatData
            { 
                stat = _stat.statType,
                val = _stat.statVal,
            };

            statDatas.Add(_statData);
        }
        
        return statDatas;
    }
}
