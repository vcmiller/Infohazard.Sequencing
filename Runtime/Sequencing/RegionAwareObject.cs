namespace Infohazard.Sequencing {
    public interface IRegionAwareObject {
        public RegionRoot CurrentRegion { get; set; }
        public bool CanTransitionTo(RegionRoot region);
    }
}