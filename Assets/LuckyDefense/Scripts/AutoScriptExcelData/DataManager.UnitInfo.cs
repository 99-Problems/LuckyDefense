
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
public class UnitInfoScript
{

    [Key(0)]public int unitID;
    [Key(1)]public int nameID;
    [Key(2)]public string prefabName;
    [Key(3)]public string assetPath;
    [Key(4)]public Define.EUnitType unitType;
    [Key(5)]public Define.EUNIT_RARITY unitRarity;
    [Key(6)]public int passiveSkillID;
    [Key(7)]public int normalAttackID;
    [Key(8)]public int activeSkillID;
    [Key(9)]public int active2SkillID;

}

public partial class DataManager
{
    [Serializable][MessagePackObject]
    public class UnitInfoScriptAll
    {
        [Key(0)]public List<UnitInfoScript> result;
    }



    private List<UnitInfoScript> listUnitInfoScript = null;


    public UnitInfoScript GetUnitInfoScript(Predicate<UnitInfoScript> predicate)
    {
        return listUnitInfoScript?.Find(predicate);
    }
    public List<UnitInfoScript> GetUnitInfoScriptList { 
        get { 
                return listUnitInfoScript;
        }
    }



    void ClearUnitInfo()
    {
        listUnitInfoScript?.Clear();
    }


    async UniTask LoadScriptUnitInfo()
    {
        List<UnitInfoScript> resultScript = null;
        if(resultScript == null)
        {
            var load = await Managers.Resource.LoadScript("scripts/", "UnitInfo"); 
            if (load == "") 
            {
                Debug.LogWarning("UnitInfo is empty");
                return;
            }
            var json = JsonUtility.FromJson<UnitInfoScriptAll>("{ \"result\" : " + load + "}");
            resultScript = json.result;
        }



        listUnitInfoScript = resultScript;


    }
}


