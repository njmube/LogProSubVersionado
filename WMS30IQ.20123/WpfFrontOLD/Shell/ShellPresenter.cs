using Microsoft.Practices.Unity;
using WMComposite.Events;
using WMComposite.Modularity;
using WMComposite.Regions;
using WpfFront.Presenters;
using WpfFront.Common;
using WpfFront.Views;
using WpfFront.WMSBusinessService;

namespace WpfFront
{
    public class ShellPresenter : IShellPresenter
    {
        private readonly IUnityContainer container;
        private PopupWindow reportW;
        string curLook = "";
        InternalWindow window;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShellPresenter"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="shell">The shell.</param>
        public ShellPresenter(IUnityContainer container, IShellView shell)
        {
            this.container = container;
            Shell = shell;
            Shell.Model = this.container.Resolve<ShellPresenterModel>();
            Shell.ShowReports += new System.EventHandler<System.EventArgs>(Shell_ShowReports);
            Shell.LoadInquiry += new System.EventHandler<DataEventArgs<string>>(Shell_LoadInquiry);

            //Menu = menu;
            //Menu.Model = this.container.Resolve<MainMenuPresenterModel>();
            //Menu.OpenModule += new EventHandler<DataEventArgs<ModuleRegion>>(this.OnOpenModule);

            window = new InternalWindow();

        }



        void Shell_LoadInquiry(object sender, DataEventArgs<string> e)
        {
            if (string.IsNullOrEmpty(e.Value))
                return;

            if (e.Value == curLook && window != null && window.IsVisible)
                return;


            ClassEntity clsEntity = Shell.CboEntity.SelectedItem as ClassEntity;

            IQueriesPresenter presenter = container.Resolve<IQueriesPresenter>();
            bool result = presenter.LoadShortCut(e.Value, clsEntity.ShortcutColumnID);

            if (result)
            {
                window = Util.GetInternalWindow(Shell.ParentContent.WindowsContainer, "Inquiry Shortcut");
                presenter.Window = window;
                window.GridContent.Children.Add((QueriesView)presenter.View);
                window.Show();
                curLook = e.Value;
                return;
            }

            if (e.Value == curLook)
                return;

            curLook = e.Value;
            Util.ShowMessage("No record found.");
            
        }


        void Shell_ShowReports(object sender, System.EventArgs e)
        {
            IReportPresenter presenter = container.Resolve<IReportPresenter>();

            if (reportW != null)
            {
                try { reportW.Close(); }
                catch { }
            }
            

            reportW = new PopupWindow();
            reportW.Closing += new System.ComponentModel.CancelEventHandler(reportW_Closing);
            reportW.ShowViewInShell(presenter.View, "WMS Express Reports");
            reportW.WindowState = System.Windows.WindowState.Maximized;
            reportW.Show();
        }


        void reportW_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            
        }

        /// <summary>
        /// Gets or sets the view.
        /// </summary>
        /// <value>The view.</value>
        public IShellView Shell { get; set; }

        //public IMainMenuView Menu { get; set; }

        public void ShowMenu()
        {
            //Shell.ShowViewInShell(Menu);
        }

        private void OnOpenModule(object sender, DataEventArgs<ModuleRegion> e)
        {
            //ShellWindow sw = new ShellWindow();
            //sw.DataContext = e.Value;
            //sw.Show();
        }
    }
}
