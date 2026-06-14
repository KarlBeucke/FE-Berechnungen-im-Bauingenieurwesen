namespace FE_Berechnungen.Tragwerksberechnung;

internal class TragwerkmodellSchreiben
{
    private FeModell _tragwerkModell;

    public List<string> TragwerksdatenSchreiben(FeModell feModell)
    {
        _tragwerkModell = feModell;
        var zeilen = new List<string>
        {
            "ModellName",
            _tragwerkModell.ModellId,
            "\nRaumdimension",
            _tragwerkModell.Raumdimension + "\t" + _tragwerkModell.AnzahlKnotenfreiheitsgrade
        };

        if (_tragwerkModell.Raumdimension == 2)
        {
            zeilen.Add("\nKnoten");
            zeilen.AddRange(_tragwerkModell.Knoten.Select(knoten => knoten.Key
                                           + "\t" + knoten.Value.Koordinaten[0] + "\t" + knoten.Value.Koordinaten[1]));
        }
        else
        {
            zeilen.Add("\nModellabmessungen \n" +
                        +_tragwerkModell.MinX + "\t" + _tragwerkModell.MaxX + "\t"
                        + _tragwerkModell.MinY + "\t" + _tragwerkModell.MaxY + "\t"
                        + _tragwerkModell.MinZ + "\t" + _tragwerkModell.MaxZ);
            zeilen.Add("\nKnoten");
            zeilen.AddRange(_tragwerkModell.Knoten.Select(knoten => knoten.Key
                                            + "\t" + knoten.Value.Koordinaten[0] + "\t" + knoten.Value.Koordinaten[1]
                                            + "\t" + knoten.Value.Koordinaten[2]));
        }

        // Elemente
        var alleElemente2D3 = new List<Modelldaten.Element2D3>();
        var alleElemente3D8 = new List<Modelldaten.Element3D8>();
        foreach (var item in _tragwerkModell.Elemente)
            switch (item.Value)
            {
                case Modelldaten.Element2D3 element2D3:
                    alleElemente2D3.Add(element2D3);
                    break;
                case Modelldaten.Element3D8 element3D8:
                    alleElemente3D8.Add(element3D8);
                    break;
            }

        var alleQuerschnitte = _tragwerkModell.Querschnitt.Select(item => item.Value).ToList();

        if (alleElemente2D3.Count != 0)
        {
            zeilen.Add("\nElement2D3");
            zeilen.AddRange(alleElemente2D3.Select(item => item.ElementId + "\t" + item.KnotenIds[0] + "\t"
                                                           + item.KnotenIds[1] + "\t" + item.KnotenIds[2] + "\t"
                                                           + item.ElementQuerschnittId + "\t" +
                                                           item.ElementMaterialId));
        }

        if (alleElemente3D8.Count != 0)
        {
            zeilen.Add("\nElement3D8");
            zeilen.AddRange(alleElemente3D8.Select(item => item.ElementId + "\t" + item.KnotenIds[0] + "\t"
                                                           + item.KnotenIds[1] + "\t" + item.KnotenIds[2] + "\t" +
                                                           item.KnotenIds[3] + "\t"
                                                           + item.KnotenIds[4] + "\t" + item.KnotenIds[5] + "\t" +
                                                           item.KnotenIds[6] + "\t"
                                                           + item.KnotenIds[7] + "\t" + item.ElementMaterialId));
        }

        if (alleQuerschnitte.Count != 0)
        {
            zeilen.Add("\nQuerschnitt");
            zeilen.AddRange(alleQuerschnitte.Select(item => item.QuerschnittId + "\t"
                                                                               + item.QuerschnittsWerte[0]));
        }

        // Materialien
        zeilen.Add("\n" + "Material");
        var sb = new StringBuilder();
        foreach (var item in _tragwerkModell.Material)
        {
            sb.Clear();
            sb.Append(item.Value.MaterialId + "\t" + item.Value.MaterialWerte[0]);
            for (var i = 1; i < item.Value.MaterialWerte.Length; i++) sb.Append("\t" + item.Value.MaterialWerte[i]);
            zeilen.Add(sb.ToString());
        }

        // Lasten
        if (_tragwerkModell.Lasten.Count > 0) zeilen.Add("\nKnotenlasten");
        foreach (var item in _tragwerkModell.Lasten)
        {
            sb.Clear();
            sb.Append(item.Value.LastId + "\t" + item.Value.KnotenId + "\t" + item.Value.Lastwerte[0]);
            for (var i = 1; i < item.Value.Lastwerte.Length; i++) sb.Append("\t" + item.Value.Lastwerte[i]);
            zeilen.Add(sb.ToString());
        }

        if (_tragwerkModell.LinienLasten.Count > 0) zeilen.Add("\nLinienlasten");
        zeilen.AddRange(_tragwerkModell.LinienLasten.Select(item
            => item.Value.LastId + "\t" + item.Value.StartKnotenId + "\t" + item.Value.Lastwerte[0] + "\t" +
               item.Value.Lastwerte[1]
               + "\t" + item.Value.EndKnotenId + "\t" + item.Value.Lastwerte[2] + "\t" + item.Value.Lastwerte[3]));

        switch (_tragwerkModell.Raumdimension)
        {
            // Randbedingungen
            case 2:
                {
                    var fest = string.Empty;
                    if (_tragwerkModell.Randbedingungen.Count > 0) zeilen.Add("\n" + "Randbedingungen");
                    foreach (var item in _tragwerkModell.Randbedingungen)
                    {
                        sb.Clear();
                        fest = item.Value.Typ switch
                        {
                            1 => "x",
                            2 => "y",
                            3 => "xy",
                            _ => fest
                        };

                        sb.Append(item.Key + "\t" + item.Value.KnotenId + "\t" + fest);
                        foreach (var wert in item.Value.Vordefiniert) sb.Append("\t" + wert);
                        zeilen.Add(sb.ToString());
                    }

                    break;
                }
            case 3:
                {
                    var fest = string.Empty;
                    if (_tragwerkModell.Randbedingungen.Count > 0) zeilen.Add("\n" + "Randbedingungen");
                    foreach (var item in _tragwerkModell.Randbedingungen)
                    {
                        sb.Clear();
                        fest = item.Value.Typ switch
                        {
                            1 => "x",
                            2 => "y",
                            4 => "z",
                            _ => fest
                        };

                        sb.Append(item.Key + "\t" + item.Value.KnotenId + "\t" + item.Value.Face + "\t" + fest);
                        foreach (var wert in item.Value.Vordefiniert) sb.Append("\t" + wert);
                        zeilen.Add(sb.ToString());
                    }

                    break;
                }
        }

        // Dateiende
        zeilen.Add("\nend");
        return zeilen;
    }
}