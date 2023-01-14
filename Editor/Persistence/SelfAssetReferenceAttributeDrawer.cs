using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Infohazard.Sequencing.Editor {
    [CustomPropertyDrawer(typeof(SelfAssetReferenceAttribute))]
    public class SelfAssetReferenceAttributeDrawer : PropertyDrawer {
        private PropertyDrawer _innerDrawer;
        private FieldInfo _fieldInfoFieldInfo;

        public SelfAssetReferenceAttributeDrawer() {
            Type assetReferenceDrawerType =
                Assembly.GetAssembly(typeof(UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject))
                        .GetType("UnityEditor.AddressableAssets.GUI.AssetReferenceDrawer");
            _innerDrawer =
                (PropertyDrawer) assetReferenceDrawerType
                                 .GetConstructor(Type.EmptyTypes)!.Invoke(Array.Empty<object>());
            _fieldInfoFieldInfo = assetReferenceDrawerType.GetField("m_FieldInfo", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            _fieldInfoFieldInfo!.SetValue(_innerDrawer, fieldInfo);
            return _innerDrawer.GetPropertyHeight(property, label);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            
            if (Application.isPlaying) {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.PropertyField(position, property, label);
                EditorGUI.EndDisabledGroup();
            } else if (property.serializedObject.targetObjects.Length > 1) {
                property.serializedObject.ApplyModifiedProperties();
                foreach (Object obj in property.serializedObject.targetObjects) {

                    SerializedObject tempObj = new SerializedObject(obj);
                    SerializedProperty tempProp = tempObj.FindProperty(property.propertyPath);
                    SetAddress(tempProp);
                    tempObj.ApplyModifiedProperties();
                }
                property.serializedObject.Update();
                EditorGUI.LabelField(position, label, new GUIContent("Cannot display address of multiple objects."));
            } else {
                SetAddress(property);
                EditorGUI.BeginDisabledGroup(true);
                _fieldInfoFieldInfo!.SetValue(_innerDrawer, fieldInfo);
                _innerDrawer.OnGUI(position, property, label);
                EditorGUI.EndDisabledGroup();
            }
        }
        
        private void SetAddress(SerializedProperty property) {
            Object obj = property.serializedObject.targetObject;
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out string guid, out long id);

            if (guid != null) {
                bool validGuid = false;
                foreach (AddressableAssetGroup group in AddressableAssetSettingsDefaultObject.Settings.groups) {
                    if (group.GetAssetEntry(guid) != null) {
                        validGuid = true;
                        break;
                    }
                }

                if (!validGuid) return;
                
                property.FindPropertyRelative("m_AssetGUID").stringValue = guid;
            }
        }
    }
}