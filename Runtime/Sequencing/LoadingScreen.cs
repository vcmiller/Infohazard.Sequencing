using System.Collections.Generic;
using System.Linq;
using Infohazard.Core.Runtime;
using TMPro;

using UnityEngine;

namespace Infohazard.Sequencing.Runtime {
    public class LoadingScreen : Singleton<LoadingScreen> {
        [SerializeField] private ProgressBar _progressBar;
        [SerializeField] private TMP_Text _actionText;

        private AsyncOperation _operation;
        private AsyncOperation[] _operations;

        public void SetText(string text) {
            _actionText.text = text;
        }

        public void SetProgress(float progress) {
            _operation = null;
            _operations = null;
            _progressBar.FillAmount = progress;
        }

        public void SetProgressSource(AsyncOperation operation) {
            _operation = operation;
            _operations = null;
            UpdateFromOperation();
        }

        public void SetProgressSource(IEnumerable<AsyncOperation> operations) {
            _operations = operations.ToArray();
            _operation = null;
            UpdateFromOperation();
        }

        private void Update() {
            UpdateFromOperation();
        }

        private void UpdateFromOperation() {
            if (_operation != null) {
                _progressBar.FillAmount = _operation.progress / 0.9f;
            } else if (_operations != null && _operations.Length > 0) {
                _progressBar.FillAmount = _operations.Sum(op => op.progress / 0.9f) / _operations.Length;
            }
        }
    }
}
