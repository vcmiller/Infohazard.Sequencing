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
using Cysharp.Threading.Tasks;
using Infohazard.Core;
using UnityEngine;

namespace Infohazard.Sequencing {
    public class LoadSceneOrLevelStep : MonoBehaviour, IExecutionStep {
        [SerializeField] private SceneRef _defaultSceneToLoad;
        [SerializeField] private bool _makeActiveScene;
        [SerializeField] private bool _enableImmediately;
        [SerializeField] private SceneGroup _sceneGroup;
        [SerializeField] private bool _waitToFinish = true;

        public static readonly ExecutionStepParameter<string> ParamSceneToLoad = new ExecutionStepParameter<string>();

        public async UniTask Execute(ExecutionStepArguments arguments) {
            string sceneToLoad = ParamSceneToLoad.GetOrDefault(arguments, _defaultSceneToLoad.Path);
            SceneLoadOperations operation =
                SceneLoadingManager.Instance.LoadScene(sceneToLoad, _enableImmediately, _makeActiveScene, _sceneGroup);
            
            if (!operation.IsValid) {
                Debug.LogError($"Failed to load scene {sceneToLoad}.");
                return;
            }
            
            var level = LevelManifest.Instance.GetLevelWithSceneNameOrPath(sceneToLoad);

            var loading = LoadingScreen.Instance;
            if (loading != null && operation.IsValid) {
                loading.SetProgressSource(operation.PartialOperation);
                loading.SetText(level ? "Loading Level..." : "Loading...");
            }
            
            LoadRegionsStep.ParamLoadingLevel.Set(arguments, level);

            if (operation.IsValid && _waitToFinish) {
                if (!_enableImmediately) {
                    await operation.PartialOperation.WaitForCompletionTask();
                } else {
                    await operation.FullOperation.WaitForCompletionTask();
                }
            }
        }
    }
}
