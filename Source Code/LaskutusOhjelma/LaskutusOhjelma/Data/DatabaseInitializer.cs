using System.Windows;
using Microsoft.Data.Sqlite;

namespace LaskutusOhjelma.Data
{                                 // DatabaseInitializer-luokka, jolla luodaan tietokanta ja sen taulut, kayttaen DDL- ja DML-lauseita, seka lisataan alkudataa.
    public static class DatabaseInitializer
    {
        // Metodi tietokannan alustamiseksi.
        public static void Initialize()
        {
            try
            {
                // Ohjelman SQLite-versiossa "DROP DATABASE" sijaan (ei ole enää MariaDB-tietokantaa) eli poistetaan tiedosto ja luodaan uusi.
                string dbFile = DatabaseConnector.GetDatabaseFilePath();

                if (System.IO.File.Exists(dbFile))
                {
                    try { System.IO.File.Delete(dbFile); } catch {}
                }

                // Luodaan tietokantayhteyden ilman tietokantaa, koska ohjelma muutettiin SQLitea kayttavaksi.
                SqliteConnection connection2 = DatabaseConnector.ConnectDatabase();

                //########## DDL-lauseet #################################################################################################

                SqliteCommand createTuote = connection2.CreateCommand();
                createTuote.CommandText = @"
CREATE TABLE Tuote
(
  tuoteid INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
  nimi TEXT NOT NULL,
  kuvaus TEXT,
  tuoteryhma TEXT NOT NULL,
  hinta NUMERIC(8,2) NOT NULL,
  yksikko TEXT NOT NULL DEFAULT 'kpl',
  IsActive INTEGER NOT NULL DEFAULT 1
);";
                createTuote.ExecuteNonQuery();

                SqliteCommand createAsiakas = connection2.CreateCommand();
                createAsiakas.CommandText = @"
CREATE TABLE Asiakas
(
  asiakasID INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
  nimi TEXT NOT NULL,
  maa TEXT NOT NULL,
  kaupunki TEXT NOT NULL,
  postinumero TEXT NOT NULL,
  katuosoite TEXT NOT NULL,
  puhelinnumero TEXT NOT NULL,
  email TEXT NULL,
  asiakastyyppi TEXT NOT NULL,
  IsActive INTEGER NOT NULL DEFAULT 1
);";
                createAsiakas.ExecuteNonQuery();

                SqliteCommand createLasku = connection2.CreateCommand();
                createLasku.CommandText = @"
CREATE TABLE Lasku
(
  LaskuID INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
  tilausPVM DATE NOT NULL,
  erapaiva DATE NOT NULL,
  maksunTila TEXT NOT NULL,
  tyoTuntihinta NUMERIC(8,2) NOT NULL,
  tyoTunnit INTEGER NOT NULL,
  laskunNumeroAsiakkaalle TEXT NOT NULL,
  lisatiedot TEXT,
  asiakasID INTEGER NOT NULL,
  Poistettu INTEGER NOT NULL DEFAULT 0,
  FOREIGN KEY (asiakasID) REFERENCES Asiakas(asiakasID)
);";
                createLasku.ExecuteNonQuery();

                SqliteCommand createLaskurivi = connection2.CreateCommand();
                createLaskurivi.CommandText = @"
CREATE TABLE Laskurivi
(
  yksikkohinta NUMERIC(8,2) NOT NULL,
  yksikko TEXT NOT NULL,
  maara INTEGER NOT NULL,
  laskuriviID INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
  tuoteid INTEGER NOT NULL,
  LaskuID INTEGER NOT NULL,
  FOREIGN KEY (tuoteid) REFERENCES Tuote(tuoteid),
  FOREIGN KEY (LaskuID) REFERENCES Lasku(LaskuID)
);";
                createLaskurivi.ExecuteNonQuery();

                //########## DML-lauseet #########################################################################################################

                //##################### Tuotteet #######################################################################################################################################################################################################################################################################################################
                new SqliteCommand("INSERT INTO tuote (nimi, kuvaus, tuoteryhma, hinta, yksikko) VALUES ('Maali - Valkoinen', '8 litraa - Sisaltaa metallipurkin (1,1 kg) - Kuivumisaika 1 vrk', 'Pintakasittelyaineet', 64.00, 'purkki(8L)');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO tuote (nimi, kuvaus, tuoteryhma, hinta, yksikko) VALUES ('Savitiili', 'Koko: 285x135x75 mm. - Paino noin 3,2 kg/kpl - Vari: Punaruskea', 'Muuraustarvikkeet', 5.50, 'kpl');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO tuote (nimi, kuvaus, tuoteryhma, hinta, yksikko) VALUES ('Kattopaneeli - Valkoinen', '1 x 1 m - 4,4 kg', 'Pintamateriaalit', 24.50, 'kpl(1x1m)');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO tuote (nimi, kuvaus, tuoteryhma, hinta, yksikko) VALUES ('Maali - Musta', '8 litraa - Sisaltaa metallipurkin (1,1 kg) - Kuivumisaika 1 vrk', 'Pintakasittelyaineet', 64.00, 'purkki(8L)');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO tuote (nimi, kuvaus, tuoteryhma, hinta, yksikko) VALUES ('Kattopaneeli - Harmaa', '1 x 1 m - 4,4 kg', 'Pintamateriaalit', 24.50, 'kpl(1x1m)');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO tuote (nimi, kuvaus, tuoteryhma, hinta, yksikko) VALUES ('Petsilakka', '4,5 L - Sisaltaa metallipurkin (0,8 kg) - Sopii puupintojen suojaukseen - Kuivuu: 36 h', 'Pintakasittelyaineet', 52.00, 'purkki(4.5L)');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO tuote (nimi, kuvaus, tuoteryhma, hinta, yksikko) VALUES ('Puulakka', '3 litraa - Sisaltaa metallipurkin (0,8 kg) - Kuivuu n. 20 tunnissa.', 'Pintakasittelyaineet', 39.00, 'purkki(3L)');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO tuote (nimi, kuvaus, tuoteryhma, hinta, yksikko) VALUES ('Seinatapetti - Valkoinen', 'Paperipohjainen, kukkakuvioinen tapetti - Leveys 53 cm - Rulla(1x10m).', 'Pintamateriaalit', 27.50, 'rulla(1x10m)');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO tuote (nimi, kuvaus, tuoteryhma, hinta, yksikko) VALUES ('Seinatapetti - Harmaa',    'Paperipohjainen tapetti, kukkakuvio - Leveys 53cm - Rulla(1x8m)', 'Pintamateriaalit', 24.50, 'rulla(1x8m)');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO tuote (nimi, kuvaus, tuoteryhma, hinta, yksikko) VALUES ('Poreallas - Ulko', '5-hengen ulkoporeallas - Tilavuus: 1400 L - Paino 300 kg - Kahdeksankulmainen', 'Luksustuotteet', 5490.00, 'kpl');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO tuote (nimi, kuvaus, tuoteryhma, hinta, yksikko) VALUES ('Poreallas - Sisa', 'Kolmen hengen sisaporeallas - Tilavuus 900 litraa - Paino 272 kg - Pyorea muoto', 'Luksustuotteet', 3890.00, 'kpl');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO tuote (nimi, kuvaus, tuoteryhma, hinta, yksikko) VALUES ('Luksussisaovi', 'Korkealaatuinen valkoinen sisaovi - 90 x 210 cm - 72 kg - Materiaali: Tammi', 'Luksustuotteet', 710.00, 'kpl');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO tuote (nimi, kuvaus, tuoteryhma, hinta, yksikko) VALUES ('Viemariputki', 'Metallinen - Pituus: 1,2 m - Halkaisija: 15 cm - Paino 12,5 kg', 'LVI-tarvikkeet', 43.90, 'putki(1.2m)');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO tuote (nimi, kuvaus, tuoteryhma, hinta, yksikko) VALUES ('Keittiohana', 'Moderni kaantyva alumiini keittiohana - Juoksuputken pituus 35 cm - Paino 2,8 kg', 'LVI-tarvikkeet', 128.00, 'kpl');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO tuote (nimi, kuvaus, tuoteryhma, hinta, yksikko) VALUES ('Parkettilaatta - Musta', '50 cm x 50 cm - 1.7 kg', 'Pintamateriaalit', 17.00, 'laatta(50cmx50cm)');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO tuote (nimi, kuvaus, tuoteryhma, hinta, yksikko) VALUES ('Parkettilaatta - Valkoinen', '50 cm x 50 cm - 1.7 kg', 'Pintamateriaalit', 19.00, 'laatta(50cmx50cm)');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO tuote (nimi, kuvaus, tuoteryhma, hinta, yksikko) VALUES ('Tiiliskivi', 'Vari: Tummanpunainen. 250x120x65 mm - 5 kg', 'Muuraustarvikkeet', 8.00, 'kpl');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO tuote (nimi, kuvaus, tuoteryhma, hinta, yksikko) VALUES ('Patteriventtiili', 'Materiaali: Valurauta - Paino 1,2 kg - Suora malli, jossa on kierreliitos.', 'LVI-tarvikkeet', 35.00, 'kpl');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO tuote (nimi, kuvaus, tuoteryhma, hinta, yksikko) VALUES ('Tuplasalpaikkuna', 'Puinen design tuplasalpaikkuna - Koko 120x100 cm - 46 kg - Salpakiinnitys', 'Luksustuotteet', 440.00, 'kpl');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO tuote (nimi, kuvaus, tuoteryhma, hinta, yksikko) VALUES ('Lauta - Pitka', 'Materiaali: Kuusi - 4000 mm × 300 mm × 30 mm', 'Puutavara', 18.00, 'kpl(4m)');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO tuote (nimi, kuvaus, tuoteryhma, hinta, yksikko) VALUES ('Lauta - Lyhyt', 'Materiaali: Kuusi - 2500 mm × 250 mm × 25 mm', 'Puutavara', 11.00, 'kpl(2.5m)');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO tuote (nimi, kuvaus, tuoteryhma, hinta, yksikko) VALUES ('Naula (rasia)', 'Yhdessa rasiassa on 100 terasnaulaa. - Naulan pituus: 8 cm', 'Kiinnitystarvikkeet', 8.00, 'rasia(100kpl)');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO tuote (nimi, kuvaus, tuoteryhma, hinta, yksikko) VALUES ('Ruuvi (rasia)', 'Yhdessa rasiassa on 100 terasruuvia. - Ruuvin pituus: 6 cm', 'Kiinnitystarvikkeet', 11.00, 'rasia(100kpl)');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO tuote (nimi, kuvaus, tuoteryhma, hinta, yksikko) VALUES ('Puuliima', 'Puuliimatuubi -  2 DL. Nopeasti kuivuvaa.', 'Pintakasittelyaineet', 32.00, 'tuubi(2dl)');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO tuote (nimi, kuvaus, tuoteryhma, hinta, yksikko) VALUES ('Terasliima', 'Terasliimatuubi -  2 DL. Kuivumisaika: noin 72 h.', 'Pintakasittelyaineet', 46.50, 'tuubi(2dl)');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO tuote (nimi, kuvaus, tuoteryhma, hinta, yksikko) VALUES ('Sementti (sakki)', 'Kangassakki - 25 kg - Harmaa hienojakoinen jauhe - Sekoitetaan veden kanssa.', 'Muuraustarvikkeet', 33.00, 'sakki(25kg)');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO tuote (nimi, kuvaus, tuoteryhma, hinta, yksikko) VALUES ('Maali - Harmaa', '8 litraa - Sisaltaa metallipurkin (1,1 kg) - Kuivumisaika 1 vrk', 'Pintakasittelyaineet', 64.00, 'purkki(8L)');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO tuote (nimi, kuvaus, tuoteryhma, hinta, yksikko) VALUES ('Maalauspensseli', 'Leveys: 8 cm - Puinen varsi - Soveltuu maali- ja lakkatoihin', 'Maalaustarvikkeet', 8.50, 'kpl');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO tuote (nimi, kuvaus, tuoteryhma, hinta, yksikko) VALUES ('Parkettiliima', 'Purkki - 5L - Kayttotarkoitus: mm. lattialaattojen asennukseen.', 'Pintakasittelyaineet', 31.50, 'purkki(5L)');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO tuote (nimi, kuvaus, tuoteryhma, hinta, yksikko) VALUES ('Tapettiliima', 'Purkki - 5L - Kayttotarkoitus: Tapettien kiinnittamiseen seinaan.', 'Pintakasittelyaineet', 38.00, 'purkki(5L)');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO tuote (nimi, kuvaus, tuoteryhma, hinta, yksikko) VALUES ('Lattiatiiviste', 'Patruuna - 300 ml - Lattian saumojen ja rakenteiden tiivistamiseen - Vari: Harmaa', 'Pintakasittelyaineet', 29.00, 'patruuna(300ml)');", connection2).ExecuteNonQuery();

                //##################### Asiakkaat #####################################################################################################################################################################################################################################################################################################
                new SqliteCommand("INSERT INTO asiakas (nimi, maa, kaupunki, postinumero, katuosoite, puhelinnumero, email, asiakastyyppi) VALUES ('Sylvesteri Tallonen', 'Suomi', 'Rovaniemi', '96300', 'Rovaniementie 1', '050 822 7197', 'sylvesteri.tallonen@gmail.com', 'Yksityisasiakas');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO asiakas (nimi, maa, kaupunki, postinumero, katuosoite, puhelinnumero, email, asiakastyyppi) VALUES ('Larry Marckanen', 'Suomi', 'Jyvaskyla', '40400', 'Koripalloilijankatu 16', '040 372 4572', 'larry.marckanen@yahoo.com', 'Yksityisasiakas');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO asiakas (nimi, maa, kaupunki, postinumero, katuosoite, puhelinnumero, email, asiakastyyppi) VALUES ('Karelia-ammattikorkeakoulu', 'Suomi', 'Joensuu', '80200', 'Karjalankatu 3', '013 260 600', 'info@karelia.fi', 'Organisaatio');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO asiakas (nimi, maa, kaupunki, postinumero, katuosoite, puhelinnumero, email, asiakastyyppi) VALUES ('Nordbygg AB', 'Ruotsi', 'Karlstad', '65224', 'Jarnvagsgatan 10', '054 123 456', 'info@nordbygg.se', 'Yritys');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO asiakas (nimi, maa, kaupunki, postinumero, katuosoite, puhelinnumero, email, asiakastyyppi) VALUES ('Pohjanmaan Kuljetus Oy', 'Suomi', 'Vaasa', '65100', 'Puistokatu 18 C', '06 317 4527', 'tiedot@pohjanmaankuljetus.fi', 'Yritys');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO asiakas (nimi, maa, kaupunki, postinumero, katuosoite, puhelinnumero, email, asiakastyyppi) VALUES ('Helmi Kujala', 'Suomi', 'Turku', '20100', 'Yliopistonkatu 25', '040 556 1122', 'helmi.korhonen@gmail.com', 'Yksityisasiakas');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO asiakas (nimi, maa, kaupunki, postinumero, katuosoite, puhelinnumero, email, asiakastyyppi) VALUES ('Oulun kaupunki - Tilapalvelut', 'Suomi', 'Oulu', '90100', 'Kirkkokatu 2 A', '08 558 410', 'tilapalvelut@ouka.fi', 'Organisaatio');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO asiakas (nimi, maa, kaupunki, postinumero, katuosoite, puhelinnumero, email, asiakastyyppi) VALUES ('Maarja Kask', 'Viro', 'Rapla', '79513', 'Ranna Puiestee 5', '5345 7821', 'maarja.kask@mail.ee', 'Yksityisasiakas');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO asiakas (nimi, maa, kaupunki, postinumero, katuosoite, puhelinnumero, email, asiakastyyppi) VALUES ('Lounaislogistiikka Oy', 'Suomi', 'Espoo', '02100', 'Tapiolantie 20', '09 315 7788', 'info@lounaislogistiikka.fi', 'Yritys');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO asiakas (nimi, maa, kaupunki, postinumero, katuosoite, puhelinnumero, email, asiakastyyppi) VALUES ('Mikko Salminen', 'Suomi', 'Oulu', '90100', 'Vanhankaupunginkatu 3 C 8', '040 731 2244', 'mikko.salminen@gmail.com', 'Yksityiasiakas');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO asiakas (nimi, maa, kaupunki, postinumero, katuosoite, puhelinnumero, email, asiakastyyppi) VALUES ('Laura Niemi', 'Suomi', 'Turku', '20100', 'Linnankatu 8', '050 442 1189', 'laura.niemi@yahoo.com', 'Yksityisasiakas');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO asiakas (nimi, maa, kaupunki, postinumero, katuosoite, puhelinnumero, email, asiakastyyppi) VALUES ('Petri Koski', 'Suomi', 'Joensuu', '80100', 'Kauppakatu 19', '045 210 9988', 'petri.koski@protonmail.com', 'Yksityisasiakas');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO asiakas (nimi, maa, kaupunki, postinumero, katuosoite, puhelinnumero, email, asiakastyyppi) VALUES ('Sara Kokkinen', 'Suomi', 'Kokkola', '67100', 'Rantakatu 6', '041 556 7341', 'sara.kokkinen@gmail.com', 'Yksityisasiakas');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO asiakas (nimi, maa, kaupunki, postinumero, katuosoite, puhelinnumero, email, asiakastyyppi) VALUES ('Supercell Oy', 'Suomi', 'Helsinki', '00180', 'Jatkasaarenlaituri 1', '050 225 5331', 'facilities@supercell.com', 'Yritys');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO asiakas (nimi, maa, kaupunki, postinumero, katuosoite, puhelinnumero, email, asiakastyyppi) VALUES ('Ville Korpi', 'Suomi', 'Pori', '28100', 'Hallituskatu 13', '044 293 5502', 'ville.korpi@hotmail.com', 'Yksityisasiakas');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO asiakas (nimi, maa, kaupunki, postinumero, katuosoite, puhelinnumero, email, asiakastyyppi) VALUES ('Aki Palsamaki', 'Suomi', 'Kangashakki', '41290', 'Tervatehtaantie 93', '050 621 8724', 'huutokauppakeisari@gmail.com', 'Yksityisasiakas');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO asiakas (nimi, maa, kaupunki, postinumero, katuosoite, puhelinnumero, email, asiakastyyppi) VALUES ('Karl Nyman', 'Suomi', 'Helsinki', '22100', 'Porkkalankatu 23 D 4', '050 398 2201', 'karl.nyman@gmail.com', 'Yksityisasiakas');", connection2).ExecuteNonQuery();

                //##################### Laskut ########################################################################################################################################################################################################################################################################################################
                new SqliteCommand("INSERT INTO lasku (asiakasID, laskunNumeroAsiakkaalle, tilausPVM, erapaiva, maksunTila, tyoTuntihinta, tyoTunnit, lisatiedot) VALUES (6, 'LASKU-1001', '2024-01-10', '2024-03-10', 'maksettu', 55.00, 72, 'Takan rakentaminen.');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO lasku (asiakasID, laskunNumeroAsiakkaalle, tilausPVM, erapaiva, maksunTila, tyoTuntihinta, tyoTunnit, lisatiedot) VALUES (3, 'LASKU-1002', '2024-02-15', '2024-04-15', 'maksettu', 52.50, 240, 'Omakotitalon suurremontti ja peruskorjaus.');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO lasku (asiakasID, laskunNumeroAsiakkaalle, tilausPVM, erapaiva, maksunTila, tyoTuntihinta, tyoTunnit, lisatiedot) VALUES (2, 'LASKU-1003', '2024-06-24', '2024-08-24', 'maksettu', 68.00, 16, 'Porealtaiden lisaaminen terassille ja sisatiloihin.');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO lasku (asiakasID, laskunNumeroAsiakkaalle, tilausPVM, erapaiva, maksunTila, tyoTuntihinta, tyoTunnit, lisatiedot) VALUES (7, 'LASKU-1004', '2024-09-25', '2024-11-25', 'ei maksettu', 61.50, 140, 'Vesiputkien asentaminen talon kylpyhuoneen lattian alle.');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO lasku (asiakasID, laskunNumeroAsiakkaalle, tilausPVM, erapaiva, maksunTila, tyoTuntihinta, tyoTunnit, lisatiedot) VALUES (4, 'LASKU-1005', '2024-09-30', '2024-11-30', 'maksettu', 45.50, 16, 'Maalaamista.');", connection2).ExecuteNonQuery();

                new SqliteCommand("INSERT INTO lasku (asiakasID, laskunNumeroAsiakkaalle, tilausPVM, erapaiva, maksunTila, tyoTuntihinta, tyoTunnit, lisatiedot) VALUES (5, 'LASKU-1006', '2025-03-19', '2025-05-19', 'maksettu', 50.50, 60, 'Parketin uusiminen koko asunnon lattiarakenteisiin.');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO lasku (asiakasID, laskunNumeroAsiakkaalle, tilausPVM, erapaiva, maksunTila, tyoTuntihinta, tyoTunnit, lisatiedot) VALUES (1, 'LASKU-1007', '2025-04-19', '2025-06-19', 'ei maksettu', 50.50, 8, 'Luksussisaoven kiinnittaminen toimistohuoneeseen.');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO lasku (asiakasID, laskunNumeroAsiakkaalle, tilausPVM, erapaiva, maksunTila, tyoTuntihinta, tyoTunnit, lisatiedot) VALUES (12, 'LASKU-1008', '2025-04-23', '2025-06-23', 'ei maksettu', 58.00, 36, 'Patteriventtiilien uusiminen kerrostalossa.');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO lasku (asiakasID, laskunNumeroAsiakkaalle, tilausPVM, erapaiva, maksunTila, tyoTuntihinta, tyoTunnit, lisatiedot) VALUES (11, 'LASKU-1009', '2025-04-27', '2025-06-27', 'maksettu', 57.50, 54, 'Kattoon ja seiniin liittyvaa tyota.');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO lasku (asiakasID, laskunNumeroAsiakkaalle, tilausPVM, erapaiva, maksunTila, tyoTuntihinta, tyoTunnit, lisatiedot) VALUES (17, 'LASKU-1010', '2025-04-30', '2025-06-30', 'maksettu', 56.00, 68, 'Valiseinan rakentaminen studiotilaan,huoneen jakaminen kahtia');", connection2).ExecuteNonQuery();

                new SqliteCommand("INSERT INTO lasku (asiakasID, laskunNumeroAsiakkaalle, tilausPVM, erapaiva, maksunTila, tyoTuntihinta, tyoTunnit, lisatiedot) VALUES (10, 'LASKU-1011', '2025-05-13', '2025-07-13', 'maksettu', 52.00, 80, 'Pihamuurin rakentaminen asuinrakennuksen pihalle.');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO lasku (asiakasID, laskunNumeroAsiakkaalle, tilausPVM, erapaiva, maksunTila, tyoTuntihinta, tyoTunnit, lisatiedot) VALUES (16, 'LASKU-1012', '2025-05-15', '2025-07-15', 'ei maksettu', 50.00, 28, 'Porealtaan ja lautojen asennus.');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO lasku (asiakasID, laskunNumeroAsiakkaalle, tilausPVM, erapaiva, maksunTila, tyoTuntihinta, tyoTunnit, lisatiedot) VALUES (8, 'LASKU-1013', '2025-06-22', '2025-08-22', 'maksettu', 48.50, 20, 'Saunan lauteiden uusiminen.');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO lasku (asiakasID, laskunNumeroAsiakkaalle, tilausPVM, erapaiva, maksunTila, tyoTuntihinta, tyoTunnit, lisatiedot) VALUES (13, 'LASKU-1014', '2025-06-20', '2025-08-20', 'maksettu', 55.50, 44, 'Ikkunoiden vaihto.');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO lasku (asiakasID, laskunNumeroAsiakkaalle, tilausPVM, erapaiva, maksunTila, tyoTuntihinta, tyoTunnit, lisatiedot) VALUES (9, 'LASKU-1015', '2025-06-14', '2025-08-14', 'ei maksettu', 46.00, 26, 'Tapetin vaihto ja maalaus.');", connection2).ExecuteNonQuery();

                new SqliteCommand("INSERT INTO lasku (asiakasID, laskunNumeroAsiakkaalle, tilausPVM, erapaiva, maksunTila, tyoTuntihinta, tyoTunnit, lisatiedot) VALUES (14, 'LASKU-1016', '2025-06-17', '2025-08-17', 'maksettu', 54.50, 76, 'Parketin uusiminen.');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO lasku (asiakasID, laskunNumeroAsiakkaalle, tilausPVM, erapaiva, maksunTila, tyoTuntihinta, tyoTunnit, lisatiedot) VALUES (15, 'LASKU-1017', '2025-07-15', '2025-09-15', 'ei maksettu', 64.00, 2, 'Keittiohanan vaihto ja asennus.');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO lasku (asiakasID, laskunNumeroAsiakkaalle, tilausPVM, erapaiva, maksunTila, tyoTuntihinta, tyoTunnit, lisatiedot) VALUES (10, 'LASKU-1018', '2025-07-17', '2025-09-17', 'ei maksettu', 53.00, 94, 'Kattopaneelien asennus.');", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO lasku (asiakasID, laskunNumeroAsiakkaalle, tilausPVM, erapaiva, maksunTila, tyoTuntihinta, tyoTunnit, lisatiedot) VALUES (1, 'LASKU-1019', '2025-07-25', '2025-09-25', 'ei maksettu', 52.00, 120, 'Varaston rakentaminen.');", connection2).ExecuteNonQuery();

                //##################### Laskurivit ####################################################################################################################################################################################################################################################################################################

                //Tietokannassa valmiiksi olemassa olevat laskut:

                //Lasku 1- Takan rakentaminen:
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (1, 3, 5.50, 'kpl', 60);", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (1, 26, 33.00, 'sakki(25kg)', 1);", connection2).ExecuteNonQuery();

                //Lasku 2 - Omakotitalon suurremontti:
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (2, 28, 8.50, 'kpl', 10);", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (2, 17, 8.00, 'kpl', 160);", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (2, 26, 33.00, 'sakki(25kg)', 3);", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (2, 1, 64.00, 'purkki(8L)', 2);", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (2, 20, 18.00, 'kpl(8m)', 50);", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (2, 22, 8.00, 'rasia', 10);", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (2, 24, 32.00, 'tuubi(2DL)', 8);", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (2, 7, 39.00, 'purkki(3L)', 2);", connection2).ExecuteNonQuery();

                //Lasku 3 - Porealtaiden lisaaminen terassille ja sisatiloihin:
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (3, 10, 5490.00, 'kpl', 1);", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (3, 11, 3890.00, 'kpl', 1);", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (3, 26, 33.00, 'sakki(25kg)', 1);", connection2).ExecuteNonQuery();

                //Lasku 4 - Vesiputkien asentaminen talon kylpyhuoneen alle:
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (4, 13, 43.90, 'putki', 14);", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (4, 26, 33.00, 'sakki(25kg)', 1);", connection2).ExecuteNonQuery();

