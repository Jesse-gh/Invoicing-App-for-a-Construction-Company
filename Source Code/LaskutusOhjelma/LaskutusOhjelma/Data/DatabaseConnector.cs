using Microsoft.Data.Sqlite;

namespace LaskutusOhjelma.Data
{                                   // DatabaseConnector-luokka, jolla hoidetaan tietokantayhteyden muodostamisen.
    public static class DatabaseConnector
    {
        // ConnectionString on tassa. Tata kaytetaan ensimmaisena ennen kuin tietokantaa on luotu.
        // (Ohjelman SQLite-versiossa kaytetaan samaa yhteysmerkkijonoa molemmissa tapauksissa, mutta pidetaan 2 metodia, jotta muu koodi voi pysya ennallaan.)
        private static readonly string _dbPath = System.IO.Path.Combine(AppContext.BaseDirectory, "LaskutusOhjelma.sqlite");
        private static readonly string connectionStringNoDb = "Data Source=" + "{DBPATH}";

        // Tassa on ConnectionString joka kaytetaan kun tietokanta on luotu. Tata kayetetaan kaikissa muissa yhteyksissa, siis enimmakseen ohjelman kayton aikana.
        private static readonly string connectionString = "Data Source=" + "{DBPATH}";

        // Metodi tietokantayhteyden muodostamiseksi ilman tietokantaa.
        public static SqliteConnection ConnectNoDatabase()
        {
            SqliteConnection connection = new SqliteConnection(connectionStringNoDb.Replace("{DBPATH}", _dbPath));
            connection.Open();

            return connection;
        }

        // Metodi tietokantayhteyden muodostamiseksi tietokantaan.
        public static SqliteConnection ConnectDatabase()
        {
            SqliteConnection connection = new SqliteConnection(connectionString.Replace("{DBPATH}", _dbPath));
            connection.Open();

            using SqliteCommand pragma = connection.CreateCommand();
            pragma.CommandText = "PRAGMA foreign_keys = ON;";
            pragma.ExecuteNonQuery();

            return connection;
        }

        // Palauttaa koko polun tietokantatiedostoon (DatabaseInitializer.cs -luokka tarvitsee tätä).
        public static string GetDatabaseFilePath()
        {
            return _dbPath;
        }
    }
}