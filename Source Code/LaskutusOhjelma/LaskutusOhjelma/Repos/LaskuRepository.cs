using System.Collections.ObjectModel;
using LaskutusOhjelma.Data;
using LaskutusOhjelma.Models;
using Microsoft.Data.Sqlite;

namespace LaskutusOhjelma.Repos
{                         // LaskuRepository-luokka toteuttaa laskutietojen hakemisen ja poistamisen tietokannasta.
    public class LaskuRepository
    {                     // HaeKaikkiLaskut-metodi hakee kaikki aktiiviset (kaytossa on soft delete poistamistapa) laskut tietokannasta
                          // ja palauttaa ne ObservableCollection-tyyppisenä kokoelmana.
        public ObservableCollection<Lasku> HaeKaikkiLaskut()
        {
            ObservableCollection<Lasku> laskut = new ObservableCollection<Lasku>();

            using SqliteConnection yhteys = DatabaseConnector.ConnectDatabase();
            string sqlString = @"
        SELECT
            L.LaskuID,
            L.tilausPVM,
            L.erapaiva,
            L.maksunTila,
            L.tyoTuntihinta,
            L.tyoTunnit,
            L.laskunNumeroAsiakkaalle,
            L.lisatiedot,
            L.asiakasID,
            /* Lasketaan materiaalit vain aktiivisista tuotteista */
            COALESCE(SUM(CASE WHEN T.IsActive = 1 THEN LR.yksikkohinta * LR.maara ELSE 0 END), 0) AS MateriaalitYhteensa
        FROM Lasku L
        LEFT JOIN Laskurivi LR ON LR.LaskuID = L.LaskuID
        LEFT JOIN Tuote T ON T.tuoteid = LR.tuoteid
        WHERE L.Poistettu = 0
        GROUP BY
            L.LaskuID, L.tilausPVM, L.erapaiva, L.maksunTila,
            L.tyoTuntihinta, L.tyoTunnit, L.laskunNumeroAsiakkaalle,
            L.lisatiedot, L.asiakasID
        ORDER BY L.LaskuID;";

            using SqliteCommand komento = new SqliteCommand(sqlString, yhteys);
            using SqliteDataReader lukija = komento.ExecuteReader();

            DateTime today = DateTime.Today;

            while (lukija.Read())
            {
                Lasku lasku = new Lasku();

                lasku.LaskuId = lukija.GetInt32(lukija.GetOrdinal("LaskuID"));

                int ordinalTilaus = lukija.GetOrdinal("tilausPVM");
                DateTime dtTilaus = DateTime.Parse(lukija.GetString(ordinalTilaus));
                if (dtTilaus > today)
                    dtTilaus = today;
                lasku.Tilauspaiva = dtTilaus.ToString("yyyy-MM-dd");

                int ordEra = lukija.GetOrdinal("erapaiva");
                DateTime dtEra = DateTime.Parse(lukija.GetString(ordEra));
                lasku.Erapaiva = dtEra.ToString("yyyy-MM-dd");

                lasku.MaksunTila = lukija.GetString(lukija.GetOrdinal("maksunTila"));
                lasku.Tyotuntihinta = lukija.GetDecimal(lukija.GetOrdinal("tyoTuntihinta"));
                lasku.Tyotunnit = lukija.GetInt32(lukija.GetOrdinal("tyoTunnit"));
                lasku.LaskunNumeroAsiakkaalle = lukija.GetString(lukija.GetOrdinal("laskunNumeroAsiakkaalle"));

                int ordLisat = lukija.GetOrdinal("lisatiedot");
                if (lukija.IsDBNull(ordLisat))
                    lasku.Lisatiedot = "";
                else
                    lasku.Lisatiedot = lukija.GetString(ordLisat);

                lasku.AsiakasId = lukija.GetInt32(lukija.GetOrdinal("asiakasID"));

                int ordMats = lukija.GetOrdinal("MateriaalitYhteensa");
                if (lukija.IsDBNull(ordMats))
                    lasku.MateriaalitYhteensa = 0m;
                else
                    lasku.MateriaalitYhteensa = lukija.GetDecimal(ordMats);

                laskut.Add(lasku);
            }

            return laskut;
        }

        // PoistaLasku-metodi toteuttaa "soft delete" -poiston.
        public bool PoistaLasku(int laskuId)
        {
            using SqliteConnection yhteys = DatabaseConnector.ConnectDatabase();

            string sql = "UPDATE Lasku SET Poistettu = 1 WHERE LaskuID = @id;";
            using SqliteCommand cmd = new SqliteCommand(sql, yhteys);
            cmd.Parameters.AddWithValue("@id", laskuId);

            int rivit = cmd.ExecuteNonQuery();
            return rivit > 0;
        }
    }
}