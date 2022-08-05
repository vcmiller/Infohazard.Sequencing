using Infohazard.Core;
using UnityEngine;

namespace Infohazard.Sequencing {
    public class SetCurrentSavedSceneToLoadingSceneStep : MonoBehaviour, IExecutionStep {
        [SerializeField] private SceneRef _defaultScene;
        
        public bool IsFinished => true;
        public void ExecuteForward(ExecutionStepArguments arguments) {
            var state = PersistenceManager.Instance.LoadedStateData;
            if (state == null) {
                Debug.LogError("Trying to save current scene when no state data is loaded.");
                return;
            }

            state.CurrentScene = LoadSceneOrLevelStep.ParamSceneToLoad.GetOrDefault(arguments, _defaultScene.Name);
        }
    }
}