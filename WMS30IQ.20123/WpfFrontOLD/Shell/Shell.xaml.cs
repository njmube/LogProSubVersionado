using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Assergs.Windows;
using Assergs.Windows.Controls;
using Core.WPF;
using WMComposite.Modularity;
using WMComposite.Regions;
using System.Linq;
using Microsoft.Practices.Unity;
using WMComposite.Events;
using WpfFront.Common;
using WpfFront.Presenters;
using WpfFront.WMSBusinessService;
using WpfFront.Common.UserControls;
using WpfFront.Services;


namespace WpfFront
{
    /// <summary>
    /// Interaction logic for Shell.xaml
    /// </summary>
    /// 

    public partial class Shell : Window, IShellView, IModalService
    {
        private Object oldContent;
        private bool loaded = false;
        public event EventHandler<EventArgs> ShowReports;
        string module = "";

        public Shell()
        {
            InitializeComponent();
            ToolWindow.ModalContainerPanel = this._ModalContainer;
            CommandBinding cmdMaximizeAllWindows = new CommandBinding(
                                                       ToolWindowDockPanel.MaximizeAllWindowsCommand,
                                                       new ExecutedRoutedEventHandler(this.MaximizeAllWindow_Executed),
                                                       new CanExecuteRoutedEventHandler(this.CanExecuteMaximizeCommand));
            this.CommandBindings.Add(cmdMaximizeAllWindows);

            CommandBinding cmdRestoreAllWindows = new CommandBinding(
                                                      ToolWindowDockPanel.RestoreAllWindowsCommand,
                                                      new ExecutedRoutedEventHandler(this.RestoreAllWindow_Executed),
                                                      new CanExecuteRoutedEventHandler(this.CanExecuteRestoreCommand));
            this.CommandBindings.Add(cmdRestoreAllWindows);


        }

        public void ShowView()
        {
            this.Show();
        }

        public void ShowView(Object navigator)
        {
            ((NavigationService)navigator).Navigate(this);
        }

        public void ShowViewInShell(object view)
        {
            Grid grid = new Grid();
            grid.Children.Add((UserControl)view);
            grid.HorizontalAlignment = HorizontalAlignment.Center;
            grid.VerticalAlignment = VerticalAlignment.Center;
            this.Content = grid;
        }

        public void CloseShell()
        {
            //this.Close();
        }

        public IShellPresenterModel Model
        {
            get
            {
                return this.DataContext as IShellPresenterModel;
            }
            set
            {
                this.DataContext = value;
            }
        }



        private void ImageButton_Click(object sender, RoutedEventArgs e)
        {
            ModuleSubmenu ms = ((ImageButton)sender).CommandParameter as ModuleSubmenu;
            SetWindow(ms);
        }

        private void DropDownMenuItem_Click(object sender, RoutedEventArgs e)
        {
            
            ModuleSubmenu ms = ((HeaderedItemsControl)e.OriginalSource).Header as ModuleSubmenu; //((DropDownMenuItem)sender).Items[ ((DropDownMenuItem)sender).Items.CurrentPosition] as ModuleSubmenu;
            SetWindow(ms);          
        }

        public void SetWindow(ModuleSubmenu ms)
        {
            InternalWindow window = new InternalWindow();            
            window.Parent = this.content.WindowsContainer as Panel;
            window.CanResize = true;
            window.ShowStatusBar = false;
            window.Header = ms.Name;
            window.StartPosition = ToolWindowStartPosition.CenterParent;
            window.Height = SystemParameters.FullPrimaryScreenHeight - 150;
            window.Width = SystemParameters.FullPrimaryScreenWidth - 10;
            window.Icon =  new BitmapImage(new Uri(ms.IconPath, UriKind.Relative));
            //window.Width = 800;

            //window.Icon = Util.GetImageSource(ms.Image);            
            window.GridContent.Children.Add((UserControlBase)ms.Module.Execute(ms.PresenterType, window));
            window.Show();
        }

        public void SetWindowExternal(Type curType, string presenterName)
        {
            ModuleSubmenu ms = new ModuleSubmenu { Module = this.Model.Modules[0], 
                PresenterType = curType, Name = presenterName }; 
            SetWindow(ms);
        }

