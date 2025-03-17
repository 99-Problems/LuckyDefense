using Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CustomNode.State
{
    [CreateNodeMenuAttribute(menuName: "나/조건/공격 판단"), NodeTitle("나/조건/공격 판단")]
    public class CheckUnitAttackNode : BaseNode
    {
        [Title("사거리 체크")]
        [Input(ShowBackingValue.Never, connectionType = ConnectionType.Multiple)]
        public bool input;

        [LabelText("대상 발견")]
        [Output]
        public bool outputTrue = true;

        [LabelText("대상 없음")]
        [Output]
        public bool outputFalse = true;

        [LabelText("대상 멀음")]
        [Output]
        public bool outputFar = true;

        public override object GetValue(XNode.NodePort port)
        {
            return null;
        }

        public override void OnEnter()
        {
            base.OnEnter();

            var myLogic = GetUnitLogicInfo();
            if (myLogic.aggroTarget == null || myLogic.aggroTarget.IsDie)
            {
                MoveNextNode(this, GetPort("outputFalse"));
                return;
            }
            var distance = Vector3.Distance(myLogic.transform.position, myLogic.aggroTarget.transform.position);
            if (distance > myLogic.statSo.detectRange)
            {
                MoveNextNode(this, GetPort("outputFar"));
                return;
            }
            else
            {
                MoveNextNode(this, GetPort("outputTrue"));
                return;
            }

        }
    }
}