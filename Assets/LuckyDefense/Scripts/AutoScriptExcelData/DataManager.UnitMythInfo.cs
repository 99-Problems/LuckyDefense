
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
public class UnitMythInfoScript
{

    [Key(0)]public int unitID;
    [Key(1)]public int needUnitID1;
    [Key(2)]public int needUnitID2;
    [Key(3)]public int needUnitID3;

}

public partial class DataManager
{
    [Serializable][MessagePackObject]
    public class UnitMythInfoScriptAll
    {
        [Key(0)]public List<UnitMythInfoScript> result;
    }



    private List<UnitMythInfoScript> listUnitMythInfoScript = null;


    public UnitMythInfoScript GetUnitMythInfoScript(Predicate<UnitMythInfoScript> predicate)
    {
        return listUnitMythInfoScript?.Find(predicate);
    }
    public List<UnitMythInfoScript> GetUnitMythInfoScriptList { 
        get { 
                return listUnitMythInfoScript;
        }
    }



    void ClearUnitMythInfo()
    {
        listUnitMythInfoScript?.Clear();
    }


    async UniTask LoadScriptUnitMythInfo()
    {
        List<UnitMythInfoScript> resultScript = null;
        if(resultScript == null)
        {
            var load = await Managers.Resource.LoadScript("scripts/", "UnitMythInfo"); 
            if (load == "") 
            {
                Debug.LogWarning("UnitMythInfo is empty");
                return;
            }
            var json = JsonUtility.FromJson<UnitMythInfoScriptAll>("{ \"result\" : " + load + "}");
            resultScript = json.result;
        }



        listUnitMythInfoScript = resultScript;


    }
}


