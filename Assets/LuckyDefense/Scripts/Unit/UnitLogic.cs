using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Data;
using UnityEngine.AI;
using TMPro;
using UniRx.Triggers;
using UniRx;
using Cysharp.Threading.Tasks;
using Data.Managers;
using static Data.Define;
using System.Linq;
using CustomNode.StateManager;
using UnityEngine.Tilemaps;

public struct UnitBaseData
{
    public UnitInfoScript info;
    public int unitID;
    public UnitData data;

    public UnitBaseData(UnitInfoScript _unitInfo, UnitData _data)
    {
        info = _unitInfo;
        unitID = _unitInfo.unitID;
        data = _data;
    }
}


public class UnitLogic : MonoBehaviour
{
    [LabelText("현재 체력")]
    public long hp = 1; // 유닛 현재 HP 수치

    public long GetOriginalFullHp => stat.life;


    protected bool initialized;

    [LabelText("데미지 표시 위치 보정")]
    public Vector3 textPosition;

    [ReadOnly]
    public UnitLogic aggroTarget;
    public int UnitID => unitBaseData.unitID;
    public UnitInfoScript Info => unitBaseData.info;
    public Define.EUNIT_RARITY Rarity => unitBaseData.info.unitRarity;
    public Define.EUnitType unitType => unitBaseData.info.unitType;

    public Action<float> OnDamageEvent;
    public Action<UnitLogic> OnDieEvent;
    protected int fontTime;
    protected int fontInitTime;
    public int fontTimeMax = 30;
    protected float dieTime;
    public float dieDelay = 1f;

    private protected IUnitLogicMovement movement;
    [ShowInInspector]
    protected InGamePlayInfo playInfo;
    protected InGamePlayerInfo owner;
    public InGamePlayerInfo Owner => owner;
    public UnitGroup groupOwner;
    [SerializeField]
    internal UnitLogicStat stat;
    [ReadOnly]
    [HideInInspector]
    public float atkFrame = 0;

    public LayerMask targetLayerMask;
    private Transform target;
    public UnitStatSO statSo = new UnitStatSO();
    [NonSerialized]
    public StateManagerGraph nodeGraph;

    [ReadOnly]
    public Define.EUNIT_STATE state = Define.EUNIT_STATE.IDLE;
    private protected UnitBaseData unitBaseData;
    public string assetPath => unitBaseData.info.assetPath;
    public string prefabName => unitBaseData.info.prefabName;

    public Animator anim { get; protected set; }
    protected BoxCollider2D _boxCollider;
    public bool IsMonster => unitBaseData.info.unitType == Define.EUnitType.Monster;

    public float GetAtkSpeedPer => stat.atk_speed;

    public virtual void Reset()
    {
        stat.Set(unitBaseData.info, unitBaseData.data);
        hp = GetOriginalFullHp;
        dieTime = 0;
    }
    public virtual void Clear()
    {
        SetState(EUNIT_STATE.IDLE);
    }
    public void SetData(UnitBaseData _unitInitialData)
    {
        unitBaseData = _unitInitialData;
    }

    public virtual void FrameMove(float _deltaTime)
    {
        if (!initialized)
            return;

        if (Managers.Time.GetGameSpeed() <= 0f)
        {
            return;
        }

        movement.FrameMove(_deltaTime);

        switch (state)
        {
            case Define.EUNIT_STATE.RUN:
                break;
            case Define.EUNIT_STATE.ATTACK:
                break;
            case Define.EUNIT_STATE.DIE:
                DeadTick(_deltaTime);
                break;
            case Define.EUNIT_STATE.DESTROY:
                break;
            default:
                break;
        }

        if (IsDie)
        {
            SetState(Define.EUNIT_STATE.DIE);
            OnDieEvent = null;
            return;
        }
    }


