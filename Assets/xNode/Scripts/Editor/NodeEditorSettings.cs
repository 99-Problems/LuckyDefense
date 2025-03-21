﻿using System;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace XNodeEditor
{
    [System.Serializable]
    public class NodeEditorSettings : ScriptableObject, ISerializationCallbackReceiver
    {
        private const string settingFilePath = "Assets/LuckyDefense/Editor/XNodeSettings.asset";
        private static NodeEditorSettings instance;

        public static NodeEditorSettings Instance
        {
            get
            {
#if UNITY_EDITOR
                if (instance != null)
                    return instance;
                instance = AssetDatabase.LoadAssetAtPath<NodeEditorSettings>(settingFilePath);
                if (instance == null)
                {
                    instance = CreateInstance<NodeEditorSettings>();
                    AssetDatabase.CreateAsset(instance, settingFilePath);
                }
#endif
                return instance;
            }
        }

        [SerializeField]
        private Color32 _gridLineColor = new Color(0.45f, 0.45f, 0.45f);

        public Color32 gridLineColor
        {
            get { return _gridLineColor; }
            set
            {
                _gridLineColor = value;
                _gridTexture = null;
                _crossTexture = null;
            }
        }

        [SerializeField]
        private Color32 _gridBgColor = new Color(0.18f, 0.18f, 0.18f);

        public Color32 gridBgColor
        {
            get { return _gridBgColor; }
            set
            {
                _gridBgColor = value;
                _gridTexture = null;
            }
        }

        [Obsolete("Use maxZoom instead")]
        public float zoomOutLimit
        {
            get { return maxZoom; }
            set { maxZoom = value; }
        }

        [UnityEngine.Serialization.FormerlySerializedAs("zoomOutLimit")]
        public float maxZoom = 5f;

        public float minZoom = 1f;
        public Color32 highlightColor = Color.red;
        public bool gridSnap = true;
        public bool autoSave = true;
        public bool dragToCreate = true;
        public bool zoomToMouse = true;
        public bool portTooltips = true;

        [SerializeField]
        private string typeColorsData = "";

        [NonSerialized]
        public Dictionary<string, Color> typeColors = new Dictionary<string, Color>();

        [FormerlySerializedAs("noodleType")]
        public NoodlePath noodlePath = NoodlePath.Angled;

        public NoodleStroke noodleStroke = NoodleStroke.Full;

        private Texture2D _gridTexture;

        public Texture2D gridTexture
        {
            get
            {
                if (_gridTexture == null) _gridTexture = NodeEditorResources.GenerateGridTexture(gridLineColor, gridBgColor);
                return _gridTexture;
            }
        }

        private Texture2D _crossTexture;

        public Texture2D crossTexture
        {
            get
            {
                if (_crossTexture == null) _crossTexture = NodeEditorResources.GenerateCrossTexture(gridLineColor);
                return _crossTexture;
            }
        }

        public void OnAfterDeserialize()
        {
            // Deserialize typeColorsData
            typeColors = new Dictionary<string, Color>();
            string[] data = typeColorsData.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < data.Length; i += 2)
            {
                Color col;
                if (ColorUtility.TryParseHtmlString("#" + data[i + 1], out col))
                {
                    typeColors.Add(data[i], col);
                }
            }
        }

        public void OnBeforeSerialize()
        {
            // Serialize typeColors
            typeColorsData = "";
            foreach (var item in typeColors)
            {
                typeColorsData += item.Key + "," + ColorUtility.ToHtmlStringRGB(item.Value) + ",";
            }
        }
    }
}