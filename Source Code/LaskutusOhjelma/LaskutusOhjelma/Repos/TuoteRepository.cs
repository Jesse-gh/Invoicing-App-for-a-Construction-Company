using LaskutusOhjelma.Data;
using LaskutusOhjelma.Models;
using Microsoft.Data.Sqlite;
using System.Collections.ObjectModel;

namespace LaskutusOhjelma.Repos
{                 // TuoteRepository-luokka toteuttaa tuotteiden CRUD-toiminnot eli tuotetietojen hakemisen, lisaamisen, paivittamisen
                  // ja poistamisen (soft delete tavalla) tietokannasta.
    class TuoteRepository
    {             // HaeKaikkiTuotteet-metodi hakee kaikki aktiiviset (kaytossa on soft delete poistamistapa) tuotteet tietokannasta
                  // ja palauttaa ne ObservableCollection<Tuote> -kokoelmana.
        public ObservableCollection<Tuote> HaeKaikkiTuotteet()
        {
            ObservableCollection<Tuote> tuotteet = new ObservableCollection<Tuote>();
            using SqliteConnection yhteys = DatabaseConnector.ConnectDatabase();

            string sqlString = @"
SELECT tuoteid, nimi, kuvaus, tuoteryhma, hinta, yksikko
FROM Tuote
WHERE IsActive = 1
ORDER BY tuoteid;";
            using SqliteCommand komento = new SqliteCommand(sqlString, yhteys);
            using SqliteDataReader lukija = komento.ExecuteReader();

            while (lukija.Read())
            {
                Tuote tuote = new Tuote();

                tuote.TuoteId = lukija.GetInt32(lukija.GetOrdinal("tuoteid"));
                tuote.Nimi = lukija.GetString(lukija.GetOrdinal("nimi"));

                int ordKuvaus = lukija.GetOrdinal("kuvaus");
                if (lukija.IsDBNull(ordKuvaus))
                    tuote.Kuvaus = "";
                else
                    tuote.Kuvaus = lukija.GetString(ordKuvaus);

                tuote.Tuoteryhma = lukija.GetString(lukija.GetOrdinal("tuoteryhma"));
                tuote.Hinta = lukija.GetDecimal(lukija.GetOrdinal("hinta"));

                int ordYksikko = lukija.GetOrdinal("yksikko");
                string yksikko;
                if (lukija.IsDBNull(ordYksikko))
                    yksikko = "kpl";
                else
                    yksikko = lukija.GetString(ordYksikko);

                if (string.IsNullOrWhiteSpace(yksikko))
                    tuote.Yksikko = "kpl";
                else
                    tuote.Yksikko = yksikko;

                tuotteet.Add(tuote);
            }

            return tuotteet;
        }

        // LisaaTuote-metodi lisaa uuden tuotteen tietokantaan ja palauttaa lisatyn tuotteen tuoteid-arvon. Palauttaa 0 jos lisays epaonnistui.
        public int LisaaTuote(string nimi, string kuvaus, string tuoteryhma, decimal hinta, string yksikko)
        {
            using SqliteConnection yhteys = DatabaseConnector.ConnectDatabase();

            string sqlString = @"
        INSERT INTO Tuote (nimi, kuvaus, tuoteryhma, hinta, yksikko)
        VALUES (@nimi, @kuvaus, @tuoteryhma, @hinta, @yksikko);
        SELECT last_insert_rowid();";
            using SqliteCommand komento = new SqliteCommand(sqlString, yhteys);
            komento.Parameters.AddWithValue("@nimi", nimi);
            if (string.IsNullOrWhiteSpace(kuvaus))
                komento.Parameters.AddWithValue("@kuvaus", DBNull.Value);
            else
                komento.Parameters.AddWithValue("@kuvaus", kuvaus);
            komento.Parameters.AddWithValue("@tuoteryhma", tuoteryhma);
            komento.Parameters.AddWithValue("@hinta", hinta);
            komento.Parameters.AddWithValue("@yksikko", string.IsNullOrWhiteSpace(yksikko) ? "kpl" : yksikko);

            object idObj = komento.ExecuteScalar();
            int uusiId = Convert.ToInt32(idObj);
            return uusiId;
        }

        // PaivitaTuote-metodi paivittaa olemassa olevan tuotteen tiedot.
        public bool PaivitaTuote(int tuoteId, string nimi, string kuvaus, string tuoteryhma, decimal hinta, string yksikko)
        {
            using SqliteConnection yhteys = DatabaseConnector.ConnectDatabase();

            string sql = @"
UPDATE Tuote
SET nimi=@nimi, kuvaus=@kuvaus, tuoteryhma=@tuoteryhma, hinta=@hinta, yksikko=@yksikko
WHERE tuoteid=@id AND IsActive=1;";
            using SqliteCommand komento = new SqliteCommand(sql, yhteys);
            komento.Parameters.AddWithValue("@nimi", nimi);
            if (string.IsNullOrWhiteSpace(kuvaus))
                komento.Parameters.AddWithValue("@kuvaus", DBNull.Value);
            else
                komento.Parameters.AddWithValue("@kuvaus", kuvaus);
            komento.Parameters.AddWithValue("@tuoteryhma", tuoteryhma);
            komento.Parameters.AddWithValue("@hinta", hinta);
            komento.Parameters.AddWithValue("@yksikko", string.IsNullOrWhiteSpace(yksikko) ? "kpl" : yksikko);
            komento.Parameters.AddWithValue("@id", tuoteId);

            int rivitLkm = komento.ExecuteNonQuery();
            bool onnistui = rivitLkm > 0;

            return onnistui;
        }

        // PoistaTuote-metodi toteuttaa "soft delete" -toiminnon eli merkitsee tuotteen passiiviseksi (IsActive = 0).
        public bool PoistaTuote(int tuoteId)
        {
            using SqliteConnection yhteys = DatabaseConnector.ConnectDatabase();

            string sqlString = "UPDATE Tuote SET IsActive = 0 WHERE tuoteid = @id";
            using SqliteCommand komento = new SqliteCommand(sqlString, yhteys);
            komento.Parameters.AddWithValue("@id", tuoteId);

            int rivitLkm = komento.ExecuteNonQuery();
            bool onnistui = rivitLkm > 0;

            return onnistui;
        }
    }
}