using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DownloadManager
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {

               ParseCommandLine(args);
                RunIt();
            
        }
      
        private static void RunIt()
        {
            App app = new App();
          //  app.InitializeComponent();
            app.Run();
        }

        private static void ParseCommandLine(string[] args)
        {
            foreach (string directive in args)
                SwitchArguments(directive);
        }

        private static void SwitchArguments(string arg)
        {
            //switch (arg.ToUpper().Trim())
            //{
            //    case "/TRAINING":
            //        Global.mode = MoveServices.ApplicationMode.Training;
            //        break;
            //    case "/TEST":
            //        Global.mode = MoveServices.ApplicationMode.Training;
            //        break;
            //}
        }


        //private static void InitializeApplication(object sender, DoWorkEventArgs e)
        //{
        //    var worker = (BackgroundWorker)sender;
        //    worker.ReportProgress(0, "Registering connections");
        //    Connections.RegisterConnections(Global.mode); //SubSonic requires the connections to be in the app.config
        //    worker.ReportProgress(10, "Checking for database updates...");
        //    var currentUserVersion = GetDatabaseVersion();
        //    if (Global.DatabaseVersion > currentUserVersion)
        //    {
        //        worker.ReportProgress(50, "Updating local database...");
        //        //RunDatabaseUpdater(currentUserVersion, Global.DatabaseVersion);
        //    }
        //    if (Global.DatabaseVersion < currentUserVersion)
        //    {
        //        throw new Exception("Oops! Seems your database version is newer than what I'm designed for. I will close now...");
        //    }
        //    worker.ReportProgress(75, "Loading Reference Data...");
        //    LoadReferenceData();
        //    worker.ReportProgress(95, "Opening eForms...");
        //    Thread.Sleep(1000);
        //}

    }
}
