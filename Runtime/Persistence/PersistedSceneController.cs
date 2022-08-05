using UnityEngine;

namespace Infohazard.Sequencing {

    public class PersistedSceneController : SimpleSceneController {
        public new static PersistedSceneController Instance =>
            SimpleSceneController.Instance as PersistedSceneController;

        [SerializeField] private ExecutionStepSequencer _goToMainMenuSequencer;
        [SerializeField] private ExecutionStepSequencer _loadStateSequencer;
        [SerializeField] private ExecutionStepSequencer _loadProfileSequencer;

        public ExecutionStepSequencer GoToMainMenuSequencer => _goToMainMenuSequencer;
        public ExecutionStepSequencer LoadStateSequencer => _loadStateSequencer;
        public ExecutionStepSequencer LoadProfileSequencer => _loadProfileSequencer;

        public override void GoToMainMenu() {
            ExecutionStepArguments args = new ExecutionStepArguments();
            LoadSceneOrLevelStep.ParamSceneToLoad.Set(args, MainMenuScene.Name);
            _goToMainMenuSequencer.ExecuteForward(args);
        }

        public virtual void LoadState(string state) {
            if (PersistenceManager.Instance.LoadedProfileData == null) {
                Debug.LogError("Trying to load a state with no profile data loaded.");
                return;
            }
            
            ExecutionStepArguments args = new ExecutionStepArguments();
            LoadStateSaveDataStep.ParamStateName.Set(args, state);
            _loadStateSequencer.ExecuteForward(args);
        }

        public virtual void LoadProfile(string profile) {
            if (PersistenceManager.Instance.LoadedGlobalData == null) {
                Debug.LogError("Trying to load a profile with no global data loaded.");
                return;
            }
            
            ExecutionStepArguments args = new ExecutionStepArguments();
            LoadProfileSaveDataStep.ParamProfileName.Set(args, profile);
            _loadProfileSequencer.ExecuteForward(args);
        }

        public virtual void LoadMostRecentProfile() {
            if (PersistenceManager.Instance.LoadedGlobalData == null) {
                Debug.LogError("Trying to load a profile with no global data loaded.");
                return;
            }

            string profile = PersistenceManager.Instance.LoadedGlobalData.MostRecentProfile;
            if (string.IsNullOrEmpty(profile)) profile = "Default";
            LoadProfile(profile);
        }

        public virtual void LoadMostRecentState() {
            if (PersistenceManager.Instance.LoadedProfileData == null) {
                Debug.LogError("Trying to load a state with no profile data loaded.");
                return;
            }

            string state = PersistenceManager.Instance.LoadedProfileData.MostRecentState;
            if (string.IsNullOrEmpty(state)) state = "DefaultState";
            LoadState(state);
        }
    }
}
