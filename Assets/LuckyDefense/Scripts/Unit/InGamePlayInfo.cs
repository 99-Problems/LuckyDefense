using Cysharp.Threading.Tasks;
using Data;
using Data.Managers;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UniRx;
using UnityEngine;
using UnityEngine.Tilemaps;

public struct IngamePlayData
{
    public float limitTime;
    public float currentTime;
    public float timeTick;
    public int monsterCount;
    public int wave;
    public int gold;
    public int stone;
    public int unitCount;
    public int summonCost;
    public int summonCount;

    public IngamePlayData CopyInstance()
    {
        return new IngamePlayData()
        {
            limitTime = this.limitTime,
            currentTime = this.currentTime,
            timeTick = this.timeTick,
            monsterCount = this.monsterCount,
            wave = this.wave,
            gold = this.gold,
            stone = this.stone,
            unitCount = this.unitCount,
            summonCost = this.summonCost,
            summonCount = this.summonCount,
        };

    }
}

public class InGamePlayInfo : MonoBehaviour
{
    public enum EPLAY_STATE
    {
        SORT,
        READY,
        PLAY,
        PAUSE,
        STOP,
        END,
    }
    public ReactiveProperty<EPLAY_STATE> playState = new ReactiveProperty<EPLAY_STATE>(EPLAY_STATE.SORT);

    protected IGameData gameData;
    private GameObject particleParentObj;
    public GameObject GetParticleParentObject()
    {
        if (particleParentObj == null)
        {
            particleParentObj = new GameObject("Particle");
            particleParentObj.transform.SetParent(transform);
        }

        return particleParentObj;
    }

    private IStageClearCondition condition;
    [HideInInspector]
    public List<InGamePlayerInfo> listPlayer = new List<InGamePlayerInfo>();
    [HideInInspector]
    public List<InGamePlayerInfo> listEnemy = new List<InGamePlayerInfo>();
    [HideInInspector]
    public List<InGamePlayerInfo> listMonster = new List<InGamePlayerInfo>();

    private InGamePlayerInfo player;
    public InGamePlayerInfo Player => player;

    private InGamePlayerInfo bot;
    public InGamePlayerInfo Bot => bot;

    public float botPlayTick = 1f;
    private float curBotDelta = 0f;

    public bool isLastEvent { get; private set; } = false;

    public IngamePlayData playData;
    public IngamePlayData botPlayData;
    private bool botLogicStop;

    public GamePlayInfoScript gamePlayInfo { get; private set; }

    public bool isLastWave { get; private set; }

    private InGameSpawnInfoScript currentWave;
    private InGameSpawnInfoScript nextWave;
    public InGameSpawnInfoScript GetNextWaveInfo => nextWave;
    private bool initComplete;

    public Subject<UnitLogic> onDieMonster = new Subject<UnitLogic>();

    public ConcurrentBag<DamageParticleData> damageParticleInfo = new ConcurrentBag<DamageParticleData>();
    private List<InGameSpawnInfoScript> spawnInfoList = new List<InGameSpawnInfoScript>();
    private bool isLast;
    private int maxWave;
    private bool isBoss;
    private int summonCount;
    private bool isSpawning;

    public bool isGameOver;

    private void Start()
    {
        gameData = Managers.Scene.CurrentScene as IGameData;

        gameData.OnLoadingComplete.Subscribe(_ =>
        {

        }).AddTo(this);
    }

    private void Update()
    {
        SpawnDamageParticle();
    }

    public void Init(GamePlayInfoScript _info, InGamePlayerInfo _localPlayer, InGamePlayerInfo _enemyPlayer, List<InGameSpawnInfoScript> _spawnInfos)
    {
        if (_info == null)
            return;

        Clear();
        gamePlayInfo = _info;
        player = _localPlayer;
        bot = _enemyPlayer;

        spawnInfoList = _spawnInfos;
        currentWave = spawnInfoList.FirstOrDefault();
        nextWave = spawnInfoList.First(_ => _.wave > currentWave.wave);
        maxWave = spawnInfoList.Max(_ => _.wave);

        playData.limitTime = currentWave.waveTime;
        playData.currentTime = 0;
        playData.timeTick = 0;
        playData.wave = currentWave.wave;
        playData.gold = _info.startgold;
        playData.stone = 0;
        playData.unitCount = 0;
        playData.summonCost = gamePlayInfo.summonCost;

        botPlayData = playData.CopyInstance();

        initComplete = true;
    }

