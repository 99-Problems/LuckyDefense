using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class PopupMyth : PopupBase
{
    public GTMPro unitNameText;
    public MythInfoScrollView mythScrollVeiw;
    public UnitMythInfoScrollView infoScrollView;

    public Button exitBtn;
    public Button btnMythSummon;

    private UnitMythInfoScript info;


    private void Start()
    {
       
        mythScrollVeiw.OnItemClick.Subscribe(_ =>
        {
            info = _.Info;
            UpdateUI();
        }).AddTo(this);

        exitBtn.OnClickAsObservableThrottleFirst().Subscribe(_ =>
        {
            PressBackButton();
        });

        btnMythSummon.OnClickAsObservableThrottleFirst().Subscribe(async _ =>
        {
            btnMythSummon.interactable = false;
            var data = Managers.Scene.CurrentScene as IGameData;
            await data.PlayInfo.SummonMythUnit(data.PlayInfo.Player, info);
            
            PressBackButton();
            Managers.Popup.ShowPopupBox(Define.EPOPUP_TYPE.PopupPush, new PBPush
            {
                pushTime = 0.5f,
                strDesc = "신화급 소환",
            });

        });
    }

    public void UpdateUI()
    {
        if (info == null)
            return;

        var list = new List<UnitInfoScript>();
        if (info.needUnitID1 > 0)
        {
            var unit = Managers.Data.GetUnitInfo(info.needUnitID1);
            list.Add(unit);
        }
        if (info.needUnitID2 > 0)
        {
            var unit = Managers.Data.GetUnitInfo(info.needUnitID2);
            list.Add(unit);
        }
        if (info.needUnitID3 > 0)
        {
            var unit = Managers.Data.GetUnitInfo(info.needUnitID3);
            list.Add(unit);
        }
        var mythUnit = Managers.Data.GetUnitInfo(info.unitID);
        unitNameText.SetText(Managers.String.GetString(mythUnit.nameID));

        infoScrollView.SetItemList(list);

        btnMythSummon.gameObject.SetActive(true);
        foreach (var item in infoScrollView.GetItem())
        {
            if(item.isOwnUnit == false)
            {
                btnMythSummon.gameObject.SetActive(false);
                break;
            }
        }
    }

    public override void InitPopupbox(PopupArg _popupData)
    {
        base.InitPopupbox(_popupData);

        var list = Managers.Data.GetUnitMythInfoScriptList;
        mythScrollVeiw.SetItemList(list);
        info = list.FirstOrDefault();

        UpdateUI();
    }

    public override void OnClosePopup()
    {
        base.OnClosePopup();
    }

    public override void PressBackButton()
    {
        Managers.Popup.ClosePopupBox(this);
    }
}
