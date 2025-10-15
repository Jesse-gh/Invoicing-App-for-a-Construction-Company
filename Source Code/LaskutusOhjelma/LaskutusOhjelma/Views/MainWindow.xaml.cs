using LaskutusOhjelma.Data;
using LaskutusOhjelma.Models;
using LaskutusOhjelma.Repos;
using LaskutusOhjelma.ViewModels;
using Microsoft.Data.Sqlite;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace LaskutusOhjelma
{                                          //MainWindow-luokka sisaltaa paanakyman logiikan ja tapahtumien kasittelyn.
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();

            if (dpTilauspaiva != null)                   // Tilauspaiva, joka ei salli, etta kayttaja valitsee paivayksen tulevaisuudesta.
            {
                dpTilauspaiva.DisplayDateEnd = DateTime.Today;
                dpTilauspaiva.BlackoutDates.Clear();
                dpTilauspaiva.BlackoutDates.Add(
                    new CalendarDateRange(DateTime.Today.AddDays(1), DateTime.MaxValue));
            }
        }

        // Lisaa uuden tuotteen tietokantaan ja paivittaa kayttoliittyman nakyman.
        private void BtnLisaaTuote_Click(object sender, RoutedEventArgs e)
        {
            string nimi = tbNimi.Text;
            string kuvaus = tbKuvaus.Text;
            string tuoteryhma = tbTuoteryhma.Text;
            string yksikko = tbYksikko.Text.Trim();

            if (string.IsNullOrWhiteSpace(nimi))
            {
                MessageBox.Show("Anna tuotteen nimi.");
                return;
            }

            if (string.IsNullOrWhiteSpace(tuoteryhma))
            {
                MessageBox.Show("Anna tuotteen tuoteryhma.");
                return;
            }

            if (string.IsNullOrWhiteSpace(yksikko))
            {
                MessageBox.Show("Anna tuotteen yksikko.");
                return;
            }

            decimal hinta;
            string hintaText = tbHinta.Text.Trim().Replace('.', ',');
            if (!decimal.TryParse(hintaText, out hinta) || hinta <= 0)
            {
                MessageBox.Show("Anna tuotteen hinta. Tuotteen hinnan pitaa olla positiivinen luku.");
                return;
            }

            MainViewModel vm = DataContext as MainViewModel;
            if (vm == null) { MessageBox.Show("DataContext puuttuu."); return; }

            bool onDuplikaatti = vm.Tuotteet.Any(t =>
                string.Equals(t.Nimi, nimi, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(t.Kuvaus, kuvaus, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(t.Tuoteryhma, tuoteryhma, StringComparison.OrdinalIgnoreCase));

            if (onDuplikaatti)
            {
                MessageBox.Show("Tuotteen lisays estetty. Tuote samoilla tiedoilla on jo olemassa.");
                return;
            }

            TuoteRepository repo = new TuoteRepository();
            int uusiId = repo.LisaaTuote(nimi, kuvaus, tuoteryhma, hinta, yksikko);
            if (uusiId <= 0) { MessageBox.Show("Lisays epaonnistui."); return; }

            vm.Tuotteet.Add(new Tuote
            {
                TuoteId = uusiId,
                Nimi = nimi,
                Kuvaus = kuvaus,
                Tuoteryhma = tuoteryhma,
                Hinta = hinta,
                Yksikko = yksikko
            });

            tbNimi.Text = "";
            tbKuvaus.Text = "";
            tbTuoteryhma.Text = "";
            tbHinta.Text = "";
            tbYksikko.Text = "";
            xnameTuotteet.SelectedItem = null;

            MessageBox.Show("Tuote lisatty.");
        }

        // Paivittaa valitun tuotteen tiedot tietokannassa ja kayttoliittymassa.
        private void BtnPaivitaTuote_Click(object sender, RoutedEventArgs e)
        {
            Tuote valittu = xnameTuotteet.SelectedItem as Tuote;
            if (valittu == null) { MessageBox.Show("Valitse ensin muokattava tuote."); return; }

            string nimi = tbNimi.Text;
            string kuvaus = tbKuvaus.Text;
            string tuoteryhma = tbTuoteryhma.Text;

            string yksikko = tbYksikko.Text.Trim();
            if (string.IsNullOrWhiteSpace(yksikko))
            {
                MessageBox.Show("Anna tuotteen yksikko.");
                return;
            }


            if (string.IsNullOrWhiteSpace(nimi) || string.IsNullOrWhiteSpace(tuoteryhma))
            {
                MessageBox.Show("Anna tuotteen nimi ja tuoteryhma.");
                return;
            }

            decimal hinta;
            string hintaText = tbHinta.Text.Trim().Replace('.', ',');
            if (!decimal.TryParse(hintaText, out hinta) || hinta <= 0)
            {
                MessageBox.Show("Anna tuotteen hinta. Tuotteen hinnan pitaa olla positiivinen luku.");
                return;
            }

            MainViewModel vm = DataContext as MainViewModel;
            if (vm == null) { MessageBox.Show("DataContext puuttuu."); return; }

            bool Eq(string a, string b)
            {
                string sa = a;
                if (sa == null)
                    sa = "";
                else
                    sa = sa.Trim();

                string sb = b;
                if (sb == null)
                    sb = "";
                else
                    sb = sb.Trim();

                return string.Equals(sa, sb, StringComparison.OrdinalIgnoreCase);
            }

            foreach (Tuote t in vm.Tuotteet)
            {
                if (t.TuoteId == valittu.TuoteId) continue;
                if (Eq(t.Nimi, nimi) && Eq(t.Kuvaus, kuvaus) && Eq(t.Tuoteryhma, tuoteryhma))
                {
                    MessageBox.Show("Tuotteen paivitys estetty. Tuote samoilla tiedoilla on jo olemassa.");
                    return;
                }
            }

            TuoteRepository repo = new TuoteRepository();
            bool ok = repo.PaivitaTuote(valittu.TuoteId, nimi, kuvaus, tuoteryhma, hinta, yksikko);
            if (!ok) { MessageBox.Show("Paivitys epaonnistui."); return; }

            valittu.Nimi = nimi;
            valittu.Kuvaus = kuvaus;
            valittu.Tuoteryhma = tuoteryhma;
            valittu.Hinta = hinta;
            valittu.Yksikko = yksikko;
            xnameTuotteet.Items.Refresh();

            MessageBox.Show("Tuote paivitetty.");
        }

        // Poistaa valitun tuotteen soft delete tavalla kayttoliittyman nakymasta.
        private void BtnPoistaTuote_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel vm = DataContext as MainViewModel;
            if (vm == null) return;

            Tuote valittu = xnameTuotteet.SelectedItem as Tuote;
            if (valittu == null)
            {
                MessageBox.Show("Valitse ensin poistettava tuote.");
                return;
            }

            MessageBoxResult tulos = MessageBox.Show(
                "Haluatko varmasti poistaa tuotteen '" + valittu.Nimi + "'?",
                "Vahvista tuotteen poisto", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (tulos != MessageBoxResult.Yes) return;

            TuoteRepository repo = new TuoteRepository();
            bool onnistui = repo.PoistaTuote(valittu.TuoteId);

            if (onnistui)
            {
                vm.Tuotteet.Remove(valittu);
                MessageBox.Show("Tuote poistettu.");
            }
            else
            {
                MessageBox.Show("Tuotteen poisto epaonnistui.");
            }
        }

        // Jos kayttaja on syottanyt tietoa Tuotetiedot-valilehden kenttiin, tama nappi tyhjentaa kentat.
        private void BtnTyhjennaKentat_Click(object sender, RoutedEventArgs e)
        {
            tbNimi.Text = "";
            tbKuvaus.Text = "";
            tbTuoteryhma.Text = "";
            tbHinta.Text = "";
            tbYksikko.Text = "";
            xnameTuotteet.SelectedItem = null;
        }

        // Tuotetiedot-valilehden kentat nayttavat sen DataGridin rivin tiedot, jonka kayttaja on valinnut.
        private void xnameTuotteet_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Tuote valittu = xnameTuotteet.SelectedItem as Tuote;
            if (valittu == null)
            {
                tbNimi.Text = "";
                tbKuvaus.Text = "";
                tbTuoteryhma.Text = "";
                tbHinta.Text = "";
                tbYksikko.Text = "";
                return;
            }

            if (valittu.Nimi == null)
                tbNimi.Text = "";
            else
                tbNimi.Text = valittu.Nimi;

            if (valittu.Kuvaus == null)
                tbKuvaus.Text = "";
            else
                tbKuvaus.Text = valittu.Kuvaus;

            if (valittu.Tuoteryhma == null)
                tbTuoteryhma.Text = "";
            else
                tbTuoteryhma.Text = valittu.Tuoteryhma;

            tbHinta.Text = valittu.Hinta.ToString("0.##");

            if (string.IsNullOrWhiteSpace(valittu.Yksikko))
                tbYksikko.Text = "kpl";
            else
                tbYksikko.Text = valittu.Yksikko;
        }

        // Lisaa uuden asiakkaan tietokantaan ja paivittaa kayttoliittyman nakyman.
        private void BtnLisaaAsiakas_Click(object sender, RoutedEventArgs e)
        {
            string nimi = tbAsNimi.Text;
            string maa = tbAsMaa.Text;
            string kaupunki = tbAsKaupunki.Text;
            string postinumero = tbAsPostinumero.Text;
            string katuosoite = tbAsKatuosoite.Text;
            string puhelin = tbAsPuhelin.Text;
            string email = tbAsEmail.Text;
            string tyyppi = tbAsTyyppi.Text;

            if (string.IsNullOrWhiteSpace(nimi) ||
                string.IsNullOrWhiteSpace(maa) ||
                string.IsNullOrWhiteSpace(kaupunki) ||
                string.IsNullOrWhiteSpace(postinumero) ||
                string.IsNullOrWhiteSpace(katuosoite) ||
                string.IsNullOrWhiteSpace(puhelin) ||
                string.IsNullOrWhiteSpace(tyyppi))
            {
                MessageBox.Show("Kaikki kentat paitsi Email on taytettava ennen asiakkaan lisaamista.");

                return;
            }

            string emailOrNull;
            if (string.IsNullOrWhiteSpace(email))
                emailOrNull = null;
            else
                emailOrNull = email;

            MainViewModel vm = DataContext as MainViewModel;   // Estaa tuotteen, jolla on taysin identtiset tiedot. Yhden merkin ero kuitenkin jo sallitaan.
            if (vm == null)
            {
                MessageBox.Show("DataContext puuttuu.");
                return;
            }

            bool Eq(string a, string b)
            {
                string sa;
                if (a == null)
                    sa = "";
                else
                    sa = a.Trim();

                string sb;
                if (b == null)
                    sb = "";
                else
                    sb = b.Trim();

                return string.Equals(sa, sb, StringComparison.OrdinalIgnoreCase);
            }

            bool onDuplikaatti = false;
            foreach (Asiakas a in vm.Asiakkaat)
            {
                if (Eq(a.Nimi, nimi) &&
                    Eq(a.Maa, maa) &&
                    Eq(a.Kaupunki, kaupunki) &&
                    Eq(a.Postinumero, postinumero) &&
                    Eq(a.Katuosoite, katuosoite) &&
                    Eq(a.Puhelinnumero, puhelin) &&
                    Eq(a.Email, emailOrNull) &&
                    Eq(a.Asiakastyyppi, tyyppi))
                {
                    onDuplikaatti = true;
                    break;
                }
            }

            if (onDuplikaatti)
            {
                MessageBox.Show("Asiakkaan lisays estetty. Asiakas tasmalleen samoilla tiedoilla on jo olemassa.");
                return;
            }

            AsiakasRepository repo = new AsiakasRepository();
            int uusiId = repo.LisaaAsiakas(nimi, maa, kaupunki, postinumero, katuosoite,
                                           puhelin, emailOrNull, tyyppi);
            if (uusiId <= 0)
            {
                MessageBox.Show("Lisays tietokantaan epaonnistui.");
                return;
            }

            Asiakas uusi = new Asiakas
            {
                AsiakasId = uusiId,
                Nimi = nimi,
                Maa = maa,
                Kaupunki = kaupunki,
                Postinumero = postinumero,
                Katuosoite = katuosoite,
                Puhelinnumero = puhelin,
                Email = email,
                Asiakastyyppi = tyyppi
            };
            vm.Asiakkaat.Add(uusi);

            tbAsNimi.Text = "";
            tbAsMaa.Text = "";
            tbAsKaupunki.Text = "";
            tbAsPostinumero.Text = "";
            tbAsKatuosoite.Text = "";
            tbAsPuhelin.Text = "";
            tbAsEmail.Text = "";
            tbAsTyyppi.Text = "";

            MessageBox.Show("Asiakas lisatty.");
        }

        // Asiakastiedot-valilehden kentat nayttavat sen DataGridin rivin tiedot, jonka kayttaja on valinnut.
        private void xnameAsiakkaat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Asiakas valittu = xnameAsiakkaat.SelectedItem as Asiakas;

            if (valittu == null)
            {
                tbAsNimi.Text = "";
                tbAsMaa.Text = "";
                tbAsKaupunki.Text = "";
                tbAsPostinumero.Text = "";
                tbAsKatuosoite.Text = "";
                tbAsPuhelin.Text = "";
                tbAsEmail.Text = "";
                tbAsTyyppi.Text = "";
                return;
            }

            if (valittu.Nimi == null)
                tbAsNimi.Text = "";
            else
                tbAsNimi.Text = valittu.Nimi;

            if (valittu.Maa == null)
                tbAsMaa.Text = "";
            else
                tbAsMaa.Text = valittu.Maa;

            if (valittu.Kaupunki == null)
                tbAsKaupunki.Text = "";
            else
                tbAsKaupunki.Text = valittu.Kaupunki;

            if (valittu.Postinumero == null)
                tbAsPostinumero.Text = "";
            else
                tbAsPostinumero.Text = valittu.Postinumero;

            if (valittu.Katuosoite == null)
                tbAsKatuosoite.Text = "";
            else
                tbAsKatuosoite.Text = valittu.Katuosoite;

            if (valittu.Puhelinnumero == null)
                tbAsPuhelin.Text = "";
            else
                tbAsPuhelin.Text = valittu.Puhelinnumero;

            if (valittu.Email == null)
                tbAsEmail.Text = "";
            else
                tbAsEmail.Text = valittu.Email;

            if (valittu.Asiakastyyppi == null)
                tbAsTyyppi.Text = "";
            else
                tbAsTyyppi.Text = valittu.Asiakastyyppi;
        }

        // Jos kayttaja on syottanyt tietoa Asiakastiedot-valilehden kenttiin, tama nappi tyhjentaa kentat.
        private void BtnTyhjennaAsiakasKentat_Click(object sender, RoutedEventArgs e)
        {
            if (xnameAsiakkaat != null)
            {
                xnameAsiakkaat.SelectedItem = null;
            }

            tbAsNimi.Text = "";
            tbAsMaa.Text = "";
            tbAsKaupunki.Text = "";
            tbAsPostinumero.Text = "";
            tbAsKatuosoite.Text = "";
            tbAsPuhelin.Text = "";
            tbAsEmail.Text = "";
            tbAsTyyppi.Text = "";
        }

        // Paivittaa valitun asiakkaan tiedot tietokannassa ja kayttoliittyman nakymassa.
        private void BtnPaivitaAsiakas_Click(object sender, RoutedEventArgs e)
        {
            Asiakas valittu = xnameAsiakkaat.SelectedItem as Asiakas;
            if (valittu == null)
            {
                MessageBox.Show("Valitse ensin asiakas listasta.");
                return;
            }

            string nimi = tbAsNimi.Text;
            string maa = tbAsMaa.Text;
            string kaupunki = tbAsKaupunki.Text;
            string postinumero = tbAsPostinumero.Text;
            string katuosoite = tbAsKatuosoite.Text;
            string puhelin = tbAsPuhelin.Text;
            string email = tbAsEmail.Text;
            string tyyppi = tbAsTyyppi.Text;

            // Tassa pakotetaan tayttamaan kaikki kentat paitsi Email.
            if (string.IsNullOrWhiteSpace(nimi) ||
                string.IsNullOrWhiteSpace(maa) ||
                string.IsNullOrWhiteSpace(kaupunki) ||
                string.IsNullOrWhiteSpace(postinumero) ||
                string.IsNullOrWhiteSpace(katuosoite) ||
                string.IsNullOrWhiteSpace(puhelin) ||
                string.IsNullOrWhiteSpace(tyyppi))
            {
                MessageBox.Show("Kaikki kentat paitsi Email on taytettava ennen asiakkaan tietojen paivittamista.");
                return;
            }

            string emailOrNull;
            if (string.IsNullOrWhiteSpace(email))
            {
                emailOrNull = null;
            }
            else
            {
                emailOrNull = email;
            }

            MainViewModel vm = DataContext as MainViewModel;
            if (vm == null)
            {
                MessageBox.Show("DataContext puuttuu.");
                return;
            }

            bool Eq(string a, string b)
            {
                string sa;
                if (a == null)
                    sa = "";
                else
                    sa = a.Trim();

                string sb;
                if (b == null)
                    sb = "";
                else
                    sb = b.Trim();

                return string.Equals(sa, sb, StringComparison.OrdinalIgnoreCase);
            }


            bool onDuplikaatti = false;
            foreach (Asiakas a in vm.Asiakkaat)
            {
                if (a.AsiakasId == valittu.AsiakasId) continue;

                if (Eq(a.Nimi, nimi) &&
                    Eq(a.Maa, maa) &&
                    Eq(a.Kaupunki, kaupunki) &&
                    Eq(a.Postinumero, postinumero) &&
                    Eq(a.Katuosoite, katuosoite) &&
                    Eq(a.Puhelinnumero, puhelin) &&
                    Eq(a.Email, emailOrNull) &&
                    Eq(a.Asiakastyyppi, tyyppi))
                {
                    onDuplikaatti = true;
                    break;
                }
            }

            if (onDuplikaatti)
            {
                MessageBox.Show("Asiakkaan paivitys estetty. Asiakas tasmalleen samoilla tiedoilla on jo olemassa.");
                return;
            }

            Asiakas paivitettava = new Asiakas
            {
                AsiakasId = valittu.AsiakasId,
                Nimi = nimi,
                Maa = maa,
                Kaupunki = kaupunki,
                Postinumero = postinumero,
                Katuosoite = katuosoite,
                Puhelinnumero = puhelin,
                Email = emailOrNull,
                Asiakastyyppi = tyyppi
            };

            AsiakasRepository repo = new AsiakasRepository();
            bool ok = repo.PaivitaAsiakas(paivitettava);
            if (!ok)
            {
                MessageBox.Show("Paivitys epaonnistui.");
                return;
            }

            valittu.Nimi = paivitettava.Nimi;
            valittu.Maa = paivitettava.Maa;
            valittu.Kaupunki = paivitettava.Kaupunki;
            valittu.Postinumero = paivitettava.Postinumero;
            valittu.Katuosoite = paivitettava.Katuosoite;
            valittu.Puhelinnumero = paivitettava.Puhelinnumero;
            valittu.Email = paivitettava.Email;
            valittu.Asiakastyyppi = paivitettava.Asiakastyyppi;

            xnameAsiakkaat.Items.Refresh();

            MessageBox.Show("Asiakas paivitetty.");
        }

        // Poistaa valitun asiakkaan soft delete tavalla kayttoliittyman nakymasta.
        private void BtnPoistaAsiakas_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel vm = DataContext as MainViewModel;
            if (vm == null) return;

            Asiakas valittu = xnameAsiakkaat.SelectedItem as Asiakas;
            if (valittu == null)
            {
                MessageBox.Show("Valitse ensin asiakas listasta.");
                return;
            }

            MessageBoxResult tulos = MessageBox.Show(
                $"Haluatko varmasti poistaa asiakkaan '{valittu.Nimi}'?",
                "Vahvista asiakkaan poisto",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (tulos != MessageBoxResult.Yes)
                return;

            AsiakasRepository repo = new AsiakasRepository();
            bool ok = repo.PoistaAsiakas(valittu.AsiakasId);

            if (ok)
            {
                vm.Asiakkaat.Remove(valittu);
                MessageBox.Show("Asiakas poistettu.");
            }
            else
            {
                MessageBox.Show("Asiakkaan poisto epaonnistui.");
            }
        }

        // Alapuolella oleva metodi mahdollistaa sen, etta kayttaja voi syottaa numeroiden desimaaliksi pilkun tai pisteen.
        private static bool TryParseDecimalFlexible(string input, out decimal value)
        {
            value = 0m;
            if (string.IsNullOrWhiteSpace(input)) return false;

            input = input.Trim().Replace('.', ',');

            CultureInfo fi = new CultureInfo("fi-FI");
            return decimal.TryParse(input, NumberStyles.Number, fi, out value);
        }

        // Alapuolella oleva rivi ja sen alapuolella oleva metodi sallii vain numeroiden ja desimaalimerkkien syottamisen tekstikenttaan.
        private static readonly Regex _allowed = new Regex(@"^[0-9,.\s]+$");

        private void NumberOnly_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !_allowed.IsMatch(e.Text);
        }

        // Estaa virheellisten merkkien kirjoittamisen tekstikenttaan.
        private void NumberOnly_Paste(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(DataFormats.Text))
            {
                string text = (string)e.DataObject.GetData(DataFormats.Text);
                if (!_allowed.IsMatch(text)) e.CancelCommand();
            }
            else
            {
                e.CancelCommand();
            }
        }

        // Laskee jokaisen materiaalirivin hinnan (hinta × maara) ja paivittaa laskun kokonaissumman.
        private void MaterialInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            void CalcRow(TextBox tbPrice, TextBox tbQty, TextBox tbSum)
            {
                if (tbPrice == null || tbQty == null || tbSum == null) return;
                CultureInfo fi = new CultureInfo("fi-FI");

                decimal price;
                decimal qty;

                if (TryParseDecimalFlexible(tbPrice.Text, out price) &&
                    TryParseDecimalFlexible(tbQty.Text, out qty) &&
                                    price >= 0 && qty >= 0)
                {
                    tbSum.Text = (price * qty).ToString("N2", fi);
                }
                else
                {
                    tbSum.Text = "";
                }
            }

            CalcRow(tbMat1Hinta, tbMat1Maara, tbMat1Summa);
            CalcRow(tbMat2Hinta, tbMat2Maara, tbMat2Summa);
            CalcRow(tbMat3Hinta, tbMat3Maara, tbMat3Summa);
            CalcRow(tbMat4Hinta, tbMat4Maara, tbMat4Summa);
            CalcRow(tbMat5Hinta, tbMat5Maara, tbMat5Summa);
            CalcRow(tbMat6Hinta, tbMat6Maara, tbMat6Summa);
            CalcRow(tbMat7Hinta, tbMat7Maara, tbMat7Summa);
            CalcRow(tbMat8Hinta, tbMat8Maara, tbMat8Summa);

            UpdateLaskunYhteensa();
        }

        // Laskee ja tarvittaessa paivittaa laskun lopullisen hinnan (tyon hinta + materiaalit yhteensa).
        private void UpdateLaskunYhteensa()
        {
            if ((tbTyonHinta == null || string.IsNullOrWhiteSpace(tbTyonHinta.Text)) &&
                (tbMat1Summa == null || string.IsNullOrWhiteSpace(tbMat1Summa.Text)) &&
                (tbMat2Summa == null || string.IsNullOrWhiteSpace(tbMat2Summa.Text)) &&
                (tbMat3Summa == null || string.IsNullOrWhiteSpace(tbMat3Summa.Text)) &&
                (tbMat4Summa == null || string.IsNullOrWhiteSpace(tbMat4Summa.Text)) &&
                (tbMat5Summa == null || string.IsNullOrWhiteSpace(tbMat5Summa.Text)) &&
                (tbMat6Summa == null || string.IsNullOrWhiteSpace(tbMat6Summa.Text)) &&
                (tbMat7Summa == null || string.IsNullOrWhiteSpace(tbMat7Summa.Text)) &&
                (tbMat8Summa == null || string.IsNullOrWhiteSpace(tbMat8Summa.Text)))
            {
                if (tbLaskunYhteensa != null) tbLaskunYhteensa.Text = string.Empty;
                return;
            }

            CultureInfo fi = new CultureInfo("fi-FI");

            decimal Sum(params TextBox[] tbs)
            {
                decimal s = 0m;
                for (int i = 0; i < tbs.Length; i++)
                {
                    TextBox tb = tbs[i];
                    if (tb != null && TryParseDecimalFlexible(tb.Text, out decimal v) && v >= 0)
                    {
                        s += v;
                    }
                }

                return s;
            }

            decimal tyo = 0m;

            if (tbTyonHinta != null && TryParseDecimalFlexible(tbTyonHinta.Text, out decimal th) && th >= 0)
            {
                tyo = th;
            }

            decimal mats = Sum(
                tbMat1Summa, tbMat2Summa, tbMat3Summa, tbMat4Summa,
                tbMat5Summa, tbMat6Summa, tbMat7Summa, tbMat8Summa
            );

            if (tbLaskunYhteensa != null)
            {
                tbLaskunYhteensa.Text = (tyo + mats).ToString("N2", fi);
            }
        }

        // Laskee tyon hinnan (tyotunnit x tuntihinta) ja paivittaa laskun kokonaissumman.
        private void TyotInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tbTyotuntihinta == null || tbTyotunnit == null || tbTyonHinta == null)
                return;

            CultureInfo fi = new CultureInfo("fi-FI");

            if (TryParseDecimalFlexible(tbTyotuntihinta.Text, out decimal tuntihinta) &&
                TryParseDecimalFlexible(tbTyotunnit.Text, out decimal tunnit) &&
                tuntihinta >= 0 && tunnit >= 0)
            {
                decimal hinta = tuntihinta * tunnit;
                tbTyonHinta.Text = hinta.ToString("N2", fi);
            }
            else
            {
                tbTyonHinta.Text = string.Empty;
            }

            UpdateLaskunYhteensa();
        }

        // Lisaa uuden laskun tietokantaan ja paivittaa DataGridin listan.
        private void BtnLisaaLasku_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel vm = DataContext as MainViewModel;
            if (vm == null)
            {
                MessageBox.Show("DataContext puuttuu.");

                return;
            }

            if (dpTilauspaiva.SelectedDate == null)
            {
                MessageBox.Show("Valitse tilauspaiva.");

                return;
            }
            if (dpErapaiva.SelectedDate == null)
            {
                MessageBox.Show("Valitse erapaiva.");

                return;
            }
            if (cbAsiakas.SelectedValue == null)
            {
                MessageBox.Show("Valitse asiakas.");

                return;
            }

            DateTime tilaus = dpTilauspaiva.SelectedDate.Value.Date;
            DateTime era = dpErapaiva.SelectedDate.Value.Date;

            if (tilaus > DateTime.Today)
            {
                MessageBox.Show("Tilauspaiva ei voi olla tulevaisuudessa.");

                return;
            }
            if (era < tilaus)
            {
                MessageBox.Show("Erapaiva ei voi olla ennen tilauspaivaa.");

                return;
            }

            ComboBoxItem mtItem = cbMaksunTila.SelectedItem as ComboBoxItem;

            string maksunTila;

            if (mtItem != null && mtItem.Content != null)
            {
                string valinta = mtItem.Content.ToString().Trim().ToLower();
                if (valinta == "maksettu")
                    maksunTila = "maksettu";
                else
                    maksunTila = "ei maksettu";
            }
            else
            {
                maksunTila = "ei maksettu";
            }

            decimal tuntihinta;

            if (!TryParseDecimalFlexible(tbTyotuntihinta.Text, out tuntihinta) || tuntihinta < 0)
            {
                MessageBox.Show("Tyotuntihinta on pakko asettaa.");

                return;
            }

            decimal tunnitDec;

            if (!TryParseDecimalFlexible(tbTyotunnit.Text, out tunnitDec) || tunnitDec < 0)
            {
                MessageBox.Show("Tyotuntimaara on pakko asettaa.");

                return;
            }

            int tyotunnit = (int)Math.Round(tunnitDec, MidpointRounding.AwayFromZero);

            string lisatiedot;
            if (string.IsNullOrWhiteSpace(tbLisatiedot.Text))
                lisatiedot = null;
            else
                lisatiedot = tbLisatiedot.Text.Trim();

            int asiakasId = Convert.ToInt32(cbAsiakas.SelectedValue);

            ComboBox[] cbs = new[] { cbMat1Nimi, cbMat2Nimi, cbMat3Nimi, cbMat4Nimi, cbMat5Nimi, cbMat6Nimi, cbMat7Nimi, cbMat8Nimi };
            TextBox[] qtys = new[] { tbMat1Maara, tbMat2Maara, tbMat3Maara, tbMat4Maara, tbMat5Maara, tbMat6Maara, tbMat7Maara, tbMat8Maara };
            TextBox[] hinn = new[] { tbMat1Hinta, tbMat2Hinta, tbMat3Hinta, tbMat4Hinta, tbMat5Hinta, tbMat6Hinta, tbMat7Hinta, tbMat8Hinta };

            List<(int TuoteId, int Maara, decimal Yksikkohinta, string Yksikko)> rivit = new List<(int TuoteId, int Maara, decimal Yksikkohinta, string Yksikko)>();

            for (int i = 0; i < 8; i++)
            {
                ComboBox cb = cbs[i];
                TextBox tbQty = qtys[i];
                TextBox tbPrice = hinn[i];

                bool anyFilled =
                    (cb != null && cb.SelectedValue != null) ||
                    (tbQty != null && tbQty.Text != null && !string.IsNullOrWhiteSpace(tbQty.Text)) ||
                    (tbPrice != null && tbPrice.Text != null && !string.IsNullOrWhiteSpace(tbPrice.Text));

                if (!anyFilled)
                    continue;

                if (cb == null || cb.SelectedValue == null)
                {
                    MessageBox.Show("Valitse materiaali/tuote -riville " + (i + 1) + " tuotteen nimi.");
                    return;
                }

                decimal price;
                string priceText = (tbPrice != null && tbPrice.Text != null) ? tbPrice.Text : "";
                if (!TryParseDecimalFlexible(priceText, out price) || price < 0)
                {
                    MessageBox.Show("Anna materiaali/tuote -riville " + (i + 1) + " yksikkohinta.");
                    return;
                }

                decimal qtyDec;
                string qtyText = (tbQty != null && tbQty.Text != null) ? tbQty.Text : "";
                if (!TryParseDecimalFlexible(qtyText, out qtyDec) || qtyDec <= 0)
                {
                    MessageBox.Show("Anna materiaali/tuote -riville " + (i + 1) + " maara.");
                    return;
                }
                int qty = (int)Math.Round(qtyDec, MidpointRounding.AwayFromZero);

                int row = i + 1;
                TextBox yksBox = FindName($"tbMat{row}Yks") as TextBox;
                string yks = "";
                if (yksBox != null && yksBox.Text != null)
                    yks = yksBox.Text.Trim();
                if (string.IsNullOrEmpty(yks)) yks = "kpl";

                rivit.Add((TuoteId: Convert.ToInt32(cb.SelectedValue),
                           Maara: qty,
                           Yksikkohinta: price,
                           Yksikko: yks));
            }

            if (rivit.Count == 0)             // Kayttajan pitaa valita ainakin yhden materiaali/tuote laskua varten tai tulee virheilmoitus.
            {
                MessageBox.Show("Lisaa ainakin yksi materiaali/tuote -rivi.");

                return;
            }

            try
            {
                using SqliteConnection conn = DatabaseConnector.ConnectDatabase();

                // Lasketaan seuraava juokseva LaskuID vain laskun numerointia varten.
                int nextId;
                using (SqliteCommand cmdMax = new SqliteCommand("SELECT COALESCE(MAX(LaskuID),0)+1 FROM Lasku;", conn))
                {
                    nextId = Convert.ToInt32(cmdMax.ExecuteScalar());
                }

                string laskunNumeroAsiakkaalle = "LASKU-" + (1000 + nextId);

                int newLaskuId;

                using (SqliteCommand cmd = new SqliteCommand(
                    "INSERT INTO Lasku (tilausPVM, erapaiva, maksunTila, tyoTuntihinta, tyoTunnit, laskunNumeroAsiakkaalle, lisatiedot, asiakasID) " +
                    "VALUES (@tilaus, @era, @tila, @hinta, @tunnit, @numero, @lisat, @asiakas); SELECT last_insert_rowid();", conn))
                {
                    cmd.Parameters.AddWithValue("@tilaus", tilaus.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@era", era.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@tila", maksunTila);
                    cmd.Parameters.AddWithValue("@hinta", tuntihinta);
                    cmd.Parameters.AddWithValue("@tunnit", tyotunnit);
                    cmd.Parameters.AddWithValue("@numero", laskunNumeroAsiakkaalle);

                    if (lisatiedot == null)
                        cmd.Parameters.AddWithValue("@lisat", DBNull.Value);
                    else
                        cmd.Parameters.AddWithValue("@lisat", (object)lisatiedot);

                    cmd.Parameters.AddWithValue("@asiakas", asiakasId);

                    newLaskuId = Convert.ToInt32(cmd.ExecuteScalar());
                }

                foreach ((int TuoteId, int Maara, decimal Yksikkohinta, string Yksikko) r in rivit)
                {
                    using SqliteCommand cmdRow = new SqliteCommand(
                        "INSERT INTO Laskurivi (yksikkohinta, yksikko, maara, tuoteid, LaskuID) " +
                        "VALUES (@hinta, @yks, @maara, @tuote, @lasku);", conn);

                    cmdRow.Parameters.AddWithValue("@hinta", r.Yksikkohinta);
                    cmdRow.Parameters.AddWithValue("@yks", r.Yksikko);
                    cmdRow.Parameters.AddWithValue("@maara", r.Maara);
                    cmdRow.Parameters.AddWithValue("@tuote", r.TuoteId);
                    cmdRow.Parameters.AddWithValue("@lasku", newLaskuId);
                    cmdRow.ExecuteNonQuery();
                }

                LaskuRepository repo = new LaskuRepository();
                ObservableCollection<Lasku> uusilista = repo.HaeKaikkiLaskut();

                vm.Laskut.Clear();
                foreach (Lasku l in uusilista) vm.Laskut.Add(l);

                BtnTyhjennaLaskuKentat_Click(null, null);

                MessageBox.Show("Uusi lasku lisatty.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Laskun lisays epaonnistui:\n" + ex.Message);
            }
        }


        // Paivittaa valitun laskun tiedot tietokannassa ja kayttoliittyman nakymassa.
        private void BtnPaivitaLasku_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel vm = DataContext as ViewModels.MainViewModel;

            if (vm == null)
            {
                MessageBox.Show("DataContext puuttuu.");
                return;
            }

            Lasku valittu = xnameLaskut?.SelectedItem as Lasku;

            if (valittu == null)
            {
                MessageBox.Show("Valitse ensin muokattava lasku listasta.");

                return;
            }

            if (dpTilauspaiva.SelectedDate == null)
            {
                MessageBox.Show("Valitse tilauspaiva.");

                return;
            }

            if (dpErapaiva.SelectedDate == null)
            {
                MessageBox.Show("Valitse erapaiva.");

                return;
            }

            if (cbAsiakas.SelectedValue == null)
            {
                MessageBox.Show("Valitse asiakas.");

                return;
            }

            if (cbMaksunTila.SelectedItem is not ComboBoxItem mtItem)
            {
                MessageBox.Show("Valitse maksun tila.");

                return;
            }

            DateTime tilaus = dpTilauspaiva.SelectedDate.Value.Date;
            DateTime era = dpErapaiva.SelectedDate.Value.Date;

            if (tilaus > DateTime.Today)
            {
                MessageBox.Show("Tilauspaiva ei voi olla tulevaisuudessa.");

                return;
            }

            if (era < tilaus)
            {
                MessageBox.Show("Erapaiva ei voi olla ennen tilauspaivaa.");

                return;
            }

            string content;

            if (mtItem.Content == null)
                content = "";
            else
                content = mtItem.Content.ToString();

            string maksunTila = content.Trim().ToLower() switch
            {
                "maksettu" => "maksettu",
                "ei maksettu" => "ei maksettu",
                _ => "ei maksettu"
            };

            if (!TryParseDecimalFlexible(tbTyotuntihinta.Text, out decimal tuntihinta) || tuntihinta < 0)
            {
                MessageBox.Show("Tyotuntihinta on pakko asettaa.");
                return;
            }
            if (!TryParseDecimalFlexible(tbTyotunnit.Text, out decimal tunnitDec) || tunnitDec < 0)
            {
                MessageBox.Show("Tyotuntimaara on pakko asettaa.");
                return;
            }

            int tyotunnit = (int)Math.Round(tunnitDec, MidpointRounding.AwayFromZero);

            string lisatiedot;
            if (string.IsNullOrWhiteSpace(tbLisatiedot.Text))
                lisatiedot = null;
            else
                lisatiedot = tbLisatiedot.Text.Trim();

            int asiakasId = Convert.ToInt32(cbAsiakas.SelectedValue);

            ComboBox[] cbs = new[] { cbMat1Nimi, cbMat2Nimi, cbMat3Nimi, cbMat4Nimi, cbMat5Nimi, cbMat6Nimi, cbMat7Nimi, cbMat8Nimi };
            TextBox[] qtys = new[] { tbMat1Maara, tbMat2Maara, tbMat3Maara, tbMat4Maara, tbMat5Maara, tbMat6Maara, tbMat7Maara, tbMat8Maara };
            TextBox[] hinn = new[] { tbMat1Hinta, tbMat2Hinta, tbMat3Hinta, tbMat4Hinta, tbMat5Hinta, tbMat6Hinta, tbMat7Hinta, tbMat8Hinta };

            List<(int TuoteId, int Maara, decimal Yksikkohinta, string Yksikko)> rivit = new List<(int TuoteId, int Maara, decimal Yksikkohinta, string Yksikko)>();
            for (int i = 0; i < 8; i++)
            {
                ComboBox cb = cbs[i];
                TextBox tbQty = qtys[i];
                TextBox tbPrice = hinn[i];

                bool anyFilled =
                    (cb != null && cb.SelectedValue != null) ||
                    (tbQty != null && !string.IsNullOrWhiteSpace(tbQty.Text)) ||
                    (tbPrice != null && !string.IsNullOrWhiteSpace(tbPrice.Text));

                if (!anyFilled) continue;

                if (cb == null || cb.SelectedValue == null)
                {
                    MessageBox.Show($"Valitse materiaali/tuote -riville {i + 1} tuotteen nimi.");
                    return;
                }

                if (tbPrice == null || !TryParseDecimalFlexible(tbPrice.Text, out decimal price) || price < 0)
                {
                    MessageBox.Show($"Anna materiaali/tuote -riville {i + 1} yksikkohinta.");
                    return;
                }

                if (tbQty == null || !TryParseDecimalFlexible(tbQty.Text, out decimal qtyDec) || qtyDec <= 0)
                {
                    MessageBox.Show($"Anna materiaali/tuote -riville {i + 1} maara.");
                    return;
                }

                int qty = (int)Math.Round(qtyDec, MidpointRounding.AwayFromZero);

                int row = i + 1;

                TextBox tbYks = FindName($"tbMat{row}Yks") as TextBox;
                string yks = "";

                if (tbYks != null && tbYks.Text != null)
                    yks = tbYks.Text.Trim();

                if (string.IsNullOrEmpty(yks)) yks = "kpl";

                rivit.Add((TuoteId: Convert.ToInt32(cb.SelectedValue),
                           Maara: qty,
                           Yksikkohinta: price,
                           Yksikko: yks));
            }

            try
            {
                using SqliteConnection conn = DatabaseConnector.ConnectDatabase();
                using SqliteTransaction tx = conn.BeginTransaction();

                using (SqliteCommand cmd = new SqliteCommand(@"
    UPDATE Lasku SET
        tilausPVM = @tilaus,
        erapaiva = @era,
        maksunTila = @tila,
        tyoTuntihinta = @hinta,
        tyoTunnit = @tunnit,
        lisatiedot = @lisat,
        asiakasID = @asiakas
    WHERE LaskuID = @id;", conn, tx))
                {
                    cmd.Parameters.AddWithValue("@tilaus", tilaus.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@era", era.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@tila", maksunTila);
                    cmd.Parameters.AddWithValue("@hinta", tuntihinta);
                    cmd.Parameters.AddWithValue("@tunnit", tyotunnit);

                    if (lisatiedot == null)
                        cmd.Parameters.AddWithValue("@lisat", DBNull.Value);
                    else
                        cmd.Parameters.AddWithValue("@lisat", lisatiedot);

                    cmd.Parameters.AddWithValue("@asiakas", asiakasId);
                    cmd.Parameters.AddWithValue("@id", valittu.LaskuId);

                    if (cmd.ExecuteNonQuery() <= 0)
                        throw new Exception("Laskun paivitys ei onnistunut.");
                }

                using (SqliteCommand del = new SqliteCommand("DELETE FROM Laskurivi WHERE LaskuID=@id;", conn, tx))
                {
                    del.Parameters.AddWithValue("@id", valittu.LaskuId);
                    del.ExecuteNonQuery();
                }

                foreach ((int TuoteId, int Maara, decimal Yksikkohinta, string Yksikko) r in rivit)
                {
                    using SqliteCommand ins = new SqliteCommand(@"
        INSERT INTO Laskurivi (yksikkohinta, yksikko, maara, tuoteid, LaskuID)
        VALUES (@hinta, @yks, @maara, @tuote, @lasku);", conn, tx);

                    ins.Parameters.AddWithValue("@hinta", r.Yksikkohinta);
                    ins.Parameters.AddWithValue("@yks", r.Yksikko);
                    ins.Parameters.AddWithValue("@maara", r.Maara);
                    ins.Parameters.AddWithValue("@tuote", r.TuoteId);
                    ins.Parameters.AddWithValue("@lasku", valittu.LaskuId);
                    ins.ExecuteNonQuery();
                }

                tx.Commit();

                LaskuRepository repo = new LaskuRepository();
                ObservableCollection<Lasku> uusilista = repo.HaeKaikkiLaskut();

                vm.Laskut.Clear();
                foreach (Lasku l in uusilista) vm.Laskut.Add(l);

                Lasku uusiValinta = vm.Laskut.FirstOrDefault(l => l.LaskuId == valittu.LaskuId);

                if (uusiValinta != null)
                    xnameLaskut.SelectedItem = uusiValinta;

                MessageBox.Show("Lasku paivitetty.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Laskun paivitys epaonnistui:\n" + ex.Message);
            }
        }


        // Poistaa valitun laskun soft delete tavalla kayttoliittyman nakymasta.
        private void BtnPoistaLasku_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel vm = DataContext as MainViewModel;
            if (vm == null)
            {
                MessageBox.Show("DataContext puuttuu.");

                return;
            }

            Lasku valittu = xnameLaskut?.SelectedItem as Lasku;

            if (valittu == null)
            {
                MessageBox.Show("Valitse ensin poistettava lasku listasta.");

                return;
            }

            MessageBoxResult tulos = MessageBox.Show(
                $"Haluatko varmasti poistaa laskun '{valittu.LaskunNumeroAsiakkaalle}' (ID {valittu.LaskuId})?",
                "Vahvista poisto",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (tulos != MessageBoxResult.Yes)
                return;

            LaskuRepository repo = new LaskuRepository();

            bool ok = repo.PoistaLasku(valittu.LaskuId);

            if (!ok)
            {
                MessageBox.Show("Laskun poisto epaonnistui.");

                return;
            }

            vm.Laskut.Remove(valittu);

            BtnTyhjennaLaskuKentat_Click(null, null);

            MessageBox.Show("Lasku poistettu.");
        }

        // Jos kayttaja on syottanyt tietoa Laskut-valilehden lomakkeeseen, tama nappi poistaa tiedot,
        // jotka ovat poistettavissa (esim. "Laskuttaja:" -kenttaa ei voi tyhjentaa).
        private void BtnTyhjennaLaskuKentat_Click(object sender, RoutedEventArgs e)
        {
            if (dpTilauspaiva != null) dpTilauspaiva.SelectedDate = null;
            if (dpErapaiva != null) dpErapaiva.SelectedDate = null;

            if (cbAsiakas != null)
            {
                cbAsiakas.SelectedIndex = -1;
                cbAsiakas.Text = string.Empty;
                cbAsiakas.IsEditable = false;
                cbAsiakas.IsHitTestVisible = true;
            }

            if (cbMaksunTila != null) cbMaksunTila.SelectedIndex = -1;

            if (tbLisatiedot != null) tbLisatiedot.Text = string.Empty;

            if (tbTyotunnit != null) tbTyotunnit.Text = string.Empty;
            if (tbTyotuntihinta != null) tbTyotuntihinta.Text = string.Empty;
            if (tbTyonHinta != null) tbTyonHinta.Text = string.Empty;

            if (cbMat1Nimi != null) cbMat1Nimi.SelectedIndex = -1;
            if (tbMat1Maara != null) tbMat1Maara.Text = string.Empty;
            if (tbMat1Hinta != null) tbMat1Hinta.Text = string.Empty;
            if (tbMat1Summa != null) tbMat1Summa.Text = string.Empty;

            if (cbMat2Nimi != null) cbMat2Nimi.SelectedIndex = -1;
            if (tbMat2Maara != null) tbMat2Maara.Text = string.Empty;
            if (tbMat2Hinta != null) tbMat2Hinta.Text = string.Empty;
            if (tbMat2Summa != null) tbMat2Summa.Text = string.Empty;

            if (cbMat3Nimi != null) cbMat3Nimi.SelectedIndex = -1;
            if (tbMat3Maara != null) tbMat3Maara.Text = string.Empty;
            if (tbMat3Hinta != null) tbMat3Hinta.Text = string.Empty;
            if (tbMat3Summa != null) tbMat3Summa.Text = string.Empty;

            if (cbMat4Nimi != null) cbMat4Nimi.SelectedIndex = -1;
            if (tbMat4Maara != null) tbMat4Maara.Text = string.Empty;
            if (tbMat4Hinta != null) tbMat4Hinta.Text = string.Empty;
            if (tbMat4Summa != null) tbMat4Summa.Text = string.Empty;

            if (cbMat5Nimi != null) cbMat5Nimi.SelectedIndex = -1;
            if (tbMat5Maara != null) tbMat5Maara.Text = string.Empty;
            if (tbMat5Hinta != null) tbMat5Hinta.Text = string.Empty;
            if (tbMat5Summa != null) tbMat5Summa.Text = string.Empty;

            if (cbMat6Nimi != null) cbMat6Nimi.SelectedIndex = -1;
            if (tbMat6Maara != null) tbMat6Maara.Text = string.Empty;
            if (tbMat6Hinta != null) tbMat6Hinta.Text = string.Empty;
            if (tbMat6Summa != null) tbMat6Summa.Text = string.Empty;

            if (cbMat7Nimi != null) cbMat7Nimi.SelectedIndex = -1;
            if (tbMat7Maara != null) tbMat7Maara.Text = string.Empty;
            if (tbMat7Hinta != null) tbMat7Hinta.Text = string.Empty;
            if (tbMat7Summa != null) tbMat7Summa.Text = string.Empty;

            if (cbMat8Nimi != null) cbMat8Nimi.SelectedIndex = -1;
            if (tbMat8Maara != null) tbMat8Maara.Text = string.Empty;
            if (tbMat8Hinta != null) tbMat8Hinta.Text = string.Empty;
            if (tbMat8Summa != null) tbMat8Summa.Text = string.Empty;

            if (tbLaskunYhteensa != null)
            {
                tbLaskunYhteensa.Text = string.Empty;
            }

            UpdateLaskunYhteensa();

            _dueDateUserEdited = false;
        }


        // Tuotetiedot -valilehden Ohje-nappi ja sen toiminnallisuus.
        private void btnOhje_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Tuotteen poistaminen:\n" +
                "    -Valitse listasta yksi rivi ja paina sen jalkeen\n" +
                "    ''Poista tuote'' -nappia.\n\n" +
                "Tuotteen lisaaminen:\n" +
                "    -Tayta kaikki kentat paitsi ''Kuvaus'' ja paina sen jalkeen\n" +
                "    ''Lisaa uusi tuote'' -nappia.\n\n" +
                "Tuotteen tietojen muuttaminen/muokkaaminen/paivittaminen:\n" +
                "    1) Valitse listasta yksi rivi.\n" +
                "    2) Tee muutoksia lomakkeen kenttiin.\n" +
                "    3) Paina lopuksi ''Paivita/Muokkaa'' -nappia.\n\n" +
                "Kenttien tyhjentaminen:\n" +
                "    -Jos kentissa on tietoa ja haluat ne tyhjiksi, voit nopeuttaa\n" +
                "    toimintaasi painamalla ''Tyhjenna kentat'' -nappia.",
                "Ohje",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        // Asiakastiedot -valilehden Ohje-nappi ja sen toiminnallisuus.
        private void btnOhje_asiakastiedot_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Asiakkaan poistaminen:\n" +
                "    -Valitse listasta yksi rivi ja paina sen jalkeen\n" +
                "    ''Poista asiakas'' -nappia.\n\n" +
                "Asiakkaan lisaaminen:\n" +
                "    -Tayta kaikki kentat paitsi ''Email'' ja paina sen jalkeen\n" +
                "    ''Lisaa uusi asiakas'' -nappia.\n\n" +
                "Asiakkaan tietojen muuttaminen/muokkaaminen/paivittaminen:\n" +
                "    1) Valitse listasta yksi rivi.\n" +
                "    2) Tee muutoksia kenttiin.\n" +
                "    3) Paina lopuksi ''Paivita/Muokkaa'' -nappia.\n\n" +
                "Kenttien tyhjentaminen:\n" +
                "    -Jos kentissa on tietoa ja haluat ne tyhjiksi, voit nopeuttaa\n" +
                "    toimintaasi painamalla ''Tyhjenna kentat'' -nappia.",
                "Ohje",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        // Laskut -valilehden Ohje-nappi ja sen toiminnallisuus.
        private void BtnOhje_Laskut_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Laskun poistaminen:\n" +
                "    -Valitse listasta yksi rivi ja paina sen jalkeen ''Poista lasku'' -nappia.\n\n" +
                "Laskun lisaaminen:\n" +
                "    -Tiedot, jotka pitaa tayttaa: Tilauspaiva, asiakas, tyotuntimaara,\n" +
                "     tyotuntihinta ja lisaksi vahintaan yksi tuoterivi pitaa tayttaa.\n" +
                "    -Jos valiset materiaalin/tuotteen, sille pitaa myos asettaa maara\n" +
                "     ja yksikkohinta. Yhteishinta tayttyy automaattisesti.\n" +
                "    -Tiedot joita ei tarvitse tayttaa: Maksun tila\n" +
                "     (tayttyy automaattisesti), lisatiedot, tyonhinta (autom.),\n" +
                "     loppusumma (autom.) ja erapaiva (autom.).\n" +
                "    -Huom. vaikka erapaiva ja maksun tila -tiedot tayttyvat\n" +
                "     automaattisesti, kayttaja voi myos muuttaa niiden tiedot\n" +
                "     manuaalisesti.\n" +
                "    -Erapaiva asettuu oletuksena 61 paivan paahan\n" +
                "     tilauspaivamaarasta, mutta erapaivan voi sen jalkeen muuttaa\n" +
                "     manuaalisesti.\n" +
                "    -Kenttaa, joka on ''Maara'' -kentan oikeallapuolella ei voi tayttaa,\n" +
                "     koska se edustaa yksikkoa ja se tayttyy aina automaattisesti.\n" +
                "    -Lopuksi paina 'Lisaa uusi lasku' -nappia.\n\n" +
                "Laskun muokkaaminen:\n" +
                "    1) Valitse listasta rivi.\n" +
                "    2) Tee muutoksia kenttiin.\n" +
                "    3) Paina 'Paivita/Muokkaa'.\n\n" +
                "Kenttien tyhjentaminen:\n" +
                "    -Jos kentissa on tietoa ja haluat ne tyhjiksi, voit nopeuttaa\n" +
                "     toimintaasi painamalla ''Tyhjenna kentat'' -nappia.\n" +
                "    -Voit tyhjentaa lomakkeelta yksittaisen materiaali/tuote # -rivin\n" +
                "     valitsemalla alasvetovalikosta ''< Tyhja rivi >''. Tama\n" +
                "     samalla tyhjentaa arvot ''Maara'' ja ''Yksikkohinta:'' -kentista, mikali\n" +
                "     niissa on arvot.",
                "Ohje",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        // Asettaa materiaalin hinnan ja yksikon automaattisesti, kun kayttaja valitsee tuotteen alasvetovalikosta (=Comboboxista).
        private void MatNimi_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ComboBox cb) return;

            string num = new string(cb.Name.Where(char.IsDigit).ToArray());
            Grid parent = cb.Parent as Grid;

            TextBox priceBox = cb.Tag as TextBox;
            TextBox qtyBox = null;
            TextBox yksBox = null;
            TextBox sumBox = null;

            if (parent != null)
            {
                qtyBox = parent.FindName($"tbMat{num}Maara") as TextBox;
                yksBox = parent.FindName($"tbMat{num}Yks") as TextBox;
                sumBox = parent.FindName($"tbMat{num}Summa") as TextBox;
            }

            if (cb.SelectedItem is ComboBoxItem || cb.SelectedValue == null)
            {
                if (priceBox != null) priceBox.Text = "";
                if (qtyBox != null) qtyBox.Text = "";
                if (yksBox != null) yksBox.Text = "";

                MaterialInput_TextChanged(null, null);
                return;
            }

            if (cb.SelectedItem is Tuote tuote)
            {
                if (priceBox != null)
                    priceBox.Text = tuote.Hinta.ToString("N2", new CultureInfo("fi-FI"));

                if (yksBox != null)
                {
                    if (tuote.Yksikko == null)
                        yksBox.Text = "kpl";
                    else
                        yksBox.Text = tuote.Yksikko;
                }

                MaterialInput_TextChanged(null, null);
            }
        }

        // Laskut-valilehden lomake (kentat, datepickerit ja comboboxit) nayttaa sen DataGridin rivin tiedot, jonka kayttaja on valinnut.
        private void xnameLaskut_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Lasku vmLasku = xnameLaskut?.SelectedItem as Lasku;
            if (vmLasku == null) return;

            BtnTyhjennaLaskuKentat_Click(null, null);

            (DateTime? TilausPvm, DateTime? Erapaiva, string MaksunTila, decimal TyoTuntihinta, int TyoTunnit, string Lisatiedot, int AsiakasId) tiedot = HaeLaskuKannasta(vmLasku.LaskuId);

            dpTilauspaiva.SelectedDate = tiedot.TilausPvm;
            dpErapaiva.SelectedDate = tiedot.Erapaiva;

            if (tiedot.Lisatiedot == null)
                tbLisatiedot.Text = "";
            else
                tbLisatiedot.Text = tiedot.Lisatiedot;

            CultureInfo fin = new CultureInfo("fi-FI");
            tbTyotuntihinta.Text = tiedot.TyoTuntihinta.ToString("F2", fin);

            tbTyotunnit.Text = tiedot.TyoTunnit.ToString(CultureInfo.InvariantCulture);

            cbMaksunTila.SelectedIndex = -1;

            string tila = tiedot.MaksunTila.Trim();

            if (string.Equals(tila, "maksettu", StringComparison.OrdinalIgnoreCase))
                cbMaksunTila.SelectedIndex = 0;
            else if (string.Equals(tila, "ei maksettu", StringComparison.OrdinalIgnoreCase))
                cbMaksunTila.SelectedIndex = 1;

            // Jos laskun asiakas on edelleen aktiivinen ja löytyy ComboBoxin ItemsSourcesta, valitse asiakas normaalisti,
            // muussa tapauksessa näytä nimi tekstinä ja lukitse kenttä.
            bool loytyy = false;
            ObservableCollection<Asiakas> lista = cbAsiakas.ItemsSource as ObservableCollection<LaskutusOhjelma.Models.Asiakas>;
            if (lista != null)
            {
                foreach (Asiakas a in lista)
                {
                    if (a.AsiakasId == tiedot.AsiakasId)
                    {
                        loytyy = true;
                        break;
                    }
                }
            }

            if (loytyy)
            {
                cbAsiakas.IsEditable = false;
                cbAsiakas.IsHitTestVisible = true;
                cbAsiakas.SelectedValue = tiedot.AsiakasId;
            }
            else
            {
                string nimi = HaeAsiakkaanNimiMyosPoistetuista(tiedot.AsiakasId);
                cbAsiakas.SelectedIndex = -1;
                cbAsiakas.IsEditable = true;
                cbAsiakas.Text = nimi;
                cbAsiakas.IsHitTestVisible = false;
            }


            List<(int TuoteId, int Maara, decimal Yksikkohinta, string Yksikko)> rivit = HaeLaskurivitKannasta(vmLasku.LaskuId);
            ComboBox[] cbs = new[] { cbMat1Nimi, cbMat2Nimi, cbMat3Nimi, cbMat4Nimi, cbMat5Nimi, cbMat6Nimi, cbMat7Nimi, cbMat8Nimi };
            TextBox[] qtys = new[] { tbMat1Maara, tbMat2Maara, tbMat3Maara, tbMat4Maara, tbMat5Maara, tbMat6Maara, tbMat7Maara, tbMat8Maara };
            TextBox[] hinn = new[] { tbMat1Hinta, tbMat2Hinta, tbMat3Hinta, tbMat4Hinta, tbMat5Hinta, tbMat6Hinta, tbMat7Hinta, tbMat8Hinta };
            TextBox[] sums = new[] { tbMat1Summa, tbMat2Summa, tbMat3Summa, tbMat4Summa, tbMat5Summa, tbMat6Summa, tbMat7Summa, tbMat8Summa };
            CultureInfo fi = new CultureInfo("fi-FI");

            for (int i = 0; i < rivit.Count && i < 8; i++)
            {
                (int TuoteId, int Maara, decimal Yksikkohinta, string Yksikko) r = rivit[i];
                if (cbs[i] != null) cbs[i].SelectedValue = r.TuoteId;
                if (qtys[i] != null) qtys[i].Text = r.Maara.ToString();
                if (hinn[i] != null) hinn[i].Text = r.Yksikkohinta.ToString("N2", fi);
                if (sums[i] != null) sums[i].Text = (r.Yksikkohinta * r.Maara).ToString("N2", fi);
            }

            UpdateLaskunYhteensa();
        }

        private (DateTime? TilausPvm, DateTime? Erapaiva, string MaksunTila, decimal TyoTuntihinta, int TyoTunnit, string Lisatiedot, int AsiakasId)

