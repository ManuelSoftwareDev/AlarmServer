using System.ComponentModel;
using System.Configuration.Install;

namespace AlarmServerService
{
	/// <summary>
	/// Summary description for TCPInstaller.
	/// </summary>
	[RunInstaller(true)]
	public class AlarmServerInstaller : Installer
	{
		private System.ServiceProcess.ServiceProcessInstaller AlarmServerServiceProcessInstaller;
		private System.ServiceProcess.ServiceInstaller AlarmServerServiceInstaller;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public AlarmServerInstaller()
		{
			// This call is required by the Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}


		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.AlarmServerServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.AlarmServerServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // AlarmServerServiceProcessInstaller
            // 
            this.AlarmServerServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.AlarmServerServiceProcessInstaller.Password = null;
            this.AlarmServerServiceProcessInstaller.Username = null;
            // 
            // AlarmServerServiceInstaller
            // 
            this.AlarmServerServiceInstaller.Description = "AlarmServer Host Service";
            this.AlarmServerServiceInstaller.DisplayName = "AlarmServer";
            this.AlarmServerServiceInstaller.ServiceName = "AlarmServerService";
            this.AlarmServerServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // TCPInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.AlarmServerServiceProcessInstaller,
            this.AlarmServerServiceInstaller});

		}
		#endregion
	}
}
