using Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using CustomNode;

namespace CustomNode.State
{
    /// <summary> Defines an example nodegraph that can be created as an asset in the Project window. </summary>
    [Serializable, CreateAssetMenu(fileName = "New State Graph", menuName = "Custom Node/상태 시퀀스")]
    public class StateGraph : XNode.NodeGraph, IGraphExtend
    {
        private int skillID;
        private UnitLogic logic;
        private Define.EUNIT_STATE state;
        private List<BaseNode> current = new List<BaseNode>();
        private List<BaseNode> add = new List<BaseNode>();
        private List<BaseNode> remove = new List<BaseNode>();
      
        protected override void OnDestroy()
        {
            logic = null;
            current.Clear();
            add.Clear();
            remove.Clear();
            base.OnDestroy();
        }

        public void Init(UnitLogic _logic, Define.EUNIT_STATE _state)
        {
            state = _state;
            logic = _logic;
            //skillID = _logic.GetSkillID(_state);
        }

        public void FrameMove(float _deltaTime)
        {
            foreach (var mit in current)
            {
                mit.FrameMove(_deltaTime);
            }

            foreach (var mit in add)
            {
                current.Add(mit);
            }

            foreach (var mit in remove)
            {
                current.Remove(mit);
            }

            remove.Clear();

            add.Clear();
        }

        public void AddUpdateEvent(BaseNode node)
        {
            add.Add(node);
        }

        public void RemoveUpdateEvent(BaseNode node)
        {
            remove.Add(node);
        }

        public UnitLogic GetUnitLogic()
        {
            return logic;
        }

        public Define.EUNIT_STATE GetState()
        {
            return state;
        }

        public int GetSkillID()
        {
            return skillID;
        }

        public void ReStart()
        {
            ClearData();
            var first = nodes.Find(x => x is EntryNode) as EntryNode;
            first?.OnEnter();
        }

        public void ClearData()
        {
            add.Clear();
            remove.Clear();
            current.Clear();
        }

        public bool IsEmptyUpdateNode()
        {
            return current.Count == 0;
        }

#if UNITY_EDITOR
        //public UnitSkillDetailInfo GetList(UnitSkillInfo unitSkillInfos)
        //{
        //    var ret = new UnitSkillDetailInfo();
        //    foreach (var mit in nodes)
        //    {
        //        if (mit is AddDebuffNode debuff)
        //        {
        //            if (debuff.debuffType != Define.EDEBUFF_TYPE.NONE && false == ret.listDebuff.Any(_ => _.type == debuff.debuffType))
        //            {
        //                var list = unitSkillInfos.args.Find(_ => _.effectArg == debuff.effectArg);
        //                ret.listDebuff.Add(new DebuffTypeList()
        //                {
        //                    type = debuff.debuffType,
        //                    duration = debuff.duration,
        //                    debuffValue = list,
        //                    ratio = 100,
        //                });
        //            }
        //        }

        //        if (mit is AddBuffNode buff)
        //        {
        //            if (buff.buffType != Define.EBUFF_TYPE.NONE && false == ret.listBuff.Any(_ => _.type == buff.buffType))
        //            {
        //                var list = unitSkillInfos.args.Find(_ => _.effectArg == buff.effectArg);
        //                ret.listBuff.Add(new BuffTypeList()
        //                {
        //                    type = buff.buffType,
        //                    duration = buff.time,
        //                    buffValue = list,
        //                    ratio = 100,
        //                });
        //            }
        //        }

        //        if (mit is AddCrowdControlNode cc)
        //        {
        //            if (cc.ccType != Define.CcType.NONE && false == ret.listCC.Any(_ => _.type == cc.ccType))
        //                ret.listCC.Add(new CCTypeList()
        //                {
        //                    type = cc.ccType,
        //                    duration = cc.duration,
        //                    buffValue = -1,
        //                    ratio = 100,
        //                });
        //        }

