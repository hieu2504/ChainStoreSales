namespace ChainStoreSalesManagement.Domain.Entities.Views
{
    public class vw_UserEffectiveFeatures
    {
        public string UserId { get; set; } = string.Empty;
        public int FeatureId { get; set; }
        public string FeatureCode { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
    }
}
