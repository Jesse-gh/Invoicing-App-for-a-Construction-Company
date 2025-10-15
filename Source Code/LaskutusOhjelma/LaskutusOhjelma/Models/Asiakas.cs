namespace LaskutusOhjelma.Models
{                                    // Asiakas-luokka vastaa sovelluksessa yksittaisen asiakkaan tietoja.
                                     // Luokkaa kaytetaan mm. asiakasrekisterin kasittelyssa ja laskutuksessa.
    public class Asiakas
    {
        public int AsiakasId { get; set; }
        public string Nimi { get; set; }
        public string Maa { get; set; }
        public string Kaupunki { get; set; }
        public string Postinumero { get; set; }
        public string Katuosoite { get; set; }
        public string Puhelinnumero { get; set; }
        public string Email { get; set; }
        public string Asiakastyyppi { get; set; }
    }
}