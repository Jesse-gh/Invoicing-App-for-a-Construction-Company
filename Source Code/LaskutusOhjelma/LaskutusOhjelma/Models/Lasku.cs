namespace LaskutusOhjelma.Models
{                                  // Lasku-luokka vastaa sovelluksessa yksittaisen laskun tietoja.
                                   // Luokkaa kaytetaan mm. laskutietojen kasittelyssa.
    public class Lasku
    {
        public int LaskuId { get; set; }
        public int AsiakasId { get; set; }
        public string LaskunNumeroAsiakkaalle { get; set; }
        public string Tilauspaiva { get; set; }
        public string Erapaiva { get; set; }
        public string MaksunTila { get; set; }
        public decimal Tyotuntihinta { get; set; }
        public int Tyotunnit { get; set; }
        public string Lisatiedot { get; set; }

        public decimal MateriaalitYhteensa { get; set; }

        public decimal Loppusumma
        {
            get
            {
                return (Tyotuntihinta * (decimal)Tyotunnit) + MateriaalitYhteensa;
            }
        }
    }
}