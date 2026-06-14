namespace FEBibliothek.Modell
{
    public class Knotenwerte
    {
        public string EinflussId { get; set; }
        public string KnotenId { get; set; }
        public double[] Werte { get; set; }

        public Knotenwerte(string knotenId, double[] werte)
        {
            KnotenId = knotenId;
            Werte = werte;
        }
        public Knotenwerte(string einflussId, string knotenId, double[] werte)
        {
            EinflussId = einflussId;
            KnotenId = knotenId;
            Werte = werte;
        }
    }
}