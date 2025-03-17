
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
public class StringKoreanScript
{

    [Key(0)]public int stringID;
    [Key(1)]public int langaugeType;
    [Key(2)]public int stringData;

}

public partial class DataManager
{
    [Serializable][MessagePackObject]
    public class StringKoreanScriptAll
    {
        [Key(0)]public List<StringKoreanScript> result;
    }



    private List<StringKoreanScript> listStringKoreanScript = null;


    public StringKoreanScript GetStringKoreanScript(Predicate<StringKoreanScript> predicate)
    {
        return listStringKoreanScript?.Find(predicate);
    }
    public List<StringKoreanScript> GetStringKoreanScriptList { 
        get { 
                return listStringKoreanScript;
        }
    }



    void ClearStringKorean()
    {
        listStringKoreanScript?.Clear();
    }


    async UniTask LoadScriptStringKorean()
    {
        List<StringKoreanScript> resultScript = null;
        if(resultScript == null)
        {
            var load = await Managers.Resource.LoadScript("scripts/string", "stringKorean"); 
            if (load == "") 
            {
                Debug.LogWarning("StringKorean is empty");
                return;
            }
            var json = JsonUtility.FromJson<StringKoreanScriptAll>("{ \"result\" : " + load + "}");
            resultScript = json.result;
        }



        listStringKoreanScript = resultScript;


    }
}


