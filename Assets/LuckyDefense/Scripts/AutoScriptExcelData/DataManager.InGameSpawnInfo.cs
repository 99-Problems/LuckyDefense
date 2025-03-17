
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
public class InGameSpawnInfoScript
{

    [Key(0)]public int grade;
    [Key(1)]public int wave;
    [Key(2)]public int waveTime;
    [Key(3)]public int spawnTick;
    [Key(4)]public int unitID;
    [Key(5)]public int statGrade;
    [Key(6)]public int waveClearGold;

}

public partial class DataManager
{
    [Serializable][MessagePackObject]
    public class InGameSpawnInfoScriptAll
    {
        [Key(0)]public List<InGameSpawnInfoScript> result;
    }



    private List<InGameSpawnInfoScript> listInGameSpawnInfoScript = null;


    public InGameSpawnInfoScript GetInGameSpawnInfoScript(Predicate<InGameSpawnInfoScript> predicate)
    {
        return listInGameSpawnInfoScript?.Find(predicate);
    }
    public List<InGameSpawnInfoScript> GetInGameSpawnInfoScriptList { 
        get { 
                return listInGameSpawnInfoScript;
        }
    }



    void ClearInGameSpawnInfo()
    {
        listInGameSpawnInfoScript?.Clear();
    }


    async UniTask LoadScriptInGameSpawnInfo()
    {
        List<InGameSpawnInfoScript> resultScript = null;
        if(resultScript == null)
        {
            var load = await Managers.Resource.LoadScript("scripts/", "InGameSpawnInfo"); 
            if (load == "") 
            {
                Debug.LogWarning("InGameSpawnInfo is empty");
                return;
            }
            var json = JsonUtility.FromJson<InGameSpawnInfoScriptAll>("{ \"result\" : " + load + "}");
            resultScript = json.result;
        }



        listInGameSpawnInfoScript = resultScript;


    }
}


