using Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CustomNode.State
{
    [CreateNodeMenuAttribute(menuName: "공통/특정 대상에 뭔가를 한다", 1), NodeTitle("공통/특정 대상에 뭔가를 한다")]
    public class ExecuteAttackTargetNode : BaseNode
    {
        [Input(ShowBackingValue.Never, connectionType = ConnectionType.Multiple)]
        public bool input;

        [Input(typeConstraint = TypeConstraint.Strict, backingValue = ShowBackingValue.Never)]
        public UnitLogic targetUnit;

        [LabelText("스킬 ID")]
        public int skillID = -1;

        [LabelText("공간 사이즈")]
        public Vector3 size;


        [LabelText("지연 대기 시간")]
        public float delayTime = 0;

        [LabelText("반복 시간")]
        public float tick = 1;

        [LabelText("재생 시간")]
        public float duration = 0.1f;

        [LabelText("커스텀 위치")]
        public bool isCustomPosition;

        [LabelText("공간 위치")]
        [ShowIf("@isCustomPosition != true")]
        public Vector3 offset;

        [LabelText("공격 리치 조정")]
        public float length = 1f;

        [LabelText("투사체")]
        public bool isMove;

        [ShowIf("@isMove")]
        public float projectileSpeed;

        [Output]
        public bool output = true;

        public override void OnEnter()
        {
            base.OnEnter();
            var unitLogic = GetUnitLogicInfo();
            if (unitLogic == null)
            {
                MoveNextNode(this, GetPort("output"));
                return;
            }
            var pos = offset;
            
            
            targetUnit = unitLogic.aggroTarget;
            pos += unitLogic.transform.position;
            //if(targetUnit != null)
            //{
            //    pos += targetUnit.transform.position;
            //}

            var skillInfo = Managers.Data.GetUnitSkillInfoScript(_ => _.skillID == skillID);
            if(skillInfo == null)
            {
                Debug.LogError("스킬정보 없음");
                MoveNextNode(this, GetPort("output"));
                return;
            }

            var direction = Vector3.zero;
            if (targetUnit != null)
            {
                direction = (targetUnit.transform.position - unitLogic.transform.position).normalized;
                pos += direction *length;
            }
            

            unitLogic.AddDamageArea(skillID, pos, -1, size, tick, delayTime, duration, 
                                unitLogic.stat.atk * skillInfo.effectArg, skillInfo.damageType,
                                isMove, direction, isMove ? projectileSpeed : 0);
            //Debug.ColorLog($"{unitLogic.GetOwner.name}'s {unitLogic.name} attacked");
            MoveNextNode(this, GetPort("output"));
        }

        public override object GetValue(XNode.NodePort port)
        {
            return null;
        }
    }
}