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

using Infohazard.Sequencing;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Infohazard.Sequencing.Editor {
    [InitializeOnLoad]
    public static class PlayModeStateChangedSceneLoader {
        static PlayModeStateChangedSceneLoader() {
            if (!SequencingEditorPrefs.AutoLoadScene0InEditor) return;
            EditorApplication.playModeStateChanged -= EditorApplication_PlayModeStateChanged;
            EditorApplication.playModeStateChanged += EditorApplication_PlayModeStateChanged;
        }

        private static void EditorApplication_PlayModeStateChanged(PlayModeStateChange mode) {
            if (!SequencingEditorPrefs.AutoLoadScene0InEditor) return;
            if (mode == PlayModeStateChange.ExitingEditMode) {
                LoadFromInitialScene(true);
            } else if (mode == PlayModeStateChange.EnteredEditMode) {
                ReloadEditingScenes();
            }
        }

        public static void LoadFromInitialScene(bool loadAllOpenRegions) {
            EditorSceneManager.SaveOpenScenes();
            EditorPrefs.SetString("ActiveScene", SceneManager.GetActiveScene().path);
            EditorPrefs.SetInt("OpenSceneCount", SceneManager.sceneCount);
            for (int i = 0; i < SceneManager.sceneCount; i++) {
                var scene = SceneManager.GetSceneAt(i);
                EditorPrefs.SetString($"OpenScenes[{i}].name", scene.path);
                EditorPrefs.SetBool($"OpenScenes[{i}].loaded", scene.isLoaded);
            }

            EditorPrefs.SetInt("OpenLevel", -1);
            foreach (var level in LevelManifest.Instance.Levels) {
                bool isLoaded = SceneManager.GetSceneByPath(level.Scene.Path).isLoaded;
                foreach (LevelManifestRegionEntry region in level.Regions) {
                    bool isRegionLoaded = SceneManager.GetSceneByPath(region.Scene.Path).isLoaded;
                    EditorPrefs.SetBool($"OpenLevelRegions[{region.RegionID}].loaded", isRegionLoaded && loadAllOpenRegions);
                    if (isRegionLoaded) isLoaded = true;
                }

                if (isLoaded) {
                    EditorPrefs.SetInt("OpenLevel", level.LevelID);
                    break;
                }
            }

            EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path, OpenSceneMode.Single);
        }

        public static void ReloadEditingScenes() {
            int count = EditorPrefs.GetInt("OpenSceneCount", 0);

            for (int i = 0; i < count; i++) {
                string name = EditorPrefs.GetString($"OpenScenes[{i}].name", null);
                bool isLoaded = EditorPrefs.GetBool($"OpenScenes[{i}].loaded", true);
                if (string.IsNullOrEmpty(name)) continue;

                EditorSceneManager.OpenScene(name, i == 0 ? OpenSceneMode.Single : OpenSceneMode.Additive);
                if (!isLoaded) {
                    var loadedScene = SceneManager.GetSceneByName(name);
                    EditorSceneManager.CloseScene(loadedScene, false);
                }
            }

            SceneManager.SetActiveScene(SceneManager.GetSceneByPath(EditorPrefs.GetString("ActiveScene", null)));
        }
    }
}