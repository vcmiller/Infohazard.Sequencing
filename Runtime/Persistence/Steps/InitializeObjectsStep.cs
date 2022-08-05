using System.Collections;
using UnityEngine;

namespace Infohazard.Sequencing {
    public class InitializeObjectsStep : MonoBehaviour, IExecutionStep {
        public bool IsFinished { get; private set; }
        
        public void ExecuteForward(ExecutionStepArguments arguments) {
            var level = PersistedLevelRoot.Current;
            if (!level) {
                IsFinished = true;
                return;
            }

            IsFinished = false;
            StartCoroutine(CRT_Execution(level));
        }

        private IEnumerator CRT_Execution(PersistedLevelRoot level) {
            yield return null;
            level.LoadObjects();
            IsFinished = true;
        }
    }
}