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
            serializedObject.Update();
            
            var prefabIDProp = serializedObject.FindProperty("_dynamicPrefabID");
            var instanceIDProp = serializedObject.FindProperty("_instanceID");

            bool anyPrefabs = false;
            bool allIncluded = true;
            foreach (PersistedGameObject targetObj in targets.Cast<PersistedGameObject>()) {
                PrefabStage stage = PrefabStageUtility.GetPrefabStage(targetObj.gameObject);
                PrefabAssetType type = PrefabUtility.GetPrefabAssetType(targetObj);
                PrefabInstanceStatus status = PrefabUtility.GetPrefabInstanceStatus(targetObj);

                bool isPrefabAsset = (stage != null && targetObj.transform.parent == null) ||
                                     ((type == PrefabAssetType.Regular || type == PrefabAssetType.Variant) &&
                                      status == PrefabInstanceStatus.NotAPrefab);
                
                if (!isPrefabAsset) continue;
                
                GlobalObjectId id = GlobalObjectId.GetGlobalObjectIdSlow(targetObj);
                instanceIDProp.longValue = 0;
                prefabIDProp.intValue =
                    stage != null
                        ? Math.Abs(AssetDatabase.GUIDFromAssetPath(stage.assetPath).GetHashCode())
                        : Math.Abs(id.assetGUID.GetHashCode());
                anyPrefabs = true;
                allIncluded &= DynamicObjectManifest.Instance.GetEntry(targetObj.DynamicPrefabID) != null;
            }

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(instanceIDProp);
            EditorGUILayout.PropertyField(prefabIDProp);
            EditorGUI.EndDisabledGroup();
            
            serializedObject.ApplyModifiedProperties();

            if (anyPrefabs) {
                bool isIncluded = allIncluded;
                EditorGUI.BeginChangeCheck();
                bool newIsIncluded = EditorGUILayout.Toggle("Include in Manifest", isIncluded);
                if (EditorGUI.EndChangeCheck()) {
                    foreach (PersistedGameObject targetObj in targets.Cast<PersistedGameObject>()) {
                        PrefabStage stage = PrefabStageUtility.GetPrefabStage(targetObj.gameObject);
                        if (newIsIncluded) {
                            if (DynamicObjectManifest.Instance.GetEntry(targetObj.DynamicPrefabID) != null) continue;
                            string path = stage != null
                                ? CoreEditorUtility.GetResourcePath(stage.assetPath)
                                : CoreEditorUtility.GetResourcePath(targetObj);
                            DynamicObjectManifest.Instance.AddObject(targetObj.DynamicPrefabID, path);
                        } else {
                            DynamicObjectManifest.Instance.RemoveObject(targetObj.DynamicPrefabID);
                        }
                    }
                }
            }
            
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, ExcludeProps);
            serializedObject.ApplyModifiedProperties();
        }
    }
}