                //Lasku 5 - Maalaamista:
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (5, 28, 8.50, 'kpl', 15);", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (5, 1, 64.00, 'purkki(8L)', 1);", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (5, 2, 64.00, 'purkki(8L)', 1);", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (5, 27, 64.00, 'purkki(8L)', 1);", connection2).ExecuteNonQuery();

                //Lasku 6 - Parketin uusiminen koko asunnon lattiarakenteisiin:
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (6, 16, 19.00, 'laatta(50cmx50cm)', 120);", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (6, 29, 31.50, 'purkki(5L)', 7);", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (6, 31, 29.00, 'patruuna', 12);", connection2).ExecuteNonQuery();

                //Lasku 7 - Luksussisaoven kiinnittaminen toimistohuoneeseen:
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (7, 12, 710.00, 'kpl', 1);", connection2).ExecuteNonQuery();

                //Lasku 8 - Patteriventtiilien uusiminen kerrostalossa:
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (8, 18, 35.00, 'kpl', 140);", connection2).ExecuteNonQuery();

                //Lasku 9 - Kattoon ja seiniin liittyvaa tyota:
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (9, 1, 64.00, 'purkki(8L)', 4);", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (9, 27, 64.00, 'purkki(8L)', 2);", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (9, 28, 8.50, 'kpl', 10);", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (9, 8, 24.50, 'rulla(1x10m)', 6);", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (9, 9, 24.50, 'rulla(1x8m)', 6);", connection2).ExecuteNonQuery();

