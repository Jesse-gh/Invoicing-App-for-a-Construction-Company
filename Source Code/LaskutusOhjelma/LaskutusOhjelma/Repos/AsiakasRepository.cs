using LaskutusOhjelma.Data;
using LaskutusOhjelma.Models;
using Microsoft.Data.Sqlite;
using System.Collections.ObjectModel;

namespace LaskutusOhjelma.Repos
{                       // AsiakasRepository-luokka toteuttaa asiakkaiden CRUD-toiminnot eli asiakastietojen hakemisen, lisaamisen,
                        // paivittamisen ja poistamisen tietokannasta.                                
    public class AsiakasRepository
    {                   // HaeKaikkiAsiakkaat-metodi hakee kaikki asiakkaat tietokannasta ja palauttaa ne ObservableCollection-tyyppisenä kokoelmana.
        public ObservableCollection<Asiakas> HaeKaikkiAsiakkaat()
        {
            ObservableCollection<Asiakas> asiakkaat = new ObservableCollection<Asiakas>();

            SqliteConnection yhteys = DatabaseConnector.ConnectDatabase();
            string sqlString = "SELECT asiakasID, nimi, maa, kaupunki, postinumero, katuosoite, puhelinnumero, email, asiakastyyppi FROM Asiakas WHERE IsActive=1 ORDER BY asiakasID;";

            SqliteCommand komento = new SqliteCommand(sqlString, yhteys);
            SqliteDataReader lukija = komento.ExecuteReader();

            while (lukija.Read())
            {
                Asiakas asiakas = new Asiakas();

                asiakas.AsiakasId = lukija.GetInt32(lukija.GetOrdinal("asiakasID"));
                asiakas.Nimi = lukija.GetString(lukija.GetOrdinal("nimi"));
                asiakas.Maa = lukija.GetString(lukija.GetOrdinal("maa"));
                asiakas.Kaupunki = lukija.GetString(lukija.GetOrdinal("kaupunki"));
                asiakas.Postinumero = lukija.GetString(lukija.GetOrdinal("postinumero"));
                asiakas.Katuosoite = lukija.GetString(lukija.GetOrdinal("katuosoite"));
                asiakas.Puhelinnumero = lukija.GetString(lukija.GetOrdinal("puhelinnumero"));
                int ordEmail = lukija.GetOrdinal("email");
                if (lukija.IsDBNull(ordEmail))
                    asiakas.Email = "";
                else
                    asiakas.Email = lukija.GetString(ordEmail);
                asiakas.Asiakastyyppi = lukija.GetString(lukija.GetOrdinal("asiakastyyppi"));

                asiakkaat.Add(asiakas);
            }

            lukija.Close();
            yhteys.Close();

            return asiakkaat;
        }

        // LisaaAsiakas-metodi lisaa uuden asiakkaan tietokantaan ja palauttaa lisatyn asiakkaan ID:n.
        public int LisaaAsiakas(string nimi, string maa, string kaupunki, string postinumero, string katuosoite, string puhelinnumero, string email,
                                string asiakastyyppi)
        {
            SqliteConnection yhteys = DatabaseConnector.ConnectDatabase();

            string sql =
                "INSERT INTO Asiakas " +
                "(nimi, maa, kaupunki, postinumero, katuosoite, puhelinnumero, email, asiakastyyppi) " +
                "VALUES (@nimi, @maa, @kaupunki, @postinumero, @katuosoite, @puhelin, @email, @tyyppi); " +
                "SELECT last_insert_rowid();";

            SqliteCommand komento = new SqliteCommand(sql, yhteys);
            komento.Parameters.AddWithValue("@nimi", nimi);
            komento.Parameters.AddWithValue("@maa", maa);
            komento.Parameters.AddWithValue("@kaupunki", kaupunki);
            komento.Parameters.AddWithValue("@postinumero", postinumero);
            komento.Parameters.AddWithValue("@katuosoite", katuosoite);
            komento.Parameters.AddWithValue("@puhelin", puhelinnumero);
            if (string.IsNullOrWhiteSpace(email))
                komento.Parameters.AddWithValue("@email", DBNull.Value);
            else
                komento.Parameters.AddWithValue("@email", email);
            komento.Parameters.AddWithValue("@tyyppi", asiakastyyppi);

            int uusiId = Convert.ToInt32(komento.ExecuteScalar());
            yhteys.Close();

            return uusiId;
        }

        // PaivitaAsiakas-metodi paivittaa asiakkaan tiedot.
        public bool PaivitaAsiakas(Asiakas a)
        {
            SqliteConnection yhteys = DatabaseConnector.ConnectDatabase();

            string sql = "UPDATE Asiakas " +
                         "SET nimi=@nimi, maa=@maa, kaupunki=@kaupunki, postinumero=@postinumero, " +
                         "katuosoite=@katuosoite, puhelinnumero=@puhelin, email=@email, asiakastyyppi=@tyyppi " +
                         "WHERE asiakasID=@id;";

            SqliteCommand komento = new SqliteCommand(sql, yhteys);
            komento.Parameters.AddWithValue("@nimi", a.Nimi);
            komento.Parameters.AddWithValue("@maa", a.Maa);
            komento.Parameters.AddWithValue("@kaupunki", a.Kaupunki);
            komento.Parameters.AddWithValue("@postinumero", a.Postinumero);
            komento.Parameters.AddWithValue("@katuosoite", a.Katuosoite);
            komento.Parameters.AddWithValue("@puhelin", a.Puhelinnumero);
            if (string.IsNullOrWhiteSpace(a.Email))
                komento.Parameters.AddWithValue("@email", DBNull.Value);
            else
                komento.Parameters.AddWithValue("@email", a.Email);
            komento.Parameters.AddWithValue("@tyyppi", a.Asiakastyyppi);
            komento.Parameters.AddWithValue("@id", a.AsiakasId);

            int rivitLkm = komento.ExecuteNonQuery();
            yhteys.Close();
            bool rivit = rivitLkm > 0;

            return rivit;
        }

        // PoistaAsiakas-metodi toteuttaa "soft delete" -toiminnon eli merkitsee asiakkaan passiiviseksi (IsActive = 0) tietokannassa poistamisen sijaan.
        // Tama on hyodyllinen, koska se sallii asiakastietojen palauttamisen tarvittaessa ja auttaa pitamaan historiatiedot tallessa.
        public bool PoistaAsiakas(int asiakasId)
        {
            SqliteConnection yhteys = DatabaseConnector.ConnectDatabase();

            string sqlString = "UPDATE Asiakas SET IsActive = 0 WHERE asiakasid = @id";
            SqliteCommand komento = new SqliteCommand(sqlString, yhteys);
            komento.Parameters.AddWithValue("@id", asiakasId);

            int rivitLkm = komento.ExecuteNonQuery();
            yhteys.Close();

            bool onnistui = rivitLkm > 0;

            return onnistui;
        }
    }
}