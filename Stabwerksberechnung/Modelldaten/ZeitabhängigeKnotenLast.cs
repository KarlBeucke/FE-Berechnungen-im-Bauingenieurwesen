namespace FE_Berechnungen.Stabwerksberechnung.Modelldaten;

public class ZeitabhängigeKnotenLast : AbstraktZeitabhängigeKnotenlast
{
    public ZeitabhängigeKnotenLast(string lastId, string knotenId, int knotenFreiheitsgrad,
        string datei, bool boden)
    {
        LastId = lastId;
        KnotenId = knotenId;
        KnotenFreiheitsgrad = knotenFreiheitsgrad;
        Datei = datei;
        Bodenanregung = boden;
    }
    public ZeitabhängigeKnotenLast(string lastId, string knotenId, int knotenFreiheitsgrad, bool boden)
    {
        LastId = lastId;
        KnotenId = knotenId;
        KnotenFreiheitsgrad = knotenFreiheitsgrad;
        Bodenanregung = boden;
    }

    public override double[] BerechneLastVektor()
    {
        throw new NotImplementedException();
    }
}