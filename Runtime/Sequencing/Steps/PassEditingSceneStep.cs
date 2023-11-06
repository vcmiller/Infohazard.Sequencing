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
using Cysharp.Threading.Tasks;
using UnityEngine;

#if UNITY_EDITOR
using System.IO;

using UnityEditor;

#endif

namespace Infohazard.Sequencing {
    public class PassEditingSceneStep : MonoBehaviour, IExecutionStep {

        private ExecutionStepSequencer _executingSequencer;
        
        public UniTask Execute(ExecutionStepArguments arguments) {
#if UNITY_EDITOR
            ExecutionStepParameter<string> sceneParam = LoadSceneOrLevelStep.ParamSceneToLoad;
            ExecutionStepParameter<IEnumerable<int>> regionsParam = LoadRegionsStep.ParamRegionsToLoad;

            int level = EditorPrefs.GetInt("OpenLevel", -1);
            LevelManifestLevelEntry levelEntry = LevelManifest.Instance.GetLevelWithID(level);
            if (levelEntry != null) {
                sceneParam.Set(arguments, levelEntry.Scene.Path);
                List<int> openRegions = new List<int>();
                foreach (LevelManifestRegionEntry region in levelEntry.Regions) {
                    if (EditorPrefs.GetBool($"OpenLevelRegions[{region.RegionID}].loaded", false)) {
                        openRegions.Add(region.RegionID);
                    }
                }

                if (openRegions.Count > 0) {
                    regionsParam.Set(arguments, openRegions);
                }
            } else {
                string scenePath = EditorPrefs.GetString("ActiveScene", null);
                for (int i = 1; i < EditorBuildSettings.scenes.Length; i++) {
                    var scene = EditorBuildSettings.scenes[i];
                    if (scene.path == scenePath) {
                        sceneParam.Set(arguments, scenePath);
                    }
                }
            }
#endif
            return UniTask.CompletedTask;
        }
    }
}