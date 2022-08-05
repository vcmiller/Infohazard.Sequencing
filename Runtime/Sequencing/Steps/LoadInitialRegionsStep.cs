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
        
        public void ExecuteForward(ExecutionStepArguments arguments) {
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