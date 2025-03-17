using Data;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;


public class MythInfoScrollViewItem : BaseScrollViewItem<UnitMythInfoScript>
{

    private UnitMythInfoScript info;
    public UnitMythInfoScript Info => info;

    private void Start()
    {
        
    }

    public override void Init(UnitMythInfoScript _info, int _index)
    {
        if (_info == null)
            return;

        info =_info;
        
       
        UpdateUI();
    }

    public void UpdateUI()
    {
        
    }

    public void Select(int unitID)
    {

    }
}
