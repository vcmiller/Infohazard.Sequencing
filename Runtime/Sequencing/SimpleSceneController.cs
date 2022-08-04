using Infohazard.Core.Runtime;
using UnityEngine;

namespace Infohazard.Sequencing.Runtime {
    public class SimpleSceneController : Singleton<SimpleSceneController> {
        [SerializeField] private SceneRef _mainMenuScene;
        [SerializeField] private ExecutionStepSequencer _goToSceneSequencer;

        public SceneRef MainMenuScene => _mainMenuScene;
        public ExecutionStepSequencer GoToSceneSequencer => _goToSceneSequencer;

        public virtual void GoToScene(string scene) {
            ExecutionStepArguments args = new ExecutionStepArguments();
            LoadSceneOrLevelStep.ParamSceneToLoad.Set(args, scene);
            _goToSceneSequencer.ExecuteForward(args);
        }

        public virtual void GoToMainMenu() {
            ExecutionStepArguments args = new ExecutionStepArguments();
            LoadSceneOrLevelStep.ParamSceneToLoad.Set(args, _mainMenuScene.Name);
            _goToSceneSequencer.ExecuteForward(args);
        }
    }
}