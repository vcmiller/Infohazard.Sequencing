using System;
using UnityEngine;

namespace Infohazard.Sequencing {
    public class PersistedTransform : PersistedComponent<PersistedTransform.StateInfo> {
        [SerializeField] private Transform _transform;
        [SerializeField] private bool _savePosition = true;
        [SerializeField] private bool _saveRotation = true;
        [SerializeField] private bool _saveScale = false;
        [SerializeField] private bool _localSpace = false;

        public override void LoadDefaultState() {
            base.LoadDefaultState();
            SaveState();
        }

        public override void LoadState() {
            base.LoadState();
            if (_savePosition) {
                if (_localSpace) _transform.localPosition = State.Position;
                else _transform.position = State.Position;
            }

            if (_saveRotation) {
                if (_localSpace) _transform.localRotation = State.Rotation;
                else _transform.rotation = State.Rotation;
            }
            
            if (_saveScale) transform.localScale = _transform.localScale;
        }

        public override void WriteState() {
            base.WriteState();
            SaveState();
        }

        private void SaveState() {
            if (_savePosition) State.Position = _localSpace ? _transform.localPosition : _transform.position;
            if (_saveRotation) State.Rotation = _localSpace ? _transform.localRotation : _transform.rotation;
            if (_saveScale) State.Scale = _transform.localScale;
        }

        private void Reset() {
            _transform = transform;
        }

        [Serializable]
        public class StateInfo : PersistedData {
            private Vector3 _position;
            private Quaternion _rotation;
            private Vector3 _scale;

            public Vector3 Position {
                get => _position;
                set {
                    if (_position == value) return;
                    _position = value;
                    NotifyStateChanged();
                }
            }

            public Quaternion Rotation {
                get => _rotation;
                set {
                    if (_rotation == value) return;
                    _rotation = value;
                    NotifyStateChanged();
                }
            }

            public Vector3 Scale {
                get => _scale;
                set {
                    if (_scale == value) return;
                    _scale = value;
                    NotifyStateChanged();
                }
            }
        }
    }

}