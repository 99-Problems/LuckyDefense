using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Data;
using UnityEngine;
using UnityEngine.XR;
using XNode;
using CustomNode.State;

namespace CustomNode.StateManager
{
    [Serializable]
    public class ActionInfo
    {
        public Define.EUNIT_STATE state;
        public StateGraph graph;
    }

    [CreateNodeMenu(menuName: "상태매니저"), NodeWidth(500)]
    public class StateManagerNode : BaseNode
    {
        [TableList(AlwaysExpanded = true)]
        public List<ActionInfo> actionInfo;

        private UnitLogic logicInfo;
        private Define.EUNIT_STATE prevState = Define.EUNIT_STATE.DESTROY;
        private ActionInfo currentAction;
        private ActionInfo nextAction;

        public override void OnEnter()
        {
            base.OnEnter();
            logicInfo = GetUnitLogicInfo();
        }

        public override object GetValue(XNode.NodePort port)
        {
            return null;
        }


        public override void FrameMove(float _deltaTime)
        {
            if (nextAction != null)
            {
                currentAction = nextAction;
                nextAction = null;
                //함수 실행 순서 유지 필요(재귀로 돌아가기떄문에 nextAction이 Restart에서 재설정되는 경우가 있음)
                if (currentAction.graph != null)
                    currentAction.graph.ClearData();
            }

            if (currentAction == null)
            {
//#if UNITY_EDITOR
//                if (logicInfo.debug)
//                    Debug.LogWarning($"action is null");
//#endif
                return;
            }

            if (currentAction.graph == null)
            {
                Debug.LogWarning($"{logicInfo.name} 공격 {currentAction.state} graph is null");
                logicInfo.SetState(Define.EUNIT_STATE.IDLE);
                return;
            }

            if (currentAction.graph.IsEmptyUpdateNode())
            {
                currentAction.graph.ReStart();
            }

            // if (!logicInfo.damageReaction)
                currentAction.graph.FrameMove(_deltaTime);
        }

        public void ChangeState(Define.EUNIT_STATE _state)
        {
            if (prevState == _state)
                return;
            prevState = _state;
            foreach (var mit in actionInfo)
            {
                if (_state == mit.state)
                {
                    nextAction = mit;
                    break;
                }
            }

            if (nextAction == null)
            {
                nextAction = new ActionInfo
                {
                    state = _state,
                };
            }
        }
    }
}