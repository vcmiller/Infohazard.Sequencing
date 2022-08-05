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

using UnityEngine;

namespace Infohazard.Sequencing {
    public class PersistedComponentEnabled : PersistedComponent<PersistedComponentEnabled.StateInfo> {
        [SerializeField] private Behaviour _target = null;
        
        public override void LoadState() {
            base.LoadState();
            if (_target) _target.enabled = State.Enabled;
        }

        public override void WriteState() {
            base.WriteState();
            if (_target) State.Enabled = _target.enabled;
        }

        public class StateInfo : PersistedData {
            private bool _enabled;

            public bool Enabled {
                get => _enabled;
                set {
                    if (_enabled == value) return;
                    _enabled = value;
                    NotifyStateChanged();
                }
            }
        }
    }
}