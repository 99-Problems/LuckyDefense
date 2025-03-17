
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
public class UnitSellInfoScript
{

    [Key(0)]public Define.EUNIT_RARITY rarity;
    [Key(1)]public Define.ItemType itemType;
    [Key(2)]public int itemCount;

}

public partial class DataManager
{
    [Serializable][MessagePackObject]
    public class UnitSellInfoScriptAll
    {
        [Key(0)]public List<UnitSellInfoScript> result;
    }



    private List<UnitSellInfoScript> listUnitSellInfoScript = null;


    public UnitSellInfoScript GetUnitSellInfoScript(Predicate<UnitSellInfoScript> predicate)
    {
        return listUnitSellInfoScript?.Find(predicate);
    }
    public List<UnitSellInfoScript> GetUnitSellInfoScriptList { 
        get { 
                return listUnitSellInfoScript;
        }
    }



    void ClearUnitSellInfo()
    {
        listUnitSellInfoScript?.Clear();
    }


    async UniTask LoadScriptUnitSellInfo()
    {
        List<UnitSellInfoScript> resultScript = null;
        if(resultScript == null)
        {
            var load = await Managers.Resource.LoadScript("scripts/", "UnitSellInfo"); 
            if (load == "") 
            {
                Debug.LogWarning("UnitSellInfo is empty");
                return;
            }
            var json = JsonUtility.FromJson<UnitSellInfoScriptAll>("{ \"result\" : " + load + "}");
            resultScript = json.result;
        }



        listUnitSellInfoScript = resultScript;


    }
}


