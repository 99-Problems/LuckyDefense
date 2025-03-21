using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx.Triggers;
using UniRx;
using Cysharp.Threading.Tasks;
using Data;
using Data.Managers;
using System.Linq;
using Unity.Linq;
using UnityEngine.EventSystems;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Playables;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

public interface IGameData
{
    InGamePlayInfo PlayInfo { get; set; }
    IObservable<bool?> OnLoadingComplete { get; }
    Action<float> OnFrameMove { set; get; }
    Define.ECONTENT_TYPE ContentType { get; set; }
}

public interface IStageClearCondition
{
    bool IsStageEndCondition();
}

public interface IGameState
{
    IObservable<Define.EGAME_STATE> OnStateObservable { get; }

    IObservable<bool> MenuVisibleObservable();
    void SetMenuVisible(bool _b);
    bool GetMenuVisible();
    Define.EGAME_STATE GetGameState { get; }
    bool IsShowClearSequence { get; }
    void Result();

}



public class GameSceneInit : BaseScene, IGameData, IStageClearCondition, IGameState
{
    public Tilemap playerGround;
    public Tilemap enemyGround;
    public Tilemap player2Ground;
    public Tilemap enemy2Ground;

    private OneReplaySubject<bool?> loadingComplete = new OneReplaySubject<bool?>(null);
    private InGamePlayerInfo localPlayer;
    private InGamePlayerInfo otherPlayer;
    private InGamePlayerInfo monsterPlayer;
    private InGamePlayerInfo monsterPlayer2;
    private InGamePlayInfo playInfo;
    [HideInInspector]
    public List<InGamePlayerInfo> gamePlayerInfo = new List<InGamePlayerInfo>();
    [HideInInspector]
    private List<InGamePlayerInfo> gameMonsterInfo = new List<InGamePlayerInfo>();

    InGamePlayInfo IGameData.PlayInfo
    {
        get => playInfo;
        set => playInfo = value;
    }

    #region 에디터 설정
#if UNITY_EDITOR
    private const string SpeedDebugger = "Menu/스피드토글";
    public static bool isSpeedDebugger => EditorPrefs.GetBool(SpeedDebugger);


    [MenuItem(SpeedDebugger)]
    private static void DebuggerToggle()
    {
        var isDebug = !isSpeedDebugger;
        Menu.SetChecked(SpeedDebugger, isDebug);
        EditorPrefs.SetBool(SpeedDebugger, isDebug);
        SceneView.RepaintAll();
    }
    private static Rect windowRect = new Rect(360, 70, 120, 115+ 30);

    private void OnGUI()
    {
        if (!isSpeedDebugger)
        {
            return;
        }

        var preRect = windowRect;
        windowRect = GUI.Window(0, windowRect, DebugWindow, "Debug");
        if (preRect != windowRect)
        {
            EditorPrefs.SetFloat("GameX", windowRect.x);
            EditorPrefs.SetFloat("GameY", windowRect.y);
        }
    }
#endif
    #endregion

    //IGameData
    public IObservable<bool?> OnLoadingComplete => loadingComplete;
    public Action<float> OnFrameMove { get; set; }
    public Define.ECONTENT_TYPE ContentType { get; set; }

    //IGameState
    [NonSerialized]
    public ReactiveProperty<Define.EGAME_STATE> gameState =
        new ReactiveProperty<Define.EGAME_STATE>(Define.EGAME_STATE.LOADING);

    Define.EGAME_STATE IGameState.GetGameState => gameState.Value;
    private BoolReactiveProperty menuVisible = new BoolReactiveProperty(false);
    private Define.EGAME_STATE prevState;


    public IObservable<Define.EGAME_STATE> OnStateObservable => gameState.AsObservable();

    public bool IsShowClearSequence => showEndSequence;

    Action endSequence;
    private bool showEndSequence;
    private bool isGameStart;


    void Start()
    {
        Loading();
    }

