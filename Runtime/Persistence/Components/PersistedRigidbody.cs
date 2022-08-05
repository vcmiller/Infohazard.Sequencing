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

using System;

using UnityEngine;

namespace Infohazard.Sequencing {
    public class PersistedRigidbody : PersistedComponent<PersistedRigidbody.StateInfo> {
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private bool _saveVelocity = true;
        [SerializeField] private bool _saveAngularVelocity = true;

        public override void LoadState() {
            base.LoadState();
            if (_saveVelocity) _rigidbody.velocity = State.Velocity;
            if (_saveAngularVelocity) _rigidbody.angularVelocity = State.AngularVelocity;
        }

        public override void WriteState() {
            base.WriteState();
            SaveState();
        }

        private void SaveState() {
            if (_saveVelocity) State.Velocity = _rigidbody.velocity;
            if (_saveAngularVelocity) State.AngularVelocity = _rigidbody.angularVelocity;
        }

        private void Reset() {
            _rigidbody = GetComponent<Rigidbody>();
        }

        [Serializable]
        public class StateInfo : PersistedData {
            private Vector3 _velocity;
            private Vector3 _angularVelocity;

            public Vector3 Velocity {
                get => _velocity;
                set {
                    if (_velocity == value) return;
                    _velocity = value;
                    NotifyStateChanged();
                }
            }

            public Vector3 AngularVelocity {
                get => _angularVelocity;
                set {
                    if (_angularVelocity == value) return;
                    _angularVelocity = value;
                    NotifyStateChanged();
                }
            }
        }
    }
}