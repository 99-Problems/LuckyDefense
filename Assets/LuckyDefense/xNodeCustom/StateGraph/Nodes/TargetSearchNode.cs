using Data;
using Sirenix.OdinInspector;

namespace CustomNode.State
{
    [CreateNodeMenuAttribute(menuName: "나/조건/탐색 대상 선정"), NodeTitle("나/조건/탐색 대상 선정")]
    public class TargetSearchNode : BaseNode
    {
        [Title("결과 성공 시 대상으로 저장")]
        [Input(ShowBackingValue.Never, connectionType = ConnectionType.Multiple)]
        public bool input;

        [LabelText("탐색 대상")]
        public Define.ESEARCH_TARGET_TYPE type;

        [LabelText("대상 발견")]
        [Output]
        public bool outputTrue = true;

        [LabelText("탐색한 대상")]
        [Output]
        public UnitLogic outputTarget;

        [LabelText("탐색 실패")]
        [Output]
        public bool outputFalse = true;


        public override object GetValue(XNode.NodePort port)
        {
            if (port.fieldName == "outputTarget")
                return outputTarget;

            return null;
        }

        public override void OnEnter()
        {
            base.OnEnter();

            var myLogic = GetUnitLogicInfo();

            myLogic.SearchTarget(type);

            if (myLogic.aggroTarget == null)
            {
                MoveNextNode(this, GetPort("outputFalse"));
                return;
            }
            else
            {
                outputTarget = myLogic.aggroTarget;
                MoveNextNode(this, GetPort("outputTrue"));
                return;
            }
        }
    }
}