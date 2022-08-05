using Infohazard.Core;
using UnityEngine;

namespace Infohazard.Sequencing {
    public class PauseStep : MonoBehaviour, IExecutionStep {
        [SerializeField] private bool _paused;
        
        public bool IsFinished => true;
        public void ExecuteForward(ExecutionStepArguments arguments) {
            Pause.Paused = _paused;
        }
    }

}