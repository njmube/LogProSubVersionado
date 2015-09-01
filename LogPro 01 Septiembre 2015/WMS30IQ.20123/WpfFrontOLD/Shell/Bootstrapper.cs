using System;
using System.ComponentModel;
using WMComposite.Modularity;
using WMComposite.Regions;
using WMComposite.Unity;




namespace WpfFront
{
    public class Bootstrapper : UnityBootstrapper
    {
        public Bootstrapper(Boolean page)
            : base(page)
        {
        }

        public Bootstrapper()
            : base()
        {
        }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();
            Container.RegisterType<IShellView, Shell>();
            Container.RegisterType<IShellPresenter, ShellPresenter>();
        }

        protected override void CreateShell()
        {
            IShellPresenter presenter = Container.Resolve<ShellPresenter>();
            Container.RegisterInstance<IShellPresenter>(presenter);

            if (!IsPage)
                presenter.Shell.ShowView();
        }

        protected override IModuleEnumerator ModuleEnumerator
        {
            get
            {
                return new StaticModuleEnumerator().AddModule(typeof(StartModules));
            }
        }
    }
}
