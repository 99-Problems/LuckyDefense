
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
public class GamePlayInfoScript
{

    [Key(0)]public int grade;
    [Key(1)]public int startgold;
    [Key(2)]public int maxMonsterCount;
    [Key(3)]public int maxUnitCount;
    [Key(4)]public int summonCost;
    [Key(5)]public int addCost;

}

public partial class DataManager
{
    [Serializable][MessagePackObject]
    public class GamePlayInfoScriptAll
    {
        [Key(0)]public List<GamePlayInfoScript> result;
    }



    private List<GamePlayInfoScript> listGamePlayInfoScript = null;


    public GamePlayInfoScript GetGamePlayInfoScript(Predicate<GamePlayInfoScript> predicate)
    {
        return listGamePlayInfoScript?.Find(predicate);
    }
    public List<GamePlayInfoScript> GetGamePlayInfoScriptList { 
        get { 
                return listGamePlayInfoScript;
        }
    }



    void ClearGamePlayInfo()
    {
        listGamePlayInfoScript?.Clear();
    }


    async UniTask LoadScriptGamePlayInfo()
    {
        List<GamePlayInfoScript> resultScript = null;
        if(resultScript == null)
        {
            var load = await Managers.Resource.LoadScript("scripts/", "GamePlayInfo"); 
            if (load == "") 
            {
                Debug.LogWarning("GamePlayInfo is empty");
                return;
            }
            var json = JsonUtility.FromJson<GamePlayInfoScriptAll>("{ \"result\" : " + load + "}");
            resultScript = json.result;
        }



        listGamePlayInfoScript = resultScript;


    }
}


