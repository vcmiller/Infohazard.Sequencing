// The MIT License (MIT)
// 
// Copyright (c) 2022-present Vincent Miller
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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