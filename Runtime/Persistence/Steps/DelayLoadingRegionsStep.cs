using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Infohazard.Sequencing {
    public class DelayLoadingRegionsStep : MonoBehaviour, IExecutionStep {
        public UniTask Execute(ExecutionStepArguments arguments) {
            PersistedLevelRoot level = PersistedLevelRoot.Current;
            if (!level) {
                return UniTask.CompletedTask;
            }

            level.DelayRegionLoading();
            return UniTask.CompletedTask;
        }
    }
}