        public void ShowAllContent(bool show)
        {
            if (show)
            {
                if (oldContent != null)
                    this.Content = oldContent;
            }
            else
            {
                oldContent = this.Content;
                this.Content = null;
            }
        }

        private Stack<BackNavigationEventHandler> backFunctions = new Stack<BackNavigationEventHandler>();

        public void NavigateTo(object useCase, BackNavigationEventHandler backFromDialog)
        {
            InternalWindow iw = null;
            foreach (ToolWindow window in this.content.OpenedWindows)
            {
                if (window.IsActive)
                    iw = (InternalWindow)window;
            }
            if (iw != null)
            {
                foreach (UIElement item in iw.GridContent.Children)
                {
                    item.IsEnabled = false;
                }
            }

            iw.GridContent.Children.Add((UserControlBase)useCase);
            backFunctions.Push(backFromDialog);
        }

        public void GoBackward(bool dialogReturnValue)
        {
            InternalWindow iw = null;
            foreach (ToolWindow window in this.content.OpenedWindows)
            {
                if (window.IsActive)
                    iw = (InternalWindow)window;
            }
            if (iw != null)
            {
                iw.GridContent.Children.RemoveAt(iw.GridContent.Children.Count - 1);
            }
            UIElement element = (UIElement)iw.GridContent.Children[iw.GridContent.Children.Count - 1];
            element.IsEnabled = true;

            BackNavigationEventHandler handler = backFunctions.Pop();
            if (handler != null)
                handler(dialogReturnValue);
        }

        private void DropDownHeaderButton_ButtonClick(object sender, RoutedEventArgs e)
        {

        }

        private void DropDownHeaderButton_ButtonClick_1(object sender, RoutedEventArgs e)
        {

        }

        private void WindowsSubmenu_ButtonClick(object sender, RoutedEventArgs e)
        {
            ReflectedButton btn = e.Source as ReflectedButton;

            this.content.ShowWindow(btn.Tag as ToolWindow);

            if (sender is DropDownMenuItem)
            {
                ((DropDownMenuItem)sender).CloseMenuItems();
            }
        }

        private void WindowsSubmenu_Opened(object sender, RoutedEventArgs e)
        {
            if (this.Model != null && this.Model.Modules != null)
            {

                SetWindow(new ModuleSubmenu
                {
                    Name = "Process",
                    Module = this.Model.Modules[0],
                    PresenterType = Type.GetType("WpfFront.Presenters.ProductCategoryPresenter"), 
                    IconPath = "/WpfFront;component/Images/Icons/48x48/Unit.png"
                });
            }            
            ProcessWrapPanel(this._WrapPanel);
        }

        private void WindowsSubmenu_Opened1(object sender, RoutedEventArgs e)
        {

            ProcessWrapPanel(this._WrapPanel1);
        }

        private void WindowsSubmenu_Opened2(object sender, RoutedEventArgs e)
        {

            ProcessWrapPanel(this._WrapPanel2);
        }

        private void WindowsSubmenu_Opened3(object sender, RoutedEventArgs e)
        {

            ProcessWrapPanel(this._WrapPanel3);
        }


        private void WindowsSubmenu_Opened4(object sender, RoutedEventArgs e)
        {

            ProcessWrapPanel(this._WrapPanel4);
        }


        private void WindowsSubmenu_Opened5(object sender, RoutedEventArgs e)
        {

            ProcessWrapPanel(this._WrapPanel5);
        }



