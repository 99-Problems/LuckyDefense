using System;
using Data;
using Sirenix.OdinInspector;
using UnityEngine.Serialization;

namespace CustomNode.State
{
    [CreateNodeMenuAttribute(menuName: "나/상태 변경"), NodeTitle("나/상태 변경")]
    public class SetStateNode : BaseNode
    {
        [Input(ShowBackingValue.Never, connectionType = ConnectionType.Multiple)]
        public bool input;

        [LabelText("변경 상태")]
        public Define.EUNIT_STATE state;


        public override object GetValue(XNode.NodePort port)
        {
            return null;
        }

        public override void OnEnter()
        {
            base.OnEnter();

            var logicInfo = GetUnitLogicInfo();

            logicInfo.SetState(state);
        }
    }
}