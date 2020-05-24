using System;
using System.Collections.Generic;
using UnityEngine;
namespace PlayfulSoftware.EditModeTests
{
#if UNITY_EDITOR
    using UnityEditor;

    [CustomEditor(typeof(GameObjectCreator))]
    class GameObjectCreatorEditor : Editor
    {
        internal GameObjectCreator targetObj => (GameObjectCreator) target;
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (GUILayout.Button("Create Non-Editable Child Cube"))
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "Child Object";
                go.hideFlags = HideFlags.NotEditable;
                go.transform.parent = targetObj.transform;
                targetObj.m_EditModeGameObjects.Add(go);
            }

            if (GUILayout.Button("Create Non-Inspectable Child Cube"))
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "Child Object";
                go.hideFlags = HideFlags.NotEditable | HideFlags.HideInInspector;
                go.transform.parent = targetObj.transform;
                targetObj.m_EditModeGameObjects.Add(go);
            }

            if (GUILayout.Button("Create HideAndDontSave Child Cube"))
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "Child Object";
                go.hideFlags = HideFlags.HideAndDontSave;
                go.transform.parent = targetObj.transform;
                targetObj.m_EditModeGameObjects.Add(go);
            }

            if (GUILayout.Button("Destroy Child Objects"))
            {
                foreach (var obj in targetObj.m_EditModeGameObjects)
                {
                    DestroyImmediate(obj);
                }
                targetObj.m_EditModeGameObjects.Clear();
            }
        }
    }
#endif // UNITY_EDITOR
    sealed class GameObjectCreator : MonoBehaviour
    {
        internal List<GameObject> m_EditModeGameObjects = new List<GameObject>();

        void OnDisable()
        {
            if (!Application.IsPlaying(gameObject))
            {
                foreach (var child in m_EditModeGameObjects)
                {
                    DestroyImmediate(child);
                }
                m_EditModeGameObjects.Clear();
            }
        }
    }
}