    public virtual void FrameMove(float _deltaTime)
    {

        if (!initComplete)
            return;

        if (playState.Value != EPLAY_STATE.END)
        {
            if (IsEndCondition() || (condition != null && condition.IsStageEndCondition()))
            {
                playState.Value = EPLAY_STATE.END;
                Managers.Time.SetGameSpeed(1);
                
                return;
            }
        }
        switch (playState.Value)
        {
            case EPLAY_STATE.SORT:
                return;
            case EPLAY_STATE.READY:
                break;
            case EPLAY_STATE.PLAY:
                playData.currentTime = playData.currentTime + _deltaTime;
                playData.timeTick += _deltaTime;

                AIPlayLogic(_deltaTime);

                if(isLast && !isSpawning)
                {
                    if(IsBossAlive() == false)
                    {
                        // 게임 클리어
                        playState.Value = EPLAY_STATE.END;
                        Debug.ColorLog("게임 클리어");
                        return;
                    }
                }
                else if (playData.currentTime > currentWave.waveTime)
                {
                    if(isBoss && IsBossAlive())
                    {
                        //Game Over
                        playState.Value = EPLAY_STATE.END;
                        Debug.Log("게임 오버",Color.red);
                        isGameOver = true;
                        return;
                    }

                    isBoss = false;

                    if (currentWave.wave < maxWave)
                    {
                        //next wave
                        currentWave = nextWave;
                        playData.wave = currentWave.wave;
                        var nextScript = spawnInfoList.Find(_ => _.wave == playData.wave + 1);
                        nextWave = nextScript != null ? nextScript : nextWave;
                        
                    }
                    else
                    {
                        isLast = true;
                    }
                    playData.limitTime = currentWave.waveTime;
                    playData.currentTime = 0;
                }

                if (isSpawning || isBoss)
                    break;


                if (currentWave.spawnTick < 0)
                {
                    //Spawn Boss
                    isBoss = true;
                    isSpawning = true;
                    SpawnMonsters(currentWave.unitID, currentWave.statGrade);
                }
                else if (playData.timeTick >= currentWave.spawnTick)
                {
                    //Spawn Monster
                    isSpawning = true;
                    playData.timeTick = 0;
                    SpawnMonsters(currentWave.unitID, currentWave.statGrade);

                }

                break;
            case EPLAY_STATE.PAUSE:
            case EPLAY_STATE.STOP:
                break;
            case EPLAY_STATE.END:
                return;
            default:
                break;
        }

        foreach (var mit in listPlayer)
        {
            mit.FrameMove(_deltaTime);
        }

        foreach (var mit in listEnemy)
        {
            mit.FrameMove(_deltaTime);
        }

        foreach (var mit in listMonster)
        {
            mit.FrameMove(_deltaTime);
        }
    }

    public void EndGame()
    {
        foreach (var player in listPlayer)
        {
            foreach (var unit in player.listUnit)
            {
                unit.Clear();
            }
        }
    }
    public void Clear()
    {

    }

    internal bool IsStageEndCondition()
    {
        return playState.Value == EPLAY_STATE.END;
    }

    public void SetStageClearCondition(IStageClearCondition _condition)
    {
        condition = _condition;
    }

    public void JoinPlayers(IEnumerable<InGamePlayerInfo> players, Define.EUnitType unitType)
    {
        if(unitType == Define.EUnitType.Player)
        {
            foreach (var mit in players)
            {
                mit.transform.SetParent(gameObject.transform);
                mit.Entry();
                if (mit.playerIndex == 1)
                    listPlayer.Add(mit);
                else if (mit.playerIndex == 2)
                    listEnemy.Add(mit);
            }
        }
        else if (unitType == Define.EUnitType.Monster)
        {
            foreach (var mit in players)
            {
                mit.transform.SetParent(gameObject.transform);
                mit.Entry();
                listMonster.Add(mit);
            }
        }
    }

    public async UniTask SetStageReady()
    {
        await UniTask.WaitUntil(() => IngameLoadingImage.instance != null ? IngameLoadingImage.instance.isloadingComplete : true);
        playState.Value = EPLAY_STATE.READY;

        StageStart();
    }
    public void StageStart()
    {
        playState.Value = EPLAY_STATE.PLAY;


        Debug.ColorLog("게임 시작", Color.green);
    }

    protected virtual bool IsEndCondition()
    {
#if UNITY_EDITOR
        if (Managers.isinfinityMode) return false;
#endif
        return IsGameOver();
            //IsAnyPlayerDie() || playData.currentTime >= playData.limitTime || isStop;
    }

