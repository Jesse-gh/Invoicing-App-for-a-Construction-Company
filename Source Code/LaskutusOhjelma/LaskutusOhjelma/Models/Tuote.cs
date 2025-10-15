namespace LaskutusOhjelma.Models
{                               // Tuote-luokka kuvaa yhden tuotteen tiedot sovelluksessa.
                                // Luokkaa kaytetaan mm. tuoterekisterin kasittelyssa ja laskuriveilla.
    class Tuote
    {
        public int TuoteId { get; set; }
        public string Nimi { get; set; }
        public string Kuvaus { get; set; }
        public string Tuoteryhma { get; set; }
        public decimal Hinta { get; set; }
        public string Yksikko { get; set; }
    }
}