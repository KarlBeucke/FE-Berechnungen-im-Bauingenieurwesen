namespace FE_Berechnungen.Stabwerksberechnung.Modelldaten;

public class FederElement : Abstrakt2D
{
    private readonly int _anzahlFreiheitsgrade;
    private readonly double[,] _steifigkeitsMatrix = new double[3, 3];

    public FederElement(string[] federKnoten, string eMaterialId, FeModell feModel)
    {
        KnotenIds = federKnoten;
        KnotenProElement = 1;
        Knoten = new Knoten[KnotenProElement];
        ElementMaterialId = eMaterialId;
        if (!feModel.Knoten.TryGetValue(KnotenIds[0], out Knoten[0]))
            throw new ModellAusnahme("\nKnoten für FederElement: " + federKnoten + " nicht im Modell gefunden");
        _anzahlFreiheitsgrade = Knoten[0].AnzahlKnotenfreiheitsgrade;
        ElementFreiheitsgrade = 3;
    }

    public override double[,] BerechneElementMatrix()
    {
        for (var i = 0; i < _anzahlFreiheitsgrade; i++)
            _steifigkeitsMatrix[i, i] = ElementMaterial.MaterialWerte[i];
        return _steifigkeitsMatrix;
    }

    public override double[] BerechneDiagonalMatrix()
    {
        throw new ModellAusnahme("\n*** Massenmatrix nicht relevant für Federlager");
    }

    // ... berechne Reaktionskräfte im Federelement ........................
    public override double[] BerechneZustandsvektor()
    {
        ElementZustand = new double[3];
        for (var i = 0; i < _anzahlFreiheitsgrade; i++)
            ElementZustand[i] = ElementMaterial.MaterialWerte[i] * Knoten[0].Knotenfreiheitsgrade[i];
        return ElementZustand;
    }

    public override double[] BerechneElementZustand(double z0, double z1)
    {
        var federKräfte = new double[3];
        return federKräfte;
    }

    public override void SetzElementSystemIndizes()
    {
        SystemIndizesElement = new int[_anzahlFreiheitsgrade];
        var counter = 0;
        for (var j = 0; j < _anzahlFreiheitsgrade; j++)
        {
            SystemIndizesElement[counter] = Knoten[0].SystemIndizes[j];
            counter++;
        }
    }

    public override Point BerechneSchwerpunkt()
    {
        var cg = new Point
        {
            X = Knoten[0].Koordinaten[0],
            Y = Knoten[0].Koordinaten[1]
        };
        return cg;
    }
}