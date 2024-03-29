﻿// The MIT License (MIT)
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
    public class LoadStateSaveDataStep : MonoBehaviour, IExecutionStep {
        [SerializeField] private string _defaultStateName;

        public static readonly ExecutionStepParameter<string> ParamStateName = new ExecutionStepParameter<string>();

        public UniTask Execute(ExecutionStepArguments arguments) {
            string stateToLoad = ParamStateName.GetOrDefault(arguments, _defaultStateName);

            bool emptyState = string.IsNullOrEmpty(stateToLoad);
            if (emptyState) {
                PersistenceManager.Instance.LoadEmptyStateData();
            } else {
                PersistenceManager.Instance.LoadStateData(stateToLoad);
            }
            
            // When loading an existing state, no need to auto save.
            // When loading an empty state, we do need to auto save.
            AutoSaveStep.DoAutoSave.Set(arguments, emptyState);

            return UniTask.CompletedTask;
        }
    }
}