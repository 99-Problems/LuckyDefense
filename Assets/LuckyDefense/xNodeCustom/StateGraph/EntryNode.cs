namespace CustomNode.State
{
    [CreateNodeMenuAttribute("시작", 10), DisallowMultipleNodesAttribute, NodeTitle("시작")]
    public class EntryNode : BaseNode
    {
        [Output] public bool output = true;

        public override object GetValue(XNode.NodePort port)
        {
            return output;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            MoveNextNode(this, GetPort("output"));
        }
    }
}