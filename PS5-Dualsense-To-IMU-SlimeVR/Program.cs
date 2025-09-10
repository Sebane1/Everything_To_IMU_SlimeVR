using AutoUpdaterDotNET;
using System.Diagnostics;

namespace Everything_To_IMU_SlimeVR
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.


            bool launchForm = true;
            try
            {
                AutoUpdater.DownloadPath = Application.StartupPath;
                AutoUpdater.Synchronous = true;
                AutoUpdater.Mandatory = true;
                AutoUpdater.UpdateMode = Mode.ForcedDownload;
                AutoUpdater.Start("https://raw.githubusercontent.com/Sebane1/Everything_To_IMU_SlimeVR/main/update.xml");
                AutoUpdater.ApplicationExitEvent += delegate ()
                {
                    launchForm = false;
                };

            } catch
            {

            }

            if (launchForm)
            {
                try
                {
                    ApplicationConfiguration.Initialize();
                    Application.Run(new ConfigurationDisplay());
                } catch (Exception ex)
                {
                    string crashLog = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fatalcrash.log");
                    File.WriteAllText(crashLog, ex.ToString());
                    Process.Start("explorer.exe", crashLog);
                }
            }
        }
    }
}