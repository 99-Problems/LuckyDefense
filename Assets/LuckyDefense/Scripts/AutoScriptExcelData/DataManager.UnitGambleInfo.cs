
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
public class UnitGambleInfoScript
{

    [Key(0)]public Define.EUNIT_RARITY rarity;
    [Key(1)]public Define.ItemType itemType;
    [Key(2)]public int itemCount;
    [Key(3)]public Int64 rate;

}

public partial class DataManager
{
    [Serializable][MessagePackObject]
    public class UnitGambleInfoScriptAll
    {
        [Key(0)]public List<UnitGambleInfoScript> result;
    }



    private List<UnitGambleInfoScript> listUnitGambleInfoScript = null;


    public UnitGambleInfoScript GetUnitGambleInfoScript(Predicate<UnitGambleInfoScript> predicate)
    {
        return listUnitGambleInfoScript?.Find(predicate);
    }
    public List<UnitGambleInfoScript> GetUnitGambleInfoScriptList { 
        get { 
                return listUnitGambleInfoScript;
        }
    }



    void ClearUnitGambleInfo()
    {
        listUnitGambleInfoScript?.Clear();
    }


    async UniTask LoadScriptUnitGambleInfo()
    {
        List<UnitGambleInfoScript> resultScript = null;
        if(resultScript == null)
        {
            var load = await Managers.Resource.LoadScript("scripts/", "UnitGambleInfo"); 
            if (load == "") 
            {
                Debug.LogWarning("UnitGambleInfo is empty");
                return;
            }
            var json = JsonUtility.FromJson<UnitGambleInfoScriptAll>("{ \"result\" : " + load + "}");
            resultScript = json.result;
        }



        listUnitGambleInfoScript = resultScript;


    }
}


