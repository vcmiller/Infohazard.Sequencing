using System.Collections;
using Infohazard.Core.Runtime;
using UnityEngine;

namespace Infohazard.Sequencing.Runtime {
    public class LoadSceneOrLevelStep : MonoBehaviour, IExecutionStep {
        [SerializeField] private SceneRef _defaultSceneToLoad;
        [SerializeField] private bool _makeActiveScene;
        [SerializeField] private bool _enableImmediately;
        [SerializeField] private SceneGroup _sceneGroup;

        public static readonly ExecutionStepParameter<string> ParamSceneToLoad = new ExecutionStepParameter<string>();
        
        public bool IsFinished { get; private set; }
        
        public void ExecuteForward(ExecutionStepArguments arguments) {
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