// Alapuolella oleva metodi hakee tietyn laskun tiedot tietokannasta.
HaeLaskuKannasta(int laskuId)
        {
            using SqliteConnection conn = DatabaseConnector.ConnectDatabase();
            using SqliteCommand cmd = new SqliteCommand(@"
        SELECT tilausPVM, erapaiva, maksunTila, tyoTuntihinta, tyoTunnit, lisatiedot, asiakasID
        FROM Lasku
        WHERE LaskuID = @id;", conn);
            cmd.Parameters.AddWithValue("@id", laskuId);

            using SqliteDataReader r = cmd.ExecuteReader();
            if (!r.Read())
                return (null, null, "", 0m, 0, "", 0);

            return (
                TilausPvm: DateTime.Parse(r.GetString(r.GetOrdinal("tilausPVM"))),
                Erapaiva: DateTime.Parse(r.GetString(r.GetOrdinal("erapaiva"))),
                MaksunTila: r.GetString(r.GetOrdinal("maksunTila")),
                TyoTuntihinta: r.GetDecimal(r.GetOrdinal("tyoTuntihinta")),
                TyoTunnit: r.GetInt32(r.GetOrdinal("tyoTunnit")),
                Lisatiedot: r.IsDBNull(r.GetOrdinal("lisatiedot")) ? "" : r.GetString(r.GetOrdinal("lisatiedot")),
                AsiakasId: r.GetInt32(r.GetOrdinal("asiakasID"))
            );
        }

        private List<(int TuoteId, int Maara, decimal Yksikkohinta, string Yksikko)>

    // Alapuolella oleva metodi hakee tietyn laskun laskurivit tietokannasta.
    HaeLaskurivitKannasta(int laskuId)
        {
            List<(int, int, decimal, string)> list = new List<(int, int, decimal, string)>();

            using SqliteConnection conn = DatabaseConnector.ConnectDatabase();
            using SqliteCommand cmd = new SqliteCommand(@"
        SELECT LR.tuoteid, LR.maara, LR.yksikkohinta, LR.yksikko
        FROM Laskurivi LR
        INNER JOIN Tuote T ON T.tuoteid = LR.tuoteid
        WHERE LR.LaskuID = @id AND T.IsActive = 1
        ORDER BY LR.laskuriviID;", conn);
            cmd.Parameters.AddWithValue("@id", laskuId);

            using SqliteDataReader r = cmd.ExecuteReader();
            while (r.Read())
                list.Add((r.GetInt32(0), r.GetInt32(1), r.GetDecimal(2), r.GetString(3)));

            return list;
        }




        private bool _dueDateUserEdited = false;
        private bool _settingDueDateProgrammatically = false;

        // Alapuolella oleva metodi paivittaa erapaivan automaattisesti tilauspaivan perusteella
        // eli antaa erapaivalle oletus/default-arvon 61 paivaa eteenpain tilauspaivasta.
        private void dpTilauspaiva_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dpTilauspaiva != null && dpTilauspaiva.SelectedDate.HasValue)
            {
                DateTime d = dpTilauspaiva.SelectedDate.Value.Date;
                if (d > DateTime.Today)
                {
                    dpTilauspaiva.SelectedDate = DateTime.Today;
                    return;
                }
            }

            if (dpTilauspaiva == null || !dpTilauspaiva.SelectedDate.HasValue)
                return;

            bool eraPuuttuu = (dpErapaiva == null) || !dpErapaiva.SelectedDate.HasValue;

            if (!_dueDateUserEdited || eraPuuttuu)
            {
                _settingDueDateProgrammatically = true;
                if (dpErapaiva != null)
                    dpErapaiva.SelectedDate = dpTilauspaiva.SelectedDate.Value.AddDays(61);
                _settingDueDateProgrammatically = false;
            }
        }

        // Jos kayttaja muuttaa erapaivaa manuaalisesti, se merkitaan kayttajan muokkaamaksi, jolloin ohjelma ei enaa korvaa sita automaattisesti.
        private void dpErapaiva_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_settingDueDateProgrammatically) return;
            _dueDateUserEdited = true;
        }

        // Palauttaa asiakkaan nimen, vaikka asiakas olisi poistettu (IsActive = 0).
        // Jos ID:tä ei ole olemassa, palauttaa tyhjän merkkijonon.
        private string HaeAsiakkaanNimiMyosPoistetuista(int asiakasId)
        {
            using SqliteConnection conn = DatabaseConnector.ConnectDatabase();
            using SqliteCommand cmd = new SqliteCommand("SELECT nimi FROM Asiakas WHERE asiakasID = @id;", conn);
            cmd.Parameters.AddWithValue("@id", asiakasId);

            object tulos = cmd.ExecuteScalar();
            if (tulos == null || tulos == DBNull.Value)
                return string.Empty;

            string nimi = Convert.ToString(tulos);
            if (nimi == null)
                return string.Empty;

            return nimi;
        }
    }
}