using Infohazard.Core.Runtime;
using UnityEngine;

namespace Infohazard.Sequencing.Runtime {
    public class PassSceneStep : MonoBehaviour, IExecutionStep {
        [SerializeField] private SceneRef _sceneToLoad;
        public bool IsFinished => true;
        
        public void ExecuteForward(ExecutionStepArguments arguments) {
            LoadSceneOrLevelStep.ParamSceneToLoad.Set(arguments, _sceneToLoad.Name);
        }
    }
}