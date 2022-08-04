using UnityEditor;

namespace Infohazard.Sequencing.Editor {
    public static class SequencingEditorPrefs {
        public static bool AutoLoadScene0InEditor {
            get => EditorPrefs.GetBool(nameof(AutoLoadScene0InEditor), false);
            set => EditorPrefs.SetBool(nameof(AutoLoadScene0InEditor), value);
        }
    }
}