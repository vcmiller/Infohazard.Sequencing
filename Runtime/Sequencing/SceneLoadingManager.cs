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
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Infohazard.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Infohazard.Sequencing {
    public class SceneLoadingManager : Singleton<SceneLoadingManager> {
        private Dictionary<SceneGroup, SceneGroupInfo> _sceneGroups = new Dictionary<SceneGroup, SceneGroupInfo>();
        private Dictionary<string, SceneState> _sceneLoadingStates = new Dictionary<string, SceneState>();

        [SerializeField] private SceneGroup _defaultGroup;

        public event Action AllScenesFinishedLoading;

        public bool IsUnloadingAnyScenes => _sceneGroups.Values.Any(g => g.UnloadingScenes.Count > 0);

        public bool IsLoadingAnyScenes(SceneLoadingType type = SceneLoadingType.All) =>
            _sceneGroups.Values.Any(g => g.LoadingScenes.Any(op => (op.SceneType & type) != 0));

        public SceneStateType
            GetSceneLoadedState(string sceneName, out LevelRoot levelRoot, out RegionRoot regionRoot) {
            if (_sceneLoadingStates.TryGetValue(sceneName, out SceneState state) && state.LoadedInfo != null) {
                levelRoot = state.LoadedInfo.Level;
                regionRoot = state.LoadedInfo.Region;
            } else {
                levelRoot = null;
                regionRoot = null;
            }

            return state.Type;
        }

        public SceneLoadOperations LoadScenes(IEnumerable<string> scenes, bool autoActivate, SceneGroup group = null) {
            List<IProgressSource> loadingFullSources = new List<IProgressSource>();
            List<IProgressSource> loadingPartialSources = new List<IProgressSource>();
            foreach (string sceneName in scenes) {
                SceneLoadOperations sceneLoad = LoadScene(sceneName, autoActivate, false, group);
                if (sceneLoad.IsValid) {
                    loadingFullSources.Add(sceneLoad.FullOperation);
                    loadingPartialSources.Add(sceneLoad.PartialOperation);
                }
            }

            if (loadingFullSources.Count > 0) {
                return new SceneLoadOperations {
                    FullOperation = new MultiProgressSource(loadingFullSources),
                    PartialOperation = new MultiProgressSource(loadingPartialSources),
                };
            }

            return default;
        }

        public SceneLoadOperations LoadScene(string sceneNameOrPath, bool autoActivate, bool setActiveScene,
                                             SceneGroup group) {
            if (!group) group = _defaultGroup;
            if (!_sceneGroups.TryGetValue(group, out SceneGroupInfo groupInfo)) {
                groupInfo = new SceneGroupInfo();
                _sceneGroups.Add(group, groupInfo);
            }

            string sceneName = Path.GetFileNameWithoutExtension(sceneNameOrPath);

            _sceneLoadingStates.TryGetValue(sceneName, out SceneState state);

            if (state.Type == SceneStateType.Loading || state.Type == SceneStateType.Cancelled) {
                foreach (SceneLoadingOperation loadingScene in groupInfo.LoadingScenes) {
                    if (loadingScene.SceneName != sceneName) continue;
                    loadingScene.IsCancelled = false;
                    loadingScene.SetActiveOnComplete = setActiveScene;
                    loadingScene.Operation.allowSceneActivation = autoActivate;
                    _sceneLoadingStates[sceneName] = new SceneState {
                        Type = SceneStateType.Loading,
                        LoadingOperation = loadingScene,
                    };
                    return new SceneLoadOperations {
                        FullOperation = loadingScene,
                        PartialOperation = loadingScene.PartialOperation,
                    };
                }
            }

            if (state.Type == SceneStateType.Loaded) {
                return default;
            }

            SceneLoadingType type = LevelManifest.Instance.GetLevelWithSceneNameOrPath(sceneName)
                ? SceneLoadingType.Level
                : LevelManifest.Instance.Levels.Any(l => l.GetRegionWithSceneName(sceneName))
                    ? SceneLoadingType.Region
                    : SceneLoadingType.Scene;

            AsyncOperation sceneOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            if (sceneOperation == null) {
#if UNITY_EDITOR
                sceneOperation = EditorSceneManager.LoadSceneAsyncInPlayMode(
                    sceneNameOrPath, new LoadSceneParameters(LoadSceneMode.Additive));

                if (sceneOperation != null) {
                    Debug.LogWarning($"Loading scene {sceneNameOrPath} that is not in the build settings. This will not work in a build.");
                }

                if (sceneOperation == null) {
                    return default;
                }
#else
                return default;
#endif
            }

            sceneOperation.allowSceneActivation = autoActivate;

            SceneLoadingOperation op = new SceneLoadingOperation {
                Operation = sceneOperation,
                IsCancelled = false,
                SceneName = sceneName,
                SceneType = type,
                SetActiveOnComplete = setActiveScene,
                GroupInfo = groupInfo,
                PartialOperation = new SceneLoadingPartialOperation {
                    Operation = sceneOperation,
                },
            };
            op.Task = LoadSceneAsync(op);

            _sceneLoadingStates[sceneName] = new SceneState {
                Type = SceneStateType.Loading,
                LoadingOperation = op,
            };

            groupInfo.LoadingScenes.Add(op);

            return new SceneLoadOperations {
                FullOperation = op,
                PartialOperation = op.PartialOperation,
            };
        }

        private async UniTask LoadSceneAsync(SceneLoadingOperation operation) {
            await operation.Operation;

            Scene scene = SceneManager.GetSceneByName(operation.SceneName);
            if (!scene.isLoaded) {
                Debug.LogError($"Unexpected: scene {operation.SceneName} not loaded after operation complete.");
                operation.GroupInfo.LoadingScenes.Remove(operation);
                _sceneLoadingStates[scene.name] = new SceneState {
                    Type = SceneStateType.Unloaded,
                    LoadedInfo = null,
                };
                return;
            }

            if (!operation.IsCancelled) {
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

                _sceneLoadingStates[scene.name] = new SceneState {
                    Type = SceneStateType.Loading,
                    LoadedInfo = sceneInfo,
                };

                if (sceneInfo.Level) {
                    await sceneInfo.Level.Initialize();
                }

                if (sceneInfo.Region) {
                    await sceneInfo.Region.Initialize();
                }

                if (!operation.IsCancelled) {
                    operation.GroupInfo.LoadedScenes.Add(sceneInfo);

                    _sceneLoadingStates[scene.name] = new SceneState {
                        Type = SceneStateType.Loaded,
                        LoadedInfo = sceneInfo,
                    };

                    if (operation.SetActiveOnComplete) {
                        SceneManager.SetActiveScene(scene);
                    }
                }
            }

            operation.GroupInfo.LoadingScenes.Remove(operation);

            if (operation.IsCancelled) {
                UnloadCleanedUpScene(scene, operation.GroupInfo);
            }

            if (!_sceneGroups.Any(pair => pair.Value.LoadingScenes.Count > 0)) {
                AllScenesFinishedLoading?.Invoke();
            }
        }

        private IProgressSource UnloadSceneInternal(Scene scene, SceneGroupInfo groupInfo) {
            if (!_sceneLoadingStates.TryGetValue(scene.name, out SceneState state) || state.LoadedInfo == null) {
                Debug.LogError($"Trying to unload scene {scene.name} which could not be found in the dictionary.");
                return null;
            }

            if (state.LoadedInfo.Level) state.LoadedInfo.Level.Cleanup();
            if (state.LoadedInfo.Region) state.LoadedInfo.Region.Cleanup();

            return UnloadCleanedUpScene(scene, groupInfo);
        }

        private IProgressSource UnloadCleanedUpScene(Scene scene, SceneGroupInfo groupInfo) {
            AsyncOperation op = SceneManager.UnloadSceneAsync(scene);
            SceneUnloadingOperation unloadingOperation = new SceneUnloadingOperation {
                Operation = op,
                SceneName = scene.name,
                GroupInfo = groupInfo,
            };
            unloadingOperation.Task = UnloadSceneInternalAsync(unloadingOperation);

            _sceneLoadingStates[scene.name] = new SceneState {
                Type = SceneStateType.Unloading,
                UnloadingOperation = unloadingOperation,
            };
            groupInfo.UnloadingScenes.Add(unloadingOperation);
            return unloadingOperation;
        }

        private async UniTask UnloadSceneInternalAsync(SceneUnloadingOperation operation) {
            await operation.Operation;

            operation.GroupInfo.UnloadingScenes.Remove(operation);
            _sceneLoadingStates[operation.SceneName] = new SceneState {
                Type = SceneStateType.Unloaded,
            };
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

        public IProgressSource UnloadScene(string sceneName) {
            if (!_sceneLoadingStates.TryGetValue(sceneName, out SceneState state) ||
                state.Type == SceneStateType.Unloaded ||
                state.Type == SceneStateType.Cancelled ||
                state.Type == SceneStateType.Unloading) {
                return null;
            }

            if (state.Type == SceneStateType.Loaded) {
                foreach (SceneGroupInfo groupInfo in _sceneGroups.Values) {
                    for (int index = 0; index < groupInfo.LoadedScenes.Count; index++) {
                        Scene scene = groupInfo.LoadedScenes[index].Scene;
                        if (scene.name != sceneName) continue;
                        IProgressSource source = UnloadSceneInternal(scene, groupInfo);
                        groupInfo.LoadedScenes.RemoveAt(index);

                        return source;
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
                        return loadingScene;
                    }
                }
            }

            Debug.LogError($"Invalid state: scene {sceneName} state is set to {state}, but could not find scene.");
            return null;
        }

        public void ActivateLoadingScenes(SceneGroup group = null,
                                          SceneLoadingType typesToActivate = SceneLoadingType.All) {
            if (!group) group = _defaultGroup;
            if (!_sceneGroups.TryGetValue(group, out SceneGroupInfo groupInfo)) return;

            foreach (SceneLoadingOperation op in groupInfo.LoadingScenes) {
                if ((op.SceneType & typesToActivate) != 0) {
                    op.Operation.allowSceneActivation = true;
                }
            }
        }

        private class SceneGroupInfo {
            public List<SceneLoadingOperation> LoadingScenes { get; } = new List<SceneLoadingOperation>();
            public List<LoadedSceneInfo> LoadedScenes { get; } = new List<LoadedSceneInfo>();
            public List<SceneUnloadingOperation> UnloadingScenes { get; } = new List<SceneUnloadingOperation>();
        }

        private class SceneLoadingOperation : IProgressSource {
            public UniTask Task { get; set; }
            public AsyncOperation Operation { get; set; }
            public string SceneName { get; set; }
            public bool SetActiveOnComplete { get; set; }
            public SceneLoadingType SceneType { get; set; }
            public bool IsCancelled { get; set; }
            public SceneGroupInfo GroupInfo { get; set; }
            public float Progress => Operation.progress;
            public SceneLoadingPartialOperation PartialOperation { get; set; }

            public UniTask WaitForCompletionTask() => Task;
        }

        private class SceneLoadingPartialOperation : IProgressSource {
            public AsyncOperation Operation { get; set; }
            public float Progress => Operation.progress / 0.9f;

            public UniTask WaitForCompletionTask() {
                return UniTask.WaitUntil(() => Operation.progress >= 0.9f);
            }
        }

        private class LoadedSceneInfo {
            public Scene Scene { get; set; }
            public SceneLoadingType SceneType { get; set; }
            public LevelRoot Level { get; set; }
            public RegionRoot Region { get; set; }
        }

        private class SceneUnloadingOperation : IProgressSource {
            public UniTask Task { get; set; }
            public AsyncOperation Operation { get; set; }
            public string SceneName { get; set; }
            public SceneGroupInfo GroupInfo { get; set; }
            public float Progress => Operation.progress;

            public UniTask WaitForCompletionTask() => Task;
        }

        private struct SceneState {
            public SceneStateType Type { get; set; }
            public LoadedSceneInfo LoadedInfo { get; set; }
            public SceneLoadingOperation LoadingOperation { get; set; }
            public SceneUnloadingOperation UnloadingOperation { get; set; }
        }
    }

    public enum SceneStateType {
        Unloaded,
        Loading,
        Cancelled,
        Loaded,
        Unloading
    }

    [Flags]
    public enum SceneLoadingType {
        Scene = 1 << 0,
        Level = 1 << 1,
        Region = 1 << 2,

        All = Scene | Level | Region,
    }

    public struct SceneLoadOperations {
        public IProgressSource FullOperation;
        public IProgressSource PartialOperation;

        public bool IsValid => FullOperation != null;
    }
}