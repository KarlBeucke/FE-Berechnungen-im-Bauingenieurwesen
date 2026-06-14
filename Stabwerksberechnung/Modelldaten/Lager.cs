namespace FE_Berechnungen.Stabwerksberechnung.Modelldaten;

public class Lager : AbstraktRandbedingung
{
    public const int XFest = 1,
        YFest = 2,
        RFest = 4,
        XYFest = 3,
        XRFest = 5,
        YRFest = 6,
        XYRFest = 7;

    public Lager(string knotenId, int lagerTyp, IReadOnlyList<double> pre, FeModell modell)
    {
        Typ = lagerTyp;
        if (!modell.Knoten.TryGetValue(knotenId, out _))
            throw new ModellAusnahme("\nLagerknoten " + knotenId + " nicht definiert");

        Vordefiniert = new double[pre.Count];
        Festgehalten = new bool[pre.Count];
        for (var i = 0; i < pre.Count; i++) Festgehalten[i] = false;
        KnotenId = knotenId;

        switch (lagerTyp)
        {
            case XFest:
                Vordefiniert[0] = pre[0];
                Festgehalten[0] = true;
                break;
            case YFest:
                Vordefiniert[1] = pre[1];
                Festgehalten[1] = true;
                break;
            case RFest:
                Vordefiniert[2] = pre[2];
                Festgehalten[2] = true;
                break;
            case XYFest:
                Vordefiniert[0] = pre[0];
                Festgehalten[0] = true;
                Vordefiniert[1] = pre[1];
                Festgehalten[1] = true;
                break;
            case XRFest:
                Vordefiniert[0] = pre[0];
                Festgehalten[0] = true;
                Vordefiniert[2] = pre[2];
                Festgehalten[2] = true;
                break;
            case YRFest:
                Vordefiniert[1] = pre[1];
                Festgehalten[1] = true;
                Vordefiniert[2] = pre[2];
                Festgehalten[2] = true;
                break;
            case XYRFest:
                Vordefiniert[0] = pre[0];
                Festgehalten[0] = true;
                Vordefiniert[1] = pre[1];
                Festgehalten[1] = true;
                Vordefiniert[2] = pre[2];
                Festgehalten[2] = true;
                break;
        }
    }
}