        private void ProcessWrapPanel(WrapPanel wrapPanel)
        {
            wrapPanel.Children.Clear();

            foreach (ToolWindow window in this.content.OpenedWindows)
            {
                ReflectedButton btn = new ReflectedButton();

                Image img = new Image();
                img.VerticalAlignment = VerticalAlignment.Top;
                img.Margin = new Thickness(2);
                img.Width = 64;

                BitmapImage iconImage = new BitmapImage();
                iconImage.BeginInit();
                iconImage.UriSource = new Uri("/WpfFront;component/Images/Window128.png", UriKind.Relative);
                iconImage.DecodePixelWidth = 64;
                iconImage.EndInit();

                img.Source = iconImage;

                btn.Content = img;
                btn.Margin = new Thickness(5);
                btn.Text = window.Header.ToString();
                btn.Tag = window;

                Border b = new Border();
                b.Width = 100;
                b.Height = 100;
                b.Background = Brushes.AliceBlue;

                ToolTip t = new ToolTip();

                LinearGradientBrush toolTipBackground = new LinearGradientBrush();
                toolTipBackground.StartPoint = new Point(0, 0);
                toolTipBackground.EndPoint = new Point(0, 1);
                toolTipBackground.GradientStops.Add(new GradientStop(OfficeColors.Background.OfficeColor4, 0));
                toolTipBackground.GradientStops.Add(new GradientStop(OfficeColors.Background.OfficeColor5, 0.5));
                toolTipBackground.GradientStops.Add(new GradientStop(OfficeColors.Background.OfficeColor4, 1));

                t.Background = Brushes.White;
                t.BorderThickness = new Thickness(3);
                t.BorderBrush = toolTipBackground;
                t.Content = new Rectangle();
                t.Padding = new Thickness(4);

                btn.ToolTip = t;
                btn.ToolTipOpening += new ToolTipEventHandler(btn_ToolTipOpening);
                wrapPanel.Children.Add(btn);
            }
        }



        void btn_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            ReflectedButton btn = sender as ReflectedButton;
            ToolWindow window = btn.Tag as ToolWindow;
            ToolTip toolTip = btn.ToolTip as ToolTip;
            Rectangle rect = toolTip.Content as Rectangle;

            VisualBrush vb = new VisualBrush(window);
            vb.Opacity = 0.8;
            vb.AlignmentX = AlignmentX.Center;
            vb.AlignmentY = AlignmentY.Center;
            vb.Stretch = Stretch.Uniform;
            rect.Fill = vb;
            rect.Width = 250;
            rect.Height = window.ActualHeight * (250 / window.ActualWidth);
        }

