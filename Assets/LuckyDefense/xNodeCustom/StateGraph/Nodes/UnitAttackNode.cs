using Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CustomNode.State
{
    [CreateNodeMenuAttribute(menuName: "��/�ൿ/����"), NodeTitle("��/����/����")]
    public class UnitAttackNode : BaseNode
    {
        [Title("���� Ÿ�Կ� ���� �ൿ")]
        [Input(ShowBackingValue.Never, connectionType = ConnectionType.Multiple)]
        public bool input;

        [LabelText("���� �ִ�")]
        [InspectorName("�̸�")]
        public string aniName = "Attack";

        [LabelText("����")]
        [Output]
        public bool outputTrue = true;

        [LabelText("���� ����")]
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