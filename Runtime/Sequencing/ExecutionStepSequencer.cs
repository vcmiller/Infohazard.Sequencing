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

using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Infohazard.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Infohazard.Sequencing {
    public class ExecutionStepSequencer : MonoBehaviour {
        [SerializeField] private bool _playOnAwake = true;
        
        private List<IExecutionStep> _steps;
        private int _currentStep;

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

                Execute(new ExecutionStepArguments()).Forget();
            }
        }

        public async UniTask Execute(ExecutionStepArguments arguments) {
            for (int i = 0; i < _steps.Count; i++) {
                _currentStep = i;
                IExecutionStep step = _steps[i];
                await step.Execute(arguments);
            }
        }
    }

    public interface IExecutionStep {
        public UniTask Execute(ExecutionStepArguments arguments);
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