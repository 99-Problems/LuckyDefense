
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
public class UnitSummonInfoScript
{

    [Key(0)]public Define.EUNIT_RARITY rarity;
    [Key(1)]public Int64 rate;

}

public partial class DataManager
{
    [Serializable][MessagePackObject]
    public class UnitSummonInfoScriptAll
    {
        [Key(0)]public List<UnitSummonInfoScript> result;
    }



    private List<UnitSummonInfoScript> listUnitSummonInfoScript = null;


    public UnitSummonInfoScript GetUnitSummonInfoScript(Predicate<UnitSummonInfoScript> predicate)
    {
        return listUnitSummonInfoScript?.Find(predicate);
    }
    public List<UnitSummonInfoScript> GetUnitSummonInfoScriptList { 
        get { 
                return listUnitSummonInfoScript;
        }
    }



    void ClearUnitSummonInfo()
    {
        listUnitSummonInfoScript?.Clear();
    }


    async UniTask LoadScriptUnitSummonInfo()
    {
        List<UnitSummonInfoScript> resultScript = null;
        if(resultScript == null)
        {
            var load = await Managers.Resource.LoadScript("scripts/", "UnitSummonInfo"); 
            if (load == "") 
            {
                Debug.LogWarning("UnitSummonInfo is empty");
                return;
            }
            var json = JsonUtility.FromJson<UnitSummonInfoScriptAll>("{ \"result\" : " + load + "}");
            resultScript = json.result;
        }



        listUnitSummonInfoScript = resultScript;


    }
}


