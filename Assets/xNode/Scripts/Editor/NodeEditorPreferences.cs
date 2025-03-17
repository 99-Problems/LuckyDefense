using System;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace XNodeEditor {
    public enum NoodlePath { Curvy, Straight, Angled, ShaderLab }
    public enum NoodleStroke { Full, Dashed }

    public static class NodeEditorPreferences {

        /// <summary> The last editor we checked. This should be the one we modify </summary>
        private static XNodeEditor.NodeGraphEditor lastEditor;
        /// <summary> The last key we checked. This should be the one we modify </summary>
        private static string lastKey = "xNode.Settings";

        private static Dictionary<Type, Color> typeColors = new Dictionary<Type, Color>();
        private static Dictionary<string, NodeEditorSettings> settings = new Dictionary<string, NodeEditorSettings>();

        /// <summary> Get settings of current active editor </summary>
        public static NodeEditorSettings GetSettings() {
            if (XNodeEditor.NodeEditorWindow.current == null) return new NodeEditorSettings();

            if (lastEditor != XNodeEditor.NodeEditorWindow.current.graphEditor) {
                object[] attribs = XNodeEditor.NodeEditorWindow.current.graphEditor.GetType().GetCustomAttributes(typeof(XNodeEditor.NodeGraphEditor.CustomNodeGraphEditorAttribute), true);
                if (attribs.Length == 1) {
                    XNodeEditor.NodeGraphEditor.CustomNodeGraphEditorAttribute attrib = attribs[0] as XNodeEditor.NodeGraphEditor.CustomNodeGraphEditorAttribute;
                    lastEditor = XNodeEditor.NodeEditorWindow.current.graphEditor;
                    lastKey = attrib.editorPrefsKey;
                } else return null;
            }
            if (!settings.ContainsKey(lastKey)) VerifyLoaded();
            return settings[lastKey];
        }

#if UNITY_2019_1_OR_NEWER
        [SettingsProvider]
        public static SettingsProvider CreateXNodeSettingsProvider() {
            SettingsProvider provider = new SettingsProvider("Preferences/Node Editor", SettingsScope.User) {
                guiHandler = (searchContext) => { XNodeEditor.NodeEditorPreferences.PreferencesGUI(); },
                keywords = new HashSet<string>(new [] { "xNode", "node", "editor", "graph", "connections", "noodles", "ports" })
            };
            return provider;
        }
#endif

#if !UNITY_2019_1_OR_NEWER
        [PreferenceItem("Node Editor")]
#endif
        private static void PreferencesGUI() {
            VerifyLoaded();
            NodeEditorSettings nodeEditorSettings = NodeEditorPreferences.settings[lastKey];

            if (GUILayout.Button(new GUIContent("Documentation", "https://github.com/Siccity/xNode/wiki"), GUILayout.Width(100))) Application.OpenURL("https://github.com/Siccity/xNode/wiki");
            EditorGUILayout.Space();

            NodeSettingsGUI(lastKey, nodeEditorSettings);
            GridSettingsGUI(lastKey, nodeEditorSettings);
            SystemSettingsGUI(lastKey, nodeEditorSettings);
            TypeColorsGUI(lastKey, nodeEditorSettings);
            if (GUILayout.Button(new GUIContent("Set Default", "Reset all values to default"), GUILayout.Width(120))) {
                ResetPrefs();
            }
        }

        private static void GridSettingsGUI(string key, NodeEditorSettings _nodeEditorSettings) {
            //Label
            EditorGUILayout.LabelField("Grid", EditorStyles.boldLabel);
            _nodeEditorSettings.gridSnap = EditorGUILayout.Toggle(new GUIContent("Snap", "Hold CTRL in editor to invert"), _nodeEditorSettings.gridSnap);
            _nodeEditorSettings.zoomToMouse = EditorGUILayout.Toggle(new GUIContent("Zoom to Mouse", "Zooms towards mouse position"), _nodeEditorSettings.zoomToMouse);
            EditorGUILayout.LabelField("Zoom");
            EditorGUI.indentLevel++;
            _nodeEditorSettings.maxZoom = EditorGUILayout.FloatField(new GUIContent("Max", "Upper limit to zoom"), _nodeEditorSettings.maxZoom);
            _nodeEditorSettings.minZoom = EditorGUILayout.FloatField(new GUIContent("Min", "Lower limit to zoom"), _nodeEditorSettings.minZoom);
            EditorGUI.indentLevel--;
            _nodeEditorSettings.gridLineColor = EditorGUILayout.ColorField("Color", _nodeEditorSettings.gridLineColor);
            _nodeEditorSettings.gridBgColor = EditorGUILayout.ColorField(" ", _nodeEditorSettings.gridBgColor);
            if (GUI.changed) {
                SavePrefs(key, _nodeEditorSettings);

                NodeEditorWindow.RepaintAll();
            }
            EditorGUILayout.Space();
        }

        private static void SystemSettingsGUI(string key, NodeEditorSettings _nodeEditorSettings) {
            //Label
            EditorGUILayout.LabelField("System", EditorStyles.boldLabel);
            _nodeEditorSettings.autoSave = EditorGUILayout.Toggle(new GUIContent("Autosave", "Disable for better editor performance"), _nodeEditorSettings.autoSave);
            if (GUI.changed) SavePrefs(key, _nodeEditorSettings);
            EditorGUILayout.Space();
        }

        private static void NodeSettingsGUI(string key, NodeEditorSettings _nodeEditorSettings) {
            //Label
            EditorGUILayout.LabelField("Node", EditorStyles.boldLabel);
            _nodeEditorSettings.highlightColor = EditorGUILayout.ColorField("Selection", _nodeEditorSettings.highlightColor);
            _nodeEditorSettings.noodlePath = (NoodlePath) EditorGUILayout.EnumPopup("Noodle path", (Enum) _nodeEditorSettings.noodlePath);
            _nodeEditorSettings.noodleStroke = (NoodleStroke) EditorGUILayout.EnumPopup("Noodle stroke", (Enum) _nodeEditorSettings.noodleStroke);
            _nodeEditorSettings.portTooltips = EditorGUILayout.Toggle("Port Tooltips", _nodeEditorSettings.portTooltips);
            _nodeEditorSettings.dragToCreate = EditorGUILayout.Toggle(new GUIContent("Drag to Create", "Drag a port connection anywhere on the grid to create and connect a node"), _nodeEditorSettings.dragToCreate);
            if (GUI.changed) {
                SavePrefs(key, _nodeEditorSettings);
                NodeEditorWindow.RepaintAll();
            }
            EditorGUILayout.Space();
        }

        private static void TypeColorsGUI(string key, NodeEditorSettings _nodeEditorSettings) {
            //Label
            EditorGUILayout.LabelField("Types", EditorStyles.boldLabel);

            //Clone keys so we can enumerate the dictionary and make changes.
            var typeColorKeys = new List<Type>(typeColors.Keys);

            //Display type colors. Save them if they are edited by the user
            foreach (var type in typeColorKeys) {
                string typeColorKey = NodeEditorUtilities.PrettyName(type);
                Color col = typeColors[type];
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.BeginHorizontal();
                col = EditorGUILayout.ColorField(typeColorKey, col);
                EditorGUILayout.EndHorizontal();
                if (EditorGUI.EndChangeCheck()) {
                    typeColors[type] = col;
                    if (_nodeEditorSettings.typeColors.ContainsKey(typeColorKey)) _nodeEditorSettings.typeColors[typeColorKey] = col;
                    else _nodeEditorSettings.typeColors.Add(typeColorKey, col);
                    SavePrefs(key, _nodeEditorSettings);
                    NodeEditorWindow.RepaintAll();
                }
            }
        }

        /// <summary> Load prefs if they exist. Create if they don't </summary>
        private static NodeEditorSettings LoadPrefs() {
            // Create settings if it doesn't exist
            return NodeEditorSettings.Instance;
        }

        /// <summary> Delete all prefs </summary>
        public static void ResetPrefs() {
            if (EditorPrefs.HasKey(lastKey)) EditorPrefs.DeleteKey(lastKey);
            if (settings.ContainsKey(lastKey)) settings.Remove(lastKey);
            typeColors = new Dictionary<Type, Color>();
            VerifyLoaded();
            NodeEditorWindow.RepaintAll();
        }

        /// <summary> Save preferences in EditorPrefs </summary>
        private static void SavePrefs(string key, NodeEditorSettings _nodeEditorSettings) {
            EditorPrefs.SetString(key, JsonUtility.ToJson(_nodeEditorSettings));
        }

        /// <summary> Check if we have loaded settings for given key. If not, load them </summary>
        private static void VerifyLoaded() {
            if (!settings.ContainsKey(lastKey)) settings.Add(lastKey, LoadPrefs());
        }

        /// <summary> Return color based on type </summary>
        public static Color GetTypeColor(System.Type type) {
            VerifyLoaded();
            if (type == null) return Color.gray;
            Color col;
            if (!typeColors.TryGetValue(type, out col)) {
                string typeName = type.PrettyName();
                if (settings[lastKey].typeColors.ContainsKey(typeName)) typeColors.Add(type, settings[lastKey].typeColors[typeName]);
                else {
#if UNITY_5_4_OR_NEWER
                    UnityEngine.Random.State oldState = UnityEngine.Random.state;
                    UnityEngine.Random.InitState(typeName.GetHashCode());
#else
                    int oldSeed = UnityEngine.Random.seed;
                    UnityEngine.Random.seed = typeName.GetHashCode();
#endif
                    col = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
                    typeColors.Add(type, col);
#if UNITY_5_4_OR_NEWER
                    UnityEngine.Random.state = oldState;
#else
                    UnityEngine.Random.seed = oldSeed;
#endif
                }
            }
            return col;
        }
    }
}