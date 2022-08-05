using UnityEngine;

namespace Infohazard.Sequencing {
    public class LoadProfileSaveDataStep : MonoBehaviour, IExecutionStep {
        [SerializeField] private string _defaultProfileName = "Default";
        [SerializeField] private bool _passMostRecentState = true;

        public static readonly ExecutionStepParameter<string> ParamProfileName = new ExecutionStepParameter<string>();
        
        public bool IsFinished => true;
        
        public void ExecuteForward(ExecutionStepArguments arguments) {
            string profileToLoad = ParamProfileName.GetOrDefault(arguments, _defaultProfileName);
            PersistenceManager.Instance.LoadProfileData(profileToLoad);

            if (_passMostRecentState && !string.IsNullOrEmpty(PersistenceManager.Instance.LoadedProfileData?.MostRecentState)) {
                LoadStateSaveDataStep.ParamStateName.Set(arguments, PersistenceManager.Instance.LoadedProfileData.MostRecentState);
            }
        }
    }
}