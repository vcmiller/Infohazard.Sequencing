using System;

using UnityEngine;

namespace Infohazard.Sequencing.Runtime {
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