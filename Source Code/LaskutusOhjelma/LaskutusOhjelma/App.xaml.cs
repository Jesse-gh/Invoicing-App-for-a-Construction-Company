using LaskutusOhjelma.Data;
using System.Windows;

namespace LaskutusOhjelma
{               // App-luokka kaynnistaa sovelluksen ja se alustaa tietokannan.
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            DatabaseInitializer.Initialize();
        }
    }
}