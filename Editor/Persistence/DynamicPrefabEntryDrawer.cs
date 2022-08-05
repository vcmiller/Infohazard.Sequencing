using Infohazard.Core.Editor;
using Infohazard.Sequencing;
using UnityEditor;

using UnityEngine;

using Object = UnityEngine.Object;

namespace SBR.Editor.Persistence {
    [CustomPropertyDrawer(typeof(DynamicObjectManifest.DynamicPrefabEntry))]
    public class DynamicPrefabEntryDrawer : PropertyDrawer {
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            label = EditorGUI.BeginProperty(position, label, property);
            
            SerializedProperty resourcePathProp = property.FindPropertyRelative("_resourcePath");
            SerializedProperty prefabIDProp = property.FindPropertyRelative("_prefabID");

            string curPath = resourcePathProp.stringValue;
            Object curObject = string.IsNullOrEmpty(curPath) ? null : Resources.Load<PersistedGameObject>(curPath);
            
            EditorGUI.BeginChangeCheck();
            PersistedGameObject newObject = (PersistedGameObject)EditorGUI.ObjectField(position, label, curObject, typeof(PersistedGameObject), false);
            if (EditorGUI.EndChangeCheck()) {
                if (newObject != null) {
                    string path = CoreEditorUtility.GetResourcePath(newObject);
                    if (!string.IsNullOrEmpty(path)) {
                        resourcePathProp.stringValue = path;
                        prefabIDProp.intValue = newObject.DynamicPrefabID;
                    }
                } else {
                    resourcePathProp.stringValue = null;
                    prefabIDProp.intValue = 0;
                }
            }
                
            EditorGUI.EndProperty();
        }
    }

}