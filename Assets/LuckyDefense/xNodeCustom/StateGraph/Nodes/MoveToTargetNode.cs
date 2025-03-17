//using Data;
//using Sirenix.OdinInspector;
//using UnityEngine;

//namespace CustomNode.State
//{
//    [CreateNodeMenuAttribute(menuName: "나/행동/타겟한테 이동"), NodeTitle("나/행동/타겟한테 이동")]
//    public class MoveToTarget : BaseNode
//    {
//        [Title("적에게 이동")]
//        [Input(ShowBackingValue.Never, connectionType = ConnectionType.Multiple)]
//        public bool input;

//        [LabelText("공격범위 들어옴")]
//        [Output]
//        public bool outputTrue = true;

//        [LabelText("타겟 사라짐")]
//        [Output]
//        public bool outputFalse = true;

//        private UnitLogic myLogic;

//        public override object GetValue(XNode.NodePort port)
//        {
//            return null;
//        }

//        public override void OnEnter()
//        {
//            base.OnEnter();

//            myLogic = GetUnitLogicInfo();
//            if (myLogic.aggroTarget == null || myLogic.aggroTarget.IsDie)
//            {
//                MoveNextNode(this, GetPort("outputFalse"));
//                return;
//            }
//            myLogic.SetState(Define.EUNIT_STATE.RUN);

            

//        }

//        public override void FrameMove(float _deltaTime)
//        {
//            base.FrameMove(_deltaTime);

//            if (myLogic.aggroTarget == null || myLogic.aggroTarget.IsDie)
//            {
//                MoveNextNode(this, GetPort("outputFalse"));
//                return;
//            }

//            var distance = Vector3.Distance(myLogic.transform.position, myLogic.aggroTarget.transform.position);
//            if (distance <= myLogic.statSo.attackRange)
//            {
//                MoveNextNode(this, GetPort("outputTrue"));
//                return;
//            }
//        }
//    }
//}