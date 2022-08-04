using UnityEngine;

namespace Infohazard.Sequencing.Runtime {
    public class WaitForScenesToLoad : MonoBehaviour, IExecutionStep {
        [SerializeField] private SceneLoadingType _types = SceneLoadingType.All;
        public bool IsFinished => !SceneLoadingManager.Instance.IsLoadingAnyScenes(_types);
        public void ExecuteForward(ExecutionStepArguments arguments) { }
    }
}