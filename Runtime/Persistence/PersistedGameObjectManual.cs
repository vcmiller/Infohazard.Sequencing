namespace Infohazard.Sequencing {
    public class PersistedGameObjectManual : PersistedGameObjectBase {
        public override ulong InstanceID => SaveData?.InstanceID ?? 0;

        public void Initialize(ObjectSaveData data) {
            Initializing = true;
            SaveData = data;
            InitializeComponents();
            PostLoad();
        }
    }
}