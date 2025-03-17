
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
public class MonsterDropInfoScript
{

    [Key(0)]public int unitID;
    [Key(1)]public Define.ItemType dropItemType;
    [Key(2)]public int itemCount;

}

public partial class DataManager
{
    [Serializable][MessagePackObject]
    public class MonsterDropInfoScriptAll
    {
        [Key(0)]public List<MonsterDropInfoScript> result;
    }



    private List<MonsterDropInfoScript> listMonsterDropInfoScript = null;


    public MonsterDropInfoScript GetMonsterDropInfoScript(Predicate<MonsterDropInfoScript> predicate)
    {
        return listMonsterDropInfoScript?.Find(predicate);
    }
    public List<MonsterDropInfoScript> GetMonsterDropInfoScriptList { 
        get { 
                return listMonsterDropInfoScript;
        }
    }



    void ClearMonsterDropInfo()
    {
        listMonsterDropInfoScript?.Clear();
    }


    async UniTask LoadScriptMonsterDropInfo()
    {
        List<MonsterDropInfoScript> resultScript = null;
        if(resultScript == null)
        {
            var load = await Managers.Resource.LoadScript("scripts/", "MonsterDropInfo"); 
            if (load == "") 
            {
                Debug.LogWarning("MonsterDropInfo is empty");
                return;
            }
            var json = JsonUtility.FromJson<MonsterDropInfoScriptAll>("{ \"result\" : " + load + "}");
            resultScript = json.result;
        }



        listMonsterDropInfoScript = resultScript;


    }
}


