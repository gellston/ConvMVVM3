namespace ConvMVVM3.Core.Mvvm.Regions
{
    public static class RegionViewRegistryExtensions
    {
        public static void RegisterViewWithRegion<TView>(this IRegionViewRegistry registry, string regionName)
        {
            registry.RegisterViewWithRegion(regionName, typeof(TView));
        }
    }
}