    public bool IsGameOver()
    {
        int cnt = 0;
        foreach (var monster in listMonster)
        {
            cnt += monster.listUnit.Count;
        }
        return cnt >= gamePlayInfo.maxMonsterCount;
    }

    public int GetMonsterCount()
    {
        int count = 0;
        foreach (var monster in listMonster)
        {
            count += monster.UnitCount;
        }

        return count;
    }

    public void AddDamageParticle(UnitLogic _unitLogic, Define.EDAMAGE_TYPE _type, long _calcHp, bool _isCritical, bool _isAvoid, Vector3 position, bool isMoveRight, long accountID)
    {
        damageParticleInfo.Add(new DamageParticleData
        {
            type = _type,
            calcHp = _calcHp,
            isCritical = _isCritical,
            isAvoid = _isAvoid,
            position = position,
            isMoveRight = isMoveRight,
            accountID = accountID,
        });
    }

    private void SpawnDamageParticle()
    {
        while (!damageParticleInfo.IsEmpty)
        {
            if (damageParticleInfo.TryTake(out var info))
            {
                var clone = Managers.Pool.PopDamageParticle();
                if (clone == null)
                    return;
                clone.transform.SetParent(GetParticleParentObject().transform);
                if (info.isAvoid)
                {
                    clone.transform.position = new Vector3(info.position.x, info.position.y + 0.1f, info.position.z);
                }
                else if (false == info.isCritical)
                {
                    //            clone.transform.position = new Vector3(position.x + UnityEngine.Random.Range(-0.3f, 0.3f),
                    //              position.y + UnityEngine.Random.Range(-0.8f, 0), position.z);

                    clone.transform.position = new Vector3(info.position.x + UnityEngine.Random.Range(-0.3f, 0.3f),
                        info.position.y + UnityEngine.Random.Range(-0.5f, -0.2f), info.position.z);
                }
                else
                {
                    //            clone.transform.position = new Vector3(position.x, position.y + 0.4f, position.z);
                    clone.transform.position = new Vector3(info.position.x + UnityEngine.Random.Range(-0.15f, 0.15f),
                        info.position.y + UnityEngine.Random.Range(-0.15f, 0.15f), info.position.z);
                }

                var particle = clone.GetComponent<DamageParticle>();

                particle.Init(info.type, info.calcHp, info.isCritical, info.isAvoid);
            }
        }
    }

#if UNITY_EDITOR
    [Button("재화 디버그버튼")]
    public void DebugBtn(int gold =9999,int stone=9999)
    {
        playData.gold += gold;
        playData.stone += stone;
        botPlayData.gold += gold;
        botPlayData.stone += stone;
    }
#endif

    public void OnKillMonster(UnitLogic _unit)
    {
        var dropInfo = Managers.Data.GetMonsterDropInfoScript(_ => _.unitID == _unit.UnitID);
        if(dropInfo == null)
        {
            Debug.LogError("드롭정보 없음");
            return;
        }
        if(dropInfo.dropItemType == Define.ItemType.Gold)
        {
            playData.gold += dropInfo.itemCount;
            botPlayData.gold += dropInfo.itemCount;
        }
        else if (dropInfo.dropItemType == Define.ItemType.Stone)
        {
            playData.stone += dropInfo.itemCount;
            botPlayData.stone += dropInfo.itemCount;
        }

        Debug.ColorLog($"{Managers.String.GetString(_unit.Info.nameID)} 처치 : {dropInfo.dropItemType.ToString()} {dropInfo.itemCount} 획득");
    }

