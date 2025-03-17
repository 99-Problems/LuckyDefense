using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class PopupGamble : PopupBase
{
    public GTMPro rareRate;
    public GTMPro rareCost;
    public GTMPro heroRate;
    public GTMPro heroCost;

    public Button btnRare;
    public Button btnHero;
    public Button btnBack;


    private InGamePlayInfo playInfo;
    private IngamePlayData data;
    private UnitGambleInfoScript rareInfo;
    private UnitGambleInfoScript heroInfo;


    private void Start()
    {
        btnBack.OnClickAsObservableThrottleFirst().Subscribe(_ =>
        {
            PressBackButton();
        });

        btnRare.OnClickAsObservableThrottleFirst().Subscribe(async _ =>
        {
            btnRare.interactable = false;
            var gamble = await playInfo.GambleUnit(playInfo.Player, rareInfo);
            UpdateUI();
            Managers.Popup.ShowPopupBox(Define.EPOPUP_TYPE.PopupPush, new PBPush
            { 
                pushTime = 0.5f,
                strDesc = gamble ? "성공" : "실패",
            });

        });

        btnHero.OnClickAsObservableThrottleFirst().Subscribe(async _ =>
        {
            btnHero.interactable = false;
            var gamble = await playInfo.GambleUnit(playInfo.Player, heroInfo);
            UpdateUI();
            Managers.Popup.ShowPopupBox(Define.EPOPUP_TYPE.PopupPush, new PBPush
            {
                pushTime = 0.5f,
                strDesc = gamble ? "성공" : "실패",
            });
        });
    }

    public void UpdateUI()
    {
        rareRate.SetText(rareInfo.rate.PPMToFloat());
        heroRate.SetText(heroInfo.rate.PPMToFloat());

        bool isRare = false;
        bool isHero = false;
        if (rareInfo.itemType == Define.ItemType.Gold)
        {
            isRare = data.gold >= rareInfo.itemCount;
        }
        else if (rareInfo.itemType == Define.ItemType.Stone)
        {
            isRare = data.stone >= rareInfo.itemCount;
        }

        if (heroInfo.itemType == Define.ItemType.Gold)
        {
            isHero = data.gold >= heroInfo.itemCount;
        }
        else if (heroInfo.itemType == Define.ItemType.Stone)
        {
            isHero = data.stone >= heroInfo.itemCount;
        }

        rareCost.SetText(rareInfo.itemCount);
        rareCost.SetColor(isRare ? Color.white : Color.red);
        btnRare.interactable = isRare;

        heroCost.SetText(heroInfo.itemCount);
        heroCost.SetColor(isHero ? Color.white : Color.red);
        btnHero.interactable = isHero;
    }

    public override void InitPopupbox(PopupArg _popupData)
    {
        base.InitPopupbox(_popupData);

        var gameData = Managers.Scene.CurrentScene as IGameData;
        if (gameData == null)
            return;

        playInfo = gameData.PlayInfo;
        data = gameData.PlayInfo.playData;

        rareInfo = Managers.Data.GetUnitGambleInfoScript(_ => _.rarity == Define.EUNIT_RARITY.Rare);
        heroInfo = Managers.Data.GetUnitGambleInfoScript(_ => _.rarity == Define.EUNIT_RARITY.Hero);

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
