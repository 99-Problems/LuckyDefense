
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
public class ConstValueScript
{

    [Key(0)]public Define.ConstDefType type;
    [Key(1)]public Int64 value;

}

public partial class DataManager
{
    [Serializable][MessagePackObject]
    public class ConstValueScriptAll
    {
        [Key(0)]public List<ConstValueScript> result;
    }



    private List<ConstValueScript> listConstValueScript = null;


    public ConstValueScript GetConstValueScript(Predicate<ConstValueScript> predicate)
    {
        return listConstValueScript?.Find(predicate);
    }
    public List<ConstValueScript> GetConstValueScriptList { 
        get { 
                return listConstValueScript;
        }
    }



    void ClearConstValue()
    {
        listConstValueScript?.Clear();
    }


    async UniTask LoadScriptConstValue()
    {
        List<ConstValueScript> resultScript = null;
        if(resultScript == null)
        {
            var load = await Managers.Resource.LoadScript("scripts/", "ConstValue"); 
            if (load == "") 
            {
                Debug.LogWarning("ConstValue is empty");
                return;
            }
            var json = JsonUtility.FromJson<ConstValueScriptAll>("{ \"result\" : " + load + "}");
            resultScript = json.result;
        }



        listConstValueScript = resultScript;


    }
}