                //Lasku 10 - Valiseinan rakentaminen studiotilaan:
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (10, 17, 8.00, 'kpl', 140);", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (10, 26, 33.00, 'sakki(25kg)', 3);", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (10, 8, 24.50, 'rulla(1x10m)', 8);", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (10, 30, 38.00, 'purkki(5L)', 3);", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (10, 27, 64.00, 'purkki(8L)', 2);", connection2).ExecuteNonQuery();

                //Lasku 11 - Pihamuurin rakentaminen asuinrakennuksen pihalle:
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (11, 17, 8.00, 'kpl', 240);", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (11, 3, 5.50, 'kpl', 300);", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (11, 26, 33.00, 'sakki(25kg)', 4);", connection2).ExecuteNonQuery();

                //Lasku 12 - Porealtaan ja lautojen asennus:
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (12, 10, 5490.00, 'kpl', 1);", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (12, 20, 18.00, 'kpl(8m)', 4);", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (12, 21, 11.00, 'kpl(2.5m)', 16);", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (12, 7, 39.00, 'purkki(3L)', 1);", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (12, 23, 11.00, 'purkki(3L)', 1);", connection2).ExecuteNonQuery();

                //Lasku 13 - Saunan lauteiden uusiminen:
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (13, 20, 18.00, 'kpl(8m)', 18);", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (13, 21, 11.00, 'kpl(2.5m)', 24);", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (13, 7, 39.00, 'purkki(3L)', 3);", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (13, 22, 8.00, 'rasia', 3);", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (13, 23, 11.00, 'purkki(3L)', 1);", connection2).ExecuteNonQuery();

