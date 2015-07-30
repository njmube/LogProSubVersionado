//Listing - 5, WCFWindowsServiceInstaller.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;
// To install or uninstall this Windows service via cmd:
//
//C:\Program Files\Microsoft Visual Studio 9.0\VC>installutil.exe /u  yourpath/WCFWindowsService.exe
//C:\Program Files\Microsoft Visual Studio 9.0\VC>installutil.exe     yourpath/WCFWindowsService.exe
namespace ServiceConsole
{

    /// <summary>
    /// Summary description for ProjectInstaller.
    /// </summary>
    [RunInstaller(true)]
    public class ProjectInstaller : Installer
    {
        private ServiceProcessInstaller spInstaller;
        private ServiceInstaller srvcInstaller;
        private Container components = null;

        public ProjectInstaller()
        {
            InitializeComponent();
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                if (components != null)
                    components.Dispose();

            base.Dispose(disposing);
        }


        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.spInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.srvcInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // spInstaller
            // 
            this.spInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.spInstaller.Password = null;
            this.spInstaller.Username = null;
            // 
            // srvcInstaller
            // 
            this.srvcInstaller.ServiceName = "WMS 3.0 WCF Service";
            this.srvcInstaller.Description = "Manage the WCF Service (Communication Foundation).";
            this.srvcInstaller.DisplayName = "WMS 3.0 WCF Service";
            this.srvcInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;

            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] 
            { this.spInstaller, this.srvcInstaller });

        }


        protected override void OnCommitted(System.Collections.IDictionary savedState)
        {
            ServiceController sc = new ServiceController("WMS 3.0 WCF Service");
            sc.Start();
        }


        #endregion
    }


}
