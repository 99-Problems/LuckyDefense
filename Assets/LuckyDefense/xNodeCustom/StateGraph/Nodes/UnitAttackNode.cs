using Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CustomNode.State
{
    [CreateNodeMenuAttribute(menuName: "나/행동/공격"), NodeTitle("나/조건/공격")]
    public class UnitAttackNode : BaseNode
    {
        [Title("유닛 타입에 따라 행동")]
        [Input(ShowBackingValue.Never, connectionType = ConnectionType.Multiple)]
        public bool input;

        [LabelText("공격 애니")]
        [InspectorName("이름")]
        public string aniName = "Attack";

        [LabelText("공격")]
        [Output]
        public bool outputTrue = true;

        [LabelText("공격 실패")]
        [Output]
        public bool outputFalse = true;

        private UnitLogic myLogic;
        private float deltaTime;


        public override object GetValue(XNode.NodePort port)
        {
            return null;
        }

        public override void OnEnter()
        {
            base.OnEnter();

            myLogic = GetUnitLogicInfo();
            if (myLogic.aggroTarget == null || myLogic.aggroTarget.IsDie)
            {
                MoveNextNode(this, GetPort("outputFalse"));
                return;
            }
            deltaTime = myLogic.atkFrame;
        }

        public override void FrameMove(float _deltaTime)
        {
            base.FrameMove(_deltaTime);
            if (myLogic.aggroTarget == null || myLogic.aggroTarget.IsDie)
            {
                myLogic.atkFrame = deltaTime;
                MoveNextNode(this, GetPort("outputFalse"));
                return;
            }
            if (deltaTime > 1f)
            {
                myLogic.anim.Play(aniName, 0, myLogic.atkFrame);
                myLogic.atkFrame = 0;
                //Debug.ColorLog($"{myLogic.GetOwner.name}'s {myLogic.name} anim");
                deltaTime = 0;

                MoveNextNode(this, GetPort("outputTrue"));
                //Debug.ColorLog($"{myLogic.GetOwner.name}'s {myLogic.name} anim throw next node");
                return;
            }

            deltaTime += _deltaTime;
        }
    }
}