    protected override void Init()
    {
        base.Init();

        Managers.Scene.CurrentSceneType = Define.Scene.GameScene;
        ContentType = Define.ECONTENT_TYPE.INGAME;
    }
    private void Update()
    {
        if (Managers.Popup.IsShowPopup() || Managers.Popup.IsWaitPopup() || !loadingComplete.Value.HasValue || !isGameStart)
            return;

        #region EndPopup
        if (Input.GetKeyDown(KeyCode.Escape))
        {

            if (Managers.Popup.IsShowSystemMenu())
            {
                Managers.Popup.ShowSystenMenu(false);
                return;
            }


            var gameData = Managers.Scene.CurrentScene as IGameData;
            if (gameData != null && gameData.ContentType.UseIngameExitBtn())
            {
                return;
            }

        }
        #endregion

    }


    private async UniTask Release()
    {
        await Managers.String.LoadStringInfo(); //스트링 로딩
        await Managers.Data.LoadScript(); // 스크립트 로딩
        await UniTask.DelayFrame(1);
        await Resources.UnloadUnusedAssets();
       
    }
    public virtual async UniTaskVoid Loading()
    {
        await Release();

        gameObject.FixedUpdateAsObservable().Subscribe(_ =>
                {
                    FrameMove(Time.fixedDeltaTime);
                }).AddTo(this);


        IngameLoadingImage.LoadingEvent.OnNext(10);

        await UniTask.WaitForEndOfFrame();

        IngameLoadingImage.LoadingEvent.OnNext(20);
        await UniTask.DelayFrame(30);

        
        await Managers.Pool.CreateDamageParticle();

        


        var obj = new GameObject { name = "InGamePlayInfo" };
        var _playInfo = obj.AddComponent<InGamePlayInfo>();
        _playInfo.SetStageClearCondition(this);
        playInfo = _playInfo;
        IngameLoadingImage.LoadingEvent.OnNext(40);
        #region Player
        var player = new GameObject("Player");
        localPlayer = player.AddComponent<InGamePlayerInfo>();
        localPlayer.GameData = this;

        var gamePlayInfoScript = Managers.Data.GetGamePlayInfoScript(_ => _.grade == 1);

        gamePlayerInfo.Add(localPlayer);
        localPlayer.Init(gamePlayInfoScript, playerGround, 1);

        var _groupPivots = UnitLogicExtension.GetUnitGroupTempFormaition();
        localPlayer.groupPivots.AddRange(_groupPivots);

        #endregion

        #region Bot
        var botPlayer = new GameObject("Bot");
        otherPlayer = botPlayer.AddComponent<InGamePlayerInfo>();
        otherPlayer.GameData = this;

        gamePlayerInfo.Add(otherPlayer);
        otherPlayer.Init(gamePlayInfoScript, player2Ground, 2);
        #endregion

        #region Monsters
        var monster = new GameObject("Monsters");
        monsterPlayer = monster.AddComponent<InGamePlayerInfo>();
        monsterPlayer.GameData = this;

        gameMonsterInfo.Add(monsterPlayer);
        monsterPlayer.Init(gamePlayInfoScript, enemyGround, 1, true);

        var monster2 = new GameObject("Monsters_2");
        monsterPlayer2 = monster.AddComponent<InGamePlayerInfo>();
        monsterPlayer2.GameData = this;

        gameMonsterInfo.Add(monsterPlayer2);
        monsterPlayer2.Init(gamePlayInfoScript, enemy2Ground, 2, true);

        #endregion


        IngameLoadingImage.LoadingEvent.OnNext(50);


        await UniTask.Delay(100);
        IngameLoadingImage.LoadingEvent.OnNext(60);

        playInfo.Init(gamePlayInfoScript, localPlayer, otherPlayer, Managers.Data.GetInGameSpawnInfoScriptList.Where(_=>_.grade == 1).ToList());
        playInfo.JoinPlayers(gamePlayerInfo, Define.EUnitType.Player);
        playInfo.JoinPlayers(gameMonsterInfo, Define.EUnitType.Monster);

        IngameLoadingImage.LoadingEvent.OnNext(70);

        SetEndSequence();
        
        Resources.UnloadUnusedAssets();

        await UniTask.WaitForEndOfFrame();
        SetGameState(Define.EGAME_STATE.LOADING_COMPLETE);
        if (bgm != null)
        {
            Managers.Sound.Play(bgm, Define.Sound.Bgm);
        }

        loadingComplete.OnNext(true);
        Managers.Input.Init(this, playerGround);
        Managers.Input.isInteractable = true;
        IngameLoadingImage.LoadingEvent.OnNext(100);
        Managers.Popup.ShowReservationPopup();

        playInfo.SetStageReady();
    }

