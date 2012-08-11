namespace Pivot.Update.Service
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.c_ServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.c_ServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // c_ServiceProcessInstaller
            // 
            this.c_ServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.c_ServiceProcessInstaller.Password = null;
            this.c_ServiceProcessInstaller.Username = null;
            // 
            // c_ServiceInstaller
            // 
            this.c_ServiceInstaller.Description = "The Pivot update service, which automatically updates applications and games.";
            this.c_ServiceInstaller.DisplayName = "Pivot Update Service";
            this.c_ServiceInstaller.ServiceName = "Pivot Update Service";
            this.c_ServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.c_ServiceProcessInstaller,
            this.c_ServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller c_ServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller c_ServiceInstaller;
    }
}