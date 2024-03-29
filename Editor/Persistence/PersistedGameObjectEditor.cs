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

using System;
using System.Linq;
using Infohazard.Core.Editor;
using Infohazard.Sequencing;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;

namespace SBR.Editor.Persistence {
    [CustomEditor(typeof(PersistedGameObject))]
    [CanEditMultipleObjects]
    public class PersistedGameObjectEditor : UnityEditor.Editor {
        private static readonly string[] ExcludeProps = {"m_Script", "_dynamicPrefabID", "_instanceID"};
        
        public override void OnInspectorGUI() {
            var instanceIDProp = serializedObject.FindProperty("_instanceID");

            foreach (PersistedGameObject targetObj in targets.Cast<PersistedGameObject>()) {
                PrefabStage stage = PrefabStageUtility.GetPrefabStage(targetObj.gameObject);
                PrefabAssetType type = PrefabUtility.GetPrefabAssetType(targetObj);
                PrefabInstanceStatus status = PrefabUtility.GetPrefabInstanceStatus(targetObj);

                bool isPrefabAsset = (stage != null && targetObj.transform.parent == null) ||
                                     ((type == PrefabAssetType.Regular || type == PrefabAssetType.Variant) &&
                                      status == PrefabInstanceStatus.NotAPrefab);
                
                if (!isPrefabAsset) continue;
                if (targetObj.InstanceID != 0) {
                    Undo.RecordObject(targetObj, "Clear Instance ID");
                    targetObj.SetInstanceIDEditMode(0);
                }
            }
            
            serializedObject.Update();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(instanceIDProp);
            EditorGUI.EndDisabledGroup();
            
            serializedObject.ApplyModifiedProperties();

            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, ExcludeProps);
            serializedObject.ApplyModifiedProperties();
        }
    }
}