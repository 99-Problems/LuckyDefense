using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNodeEditor;
using static XNodeEditor.NodeGraphEditor;

namespace CustomNode.StateManager {
	[CustomNodeGraphEditor(typeof(StateManagerGraph))]
	public class StateManagerGraphEditor : NodeGraphEditor {

		/// <summary> 
		/// Overriding GetNodeMenuName lets you control if and how nodes are categorized.
	    /// In this example we are sorting out all node types that are not in the XNode.Examples namespace.
		/// </summary>
		public override string GetNodeMenuName(System.Type type) {
			if (type.Namespace == "CustomNode.StateManager") {
				return base.GetNodeMenuName(type).Replace("CustomNode/StateManager", "");
			} else return null;
		}
	}
}