
/********************************************************/
/*Auto Create File*/
/*Source : ExcelToJsonConvert*/
/********************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using MessagePack;
using UnityEngine;
using UniRx;
using Data;
using System.IO;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif


[Serializable][MessagePackObject]
public class UnitStatInfoScript
{

    [Key(0)]public int accountStatGrade;
    [Key(1)]public Define.EStatType statType;
    [Key(2)]public long statVal;

}

public partial class DataManager
{
    [Serializable][MessagePackObject]
    public class UnitStatInfoScriptAll
    {
        [Key(0)]public List<UnitStatInfoScript> result;
    }



    private List<UnitStatInfoScript> listUnitStatInfoScript = null;


    public UnitStatInfoScript GetUnitStatInfoScript(Predicate<UnitStatInfoScript> predicate)
    {
        return listUnitStatInfoScript?.Find(predicate);
    }
    public List<UnitStatInfoScript> GetUnitStatInfoScriptList { 
        get { 
                return listUnitStatInfoScript;
        }
    }



    void ClearUnitStatInfo()
    {
        listUnitStatInfoScript?.Clear();
    }


    async UniTask LoadScriptUnitStatInfo()
    {
        List<UnitStatInfoScript> resultScript = null;
        if(resultScript == null)
        {
            var load = await Managers.Resource.LoadScript("scripts/", "UnitStatInfo"); 
            if (load == "") 
            {
                Debug.LogWarning("UnitStatInfo is empty");
                return;
            }
            var json = JsonUtility.FromJson<UnitStatInfoScriptAll>("{ \"result\" : " + load + "}");
            resultScript = json.result;
        }



        listUnitStatInfoScript = resultScript;


    }
}


