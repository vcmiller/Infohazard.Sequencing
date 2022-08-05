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

using System.Collections.Generic;
using Infohazard.Core;
using UnityEngine;

namespace Infohazard.Sequencing {
    public class PassSavedSceneStep : MonoBehaviour, IExecutionStep {
        public bool IsFinished => true;

        [SerializeField] private SceneRef _defaultScene;
        
        public void ExecuteForward(ExecutionStepArguments arguments) {
            var state = PersistenceManager.Instance.LoadedStateData;
            string sceneToLoad = _defaultScene.Name;
            if (state != null && !string.IsNullOrEmpty(state.CurrentScene)) {
                sceneToLoad = state.CurrentScene;
            }
            LoadSceneOrLevelStep.ParamSceneToLoad.Set(arguments, sceneToLoad);

            var level = LevelManifest.Instance.GetLevelWithSceneName(sceneToLoad);
            if (!level) return;
            
            List<int> regionsToLoad = new List<int>();
            foreach (LevelManifestRegionEntry region in level.Regions) {
                RegionSaveData regionData = PersistenceManager.Instance.GetRegionData(level.LevelID, region.RegionID);
                if (regionData == null || !regionData.Loaded) continue;
                regionsToLoad.Add(region.RegionID);
            }

            if (regionsToLoad.Count > 0) {
                LoadInitialRegionsStep.ParamRegionsToLoad.Set(arguments, regionsToLoad);
            }
        }
    }
}