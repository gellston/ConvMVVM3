using System.Configuration;
using System.Data;
using System.Windows;

namespace ExampleActivatorUtilities
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        #region Private Property
        private readonly AppBootStrapper bootStrapper = new AppBootStrapper();
        #endregion

        #region Protected Functions

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            bootStrapper.Run(this);
        }
        #endregion
    }

}
