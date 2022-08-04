using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using System.IO;

using UnityEditor;

#endif

namespace Infohazard.Sequencing.Runtime {
    public class PassEditingSceneStep : MonoBehaviour, IExecutionStep {
        public bool IsFinished => true;

        private ExecutionStepSequencer _executingSequencer;
        
        public void ExecuteForward(ExecutionStepArguments arguments) {
#if UNITY_EDITOR
            var sceneParam = LoadSceneOrLevelStep.ParamSceneToLoad;
            var regionsParam = LoadInitialRegionsStep.ParamRegionsToLoad;

            int level = EditorPrefs.GetInt("OpenLevel", -1);
            LevelManifestLevelEntry levelEntry = LevelManifest.Instance.GetLevelWithID(level);
            if (levelEntry != null) {
                sceneParam.Set(arguments, levelEntry.Scene.Name);
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
                        sceneParam.Set(arguments, Path.GetFileNameWithoutExtension(scenePath));
                    }
                }
            }
#endif
        }
    }
}