        private void MaximizeAllWindow_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.content.MaximizeAllWindows();
        }

        private void RestoreAllWindow_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.content.RestoreAllWindows();
        }

        private void CanExecuteMaximizeCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            foreach (ToolWindow window in this.content.OpenedWindows)
            {
                if (window.IsMaximized == false)
                {
                    e.CanExecute = true;
                    this.content.MaximizeAllWindows(); //JM
                    return;
                }
            }

            e.CanExecute = false;
        }

        private void CanExecuteRestoreCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            foreach (ToolWindow window in this.content.OpenedWindows)
            {
                if (window.IsMaximized == true)
                {
                    e.CanExecute = true;
                    return;
                }
            }

            e.CanExecute = false;
        }

        #region IShellView Members


        public Panel ShellPanel()
        {
            return this.content.WindowsContainer as Panel;
        }

        #endregion


        public ToolWindow ShowViewInModalWindow(UserControlBase view, ToolWindow window)
        {
            //throw new NotImplementedException();
            InternalWindow dialog = new InternalWindow();
            dialog.Parent = window.Parent;
            dialog.CanResize = true;
            dialog.ShowStatusBar = false;
            dialog.StartPosition = ToolWindowStartPosition.CenterParent;
            dialog.GridContent.Children.Add(view);
            dialog.ShowModalDialog();
            return dialog;
        }

        public ToolWindow ShowViewInWindow(UserControlBase view)
        {
            //throw new NotImplementedException();
            InternalWindow dialog = new InternalWindow();
            dialog.Parent = ShellPanel();
            dialog.CanResize = true;
            dialog.ShowStatusBar = false;
            dialog.StartPosition = ToolWindowStartPosition.CenterParent;
            dialog.GridContent.Children.Add(view);
            dialog.Show();
            return dialog;
        }


        private void Ribbon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((Ribbon)sender).SelectedItem == null)
                return;

            module = ((RibbonTabItem)((Ribbon)sender).SelectedItem).Name.ToString();
            mRibbon.Height = 140;

            //Check for location y company only First time.
            if (!loaded && cboLocation.SelectedItem == null)
            {
                try
                {
                    for (int i = 0; i <= cboLocation.Items.Count; i++)
                        if (((Location)cboLocation.Items[i]).LocationID == App.curLocation.LocationID)
                        {
                            cboLocation.SelectedIndex = i;
                            break;
                        }

                    for (int i = 0; i <= cboCompany.Items.Count; i++)
                        if (((Company)cboCompany.Items[i]).CompanyID == App.curCompany.CompanyID)
                        {
                            cboCompany.SelectedIndex = i;
                            break;
                        }

                    loaded = true;
                    ((ShellPresenterModel)this.DataContext).UserInfo = "Company: " + App.curCompany.Name                        
                        + " | User: " + App.curUser.UserName
                        + " | Version: " + App.curVersion;

                    cboEntity.SelectedIndex = 0;

                }
                catch { }
            }


            if (mRibbon.SelectedIndex == 0)
                return;


            if (module == "tbExit")
            {
                if (UtilWindow.ConfirmOK("Esta seguro de cerrar LogPro Express?") == true)
                    this.Close();

                return;
            }


            if (module == "tbReports" && !App.showingReports)
            {
                System.Diagnostics.Process.Start("http://sql-general/Reports/Pages/Folder.aspx");
                //App.showingReports = true;
                //ShowReports(sender, e);
                //mRibbon.SelectedIndex = 0;
                return;
            }

            App.showingReports = false;

        }

        private void cboLocation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Cambio del Actual Location
            App.curLocation = this.cboLocation.SelectedItem as Location;
            ((ShellPresenterModel)this.DataContext).UserInfo = "Company: " + App.curCompany.Name                        
                        + " | User: " + App.curUser.UserName
                        + " | Version: " + App.curVersion;

        }



        #region IShellView Members

        public ComboBox CboEntity
        {
            get
            {
                return this.cboEntity;
            }
            set
            {
                this.cboEntity = value;
            }
        }


        public event EventHandler<DataEventArgs<string>> LoadInquiry;



        public ToolWindowDockPanel ParentContent
        {
            get
            {
                return this.content;
            }
            set
            {
                this.content = value;
            }
        }

        #endregion


        private void cboEntity_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems == null)
                return;

            ClassEntity clEntity = e.AddedItems[0] as ClassEntity;

            ucProduct.Visibility = Visibility.Collapsed;
            ucBin.Visibility = Visibility.Collapsed;
            ucProduct.Product = null;
            ucBin.Bin = null;


            switch (clEntity.ClassEntityID)
            {

                case EntityID.Product:
                    ucProduct.Visibility = Visibility.Visible;
                    break;

                case EntityID.Bin:
                    ucBin.Visibility = Visibility.Visible;
                    break;
            }


        }


        private void ucProduct_OnLoadRecord(object sender, EventArgs e)
        {
           LoadInquiry(sender, new DataEventArgs<string>(ucProduct.Product.ProductCode));
        }


        private void ucBin_OnLoadLocation(object sender, EventArgs e)
        {
            LoadInquiry(sender, new DataEventArgs<string>(ucBin.Bin.BinCode));
        }

        private void goNavigateButton_Click(object sender, RoutedEventArgs e)
        {
            Uri uri = new Uri("http://severwms/Reports/Pages/Folder.aspx", UriKind.RelativeOrAbsolute);
            this.myWebBrowser.Navigate(uri);
        }

        private void btnChange_Click(object sender, RoutedEventArgs e)
        {
            //Cambio de Password          

            if (!txtNewPassword.Password.Equals(txtNewPassword2.Password))
            {
                Util.ShowError("Password does not match.");
                return;
            }

            if (txtNewPassword.Password.Length < 4)
            {
                Util.ShowError("Password must be larger than 3 chars.");
                return;
            }


            try
            {
                SysUser updUser = App.curUser;

                // validó el user, ahora intenta cambiar el password
                updUser.Password = Util.CryptPasswd(txtNewPassword.Password, updUser.UserName);
                updUser.ModDate = DateTime.Now;
                (new WMSServiceClient()).UpdateSysUser(updUser);

                Util.ShowMessage("Password  was changed.");
                txtNewPassword.Password = txtNewPassword2.Password = "";

            }
            catch { }

        }


    }

}
