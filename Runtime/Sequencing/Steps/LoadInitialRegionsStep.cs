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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Infohazard.Sequencing {
    public class LoadInitialRegionsStep : MonoBehaviour, IExecutionStep {
        [SerializeField] private bool _enableImmediately;
        [SerializeField] private SceneGroup _sceneGroup;
        
        public static readonly ExecutionStepParameter<LevelManifestLevelEntry> ParamLoadingLevel =
            new ExecutionStepParameter<LevelManifestLevelEntry>();

        public static readonly ExecutionStepParameter<IEnumerable<int>> ParamRegionsToLoad =
            new ExecutionStepParameter<IEnumerable<int>>();

        public bool IsFinished { get; private set; }

        protected virtual IEnumerable<int> DefaultRegionsToLoad(LevelManifestLevelEntry level) {
            foreach (LevelManifestRegionEntry region in level.Regions) {
                if (region.LoadedByDefault || region.AlwaysLoaded) {
                    yield return region.RegionID;
                }
            }
        }
        
        public void Execute(ExecutionStepArguments arguments) {
            if (ParamLoadingLevel.Get(arguments, out LevelManifestLevelEntry level) && level != null) {
                StartCoroutine(CRT_Execution(level, arguments));
            } else {
                IsFinished = true;
            }
        }

        private IEnumerator CRT_Execution(LevelManifestLevelEntry level, ExecutionStepArguments arguments) {
            IsFinished = false;
            
            HashSet<int> regionsToLoad =
                new HashSet<int>(ParamRegionsToLoad.GetOrDefault(arguments, DefaultRegionsToLoad(level)));
            
            List<AsyncOperation> regionOperations = SceneLoadingManager.Instance.LoadScenes(
                level.Regions.Where(r => regionsToLoad.Contains(r.RegionID)).Select(r => r.Scene.Name),
                _enableImmediately, _sceneGroup);

            var loading = LoadingScreen.Instance;
            if (loading && regionOperations.Count > 0) {
                loading.SetText("Loading Regions...");
                loading.SetProgressSource(regionOperations);
            }

            if (_enableImmediately) {
                foreach (AsyncOperation operation in regionOperations) {
                    yield return operation;
                }
            } else {
                while (regionOperations.Count > 0 && regionOperations.Any(op => op.progress < 0.9f)) {
                    yield return null;
                }
            }
            
            IsFinished = true;
        }
    }
}