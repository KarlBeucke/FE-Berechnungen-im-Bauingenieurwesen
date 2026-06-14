using FE_Berechnungen.Wärmeberechnung.Modelldaten;

namespace FE_Berechnungen.Wärmeberechnung;

internal class WärmemodellSchreiben
{
    private FeModell _wärmeModell;

    public List<string> WärmedatenSchreiben(FeModell feModell)
    {
        _wärmeModell = feModell;
        var sb = new StringBuilder();
        var zeilen = new List<string>
        {
            "ModellName",
            _wärmeModell.ModellId,
            "\nRaumdimension",
            _wärmeModell.Raumdimension + "\t" + _wärmeModell.AnzahlKnotenfreiheitsgrade,
            // Knoten
            "\nKnoten"
        };

        if (_wärmeModell.Raumdimension == 2)
            zeilen.AddRange(_wärmeModell.Knoten.Select(knoten => knoten.Key
                                                                 + "\t" + knoten.Value.Koordinaten[0] + "\t" +
                                                                 knoten.Value.Koordinaten[1]));
        else
            zeilen.AddRange(_wärmeModell.Knoten.Select(knoten => knoten.Key
                                                                 + "\t" + knoten.Value.Koordinaten[0] + "\t" +
                                                                 knoten.Value.Koordinaten[1] + "\t" +
                                                                 knoten.Value.Koordinaten[2]));

        // Elemente
        var alleElement2D2 = new List<Element2D2>();
        var alleElement2D3 = new List<Element2D3>();
        var alleElement2D4 = new List<Element2D4>();
        var alleElement3D8 = new List<Element3D8>();
        foreach (var item in _wärmeModell.Elemente)
            switch (item.Value)
            {
                case Element2D2 element2D2:
                    alleElement2D2.Add(element2D2);
                    break;
                case Element2D3 element2D3:
                    alleElement2D3.Add(element2D3);
                    break;
                case Element2D4 element2D4:
                    alleElement2D4.Add(element2D4);
                    break;
                case Element3D8 element3D8:
                    alleElement3D8.Add(element3D8);
                    break;
            }

        if (alleElement2D2.Count > 0)
        {
            zeilen.Add("\n" + "Elemente2D2Knoten");
            zeilen.AddRange(alleElement2D2.Select(item => item.ElementId + "\t" + item.KnotenIds[0] + "\t"
                                                          + item.KnotenIds[1] + "\t" + item.ElementMaterialId));
        }

        if (alleElement2D3.Count > 0)
        {
            zeilen.Add("\n" + "Elemente2D3Knoten");
            zeilen.AddRange(alleElement2D3.Select(item => item.ElementId + "\t" + item.KnotenIds[0] + "\t"
                                                          + item.KnotenIds[1] + "\t" + item.KnotenIds[2] + "\t" +
                                                          item.ElementMaterialId));
        }

        if (alleElement2D4.Count > 0)
        {
            zeilen.Add("\n" + "Elemente2D4Knoten");
            zeilen.AddRange(alleElement2D4.Select(item => item.ElementId + "\t" + item.KnotenIds[0] + "\t"
                                                          + item.KnotenIds[1] + "\t" + item.KnotenIds[2] + "\t" +
                                                          item.KnotenIds[3] + "\t" + item.ElementMaterialId));
        }

        if (alleElement3D8.Count > 0)
        {
            zeilen.Add("\n" + "Elemente3D8Knoten");
            zeilen.AddRange(alleElement3D8.Select(item => item.ElementId + "\t" + item.KnotenIds[0] + "\t"
                                                          + item.KnotenIds[1] + "\t" + item.KnotenIds[2] + "\t" +
                                                          item.KnotenIds[3] + "\t"
                                                          + item.KnotenIds[4] + "\t" + item.KnotenIds[5] + "\t" +
                                                          item.KnotenIds[6] + "\t"
                                                          + item.KnotenIds[7] + "\t" + item.ElementMaterialId));
        }

        // Materialien
        if (_wärmeModell.Material.Count > 0)
        {
            zeilen.Add("\n" + "Material");
            foreach (var item in _wärmeModell.Material)
            {
                sb.Clear();
                switch (_wärmeModell.Raumdimension)
                {
                    case 1:
                        {
                            sb.Append(item.Value.MaterialId + "\t" + item.Value.MaterialWerte[0]);
                            if (item.Value.MaterialWerte[3] > 0) sb.Append("\t" + item.Value.MaterialWerte[3]);
                            break;
                        }
                    case 2:
                        {
                            var isotrop = true;
                            var instationär = false;
                            var delta = item.Value.MaterialWerte[0] - item.Value.MaterialWerte[1];
                            if (delta > double.Epsilon) isotrop = false;
                            if (item.Value.MaterialWerte.Length > 3 && item.Value.MaterialWerte[3] > 0) instationär = true;
                            switch (isotrop)
                            {
                                case true when !instationär:
                                    sb.Append(item.Value.MaterialId + "\t" + item.Value.MaterialWerte[0]);
                                    break;
                                case true when instationär:
                                    sb.Append(item.Value.MaterialId + "\t" + item.Value.MaterialWerte[0]
                                              + "\t" + item.Value.MaterialWerte[3]);
                                    break;
                                default:
                                    {
                                        if (!isotrop & !instationär) sb.Append(item.Value.MaterialId + "\t" + item.Value.MaterialWerte[0]
                                                                               + "\t" + item.Value.MaterialWerte[1]);
                                        else if (!isotrop && instationär) sb.Append(item.Value.MaterialId + "\t" + item.Value.MaterialWerte[0]
                                                                                    + "\t" + item.Value.MaterialWerte[1] + "\t"
                                                                                    + item.Value.MaterialWerte[3]);
                                        break;
                                    }
                            }
                            break;
                        }
                    case 3:
                        {
                            var isotrop = true;
                            var instationär = false;
                            var deltaY = item.Value.MaterialWerte[0] - item.Value.MaterialWerte[1];
                            var deltaZ = item.Value.MaterialWerte[0] - item.Value.MaterialWerte[2];
                            if (deltaY > double.Epsilon || deltaZ > double.Epsilon) isotrop = false;
                            if (item.Value.MaterialWerte[3] > 0) instationär = true;
                            switch (isotrop)
                            {
                                case true when !instationär:
                                    sb.Append(item.Value.MaterialId + "\t" + item.Value.MaterialWerte[0]);
                                    break;
                                case true when instationär:
                                    sb.Append(item.Value.MaterialId + "\t" + item.Value.MaterialWerte[0]
                                              + "\t" + item.Value.MaterialWerte[3]);
                                    break;
                                default:
                                    {
                                        if (!isotrop & !instationär) sb.Append(item.Value.MaterialId + "\t" + item.Value.MaterialWerte[0]
                                                                               + "\t" + item.Value.MaterialWerte[1]
                                                                               + "\t" + item.Value.MaterialWerte[2]);
                                        else if (!isotrop && instationär) sb.Append(item.Value.MaterialId + "\t" + item.Value.MaterialWerte[0]
                                                                                    + "\t" + item.Value.MaterialWerte[1] + "\t"
                                                                                    + "\t" + item.Value.MaterialWerte[2] + "\t"
                                                                                    + item.Value.MaterialWerte[3]);
                                        break;
                                    }
                            }

                            break;
                        }
                }
                zeilen.Add(sb.ToString());
            }
        }

        // Lasten
        if (_wärmeModell.Lasten.Count > 0)
        {
            sb.Clear();
            sb.Append("\n" + "KnotenLasten");
            zeilen.Add(sb.ToString());
            foreach (var item in _wärmeModell.Lasten)
            {
                sb.Clear();
                sb.Append(item.Value.LastId + "\t" + item.Value.KnotenId + "\t" + item.Value.Lastwerte[0]);
                for (var i = 1; i < item.Value.Lastwerte.Length; i++) sb.Append("\t" + item.Value.Lastwerte[i]);
                zeilen.Add(sb.ToString());
            }
        }

        if (_wärmeModell.LinienLasten.Count > 0)
        {
            sb.Clear();
            sb.Append("\n" + "LinienLasten");
            zeilen.Add(sb.ToString());
            foreach (var item in _wärmeModell.LinienLasten)
            {
                sb.Clear();
                sb.Append(item.Value.LastId + "\t" + item.Value.StartKnotenId + "\t" + item.Value.EndKnotenId + "\t"
                          + item.Value.Lastwerte[0] + "\t" + item.Value.Lastwerte[1]);
                zeilen.Add(sb.ToString());
            }
        }

        var alleElementlasten3 = new List<ElementLast3>();
        var alleElementlasten4 = new List<ElementLast4>();
        foreach (var item in _wärmeModell.ElementLasten)
            switch (item.Value)
            {
                case ElementLast3 elementlast3:
                    alleElementlasten3.Add(elementlast3);
                    break;
                case ElementLast4 elementlast4:
                    alleElementlasten4.Add(elementlast4);
                    break;
            }

        if (alleElementlasten3.Count > 0)
        {
            zeilen.Add("\n" + "ElementLast3");
            zeilen.AddRange(alleElementlasten3.Select(item => item.LastId + "\t" + item.ElementId + "\t"
                                                              + item.Lastwerte[0] + "\t" + item.Lastwerte[1] + "\t" +
                                                              item.Lastwerte[2]));
        }

        if (alleElementlasten4.Count != 0)
        {
            zeilen.Add("\n" + "ElementLast4");
            zeilen.AddRange(alleElementlasten4.Select(item => item.LastId + "\t" + item.ElementId + "\t"
                                                              + item.Lastwerte[0] + "\t" + item.Lastwerte[1]
                                                              + "\t" + item.Lastwerte[2] + "\t" + item.Lastwerte[3]));
        }

        // Randbedingungen
        if (_wärmeModell.Randbedingungen.Count > 0)
        {
            zeilen.Add("\n" + "Randbedingungen");
            foreach (var item in _wärmeModell.Randbedingungen)
            {
                sb.Clear();
                sb.Append(item.Value.RandbedingungId + "\t" + item.Value.KnotenId + "\t" + item.Value.Vordefiniert[0]);
                zeilen.Add(sb.ToString());
            }
        }

        // Eigenlösungen
        if (_wärmeModell.Eigenzustand != null)
        {
            zeilen.Add("\n" + "Eigenlösungen");
            zeilen.Add(_wärmeModell.Eigenzustand.Id + "\t" + _wärmeModell.Eigenzustand.AnzahlZustände);
        }

        // Zeitintegration Parameter
        if (_wärmeModell.Zeitintegration != null)
        {
            zeilen.Add("\n" + "Zeitintegration");
            zeilen.Add(_wärmeModell.Zeitintegration.Id + "\t" + _wärmeModell.Zeitintegration.Tmax + "\t" +
                       _wärmeModell.Zeitintegration.Dt + "\t" + _wärmeModell.Zeitintegration.Parameter1);

            // Anfangsbedingungen
            if (_wärmeModell.Zeitintegration.VonStationär || _wärmeModell.Zeitintegration.Anfangsbedingungen.Count > 0)
                zeilen.Add("\n" + "Anfangstemperaturen");
            if (_wärmeModell.Zeitintegration.VonStationär)
            {
                zeilen.Add("stationäre Loesung");
            }
            else
            {
                zeilen.AddRange(from Knotenwerte knotenwerte in _wärmeModell.Zeitintegration.Anfangsbedingungen
                                select knotenwerte.KnotenId + "\t" + knotenwerte.Werte[0]);
            }

            // zeitabhängige Randbedingungen
            if (_wärmeModell.ZeitabhängigeRandbedingung.Count > 0)
            {
                zeilen.Add("\n" + "Zeitabhängige Randtemperaturen");
                foreach (var item in _wärmeModell.ZeitabhängigeRandbedingung)
                {
                    sb.Clear();
                    sb.Append(item.Value.RandbedingungId + "\t" + item.Value.KnotenId);
                    switch (item.Value.VariationsTyp)
                    {
                        case 0:
                            sb.Append("\tdatei");
                            break;
                        case 1:
                            sb.Append("\tkonstant" + item.Value.KonstanteTemperatur);
                            break;
                        case 2:
                            sb.Append("\tharmonisch\t" + item.Value.Amplitude + "\t" + item.Value.Frequenz + "\t" +
                                      item.Value.PhasenWinkel);
                            break;
                        case 3:
                            {
                                sb.Append("\tlinear");
                                var anzahlIntervalle = item.Value.Intervall.Length;
                                for (var i = 0; i < anzahlIntervalle; i += 2)
                                    sb.Append("\t" + item.Value.Intervall[i] + ";" + item.Value.Intervall[i + 1]);
                                break;
                            }
                    }
                    zeilen.Add(sb.ToString());
                }
            }
        }

        // zeitabhängige Knotentemperaturen
        if (_wärmeModell.ZeitabhängigeKnotenLasten.Count > 0)
        {
            zeilen.Add("\n" + "Zeitabhängige Knotenlast");
            foreach (var item in _wärmeModell.ZeitabhängigeKnotenLasten)
            {
                sb.Clear();
                sb.Append(item.Value.LastId + "\t" + item.Value.KnotenId);
                switch (item.Value.VariationsTyp)
                {
                    case 0:
                        sb.Append("\tdatei");
                        break;
                    case 2:
                        sb.Append("\tharmonisch\t" + item.Value.Amplitude + "\t" + item.Value.Frequenz + "\t" +
                                  item.Value.PhasenWinkel);
                        break;
                    case 3:
                        {
                            sb.Append("\tlinear");
                            var anzahlIntervalle = item.Value.Intervall.Length;
                            for (var i = 0; i < anzahlIntervalle; i += 2)
                                sb.Append("\t" + item.Value.Intervall[i] + ";" + item.Value.Intervall[i + 1]);

                            break;
                        }
                }
                zeilen.Add(sb.ToString());
            }
        }

        // zeitabhängige Elementtemperaturen
        if (_wärmeModell.ZeitabhängigeElementLasten.Count > 0)
        {
            zeilen.Add("\n" + "Zeitabhängige Elementtemperaturen");
            foreach (var item in _wärmeModell.ZeitabhängigeElementLasten)
            {
                sb.Clear();
                sb.Append(item.Key + "\t" + item.Value.ElementId);

                if (item.Value.VariationsTyp == 1)
                {
                    sb.Append("\tkonstant");
                    foreach (var wert in item.Value.P) sb.Append("\t" + wert);
                }
                zeilen.Add(sb.ToString());
            }
        }

        // Dateiende
        zeilen.Add("\nend");
        return zeilen;
    }
}