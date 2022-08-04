using System.Collections.Generic;
using Infohazard.Core.Runtime;
using UnityEngine;

namespace Infohazard.Sequencing.Runtime {
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