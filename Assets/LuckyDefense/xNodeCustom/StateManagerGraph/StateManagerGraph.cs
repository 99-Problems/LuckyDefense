using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using CustomNode.State;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using XNode;

namespace CustomNode.StateManager
{
    /// <summary> Defines an example nodegraph that can be created as an asset in the Project window. </summary>
    [Serializable, CreateAssetMenu(fileName = "New State Manager Graph", menuName = "Custom Node/상태 관리자")]
    public class StateManagerGraph : XNode.NodeGraph, IGraphExtend
    {
        private UnitLogic logic;
        private List<BaseNode> current = new List<BaseNode>();

        private StateManagerNode head = null;

        public void Init(UnitLogic _logic)
        {
            current.Clear();
            logic = _logic;
            head = nodes.Find(x => x is StateManagerNode) as StateManagerNode;

            foreach (var mit in head.actionInfo)
            {
                var origin = mit.graph;
                if (null == origin)
                {
                    continue;
                }

                var clone = origin.Copy() as StateGraph;
                clone.Init(_logic, mit.state);
                mit.graph = clone;
            }

            head.OnEnter();
        }

        protected override void OnDestroy()
        {
            foreach (var mit in head.actionInfo)
            {
                if (null == mit.graph)
                    continue;
                Destroy(mit.graph);
                mit.graph = null;
            }

            current.Clear();
            logic = null;
            head = null;
            base.OnDestroy();
        }

        public void FrameMove(float _deltaTime)
        {
            foreach (var mit in current)
            {
                mit.FrameMove(_deltaTime);
            }
        }

        public void AddUpdateEvent(BaseNode node)
        {
            current.Add(node);
        }

        public void RemoveUpdateEvent(BaseNode node)
        {
            current.Remove(node);
        }

        public UnitLogic GetUnitLogic()
        {
            return logic;
        }

        public Define.EUNIT_STATE GetState()
        {
            return logic.state;
        }

        public int GetSkillID()
        {
            return 0;
        }

        public void ChangeState(Define.EUNIT_STATE _state)
        {
            head.ChangeState(_state);
        }

        public void SetFindMissingStateFillUp(StateManagerGraph _compareGraph, UnitLogic _unitLogic)
        {
            foreach (var mit in _compareGraph.head.actionInfo)
            {
                if (mit.graph == null)
                    continue;
                if (false == head.actionInfo.Any(_ => _.state == mit.state))
                {
                    var clone = mit.graph.Copy() as StateGraph;
                    clone.Init(_unitLogic, mit.state);
                    head.actionInfo.Add(new ActionInfo
                    {
                        state = mit.state,
                        graph = clone,
                    });
                }
            }
        }

        public void ReplaceStateGraph(Define.EUNIT_STATE _state, StateGraph _sequence)
        {
            var clone = _sequence.Copy() as StateGraph;
            clone.Init(logic, _state);
            var targetAction = head.actionInfo.Find(_ => _.state == _state);
            if (targetAction == null)
            {
                head.actionInfo.Add(new ActionInfo
                {
                    state = _state, graph = clone,
                });
            }
            else
            {
                if (targetAction.graph != null)
                    Destroy(targetAction.graph);
                targetAction.graph = clone;
            }
        }

        public StateGraph GetGraph(Define.EUNIT_STATE _state)
        {
            var _head = nodes.Find(x => x is StateManagerNode) as StateManagerNode;
            var targetNode = _head.actionInfo.Find(_ => _.state == _state);
            if (targetNode == null)
                return null;
            return targetNode.graph;
        }
    }
}