                //Lasku 14 - Ikkunoiden vaihto:
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (14, 15, 440.00, 'kpl', 4);", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (14, 23, 11.00, 'purkki(3L)', 1);", connection2).ExecuteNonQuery();

                //Lasku 15 - Tapetin vaihto ja maalaus:
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (15, 8, 24.50, 'rulla(1x10m)', 8);", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (15, 1, 64.00, 'purkki(8L)', 3);", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (15, 28, 8.50, 'kpl', 12);", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (15, 30, 38.00, 'purkki(5L)', 3);", connection2).ExecuteNonQuery();

                //Lasku 16 - Parketin uusiminen:
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (16, 15, 17.00, 'laatta(50cmx50cm)', 45);", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (16, 16, 19.00, 'laatta(50cmx50cm)', 45);", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (16, 31, 29.00, 'patruuna', 10);", connection2).ExecuteNonQuery();

                //Lasku 17 - Keittiohanan vaihto ja asennus:
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (17, 14, 128.00, 'kpl', 1);", connection2).ExecuteNonQuery();

                //Lasku 18 - Kattopaneelien asennus:
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (18, 5, 24.50, 'kpl(1x1m)', 60);", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (18, 23, 11.00, 'purkki(3L)', 2);", connection2).ExecuteNonQuery();

                //Lasku 19 - Varaston rakentaminen:
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (19, 20, 18.00, 'kpl(4m)', 25);", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (19, 21, 11.00, 'kpl(2.5m)', 40);", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (19, 17, 8.00, 'kpl', 200);", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (19, 26, 33.00, 'sakki(25kg)', 5);", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (19, 22, 8.00, 'rasia(100kpl)', 4);", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (19, 23, 11.00, 'rasia(100kpl)', 2);", connection2).ExecuteNonQuery();
                new SqliteCommand("INSERT INTO laskurivi (laskuID, tuoteid, yksikkohinta, yksikko, maara) VALUES (19, 24, 32.00, 'tuubi(2dl)', 3);", connection2).ExecuteNonQuery();

                connection2.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Tietokannan alustus epaonnistui:\n" + ex.Message, "Virhe");
            }
        }
    }
}