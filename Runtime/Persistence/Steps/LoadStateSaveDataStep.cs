using UnityEngine;

namespace Infohazard.Sequencing.Runtime {
    public class LoadStateSaveDataStep : MonoBehaviour, IExecutionStep {
        [SerializeField] private string _defaultStateIndex = "DefaultState";

        public static readonly ExecutionStepParameter<string> ParamStateName = new ExecutionStepParameter<string>();
        
        public bool IsFinished => true;
        
        public void ExecuteForward(ExecutionStepArguments arguments) {
            string stateToLoad = ParamStateName.GetOrDefault(arguments, _defaultStateIndex);
            PersistenceManager.Instance.LoadStateData(stateToLoad);
        }
    }
}