    public async UniTask<bool> GambleUnit(InGamePlayerInfo _spawnPlayer, UnitGambleInfoScript _info)
    {
        float random = _spawnPlayer.GetRandom(0f, 100f);
        if(random >= _info.rate.PPMToFloat())
            return false;

        var logic = await SummonRandUnit(_spawnPlayer, _spawnPlayer.tilemap, Vector3Int.zero, _info.rarity, true);

        if(logic == null)
        {
            Debug.LogError("spawn failed: no unitLogic");
        }
        if(_info.itemType == Define.ItemType.Gold)
        {
            playData.gold -= _info.itemCount;
        }
        else if (_info.itemType == Define.ItemType.Stone)
        {
            playData.stone -= _info.itemCount;
        }
            
        playData.unitCount = _spawnPlayer.UnitCount;

        return true;
    }
    public async UniTask<UnitLogic> SummonMythUnit(InGamePlayerInfo _spawnPlayer, UnitMythInfoScript _info)
    {
        if (_info == null)
            return null;

        var list = new List<int>();
        if(_info.needUnitID1 > 0)
        {
            list.Add(_info.needUnitID1);
        }
        if (_info.needUnitID2 > 0)
        {
            list.Add(_info.needUnitID2);
        }
        if (_info.needUnitID3 > 0)
        {
            list.Add(_info.needUnitID3);
        }

        foreach (var unitID in list)
        {
            var unit = _spawnPlayer.GetUnitFromID(unitID);
            if (unit == null)
            {
                Debug.LogError($"{unitID} 유닛 없음");
                return null;
            }

            var _group = _spawnPlayer.listUnitGroup.Find(_ => _.groupUnitID == unitID);
            _spawnPlayer.RemoveUnit(_group);
        }

        var pos = _spawnPlayer.FindEmptyTile();
        var unitLogic = await SpawnUnit(_spawnPlayer, _info.unitID, _info.unitID, pos, Define.EUnitType.Player, _spawnPlayer.tilemap);
        var group = _spawnPlayer.AddUnit(unitLogic);
        _spawnPlayer.PlaceUnit(group, pos);

        return null;
    }

    public async UniTask<UnitLogic> SummonRandUnit(InGamePlayerInfo _spawnPlayer, Tilemap _tilemap, Vector3Int _pos,
                                    Define.EUNIT_RARITY _rarity = Define.EUNIT_RARITY.None, bool isFixedRarity = false)
    {
        float random = _spawnPlayer.GetRandom(0f, 100f);

        var summonInfos = Managers.Data.GetUnitSummonInfoScriptList.OrderByDescending(_=>_.rarity);
        if(_rarity > Define.EUNIT_RARITY.None)
        {
            if(isFixedRarity)
            {
                summonInfos = summonInfos.Where(_ => _.rarity == _rarity).OrderByDescending(_ => _.rarity);
            }
            else
            {
                summonInfos = summonInfos.Where(_ => _.rarity > _rarity).OrderByDescending(_ => _.rarity);
            }
            
        }

        var Info = summonInfos.LastOrDefault();

        float curRate = 0f;
        foreach (var _info in summonInfos)
        {
            var rate = _info.rate.PPMToFloat();
            if (random + curRate < rate)
            {
                Info = _info;
                break;
            }

            curRate += rate;
        }
        var list = Managers.Data.GetUnitInfoRarityList(Info.rarity, Define.EUnitType.Player);
        int unitIndex = _spawnPlayer.GetRandom(0, list.Count);
        var randUnit = list[unitIndex];
        var pos = Vector3Int.zero;
        if (_pos == Vector3Int.zero)
        {
            pos = _spawnPlayer.FindEmptyTile();
        }
        else
        {
            pos = _pos;
        }

        if(pos == Vector3Int.zero)
        {
            Debug.LogError("자리없음");
            return null;
        }

        var unitLogic = await SpawnUnit(_spawnPlayer, randUnit.unitID, randUnit.unitID, pos, Define.EUnitType.Player, _tilemap);
        var group = _spawnPlayer.AddUnit(unitLogic);
        _spawnPlayer.PlaceUnit(group, pos);

        if(!isFixedRarity && _spawnPlayer == player)
        {
            playData.gold -= playData.summonCost;
            playData.summonCount++;
            playData.summonCost = gamePlayInfo.summonCost + (playData.summonCount * gamePlayInfo.addCost);
            playData.unitCount = _spawnPlayer.UnitCount;
        }
        else if(!isFixedRarity && _spawnPlayer == bot)
        {
            botPlayData.gold -= botPlayData.summonCost;
            botPlayData.summonCount++;
            botPlayData.summonCost = gamePlayInfo.summonCost + (botPlayData.summonCount * gamePlayInfo.addCost);
            botPlayData.unitCount = _spawnPlayer.UnitCount;
        }


        return unitLogic;
    }

    public async UniTaskVoid SpawnMonsters(int unitID, int _statGrade)
    {
        foreach (var monster in listMonster)
        {
            var logic = await SpawnMonster(monster, monster.tilemap, monster.startPath, unitID, _statGrade);

            Action<UnitLogic> onUnitKill = (_logic) =>
            {
                OnKillMonster(_logic);

            };

            logic.OnDieEvent += onUnitKill;
        }
        isSpawning = false;
    }

