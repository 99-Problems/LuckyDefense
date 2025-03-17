using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;

using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PBInGameResult : PopupArg
{
    public string strDesc;
    public Color colorDesc = Color.white;
    public Action onClose;
}

public class PopupInGameResult : PopupBase
{
    public UIOpenAni textAni;

    public GTMPro textDesc;

    protected PBInGameResult arg;

    public override void OnClosePopup()
    {
        arg.onClose?.Invoke();
        base.OnClosePopup();
    }

    public override void InitPopupbox(PopupArg popupData)
    {
        arg = (PBInGameResult)popupData;

        textDesc.SetText(arg.strDesc);
        textDesc.SetColor(arg.colorDesc);
    }

    public override void PressBackButton()
    {
        Managers.Popup.ClosePopupBox(gameObject);
    }
}