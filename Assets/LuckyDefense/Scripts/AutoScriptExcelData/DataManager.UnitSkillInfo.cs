
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
public class UnitSkillInfoScript
{

    [Key(0)]public int skillID;
    [Key(1)]public Define.SkillType skillType;
    [Key(2)]public Define.EDAMAGE_TYPE damageType;
    [Key(3)]public int effectArg;
    [Key(4)]public int nameID;
    [Key(5)]public int descID;

}

public partial class DataManager
{
    [Serializable][MessagePackObject]
    public class UnitSkillInfoScriptAll
    {
        [Key(0)]public List<UnitSkillInfoScript> result;
    }



    private List<UnitSkillInfoScript> listUnitSkillInfoScript = null;


    public UnitSkillInfoScript GetUnitSkillInfoScript(Predicate<UnitSkillInfoScript> predicate)
    {
        return listUnitSkillInfoScript?.Find(predicate);
    }
    public List<UnitSkillInfoScript> GetUnitSkillInfoScriptList { 
        get { 
                return listUnitSkillInfoScript;
        }
    }



    void ClearUnitSkillInfo()
    {
        listUnitSkillInfoScript?.Clear();
    }


    async UniTask LoadScriptUnitSkillInfo()
    {
        List<UnitSkillInfoScript> resultScript = null;
        if(resultScript == null)
        {
            var load = await Managers.Resource.LoadScript("scripts/", "UnitSkillInfo"); 
            if (load == "") 
            {
                Debug.LogWarning("UnitSkillInfo is empty");
                return;
            }
            var json = JsonUtility.FromJson<UnitSkillInfoScriptAll>("{ \"result\" : " + load + "}");
            resultScript = json.result;
        }



        listUnitSkillInfoScript = resultScript;


    }
}


