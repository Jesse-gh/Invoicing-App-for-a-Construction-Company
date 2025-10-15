using LaskutusOhjelma.Models;
using LaskutusOhjelma.Repos;
using System.Collections.ObjectModel;

namespace LaskutusOhjelma.ViewModels
{       // MainViewModel-luokka hallinnoi sovelluksen paanakyman tietoja ja yhdistaa sen tietokannan repositoryihin.
        // Se sisaltaa datan tuotteista, laskuista ja asiakkaista, joita kaytetaan kayttoliittymassa.
    class MainViewModel
    {
        public ObservableCollection<Tuote> Tuotteet { get; set; }
        public ObservableCollection<Lasku> Laskut { get; set; }
        public ObservableCollection<Asiakas> Asiakkaat { get; set; }

        public MainViewModel()
        {
            TuoteRepository tuoteRepo = new TuoteRepository();
            Tuotteet = tuoteRepo.HaeKaikkiTuotteet();

            LaskuRepository laskuRepo = new LaskuRepository();
            Laskut = laskuRepo.HaeKaikkiLaskut();

            AsiakasRepository asiakasRepo = new AsiakasRepository();
            Asiakkaat = asiakasRepo.HaeKaikkiAsiakkaat();
        }
    }
}