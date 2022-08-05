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
using System.Collections.Generic;
using System.Linq;
using Infohazard.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Infohazard.Sequencing {
    public class SceneLoadingManager : Singleton<SceneLoadingManager> {
        private Dictionary<SceneGroup, SceneGroupInfo> _sceneGroups = new Dictionary<SceneGroup, SceneGroupInfo>();
        private Dictionary<string, SceneState> _sceneLoadingStates = new Dictionary<string, SceneState>();

        [SerializeField] private SceneGroup _defaultGroup;

        public event Action AllScenesFinishedLoading;

        public bool IsUnloadingAnyScenes => _sceneGroups.Values.Any(g => g.UnloadingScenes.Count > 0);

        public bool IsLoadingAnyScenes(SceneLoadingType type = SceneLoadingType.All) =>
            _sceneGroups.Values.Any(g => g.LoadingScenes.Any(op => (op.SceneType & type) != 0));

        public SceneStateType GetSceneLoadedState(string sceneName, out LevelRoot levelRoot, out RegionRoot regionRoot) {
            if (_sceneLoadingStates.TryGetValue(sceneName, out SceneState state) && state.LoadedInfo != null) {
                levelRoot = state.LoadedInfo.Level;
                regionRoot = state.LoadedInfo.Region;
            } else {
                levelRoot = null;
                regionRoot = null;
            }
            
            return state.Type;
        }
        
        public List<AsyncOperation> LoadScenes(IEnumerable<string> scenes, bool autoActivate, SceneGroup group = null) {
            List<AsyncOperation> ops = new List<AsyncOperation>();
            foreach (string scene in scenes) {
                AsyncOperation op = LoadScene(scene, autoActivate, false, group);
                if (op != null) ops.Add(op);
            }

            return ops;
        }

        public AsyncOperation LoadScene(string sceneName, bool autoActivate, bool setActiveScene, SceneGroup group = null) {
            if (!group) group = _defaultGroup;
            if (!_sceneGroups.TryGetValue(group, out SceneGroupInfo groupInfo)) {
                groupInfo = new SceneGroupInfo();
                _sceneGroups.Add(group, groupInfo);
            }

            if (_sceneLoadingStates.TryGetValue(sceneName, out SceneState state) &&
                (state.Type == SceneStateType.Loaded || state.Type == SceneStateType.Loading)) {
                return null;
            }

            if (state.Type == SceneStateType.Cancelled) {
                foreach (SceneLoadingOperation loadingScene in groupInfo.LoadingScenes) {
                    if (loadingScene.SceneName != sceneName) continue;
                    loadingScene.IsCancelled = false;
                    loadingScene.SetActiveOnComplete = setActiveScene;
                    loadingScene.Operation.allowSceneActivation = autoActivate;
                    _sceneLoadingStates[sceneName] = new SceneState {
                        Type = SceneStateType.Loading,
                        LoadingOperation = loadingScene,
                    };
                    return loadingScene.Operation;
                }
            }
            
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            if (op == null) return null;
            op.allowSceneActivation = autoActivate;

            SceneLoadingType type = LevelManifest.Instance.GetLevelWithSceneName(sceneName)
                ? SceneLoadingType.Level
                : LevelManifest.Instance.Levels.Any(l => l.GetRegionWithSceneName(sceneName))
                    ? SceneLoadingType.Region
                    : SceneLoadingType.Scene;
            
            var loadingOperation = new SceneLoadingOperation {
                Operation = op,
                SceneName = sceneName,
                SetActiveOnComplete = setActiveScene,
                SceneType = type,
            };
            
            _sceneLoadingStates[sceneName] = new SceneState {
                Type = SceneStateType.Loading,
                LoadingOperation = loadingOperation,
            };
            
            groupInfo.LoadingScenes.Add(loadingOperation);

            return op;
        }

        private void UnloadSceneInternal(Scene scene, SceneGroupInfo groupInfo) {
            if (!_sceneLoadingStates.TryGetValue(scene.name, out SceneState state) || state.LoadedInfo == null) {
                Debug.LogError($"Trying to unload scene {scene.name} which could not be found in the dictionary.");
                return;
            }
            
            if (state.LoadedInfo.Level) state.LoadedInfo.Level.Cleanup();
            if (state.LoadedInfo.Region) state.LoadedInfo.Region.Cleanup();
            
            AsyncOperation op = SceneManager.UnloadSceneAsync(scene);
            SceneUnloadingOperation unloadingOperation = new SceneUnloadingOperation {
                Operation = op,
                SceneName = scene.name,
            };
            _sceneLoadingStates[scene.name] = new SceneState {
                Type = SceneStateType.Unloading,
                UnloadingOperation = unloadingOperation,
            };
            groupInfo.UnloadingScenes.Add(unloadingOperation);
        }

        public void UnloadScenes(SceneGroup group = null) {
            if (!group) group = _defaultGroup;
            if (!_sceneGroups.TryGetValue(group, out SceneGroupInfo groupInfo)) return;

            foreach (SceneLoadingOperation loadingScene in groupInfo.LoadingScenes) {
                loadingScene.IsCancelled = true;
                _sceneLoadingStates[loadingScene.SceneName] = new SceneState {
                    Type = SceneStateType.Cancelled,
                    LoadingOperation = loadingScene,
                };
            }

            foreach (LoadedSceneInfo scene in groupInfo.LoadedScenes) {
                UnloadSceneInternal(scene.Scene, groupInfo);
            }
            groupInfo.LoadedScenes.Clear();
        }

        public bool UnloadScene(string sceneName) {
            if (!_sceneLoadingStates.TryGetValue(sceneName, out SceneState state) ||
                state.Type == SceneStateType.Unloaded ||
                state.Type == SceneStateType.Cancelled ||
                state.Type == SceneStateType.Unloading) {
                return false;
            }

            if (state.Type == SceneStateType.Loaded) {
                foreach (SceneGroupInfo groupInfo in _sceneGroups.Values) {
                    for (int index = 0; index < groupInfo.LoadedScenes.Count; index++) {
                        Scene scene = groupInfo.LoadedScenes[index].Scene;
                        if (scene.name != sceneName) continue;
                        UnloadSceneInternal(scene, groupInfo);
                        groupInfo.LoadedScenes.RemoveAt(index);
                        
                        return true;
                    }
                }
            } else if (state.Type == SceneStateType.Loading) {
                foreach (SceneGroupInfo groupInfo in _sceneGroups.Values) {
                    foreach (SceneLoadingOperation loadingScene in groupInfo.LoadingScenes) {
                        if (loadingScene.SceneName != sceneName) continue;
                        loadingScene.IsCancelled = true;
                        _sceneLoadingStates[loadingScene.SceneName] = new SceneState {
                            Type = SceneStateType.Cancelled,
                            LoadingOperation = loadingScene,
                        };
                        return true;
                    }
                }
            }

            Debug.LogError($"Invalid state: scene {sceneName} state is set to {state}, but could not find scene.");
            return false;
        }

        public void ActivateLoadingScenes(SceneGroup group = null, SceneLoadingType typesToActivate = SceneLoadingType.All) {
            if (!group) group = _defaultGroup;
            if (!_sceneGroups.TryGetValue(group, out SceneGroupInfo groupInfo)) return;

            foreach (SceneLoadingOperation op in groupInfo.LoadingScenes) {
                if ((op.SceneType & typesToActivate) != 0) {
                    op.Operation.allowSceneActivation = true;
                }
            }
        }

        private void Update() {
            bool anyLoadFinished = false;
            foreach (SceneGroupInfo groupInfo in _sceneGroups.Values) {
                foreach (SceneLoadingOperation operation in groupInfo.LoadingScenes) {
                    if (!operation.Operation.isDone) continue;
                    
                    Scene scene = SceneManager.GetSceneByName(operation.SceneName);
                    if (!scene.isLoaded) {
                        Debug.LogError($"Unexpected: scene {operation.SceneName} not loaded after operation complete.");
                        continue;
                    }

                    if (operation.IsCancelled) {
                        AsyncOperation op = SceneManager.UnloadSceneAsync(scene);
                        SceneUnloadingOperation unloadingOperation = new SceneUnloadingOperation {
                            Operation = op,
                            SceneName = scene.name,
                        };
                        groupInfo.UnloadingScenes.Add(unloadingOperation);
                        _sceneLoadingStates[scene.name] = new SceneState {
                            Type = SceneStateType.Unloading,
                            UnloadingOperation = unloadingOperation,
                        };
                        continue;
                    }

                    var sceneInfo = new LoadedSceneInfo {
                        Scene = scene,
                        SceneType = operation.SceneType,
                        Level = operation.SceneType == SceneLoadingType.Level
                            ? scene.GetRootGameObjects().FirstOrDefaultWhere(
                                (GameObject obj, out LevelRoot lvl) => obj.TryGetComponent(out lvl))
                            : null,
                        Region = operation.SceneType == SceneLoadingType.Region
                            ? scene.GetRootGameObjects().FirstOrDefaultWhere(
                                ((GameObject obj, out RegionRoot reg) => obj.TryGetComponent(out reg)))
                            : null,
                    };
                    
                    groupInfo.LoadedScenes.Add(sceneInfo);
                    anyLoadFinished = true;
                    
                    _sceneLoadingStates[scene.name] = new SceneState {
                        Type = SceneStateType.Loaded,
                        LoadedInfo = sceneInfo,
                    };
                    if (operation.SetActiveOnComplete) {
                        SceneManager.SetActiveScene(scene);
                    }

                    if (sceneInfo.Level) sceneInfo.Level.Initialize();
                    if (sceneInfo.Region) sceneInfo.Region.Initialize();
                }

                foreach (SceneUnloadingOperation operation in groupInfo.UnloadingScenes) {
                    if (!operation.Operation.isDone) continue;
                    _sceneLoadingStates[operation.SceneName] = new SceneState {
                        Type = SceneStateType.Unloaded,
                    };
                }

                groupInfo.LoadingScenes.RemoveAll(op => op.Operation.isDone);
                groupInfo.UnloadingScenes.RemoveAll(op => op.Operation.isDone);
            }

            if (anyLoadFinished && !_sceneGroups.Any(pair => pair.Value.LoadingScenes.Count > 0)) {
                AllScenesFinishedLoading?.Invoke();
            }
        }

        private class SceneGroupInfo {
            public List<SceneLoadingOperation> LoadingScenes { get; } = new List<SceneLoadingOperation>();
            public List<LoadedSceneInfo> LoadedScenes { get; } = new List<LoadedSceneInfo>();
            public List<SceneUnloadingOperation> UnloadingScenes { get; } = new List<SceneUnloadingOperation>();
        }
        
        private class SceneLoadingOperation {
            public AsyncOperation Operation { get; set; }
            public string SceneName { get; set; }
            public bool SetActiveOnComplete { get; set; }
            public SceneLoadingType SceneType { get; set; }
            public bool IsCancelled { get; set; }
        }
        
        private class LoadedSceneInfo {
            public Scene Scene { get; set; }
            public SceneLoadingType SceneType { get; set; }
            public LevelRoot Level { get; set; }
            public RegionRoot Region { get; set; }
        }
        
        private class SceneUnloadingOperation {
            public AsyncOperation Operation { get; set; }
            public string SceneName { get; set; }
        }

        private struct SceneState {
            public SceneStateType Type { get; set; }
            public LoadedSceneInfo LoadedInfo { get; set; }
            public SceneLoadingOperation LoadingOperation { get; set; }
            public SceneUnloadingOperation UnloadingOperation { get; set; }
        }
    }

    public enum SceneStateType {
        Unloaded, Loading, Cancelled, Loaded, Unloading
    }

    [Flags]
    public enum SceneLoadingType {
        Scene = 1 << 0,
        Level = 1 << 1,
        Region = 1 << 2,
        
        All = Scene | Level | Region,
    }
}