    private void FrameMove(float _delta)
    {
        if (!loadingComplete.Value.HasValue)
            return;

        if(Managers.Time.IsPause)
        {
            if (gameState.Value != Define.EGAME_STATE.PAUSE)
            {
                prevState = gameState.Value;
                SetGameState(Define.EGAME_STATE.PAUSE);
            }
            return;
        }

        switch (gameState.Value)
        {
            case Define.EGAME_STATE.LOADING:
                break;
            case Define.EGAME_STATE.LOADING_COMPLETE:
                SetGameState(Define.EGAME_STATE.ENTRY);
                break;
            case Define.EGAME_STATE.ENTRY:
                if(playInfo.playState.Value == InGamePlayInfo.EPLAY_STATE.PLAY)
                    SetGameState(Define.EGAME_STATE.ENTRY_COMPLETE);
                break;
            case Define.EGAME_STATE.PLAY:
                OnFrameMove?.Invoke(_delta);
                playInfo?.FrameMove(_delta);

                if (playInfo.IsStageEndCondition())
                {
                    SetGameState(Define.EGAME_STATE.RESULT);
                }
                break;
            case Define.EGAME_STATE.PAUSE:
                break;
            case Define.EGAME_STATE.RESULT:
                break;
            case Define.EGAME_STATE.COMMANDER:
                break;
            case Define.EGAME_STATE.MANAGE:
                break;
            case Define.EGAME_STATE.ENTRY_COMPLETE:
                SetGameState(Define.EGAME_STATE.PLAY);
                break;
            default:
                break;
        }
    }

    public void SetGameState(Define.EGAME_STATE _state)
    {
        switch (_state)
        {
            case Define.EGAME_STATE.LOADING:
                break;
            case Define.EGAME_STATE.LOADING_COMPLETE:
                break;
            case Define.EGAME_STATE.ENTRY:
                break;
            case Define.EGAME_STATE.PLAY:
                break;
            case Define.EGAME_STATE.PAUSE:
                SetGameState(prevState);
                break;
            case Define.EGAME_STATE.RESULT:
                playInfo?.EndGame();
                Result();
                break;
            case Define.EGAME_STATE.COMMANDER:
                break;
            case Define.EGAME_STATE.MANAGE:
                break;
            case Define.EGAME_STATE.ENTRY_COMPLETE:
                isGameStart = true;
                break;
            default:
                break;
        }

        gameState.Value = _state;
    }

    public override void Clear()
    {
    }



    public bool IsStageEndCondition()
    {
        return false;
    }

    public void Result()
    {
        endSequence?.Invoke();
        endSequence = null;
    }

    private void SetEndSequence()
    {
        if (endSequence == null)
        {
            endSequence = () =>
            {
                Managers.Popup.CloseAllPopupBox();
                OpenResultPopup();
#if LOG_ENABLE && UNITY_EDITOR
                string log = "";
                log += "GAME END".ToColor(Color.red);


                Debug.Log(log);
#endif
            };
        }





        async void OpenResultPopup()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1));
            Managers.Time.SetGameSpeed(1f);
            bool isGameOver = playInfo.IsGameOver() || playInfo.isGameOver;
            Managers.Popup.ShowPopupBox(Define.EPOPUP_TYPE.PopupInGameResult, new PBInGameResult()
            {
               strDesc = isGameOver ? "GAME OVER" : "CLEAR",
               colorDesc = isGameOver ? Color.red : Color.white,
            });
        }
    }

    public IObservable<bool> MenuVisibleObservable()
    {
        return menuVisible;
    }

    public void SetMenuVisible(bool _b)
    {
        menuVisible.Value = _b;
    }

    public bool GetMenuVisible()
    {
        return menuVisible.Value;
    }

    void OnApplicationPause(bool isPaused)
    {
        if (gameState.Value != Define.EGAME_STATE.PLAY || gameState.Value == Define.EGAME_STATE.PAUSE)
            return;

        if(!Managers.Time.IsPause && isPaused)
        {
            //Debug.ColorLog($"퍼즈");
        }
    }
}
