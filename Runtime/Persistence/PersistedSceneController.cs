// The MIT License (MIT)
// 
// Copyright (c) 2022-present Vincent Miller
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using Cysharp.Threading.Tasks;
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
            _goToMainMenuSequencer.Execute(args).Forget();
        }

        public virtual void LoadState(string state) {
            if (PersistenceManager.Instance.LoadedProfileData == null) {
                Debug.LogError("Trying to load a state with no profile data loaded.");
                return;
            }
            
            ExecutionStepArguments args = new ExecutionStepArguments();
            LoadStateSaveDataStep.ParamStateName.Set(args, state);
            _loadStateSequencer.Execute(args).Forget();
        }

        public virtual void LoadProfile(string profile) {
            if (PersistenceManager.Instance.LoadedGlobalData == null) {
                Debug.LogError("Trying to load a profile with no global data loaded.");
                return;
            }
            
            ExecutionStepArguments args = new ExecutionStepArguments();
            LoadProfileSaveDataStep.ParamProfileName.Set(args, profile);
            _loadProfileSequencer.Execute(args).Forget();
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
