using System;
using UnityEngine;

namespace XNode
{
    /// <summary> Base node for the LogicToy system </summary>
    public  abstract partial class Node : ScriptableObject
    {
        public Action onStateChange;

        public void MoveNextNode(Node parent, NodePort output)
        {
            parent.OnExit();

            // Loop through port connections
            int connectionCount = output.ConnectionCount;
            for(int i = 0; i < connectionCount; i++)
             {
                 NodePort connectedPort = output.GetConnection(i);
                 if (connectedPort == null)
                     continue;
                 // Get connected ports logic node
                 Node connectedNode = connectedPort.node;

                 // Trigger it
                 if(connectedNode != null) connectedNode.OnEnter();
                 if(connectedNode != null) connectedNode.FrameMove(0);
             }

            if(onStateChange != null) onStateChange();
        }

        public abstract void OnEnter();

        public abstract void OnExit();

        public abstract void FrameMove(float _deltaTime);
    }
}