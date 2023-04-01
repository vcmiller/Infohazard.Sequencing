using UnityEngine;

namespace Infohazard.Sequencing {
    public class DelayLoadingRegionsStep : MonoBehaviour, IExecutionStep {
        public bool IsFinished => true;
        
        public void Execute(ExecutionStepArguments arguments) {
            var level = PersistedLevelRoot.Current;
            if (!level) {
                return;
            }

            level.DelayRegionLoading();
        }
    }
}