    public async UniTask<UnitLogic> SpawnMonster(InGamePlayerInfo _spawnPlayer, Tilemap _tilemap, Vector3Int spawnPos, int _unitID = 9, int _statGrade = 9)
    {
        var unitLogic = await SpawnUnit(_spawnPlayer, _unitID, _statGrade, _spawnPlayer.startPath, Define.EUnitType.Monster, _tilemap, _spawnPlayer.GetTiles);
        var unitList = new List<UnitLogic>();
        unitList.Add(unitLogic);
        _spawnPlayer.AddUnit(unitList);
        unitLogic.gameObject.transform.localScale = Vector3.one * 0.3f;
        unitLogic.gameObject.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.Linear);

        return unitLogic;
    }


    private async UniTask<UnitLogic> SpawnUnit(InGamePlayerInfo _spawnPlayer, int _unitID, int _statGrade, Vector3 _pos,
                                                        Define.EUnitType _unitType, Tilemap _tilemap, List<Vector3Int> _tiles = null)
    {
        var _unitInfo = Managers.Data.GetUnitInfo(_unitID);

#if UNITY_EDITOR
        if(_spawnPlayer == null)
        {
            Debug.LogError($"Spawn Unit Failed : No owner (unitID : {_unitID})");
        }

        if (_unitInfo == null)
        {
            Debug.LogError("Spawn Unit Failed " + _unitID);
        }
#endif

        var clone = Managers.Pool.PopUnit(_unitInfo);
        if (clone == null)
        {
            clone = await Managers.Pool.CreateUnitPool(_unitInfo);
        }

        clone.transform.SetParent(_spawnPlayer.transform);
        //clone.transform.position = _pos;
        clone.gameObject.SetActive(true);
        var unitLogic = clone.GetComponent<UnitLogic>();
        var _data = new UnitData
        { 
            UnitID = _unitID,
            StatGrade = _statGrade,
        };


        var unitBaseData = new UnitBaseData
        {
            info = _unitInfo,
            unitID = _unitID,
            data = _data,
        };



        unitLogic.Init(unitBaseData, this, _spawnPlayer, _tilemap, _tiles);
        switch (_unitType)
        {
            case Define.EUnitType.Player:
                
                break;
            case Define.EUnitType.Monster:
                //var monsterLogic = unitLogic as MonsterLogic;
                //if (monsterLogic)
                //{
                //    monsterLogic.Init(_index);
                //}
                break;
            default:
                break;
        }

        return unitLogic;
    }


    public async UniTask<UnitLogic> FusionUnit(InGamePlayerInfo _spawnPlayer, UnitGroup _group)
    {
        Vector3Int pos = Vector3Int.zero;
        Define.EUNIT_RARITY rarity = _group.unitLogic.Rarity;
        int unitCount = _group.GetUnitCount();
        for (int i = 0; i < unitCount; i++)
        {
            pos = _spawnPlayer.RemoveUnit(_group);
        }
        var logic = await SummonRandUnit(_spawnPlayer, _spawnPlayer.tilemap, pos, rarity, true);
        Debug.ColorLog($"{_spawnPlayer.name} : {rarity.ToString()} 유닛 조합 -> {Managers.String.GetString(logic.Info.nameID)} ({logic.Rarity.ToString()})");
        return logic;
    }

    public bool IsBossAlive()
    {
        foreach (var monster in listMonster)
        {
            foreach (var _unit in monster.listUnit)
            {
                if (_unit.Rarity == Define.EUNIT_RARITY.Boss)
                    return true;
            }
        }

        return false;
    }

    public void AIPlayLogic(float _deltaTime)
    {
        if (botLogicStop)
            return;

        curBotDelta += _deltaTime;
        if(curBotDelta >= botPlayTick)
        {
            curBotDelta = 0;
        }
        else
        {
            return;
        }


        var isLimitUnit = botPlayData.unitCount >= gamePlayInfo.maxUnitCount;
        var isSummonable = botPlayData.gold >= botPlayData.summonCost && !isLimitUnit;

        if(isSummonable)
        {
            botLogicStop = true;
            async void BotSummon()
            {
                await SummonRandUnit(bot, bot.tilemap, Vector3Int.zero);
                botLogicStop = false;
            }

            BotSummon();
        }

        foreach (var group in bot.listUnitGroup)
        {
            if(group.IsMaxUnit() && group.unitLogic.Rarity.IsFusionableRarity())
            {
                botLogicStop = true;
                async void BotFusion()
                {
                    await FusionUnit(bot, group);
                    botLogicStop = false;
                }
                BotFusion();
                break;
            }
        }
    }
}