    public virtual void Init(UnitBaseData _unitBaseData, InGamePlayInfo _info, InGamePlayerInfo _owner, 
                                Tilemap _tilemap, List<Vector3Int> _pathTiles = null)
    {
        playInfo = _info;
        owner = _owner;
        unitBaseData = _unitBaseData;
        anim = GetComponent<Animator>();
        _boxCollider = GetComponent<BoxCollider2D>();
        
        stat = new UnitLogicStat();


        if (nodeGraph)
        {
            Destroy(nodeGraph);
        }

        if(statSo.stateManager)
        {
            nodeGraph = statSo.stateManager.Copy() as StateManagerGraph;
            nodeGraph.Init(this);
            nodeGraph.SetFindMissingStateFillUp(nodeGraph, this);
        }

        movement = new UnitLogicMovement();
        movement.Init(this, _tilemap, _pathTiles);
        SetState(EUNIT_STATE.IDLE);



        initialized = true;
    }

    
#if LOG_ENABLE && UNITY_EDITOR
    internal string log;

#endif
    public long CalcDamage(Define.EDAMAGE_TYPE edamageType, UnitLogic _attacker, long _skillDamage,
        ref bool _isCritical, ref bool _retAvoid)
    {
        _retAvoid = false;
        float random = 0f;

#if LOG_ENABLE && UNITY_EDITOR
        log = "";
        if(_attacker != null)
        {
            log += $"{_attacker.name} -> {this.name}\t";
            log += $"기본 수치: {_skillDamage}{Environment.NewLine} ";
            log += $"계산 전 체력: {hp}{Environment.NewLine}";
        }
#endif

        long damage = 0;
        damage = _skillDamage;

        random = owner.GetRandom(0, 100);
        if (edamageType != EDAMAGE_TYPE.MAGICAL && random < stat.critical)
        {
#if LOG_ENABLE && UNITY_EDITOR
            log += $"크리티컬".ToColor();
#endif
            var criticalDamage = Mathf.Max(Managers.Data.GetConstValue(Define.ConstDefType.CritDmg).PPMToFloat(),statSo.critDmgRate);
            damage = (long)(damage * criticalDamage);
            _isCritical = true;
        }

        var damageReduce = 0f;
        damageReduce += (float)stat.def;
        damage = (long)(damage * (1 - (damageReduce / 100)));

#if LOG_ENABLE && UNITY_EDITOR
        log += $"{Environment.NewLine} 최종 데미지 : "+ $"{damage}".ToColor(Color.green);

        Debug.Log(log);
#endif
        return damage;
    }
    public virtual long TakeDamage(UnitLogic _attacker, Define.EDAMAGE_TYPE _damageType,
        long _skillDamage, ref bool _isCritical,
        ref bool _retAvoid)
    {
        if (initialized == false)
            return 0;

        long damage = 0;
        long calc = 0;

        damage = CalcDamage(_damageType, _attacker, _skillDamage, ref _isCritical, ref _retAvoid);
        
        if (!IsDie)
        {
            calc = damage < 0 ? 0 : AddHp(_attacker, -damage);
        }
        else
        {
            return 0;
        }
        if(playInfo)
        {
            playInfo.AddDamageParticle(this,
          _damageType,
          damage,
          _isCritical,
          _retAvoid,
          transform.position + new Vector3(0, (_isCritical ? fontTime * 0.15f : 0), 0) + textPosition,
          _attacker.transform.position.x < transform.position.x,
          -1);
        }
       
        return calc;
    }
    public virtual long AddHp(UnitLogic _effector, long _calcHp)
    {
        if (_calcHp < 0)
        {
            OnDamageEvent?.Invoke(_calcHp);
        }

        hp += _calcHp;
        if (hp <= 0)
        {
            _calcHp -= hp;
            hp = 0;
            OnDieEvent?.Invoke(this);
        }
        if(hp > GetOriginalFullHp)
        {
            hp = GetOriginalFullHp;
        }

        return _calcHp;
    }
    
    public virtual bool IsDie => hp <= 0 || state == Define.EUNIT_STATE.DIE
                                || state == Define.EUNIT_STATE.DESTROY;
    

    [Button]
    public virtual void SetState(Define.EUNIT_STATE _state)
    {
        state = _state;
        anim.SetBool("IsIdle", _state == Define.EUNIT_STATE.IDLE);
        anim.SetBool("IsRun", _state == Define.EUNIT_STATE.RUN);
        anim.SetBool("IsAttacking", _state == Define.EUNIT_STATE.ATTACK ||
                               _state == Define.EUNIT_STATE.ACTIVE_SKILL ||
                               _state == Define.EUNIT_STATE.ACTIVE_SKILL2);

        anim.SetBool("IsDead", _state == Define.EUNIT_STATE.DIE ||
                               _state == Define.EUNIT_STATE.DESTROY);

        nodeGraph?.ChangeState(_state);
    }

    public void DeadTick(float _deltaTime)
    {
        dieTime += _deltaTime;
        if (dieTime >= dieDelay)
        {
            SetState(Define.EUNIT_STATE.DESTROY);
            gameObject.SetActive(false);
            owner.listUnit.Remove(this);
            Managers.Pool.PushUnit(unitBaseData.info.assetPath, unitBaseData.info.prefabName, this);


        }
    }
    void OnDrawGizmosSelected()
    {
        if (Application.isPlaying && (Info == null || Info.unitType == EUnitType.Monster))
            return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, statSo.detectRange);
    }
    public void SearchTarget(Define.ESEARCH_TARGET_TYPE type)
    {
        if (aggroTarget != null && !aggroTarget.IsDie)
        {
            var distance = Vector3.Distance(transform.position, aggroTarget.transform.position);
            if (distance > statSo.detectRange)
                aggroTarget = null;


            return;
        }

        var hits = Physics2D.OverlapCircleAll(transform.position, statSo.detectRange, targetLayerMask);
        if (hits.Length == 0)
        {
            aggroTarget = null; // 타겟 없음
            return;
        }
        UnitLogic targetUnit = null;
        switch (type)
        {
            case ESEARCH_TARGET_TYPE.NeareastEnemy:
                var minDistance = float.MaxValue;
                foreach (var hit in hits)
                {
                    target = hit.transform;
                    var aggro = target.GetComponent<UnitLogic>();
                    if (aggro && aggro.owner != this.owner && aggro.IsDie == false)
                    {
                        var distance = Vector3.Distance(transform.position, hit.transform.position);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            targetUnit = aggro;
                        }
                    }
                }
                 
                aggroTarget = targetUnit;
                return;

        }
    }
}
