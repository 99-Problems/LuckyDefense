using Cysharp.Threading.Tasks;
using Data;
using Data.Managers;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public Canvas uiView;
    GraphicRaycaster graphicRaycaster;
    public bool isVisible = true;

    public Image monsterProgressBar;
    public GTMPro monsterCountText;
    public GTMPro timerText;
    public GTMPro waveCountText;
    public GTMPro stoneCountText;
    public GTMPro summonCostText;
    public GTMPro unitCountText;
    public GTMPro goldText;

    public Button summonBtn;
    public Button gambleBtn;
    public Button mythBtn;

    public Canvas groupInfoCanvas;
    public GameObject rangeObj;
    public float rangeCorrection = 0.75f;
    public GameObject moveGuideObj;
    public Button sellBtn;
    public Button fusionBtn;
    public GTMPro sellCostText;
    public GameObject goldIcon;
    public GameObject stoneIcon;

    public UnitInfoUI unitInfoUI;

    private IGameData gameData;


    private Subject<Unit> unitChangeEvent = new Subject<Unit>();
    private bool isSummonable;
    private Vector3Int tilePos;

    private void Start()
    {
        isVisible = false;
        graphicRaycaster = gameObject.GetOrAddComponent<GraphicRaycaster>();

        if (uiView == null)
            uiView = GetComponent<Canvas>();

        groupInfoCanvas.worldCamera = Camera.main;
        groupInfoCanvas.gameObject.transform.rotation = Camera.main.transform.rotation;

        gameData = Managers.Scene.CurrentScene as IGameData;
        gameData.OnLoadingComplete.Subscribe(_ =>
        {
            gameObject.UpdateAsObservable().Subscribe(OnUpdateEvent).AddTo(this);

            gameData.PlayInfo.Player.onSelectGroup.Subscribe(_group =>
            {
                UpdateGroupInfo(_group);
            }).AddTo(this);

            Managers.Input.dragDir.Subscribe(dir =>
            {
                moveGuideObj.SetActive(gameData.PlayInfo.Player.selectGroup != null);
                unitInfoUI.gameObject.SetActive(false);
                fusionBtn.gameObject.SetActive(false);
                sellBtn.gameObject.SetActive(false);
                if (gameData.PlayInfo.Player.selectGroup != null)
                {
                    Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    tilePos = gameData.PlayInfo.Player.tilemap.WorldToCell(mouseWorldPos);

                    if (gameData.PlayInfo.Player.tilemap.HasTile(tilePos))
                    {
                        // UI를 타일 중심 위치로 이동
                        moveGuideObj.transform.position = gameData.PlayInfo.Player.tilemap.GetCellCenterWorld(tilePos);
                    }

                }

            }).AddTo(this);

            Managers.Input.dragSubject.Subscribe(dir =>
            {
                moveGuideObj.SetActive(false);

                if (gameData.PlayInfo.Player.selectGroup != null && gameData.PlayInfo.Player.tilemap.HasTile(tilePos))
                    gameData.PlayInfo.Player.MoveTile(gameData.PlayInfo.Player.selectGroup, tilePos);

                UpdateGroupInfo(gameData.PlayInfo.Player.selectGroup);
            }).AddTo(this);

            summonBtn.OnClickAsObservable().Subscribe(async task =>
            {
                if (!isSummonable)
                    return;

                var playInfo = gameData?.PlayInfo;
                if (playInfo == null)
                    return;

                summonBtn.interactable = false;

                await playInfo.SummonRandUnit(playInfo.Player, playInfo.Player.tilemap, Vector3Int.zero);

                summonBtn.interactable = true;

            }).AddTo(this);

            gambleBtn.OnClickAsObservableThrottleFirst().Subscribe(_ =>
            {
                if (Managers.Popup.IsPopupActive(Define.EPOPUP_TYPE.PopupGamble))
                    return;

                Managers.Popup.ShowPopupBox(Define.EPOPUP_TYPE.PopupGamble, PopupArg.empty, false);

            }).AddTo(this);

            fusionBtn.OnClickAsObservableThrottleFirst().Subscribe(async _1 =>
            {
                fusionBtn.interactable = false;
                var player = gameData.PlayInfo.Player;
                await gameData.PlayInfo.FusionUnit(player, player.selectGroup);
                UpdateGroupInfo(player.selectGroup);
            });

            sellBtn.OnClickAsObservableThrottleFirst().Subscribe(_ =>
            {
                var group = gameData.PlayInfo.Player.selectGroup;
                if (group == null)
                    return;

                var sellResult = gameData.PlayInfo.Player.SellUnit(group);
                gameData.PlayInfo.playData.unitCount--;
                switch (sellResult.Item1)
                {
                    case Define.ItemType.Gold:
                        gameData.PlayInfo.playData.gold += sellResult.Item2;
                        break;
                    case Define.ItemType.Stone:
                        gameData.PlayInfo.playData.stone += sellResult.Item2;
                        break;
                    default:
                        break;
                }

                UpdateGroupInfo(group);
            });

            mythBtn.OnClickAsObservableThrottleFirst().Subscribe(_ =>
            {
                Managers.Popup.ShowPopupBox(Define.EPOPUP_TYPE.PopupMyth, PopupArg.empty);
            });
        }).AddTo(this);

        var gameState = Managers.Scene.CurrentScene as IGameState;
        gameState.OnStateObservable.Subscribe(state =>
        {
            if (state == Define.EGAME_STATE.PLAY)
            {
                isVisible = true;
                gameState.SetMenuVisible(true);
            }
        }).AddTo(this);

        gameState.MenuVisibleObservable().Subscribe(_1 =>
        {
            SetMenuVisible(_1);
        }).AddTo(this);


        Managers.Popup.OnClosePopupSubject.Subscribe(_ =>
        {
            if (isVisible)
            {
                //메뉴가 열려있을때 풀 팝업이 없다면 캔버스를 켠다
                if (Managers.Popup.CheckExistFullPopup() == false)
                {
                    SetMenuVisible(true);
                }
            }

        }).AddTo(this);

        Managers.Popup.OnshowPopupSubject.Subscribe(_ =>
        {
            if (isVisible)
            {
                //메뉴가 열려있을때 풀 팝업이 없다면 캔버스를 끈다
                if (Managers.Popup.CheckExistFullPopup())
                {
                    SetMenuVisible(false);
                }
            }
        }).AddTo(this);


    }

    public void UpdateGroupInfo(UnitGroup _group)
    {
       
        moveGuideObj.SetActive(false);
        
        unitInfoUI.gameObject.SetActive(_group?.unitLogic != null);
        unitInfoUI.Init(_group?.unitLogic);
        if (_group == null || _group.unitLogic == null)
        {
            groupInfoCanvas.gameObject.SetActive(false);
            return;
        }
        groupInfoCanvas.gameObject.SetActive(true);
        //groupInfoCanvas.transform.SetParent(_group.parentObj.transform);
        groupInfoCanvas.transform.position = _group.parentObj.transform.position;
        rangeObj.transform.localScale = Vector3.one * _group.unitLogic.statSo.detectRange * rangeCorrection;
        moveGuideObj.transform.position = _group.parentObj.transform.position;

        fusionBtn.gameObject.SetActive(_group.unitLogic.Rarity.IsFusionableRarity());
        fusionBtn.interactable = _group.IsMaxUnit();
        var sellInfo = Managers.Data.GetUnitSellInfoScript(_ => _.rarity == _group.unitLogic.Rarity);
        sellCostText.SetText(sellInfo.itemCount);
        sellBtn.gameObject.SetActive(true);
        goldIcon.SetActive(sellInfo.itemType == Define.ItemType.Gold);
        stoneIcon.SetActive(sellInfo.itemType == Define.ItemType.Stone);

        
    }

    public void SetMenuVisible(bool _b)
    {
        uiView.enabled = _b;
        graphicRaycaster.enabled = _b;
    }

    

    public void OnUpdateEvent(Unit _unit)
    {
        var playInfo = gameData?.PlayInfo;
        if (playInfo == null)
            return;


        var offset = Mathf.Max(0, playInfo.playData.limitTime - playInfo.playData.currentTime);
        
        timerText.SetText((int)offset / 60, (int)offset % 60);

        monsterProgressBar.fillAmount = (float)playInfo.GetMonsterCount() / playInfo.gamePlayInfo.maxMonsterCount;
        monsterCountText.SetText(playInfo.GetMonsterCount(), playInfo.gamePlayInfo.maxMonsterCount);

        var isLimitUnit = playInfo.playData.unitCount >= playInfo.gamePlayInfo.maxUnitCount;
        isSummonable = playInfo.playData.gold >= playInfo.playData.summonCost && !isLimitUnit;
        summonCostText.SetText(playInfo.playData.summonCost);
        summonCostText.SetColor(isSummonable ? Color.white : Color.red);

        goldText.SetText(playInfo.playData.gold);
        waveCountText.SetText(playInfo.playData.wave);
        stoneCountText.SetText(playInfo.playData.stone);
        unitCountText.SetText(playInfo.playData.unitCount, playInfo.gamePlayInfo.maxUnitCount);
        unitCountText.SetColor(isLimitUnit ? Color.red : Color.white);
    }
}
