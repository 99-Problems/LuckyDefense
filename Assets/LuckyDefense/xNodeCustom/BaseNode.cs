using Data;
using CustomNode;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

public interface IGraphExtend
{
    void FrameMove(float _deltaTime);
    void AddUpdateEvent(BaseNode _node);
    void RemoveUpdateEvent(BaseNode _node);

    UnitLogic GetUnitLogic();

    Define.EUNIT_STATE GetState();
    int GetSkillID();
}

public abstract class BaseNode : Node
{
    public override void OnCreateConnection(NodePort _from, NodePort _to)
    {
    }

    protected UnitLogic GetUnitLogicInfo()
    {
        if(graph is IGraphExtend math)
            return math.GetUnitLogic();
        return null;
    }
    protected Define.EUNIT_STATE GetStateType()
    {
        if(graph is IGraphExtend math)
            return math.GetState();
        return Define.EUNIT_STATE.IDLE;
    }
    
    protected int GetSkillID()
    {
        if(graph is IGraphExtend math)
            return math.GetSkillID();
        return 0;
    }
    
    public override void OnEnter()
    {
        if(graph is IGraphExtend math)
        {
            math.AddUpdateEvent(this);
        }

        
        // Debug.Log("Enter " + this.name);
    }

    public override void OnExit()
    {
        if(graph is IGraphExtend math)
        {
            math.RemoveUpdateEvent(this);
        }
        //Debug.Log("Exit " + this.name);
    }

    public override void FrameMove(float _deltaTime)
    {
    }
}