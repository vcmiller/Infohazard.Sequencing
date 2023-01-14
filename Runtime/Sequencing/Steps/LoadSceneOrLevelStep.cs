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

using System.Collections;
using Infohazard.Core;
using UnityEngine;

namespace Infohazard.Sequencing {
    public class LoadSceneOrLevelStep : MonoBehaviour, IExecutionStep {
        [SerializeField] private SceneRef _defaultSceneToLoad;
        [SerializeField] private bool _makeActiveScene;
        [SerializeField] private bool _enableImmediately;
        [SerializeField] private SceneGroup _sceneGroup;

        public static readonly ExecutionStepParameter<string> ParamSceneToLoad = new ExecutionStepParameter<string>();
        
        public bool IsFinished { get; private set; }
        
        public void Execute(ExecutionStepArguments arguments) {
            StartCoroutine(CRT_Execution(arguments));
        }

        private IEnumerator CRT_Execution(ExecutionStepArguments arguments) {
            IsFinished = false;

            string sceneToLoad = ParamSceneToLoad.GetOrDefault(arguments, _defaultSceneToLoad.Name);
            AsyncOperation operation = SceneLoadingManager.Instance.LoadScene(sceneToLoad, _enableImmediately,
                                                                         _makeActiveScene, _sceneGroup);
            if (operation == null) {
                Debug.LogError($"Failed to load scene {sceneToLoad}.");
                yield break;
            }
            
            var level = LevelManifest.Instance.GetLevelWithSceneName(sceneToLoad);

            var loading = LoadingScreen.Instance;
            if (loading) {
                loading.SetProgressSource(operation);
                loading.SetText(level ? "Loading Level..." : "Loading...");
            }
            
            LoadInitialRegionsStep.ParamLoadingLevel.Set(arguments, level);

            if (!_enableImmediately) {
                while (operation.progress < 0.9f) {
                    yield return null;
                }
            } else {
                yield return operation;
            }

            IsFinished = true;
        }
    }
}
