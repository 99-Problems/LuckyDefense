using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class MythInfoScrollView : BaseScrollView<MythInfoScrollViewItem, UnitMythInfoScript>
{
    protected override void InitFirstItem(MythInfoScrollViewItem _obj)
    {
        base.InitFirstItem(_obj);
        var btn = _obj.GetComponent<Button>();
        btn.OnClickAsObservable().Subscribe(_ =>
        {
            itemClickSubject.OnNext(_obj);
        }).AddTo(this);
    }
}
