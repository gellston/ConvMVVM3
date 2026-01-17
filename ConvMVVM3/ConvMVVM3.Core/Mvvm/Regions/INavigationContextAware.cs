namespace ConvMVVM3.Core.Mvvm.Regions
{
    /// <summary>
    /// Optional navigation lifecycle hooks for view or viewmodel.
    /// </summary>
    public interface INavigationContextAware
    {
        void OnNavigatedTo(INavigationContext context);

        /// <summary>
        /// Called for the previously active view/viewmodel when navigation changes.
        /// </summary>
        void OnNavigatedFrom(INavigationContext context);
    }
}
