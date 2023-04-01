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
using Cysharp.Threading.Tasks;
using Infohazard.Core;
using UnityEngine;

namespace Infohazard.Sequencing {
    public class LoadRegionsStep : MonoBehaviour, IExecutionStep {
        [SerializeField] private bool _enableImmediately;
        [SerializeField] private SceneGroup _sceneGroup;
        [SerializeField] private bool _waitToFinish = true;
        
        public static readonly ExecutionStepParameter<LevelManifestLevelEntry> ParamLoadingLevel =
            new ExecutionStepParameter<LevelManifestLevelEntry>();

        public static readonly ExecutionStepParameter<IEnumerable<int>> ParamRegionsToLoad =
            new ExecutionStepParameter<IEnumerable<int>>();

        protected virtual IEnumerable<int> DefaultRegionsToLoad(LevelManifestLevelEntry level) {
            foreach (LevelManifestRegionEntry region in level.Regions) {
                if (region.LoadedByDefault || region.AlwaysLoaded) {
                    yield return region.RegionID;
                }
            }
        }

        public async UniTask Execute(ExecutionStepArguments args) {
            if (!ParamLoadingLevel.Get(args, out LevelManifestLevelEntry level) || level == null) {
                return;
            }
            
            HashSet<int> regionsToLoad =
                new HashSet<int>(ParamRegionsToLoad.GetOrDefault(args, DefaultRegionsToLoad(level)));
            
            SceneLoadOperations regionOperations = SceneLoadingManager.Instance.LoadScenes(
                level.Regions.SelectWhere((LevelManifestRegionEntry r, out string sceneName) => {
                    sceneName = r.Scene.Name;
                    return regionsToLoad.Contains(r.RegionID);
                }),
                _enableImmediately, _sceneGroup);

            LoadingScreen loading = LoadingScreen.Instance;
            if (loading != null && regionOperations.IsValid) {
                loading.SetText("Loading Regions...");
                loading.SetProgressSource(regionOperations.PartialOperation);
            }

            if (regionOperations.IsValid && _waitToFinish) {
                if (_enableImmediately) {
                    await regionOperations.FullOperation.WaitForCompletionTask();
                } else {
                    await regionOperations.PartialOperation.WaitForCompletionTask();
                }
            }
        }
    }
}