namespace ConvMVVM3.WPF.Locator
{
    public enum ViewModelNamespaceConvention
    {
        /// <summary>
        /// Default convention: replace ".Views." -> ".ViewModels." (Prism-ish)
        /// </summary>
        Default = 0,

        /// <summary>
        /// Replace namespace segment "Views" with "ViewModels".
        /// </summary>
        ViewsToViewModels = 1,

        /// <summary>
        /// Replace namespace segment "Views" with "ViewModel".
        /// </summary>
        ViewsToViewModel = 2,

        /// <summary>
        /// Custom replacement using attached properties (NamespaceReplaceFrom/NamespaceReplaceTo).
        /// </summary>
        Custom = 3
    }
}
