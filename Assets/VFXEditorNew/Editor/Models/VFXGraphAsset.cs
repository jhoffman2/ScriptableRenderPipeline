using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;
using UnityEngine.Graphing;

namespace UnityEditor.VFX
{
    public class VFXGraphAssetFactory
    {
        [MenuItem("Assets/Create/VFXGraphAsset", priority = 301)]
        private static void MenuCreateVFXGraphAsset()
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<DoCreateVFXGraphAsset>(), "New VFXGraph.asset", null, null);
        }

        internal static VFXGraphAsset CreateVFXGraphAssetAtPath(string path)
        {
            VFXGraphAsset asset = ScriptableObject.CreateInstance<VFXGraphAsset>();
            asset.name = Path.GetFileName(path);
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }
    }

    internal class DoCreateVFXGraphAsset : EndNameEditAction
    {
        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            VFXGraphAsset asset = VFXGraphAssetFactory.CreateVFXGraphAssetAtPath(pathName);
            ProjectWindowUtil.ShowCreatedAsset(asset);
        }
    }

    [Serializable]
    class VFXGraphAsset : ScriptableObject, ISerializationCallbackReceiver
    {
        public VFXGraph root { get { return m_Root; } }

        [NonSerialized]
        private VFXGraph m_Root;

        [SerializeField]
        private SerializationHelper.JSONSerializedElement m_SerializedRoot;

        public virtual void OnBeforeSerialize()
        {
            m_SerializedRoot = SerializationHelper.Serialize<VFXGraph>(m_Root);
        }

        public virtual void OnAfterDeserialize()
        {
            m_Root = SerializationHelper.Deserialize<VFXGraph>(m_SerializedRoot, null);
            m_Root.owner = this;
        }

        void OnEnable()
        {
            if (m_Root == null)
            {
                m_Root = new VFXGraph();
                m_Root.owner = this;
            }
        }
    }

    class VFXGraph : VFXModel
    {
        public ScriptableObject owner
        {
            get { return m_Owner; }
            set { m_Owner = value; }
        }

        public override bool AcceptChild(VFXModel model, int index = -1)
        {
            return true; // Can hold any model
        }

        protected override void OnInvalidate(InvalidationCause cause)
        {
            if (m_Owner != null)
            {
                EditorUtility.SetDirty(m_Owner);
                Debug.Log("Invalidate VFXAsset " + m_Owner + " - Cause: " + cause);
            }
        }

        private ScriptableObject m_Owner;
    }
}