        //        if (mit is AddDotDamageNode dot)
        //        {
        //            if (dot.type != Define.EDOT_DAMAGE_TYPE.NONE && false == ret.listDot.Any(_ => _.type == dot.type))
        //            {
        //                var list = unitSkillInfos.args.Find(_ => _.effectArg == dot.arg);
        //                ret.listDot.Add(new DotDamageList()
        //                {
        //                    type = dot.type,
        //                    duration = dot.duration,
        //                    dotValue = list,
        //                    ratio = 100,
        //                });
        //            }
                        
        //        }

        //        if (mit is PassiveEffectNode passive)
        //        {
        //            if (false == ret.listPassiveEffect.Any(_ => _.type == passive.effectType))
        //                ret.listPassiveEffect.Add(new PassiveTypeList()
        //                {
        //                    type = passive.effectType,
        //                    duration = -1,
        //                    buffValue = passive.value,
        //                    ratio = -1,
        //                });
        //        }

        //        if (mit is GetUtilityInfoNode util)
        //        {
        //            foreach (var vit in util.listBuff)
        //            {
        //                if (vit.buffType != Define.EBUFF_TYPE.NONE && false == ret.listBuff.Any(_ => _.type == vit.buffType))
        //                {
        //                    var list = unitSkillInfos.args.Find(_ => _.effectArg == vit.buffArg);
        //                    ret.listBuff.Add(new BuffTypeList()
        //                    {
        //                        type = vit.buffType,
        //                        duration = util.buffTime,
        //                        buffValue = list,
        //                        ratio = vit.ratio,
        //                    });
        //                }
        //            }

        //            foreach (var vit in util.listDebuff)
        //            {
        //                if (vit.debuffType != Define.EDEBUFF_TYPE.NONE && false == ret.listDebuff.Any(_ => _.type == vit.debuffType))
        //                {
        //                    var list = unitSkillInfos.args.Find(_ => _.effectArg == vit.debuffArg);
        //                    ret.listDebuff.Add(new DebuffTypeList()
        //                    {
        //                        type = vit.debuffType,
        //                        duration = util.debuffTime,
        //                        debuffValue = list,
        //                        ratio = vit.ratio,
        //                    });
        //                }
        //            }

        //            foreach (var vit in util.listUnitEffect)
        //            {
        //                if (false == ret.listUnitEffect.Any(_ => _.type == vit.effectType))
        //                {
        //                    if (vit.effectType == Define.EUNIT_EFFECT_TYPE.DOT_HEAL || vit.effectType == Define.EUNIT_EFFECT_TYPE.SHIELD)
        //                    {
        //                        ret.listUnitEffect.Add(new UnitEffectList()
        //                        {
        //                            type = vit.effectType,
        //                            duration = vit.value1,
        //                            buffValue = vit.value2,
        //                            ratio = vit.ratio,
        //                        });
        //                    }
        //                    else
        //                    {
        //                        ret.listUnitEffect.Add(new UnitEffectList()
        //                        {
        //                            type = vit.effectType,
        //                            duration = -1,
        //                            buffValue = vit.value1,
        //                            ratio = vit.ratio,
        //                        });
        //                    }
        //                }
        //            }

        //            if (util.dot.dotType != Define.EDOT_DAMAGE_TYPE.NONE && false == ret.listDot.Any(_ => _.type == util.dot.dotType))
        //            {
        //                var list = unitSkillInfos.args.Find(_ => _.effectArg == util.dot.arg);
        //                ret.listDot.Add(new DotDamageList()
        //                {
        //                    type = util.dot.dotType,
        //                    duration = util.dot.duration,
        //                    dotValue = list,
        //                    ratio = util.dot.ratio,
        //                });
        //            }

        //            if (util.ccType.ccType != Define.CcType.NONE && false == ret.listCC.Any(_ => _.type == util.ccType.ccType))
        //                ret.listCC.Add(
        //                    new CCTypeList()
        //                    {
        //                        type = util.ccType.ccType,
        //                        duration = util.ccType.duration,
        //                        buffValue = -1,
        //                        ratio = util.ccType.ratio,
        //                    });
        //        }
        //    }

        //    return ret;
        //}
#endif
    }
}