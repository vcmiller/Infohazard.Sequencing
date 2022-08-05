using System.Collections;
using System.Collections.Generic;
using Infohazard.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Infohazard.Sequencing {
    public class ExecutionStepSequencer : MonoBehaviour, IExecutionStep {
        [SerializeField] private bool _playOnAwake = true;
        
        private List<IExecutionStep> _steps;
        private int _currentStep;
        
        public bool IsFinished { get; private set; }

        public static bool InitialSceneIsLoaded {
            get {
                for (int i = 0; i < SceneManager.sceneCount; i++) {
                    var scene = SceneManager.GetSceneAt(i);
                    if (scene.buildIndex == 0) return true;
                }

                return false;
            }
        }

        private void Awake() {
            _steps = new List<IExecutionStep>();
            for (int i = 0; i < transform.childCount; i++) {
                Transform child = transform.GetChild(i);
                if (child.gameObject.activeSelf && child.TryGetComponent(out IExecutionStep step)) {
                    _steps.Add(step);
                }
            }
        }

        private void Start() {
            if (_playOnAwake) {
                if (!InitialSceneIsLoaded) {
                    Debug.LogError("Initial scene is not loaded!");
                    SceneControl.Quit();
                }

                ExecuteForward(new ExecutionStepArguments());
            }
        }

        public void ExecuteForward(ExecutionStepArguments arguments) {
            StartCoroutine(ExecuteForwardCoroutine(arguments));
        }

        public IEnumerator ExecuteForwardCoroutine(ExecutionStepArguments arguments) {
            IsFinished = false;
            
            for (int index = 0; index < _steps.Count; index++) {
                _currentStep = index;
                IExecutionStep step = _steps[index];
                step.ExecuteForward(arguments);
                while (!step.IsFinished) {
                    yield return null;
                }
            }

            IsFinished = true;
        }
    }

    public interface IExecutionStep {
        bool IsFinished { get; }
        
        void ExecuteForward(ExecutionStepArguments arguments);
    }

    public class ExecutionStepArguments {
        private Dictionary<object, object> _arguments;

        public bool GetArgument<T>(ExecutionStepParameter<T> param, out T value) {
            if (_arguments != null && _arguments.TryGetValue(param, out object arg)) {
                value = (T)arg;
                return true;
            }

            value = default;
            return false;
        }

        public void SetArgument<T>(ExecutionStepParameter<T> param, T value) {
            if (_arguments == null) _arguments = new Dictionary<object, object>();
            _arguments[param] = value;
        }
    }
    
    public class ExecutionStepParameter<T> {
        public bool Get(ExecutionStepArguments args, out T value) {
            return args.GetArgument(this, out value);
        }

        public T GetOrDefault(ExecutionStepArguments args, T defaultValue) {
            return Get(args, out T result) ? result : defaultValue;
        }

        public void Set(ExecutionStepArguments args, T value) {
            args.SetArgument(this, value);
        }
    }
}