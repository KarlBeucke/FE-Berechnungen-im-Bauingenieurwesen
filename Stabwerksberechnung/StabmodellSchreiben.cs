using FE_Berechnungen.Stabwerksberechnung.Modelldaten;

namespace FE_Berechnungen.Stabwerksberechnung;

internal class StabmodellSchreiben
{
    private FeModell _stabwerkModell;

    public List<string> StabwerksdatenSchreiben(FeModell feModell)
    {
        _stabwerkModell = feModell;
        var sb = new StringBuilder();
        var zeilen = new List<string>
        {
            "ModellName",
            _stabwerkModell.ModellId,
            "\nRaumdimension",
            _stabwerkModell.Raumdimension + "\t" + _stabwerkModell.AnzahlKnotenfreiheitsgrade,
            "\nKnoten"
        };
        var anzahlFreiheitsgrade = 0;
        foreach (var knoten in _stabwerkModell.Knoten)
        {
            if (knoten.Value.AnzahlKnotenfreiheitsgrade != anzahlFreiheitsgrade)
            {
                anzahlFreiheitsgrade = knoten.Value.AnzahlKnotenfreiheitsgrade;
                zeilen.Add(anzahlFreiheitsgrade.ToString("#"));
            }

            switch (_stabwerkModell.Raumdimension)
            {
                case 1:
                    zeilen.Add(knoten.Key + "\t" + knoten.Value.Koordinaten[0]);
                    break;
                case 2:
                    zeilen.Add(knoten.Key + "\t" + knoten.Value.Koordinaten[0]
                               + "\t" + knoten.Value.Koordinaten[1]);
                    break;
                case 3:
                    zeilen.Add(knoten.Key + "\t" + knoten.Value.Koordinaten[0]
                               + "\t" + knoten.Value.Koordinaten[1]
                               + "\t" + knoten.Value.Koordinaten[2]);
                    break;
                default:
                    _ = MessageBox.Show("falsche Raumdimension, muss 1, 2 oder 3 sein", "Stabwerksberechnung");
                    return null;
            }
        }

        // Elemente
        var alleFachwerkelemente = new List<Fachwerk>();
        var alleBiegebalken = new List<Biegebalken>();
        var alleBiegebalkenGelenk = new List<BiegebalkenGelenk>();
        var alleFederElemente = new List<FederElement>();
        foreach (var item in _stabwerkModell.Elemente)
            switch (item.Value)
            {
                case Fachwerk fachwerk:
                    alleFachwerkelemente.Add(fachwerk);
                    break;
                case Biegebalken biegebalken:
                    alleBiegebalken.Add(biegebalken);
                    break;
                case BiegebalkenGelenk biegebalkenGelenk:
                    alleBiegebalkenGelenk.Add(biegebalkenGelenk);
                    break;
                case FederElement federElement:
                    alleFederElemente.Add(federElement);
                    break;
            }

        var alleQuerschnitte = _stabwerkModell.Querschnitt.Select(item => item.Value).ToList();

        if (alleFachwerkelemente.Count != 0)
        {
            zeilen.Add("\nFachwerk");
            zeilen.AddRange(alleFachwerkelemente.Select(item => item.ElementId + "\t"
                                                                               + item.KnotenIds[0] + "\t" + item.KnotenIds[1] + "\t"
                                                                               + item.ElementMaterialId + "\t" + item.ElementQuerschnittId));
        }

        if (alleBiegebalken.Count != 0)
        {
            zeilen.Add("\nBiegebalken");
            zeilen.AddRange(alleBiegebalken.Select(item => item.ElementId + "\t"
                                                                          + item.KnotenIds[0] + "\t" + item.KnotenIds[1] + "\t"
                                                                          + item.ElementMaterialId + "\t" + item.ElementQuerschnittId));
        }

        if (alleBiegebalkenGelenk.Count != 0)
        {
            zeilen.Add("\nBiegebalkenGelenk");
            zeilen.AddRange(alleBiegebalkenGelenk.Select(item => item.ElementId + "\t"
                + item.KnotenIds[0] + "\t" + item.KnotenIds[1] + "\t"
                + item.ElementMaterialId + "\t" + item.ElementQuerschnittId + "\t" + item.Typ));
        }

        if (alleFederElemente.Count != 0)
        {
            zeilen.Add("\nFederelement");
            zeilen.AddRange(alleFederElemente.Select(item => item.ElementId + "\t"
                                                                            + item.KnotenIds[0] + "\t" + item.ElementMaterialId));
        }

        if (alleQuerschnitte.Count != 0)
        {
            zeilen.Add("\nQuerschnitt");
            foreach (var item in alleQuerschnitte)
            {
                sb.Clear();
                sb.Append(item.QuerschnittId + "\t" + item.QuerschnittsWerte[0]);
                for (var i = 1; i < item.QuerschnittsWerte.Length; i++) sb.Append("\t" + item.QuerschnittsWerte[i]);
                zeilen.Add(sb.ToString());
            }
        }

        // Materialien
        zeilen.Add("\nMaterial");
        foreach (var item in _stabwerkModell.Material)
        {
            sb.Clear();
            sb.Append(item.Value.MaterialId + "\t" + item.Value.MaterialWerte[0]);
            for (var i = 1; i < item.Value.MaterialWerte.Length; i++) sb.Append("\t" + item.Value.MaterialWerte[i]);
            zeilen.Add(sb.ToString());
        }

        // Lasten
        if (_stabwerkModell.Lasten.Count > 0)
        {
            zeilen.Add("\nKnotenlast");
            foreach (var item in _stabwerkModell.Lasten)
            {
                sb.Clear();
                sb.Append(item.Value.LastId + "\t" + item.Value.KnotenId + "\t" + item.Value.Lastwerte[0]);
                for (var i = 1; i < item.Value.Lastwerte.Length; i++) sb.Append("\t" + item.Value.Lastwerte[i]);
                zeilen.Add(sb.ToString());
            }
        }

        if (_stabwerkModell.PunktLasten.Count > 0)
        {
            zeilen.Add("\nPunktlast");
            foreach (var punktlast in _stabwerkModell.PunktLasten.Select(item
                         => (PunktLast)item.Value))
            {
                sb.Clear();
                zeilen.Add(punktlast.LastId + "\t" + punktlast.ElementId
                           + "\t" + punktlast.Lastwerte[0] + "\t" + punktlast.Lastwerte[1] + "\t" + punktlast.Offset);
            }
        }

        if (_stabwerkModell.ElementLasten.Count > 0)
        {
            zeilen.Add("\nLinienlast");
            foreach (var item in _stabwerkModell.ElementLasten)
            {
                sb.Clear();
                zeilen.Add(item.Value.LastId + "\t" + item.Value.ElementId
                           + "\t" + item.Value.Lastwerte[0] + "\t" + item.Value.Lastwerte[1]
                           + "\t" + item.Value.Lastwerte[2] + "\t" + item.Value.Lastwerte[3]
                           + "\t" + item.Value.InElementKoordinatenSystem);
            }
        }


        // Randbedingungen
        var fest = string.Empty;
        if (_stabwerkModell.Randbedingungen.Count > 0)
        {
            zeilen.Add("\nLager");
            foreach (var item in _stabwerkModell.Randbedingungen)
            {
                fest = item.Value.Typ switch
                {
                    1 => "x",
                    2 => "y",
                    3 => "xy",
                    7 => "xyr",
                    _ => fest
                };
                zeilen.Add(item.Value.RandbedingungId + "\t" + item.Value.KnotenId + "\t" + fest);
            }
        }

        // Eigenlösungen
        if (_stabwerkModell.Eigenzustand != null)
        {
            zeilen.Add("\nEigenlösungen");
            zeilen.Add(_stabwerkModell.Eigenzustand.Id + "\t" + _stabwerkModell.Eigenzustand.AnzahlZustände);

            // Dämpfung
            if ((_stabwerkModell.Eigenzustand.DämpfungsRaten != null))
            {
                zeilen.Add("\nDämpfung");
                sb.Clear();
                var dämpfungsmaße = _stabwerkModell.Eigenzustand.DämpfungsRaten.Cast<ModaleWerte>().ToList();
                sb.Append(dämpfungsmaße[0].Dämpfung);
                for (var i = 1; i < dämpfungsmaße.Count; i++) sb.Append("\t" + dämpfungsmaße[i].Dämpfung);
                zeilen.Add(sb.ToString());
            }
        }

        // Zeitintegration
        if (_stabwerkModell.Zeitintegration != null)
        {
            zeilen.Add("\nZeitintegration");
            zeilen.Add(_stabwerkModell.Zeitintegration.Id + "\t" + _stabwerkModell.Zeitintegration.Tmax + "\t"
                       + _stabwerkModell.Zeitintegration.Dt + "\t" + _stabwerkModell.Zeitintegration.Methode + "\t"
                       + _stabwerkModell.Zeitintegration.Parameter1 + "\t" +
                       _stabwerkModell.Zeitintegration.Parameter2);

            // Anfangsbedingungen
            if (_stabwerkModell.Zeitintegration.Anfangsbedingungen.Count > 0)
            {
                zeilen.Add("\nAnfangsbedingungen");
                foreach (var item in _stabwerkModell.Zeitintegration.Anfangsbedingungen)
                {
                    sb.Clear();
                    sb.Append(item.KnotenId);
                    foreach (var wert in item.Werte) sb.Append("\t" + wert);
                    zeilen.Add(sb.ToString());
                }
            }
        }

        // Zeitabhängige Knotenlast
        if (_stabwerkModell.ZeitabhängigeKnotenLasten.Count > 0)
        {
            zeilen.Add("\nZeitabhängige Knotenlast");
            foreach (var item in _stabwerkModell.ZeitabhängigeKnotenLasten)
            {
                switch (item.Value.VariationsTyp)
                {
                    case 0: // Anregung aus Datei
                        zeilen.Add(item.Key + "\t" + item.Value.KnotenId + "\t" + item.Value.KnotenFreiheitsgrad);
                        zeilen.Add("Datei");
                        break;
                    case 1: // Anregung aus Zeit-/Wert-Intervall Paaren
                        {
                            zeilen.Add(item.Key + "\t" + item.Value.KnotenId + "\t" + item.Value.KnotenFreiheitsgrad);
                            sb.Clear();
                            sb.Append(item.Value.Intervall[0] + ";" + item.Value.Intervall[1]);
                            for (var i = 2; i < item.Value.Intervall.Length; i = i + 2)
                            {
                                sb.Append("\t" + item.Value.Intervall[i] + ";" + item.Value.Intervall[i + 1]);
                            }
                            zeilen.Add(sb.ToString());
                            break;
                        }
                    case 2: // harmonische Anregung
                        zeilen.Add(item.Key + "\t" + item.Value.KnotenId + "\t" + item.Value.KnotenFreiheitsgrad);
                        sb.Clear();
                        sb.Append(item.Value.Amplitude + "\t" + item.Value.Frequenz + "\t" + item.Value.PhasenWinkel);
                        zeilen.Add(sb.ToString());
                        break;
                }
            }
        }

        // Dateiende
        zeilen.Add("\nend");
        return zeilen;
    }
}