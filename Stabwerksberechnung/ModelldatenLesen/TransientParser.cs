using FE_Berechnungen.Stabwerksberechnung.Modelldaten;

namespace FE_Berechnungen.Stabwerksberechnung.ModelldatenLesen;

internal class TransientParser
{
    private readonly char[] _delimiters = ['\t', ';'];
    public bool ZeitintegrationDaten;

    public void ParseZeitintegration(string[] lines, FeModell feModell)
    {
        // suche "Eigenlösungen"
        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i] != "Eigenlösungen") continue;
            FeParser.EingabeGefunden += "\nEigenlösungen";

            var teilStrings = lines[i + 1].Split(_delimiters);
            if (teilStrings.Length != 2) throw new ParseAusnahme((i + 2) + ":\nEigenlösungen, falsche Anzahl Parameter");
            var id = teilStrings[0];
            int numberOfStates;
            try
            {
                numberOfStates = short.Parse(teilStrings[1]);
            }
            catch (FormatException)
            {
                throw new ParseAusnahme((i + 2) + ":\nEigenlösungen, ungültiges  Eingabeformat");
            }
            feModell.Eigenzustand = new Eigenzustände(id, numberOfStates);
            break;
        }

        // suche "Zeitintegration"
        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i] != "Zeitintegration") continue;
            FeParser.EingabeGefunden += "\nZeitintegration";
            //id, Tmax, dt, method, parameter1, parameter2
            //method=1:beta,gamma  method=2:theta  method=3: alfa
            var teilStrings = lines[i + 1].Split(_delimiters);
            try
            {
                var id = teilStrings[0];
                switch (teilStrings.Length)
                {
                    case 5:                                     // Zeitschrittverfahren mit 1 Parameter
                        var tmax = double.Parse(teilStrings[1]);
                        var dt = double.Parse(teilStrings[2]);
                        var method = short.Parse(teilStrings[3]);
                        var parameter1 = double.Parse(teilStrings[4]);
                        feModell.Zeitintegration = new Zeitintegration(tmax, dt, method, parameter1) { Id = id };
                        break;
                    case 6:                                     // Zeitschrittverfahren mit 2 Parameter
                        tmax = double.Parse(teilStrings[1]);
                        dt = double.Parse(teilStrings[2]);
                        method = short.Parse(teilStrings[3]);
                        parameter1 = double.Parse(teilStrings[4]);
                        var parameter2 = double.Parse(teilStrings[5]);
                        feModell.Zeitintegration = new Zeitintegration(tmax, dt, method, parameter1, parameter2) { Id = id };
                        break;
                    default:
                        throw new ParseAusnahme(i + 2 + ":\nZeitintegration, falsche Anzahl Parameter");
                }
            }
            catch (FormatException)
            {
                throw new ParseAusnahme((i + 2) + ":\nZeitintegration, ungültiges  Eingabeformat");
            }

            ZeitintegrationDaten = true;
        }

        // suche "Dämpfung"
        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i] != "Dämpfung") continue;
            FeParser.EingabeGefunden += "\nDämpfung";
            do
            {
                var teilStrings = lines[i + 1].Split(_delimiters);
                try
                {
                    for (var k = 0; k < teilStrings.Length; k++)
                    {
                        var modalwerte = new ModaleWerte(double.Parse(teilStrings[k]), (k + 1).ToString());
                        feModell.Eigenzustand.DämpfungsRaten.Add(modalwerte);
                    }

                    // wenn nur ein Wert eingegeben wurde, dann wird dieser für alle Eigenzustände verwendet
                    if (teilStrings.Length == 1)
                    {
                        var modalwerte = (ModaleWerte)feModell.Eigenzustand.DämpfungsRaten[0];
                        modalwerte.Text = "alle";
                    }
                }
                catch (FormatException)
                {
                    throw new ParseAusnahme((i + 2) + ":\nDämpfung, ungültiges  Eingabeformat");
                }
                i++;
            } while (lines[i + 1].Length != 0);

            break;
        }

        // suche "Anfangsbedingungen"
        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i] != "Anfangsbedingungen") continue;
            FeParser.EingabeGefunden += "\nAnfangsbedingungen";
            try
            {
                do
                {
                    var teilStrings = lines[i + 1].Split(_delimiters);
                    var anfangsKnotenId = teilStrings[0];
                    // Anfangsverformungen und Geschwindigkeiten
                    var nodalDof = teilStrings.Length switch
                    {
                        3 => 1,
                        5 => 2,
                        7 => 3,
                        _ => throw new ParseAusnahme((i + 2) + ":\nAnfangsbedingungen, falsche Anzahl Parameter")
                    };

                    var anfangsWerte = new double[2 * nodalDof];
                    for (var k = 0; k < 2 * nodalDof; k++) anfangsWerte[k] = double.Parse(teilStrings[k + 1]);
                    feModell.Zeitintegration.Anfangsbedingungen.Add(new Knotenwerte(anfangsKnotenId, anfangsWerte));
                    i++;
                } while (lines[i + 1].Length != 0);

                break;
            }
            catch (FormatException)
            {
                throw new ParseAusnahme((i + 2) + ":\nAnfangsbedingungen, ungültiges Eingabeformat");
            }
        }

        // suche zeitabhängige Knotenlasten
        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i] != "Zeitabhängige Knotenlast") continue;
            FeParser.EingabeGefunden += "\nZeitabhängige Knotenlast";
            var boden = false;
            i++;

            do
            {
                var teilStrings = lines[i].Split(_delimiters);
                if (teilStrings.Length < 4)
                    throw new ParseAusnahme(i + 1 + ":\nZeitabhängige Knotenlast, falsche Anzahl Parameter");

                // Id der Knotenlast, Id des Knoten, Knotenfreiheitsgrad
                var knotenLastId = teilStrings[0];
                var knotenId = teilStrings[1];
                if (knotenId == "boden") boden = true;
                var knotenFreiheitsgrad = short.Parse(teilStrings[2]);

                ZeitabhängigeKnotenLast zeitabhängigeKnotenLast;
                switch (teilStrings[3])
                {
                    // 1 Wert: lies Anregung (Lastvektor) aus Datei, Variationstyp = 0, Dateiname
                    case "datei":
                        {
                            var datei = teilStrings[4];
                            zeitabhängigeKnotenLast =
                                new ZeitabhängigeKnotenLast(knotenLastId, knotenId, knotenFreiheitsgrad, datei, boden)
                                { VariationsTyp = 0 };
                            break;
                        }
                    // 3 Werte: harmonische Anregung, Variationstyp = 2, Amplitude, Frequenz, Pasenwinkel
                    case "harmonisch":
                        {
                            var amplitude = double.Parse(teilStrings[4]);
                            var circularFrequency = double.Parse(teilStrings[5]);
                            var phaseAngle = double.Parse(teilStrings[6]);
                            zeitabhängigeKnotenLast =
                                new ZeitabhängigeKnotenLast(knotenLastId, knotenId, knotenFreiheitsgrad, boden)
                                {
                                    AnregungDauer = 0,
                                    Amplitude = amplitude,
                                    Frequenz = circularFrequency,
                                    PhasenWinkel = phaseAngle,
                                    VariationsTyp = 2
                                };
                            // optional: Anregungsdauer
                            if (teilStrings.Length > 7)
                            {
                                var anregungDauer = int.Parse(teilStrings[7]);
                                zeitabhängigeKnotenLast.AnregungDauer = anregungDauer;
                            }

                            break;
                        }
                    // linear: lies Zeit-/Wert-Intervalle der Anregung mit linearer Interpolation, Variationstyp = 1
                    case "linear":
                        {
                            var interval = new double[teilStrings.Length - 4];
                            for (var j = 4; j < teilStrings.Length; j += 2)
                            {
                                interval[j - 4] = double.Parse(teilStrings[j]);
                                interval[j - 3] = double.Parse(teilStrings[j + 1]);
                            }

                            zeitabhängigeKnotenLast =
                                new ZeitabhängigeKnotenLast(knotenLastId, knotenId, knotenFreiheitsgrad, boden)
                                { Intervall = interval, VariationsTyp = 1 };
                            break;
                        }
                    default:
                        throw new ParseAusnahme((i + 2) + ":\nZeitabhängige Knotenlast, ungültiges  Eingabeformat");
                }

                feModell.ZeitabhängigeKnotenLasten.Add(knotenLastId, zeitabhängigeKnotenLast);
                i += 1;
            } while (lines[i].Length != 0);
        }
    }
}