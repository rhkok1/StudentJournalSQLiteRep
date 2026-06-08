using SQLitePCL;
using System.Windows;
using StudentJournalSQLite.Database;
using Microsoft.Data.Sqlite;

namespace StudentJournalSQLite
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Batteries.Init();
            Db.Initialize();
            base.OnStartup(e);
        }
    }
}