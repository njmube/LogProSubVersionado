using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using WMComposite.Modularity;
using WMComposite.Regions;
using WpfFront.WMSBusinessService;
using System.Collections.Generic;
using Core.BusinessEntity;
using System.Linq;

namespace WpfFront
{
    public class ShellPresenterModel : BusinessEntityBase, IShellPresenterModel
    {
        private ObservableCollection<IModule> modules;

        public Visibility AllowChangeLocation
        {
            get
            {
                return (App.curRol.Rol.IsMultiLocation == true) ?
                    Visibility.Visible : Visibility.Collapsed;
            }
        }

        public ObservableCollection<IModule> Modules
        {
            get
            {
                if (modules == null)
                    modules = new ObservableCollection<IModule>();
                return modules;
            }
        }

        public ObservableCollection<ModuleRegion> UserMenu
        {
            get
            {
               return modules[0].ModuleRegions;
            }
        }

        public Byte[] GetImage(String suri)
        {
            Uri uri = new Uri(suri, UriKind.Relative);
            Stream ms = Application.GetResourceStream(uri).Stream;

            Byte[] image = new Byte[ms.Length];
            ms.Read(image, 0, Convert.ToInt32(ms.Length - 1));
            return image;
        }

        private string _UserInfo;
        public string UserInfo
        {
            get { return _UserInfo; }

            set { 
                _UserInfo = value;
                OnPropertyChanged("UserInfo");
            }
        }

        public Visibility ShowMenu0
        {
            get { return (modules[0].ModuleRegions[0].Options == null || modules[0].ModuleRegions[0].Options.Count == 0) ? Visibility.Collapsed : Visibility.Visible; }
        }

        public Visibility ShowMenu1 //Receiving
        {
            get { return (modules[0].ModuleRegions[1].Options == null || modules[0].ModuleRegions[1].Options.Count == 0) ? Visibility.Collapsed : Visibility.Visible; }
        }

        public Visibility ShowMenu2 //Shipping
        {
            get { return (modules[0].ModuleRegions[2].Options == null || modules[0].ModuleRegions[2].Options.Count == 0) ? Visibility.Collapsed : Visibility.Visible; }
        }

        public Visibility ShowMenu3 //Inventory Basic
        {
            get { return (modules[0].ModuleRegions[3].Options == null || modules[0].ModuleRegions[3].Options.Count == 0) ? Visibility.Collapsed : Visibility.Visible; }
        }

        public Visibility ShowMenu4 //Permission
        {
            get { return (modules[0].ModuleRegions[4].Options == null || modules[0].ModuleRegions[4].Options.Count == 0) ? Visibility.Collapsed : Visibility.Visible; }
        }

        public Visibility ShowMenu5 //Master
        {
            get { return (modules[0].ModuleRegions[5].Options == null || modules[0].ModuleRegions[5].Options.Count == 0) ? Visibility.Collapsed : Visibility.Visible; }
        }

        public Visibility ShowMenu6 //Setup
        {
            get { return (modules[0].ModuleRegions[6].Options == null || modules[0].ModuleRegions[6].Options.Count == 0) ? Visibility.Collapsed : Visibility.Visible; }
        }

        public Visibility ShowMenu7 //Counting
        {
            get { return (modules[0].ModuleRegions[7].Options == null || modules[0].ModuleRegions[7].Options.Count == 0) ? Visibility.Collapsed : Visibility.Visible; }
        }
        public Visibility ShowMenu8 // Administrator
        {
            get { return (modules[0].ModuleRegions[8].Options == null || modules[0].ModuleRegions[8].Options.Count == 0) ? Visibility.Collapsed : Visibility.Visible; }
        }
        public Visibility ShowMenu9 // Consultations
        {
            get { return (modules[0].ModuleRegions[9].Options == null || modules[0].ModuleRegions[9].Options.Count == 0) ? Visibility.Collapsed : Visibility.Visible; }
        }

        public IList<Company> Companies { get { return App.CompanyList;  } }

        public IList<Location> Locations { get { return App.curUser.UserRols.Select(f=>f.Location).Distinct().ToList(); } }

        public IList<ClassEntity> EntityList { get { return App.ClassEntityList
            .Where(f => f.ShortcutColumnID > 0)
            .ToList(); } } 

    }
}
