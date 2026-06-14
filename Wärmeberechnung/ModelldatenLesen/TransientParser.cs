using FE_Berechnungen.Wärmeberechnung.Modelldaten;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen;

public class TransientParser
{
    private readonly char[] _delimiters = ['\t', ';'];
    public bool ZeitintegrationDaten;

    public void ParseZeitintegration(string[] lines, FeModell feModell)
    {
        //suche "Eigenlösungen"
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
            var teilStrings = lines[i + 1].Split(_delimiters);
            try
            {
                var tmax = double.Parse(teilStrings[1]);
                var dt = double.Parse(teilStrings[2]);
                var alfa = double.Parse(teilStrings[3]);
                feModell.Zeitintegration = new Zeitintegration(tmax, dt, alfa)
                { Id = teilStrings[0], VonStationär = false };
                ZeitintegrationDaten = true;
                break;
            }
            catch (FormatException)
            {
                throw new ParseAusnahme((i + 2) + ":\nZeitintegration, ungültiges Eingabeformat");
            }
        }

        // suche Anfangstemperaturen
        // Liste "Anfangsbedingungen" wird in Klasse Zeitintegration des FeModell instantiiert
        // Klasse Knotenwerte mit KnotenId und Werten ist in FEBibliothek definiert
        for (var i = 0; i < lines.Length; i++)
        {
            // stationäre Lösung oder knotenId (incl. "alle") mit Knotenwerten
            try
            {
                if (lines[i] != "Anfangstemperaturen") continue;
                FeParser.EingabeGefunden += "\nAnfangstemperaturen";
                do
                {
                    var teilStrings = lines[i + 1].Split(_delimiters);
                    if (teilStrings[0] == "stationäre Lösung")
                    {
                        feModell.Zeitintegration.VonStationär = true;
                    }
                    else if (teilStrings.Length == 2)
                    {
                        // knotenId inkl. alle
                        var knotenId = teilStrings[0];
                        var t0 = double.Parse(teilStrings[1]);
                        var initial = new double[1];
                        initial[0] = t0;
                        feModell.Zeitintegration.Anfangsbedingungen.Add(new Knotenwerte(knotenId, initial));
                    }
                    i++;
                } while (lines[i + 1].Length != 0);
                break;
            }
            catch (FormatException)
            {
                throw new ParseAusnahme((i + 2) + ":\nAnfangsbedingungen, ungültiges Eingabeformat");
            }
        }

        // suche zeitabhängige Randtemperaturen, eingeprägte Temperatur am Rand
        //  datei:      Name, NodeId, datei
        //  harmonisch: Name, NodeId, harmonisch, Amplitude, Frequenz, Phase 
        //  linear:     Name, NodeId, linear, Wertepaare für stückweise linearen Verlauf
        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i] != "Zeitabhängige Randbedingungen") continue;
            FeParser.EingabeGefunden += "\nZeitabhängige Randbedingungen";
            i++;
            do
            {
                var teilStrings = lines[i].Split(_delimiters);
                if (teilStrings.Length < 4)
                    throw new ParseAusnahme(i + 1 + ":\nZeitabhängige Randbedingungen, falsche Anzahl Parameter");

                var randbedingungId = teilStrings[0];
                var knotenId = teilStrings[1];

                ZeitabhängigeRandbedingung zeitabhängigeRandbedingung;

                switch (teilStrings[2])
                {
                    case "datei":
                        {
                            var datei = teilStrings[3];
                            zeitabhängigeRandbedingung = new ZeitabhängigeRandbedingung(knotenId, datei)
                            { VariationsTyp = 0 };
                            break;
                        }
                    case "harmonisch":
                        {
                            var amplitude = double.Parse(teilStrings[3]);
                            var frequenz = double.Parse(teilStrings[4]);
                            var phasenWinkel = double.Parse(teilStrings[5]);
                            zeitabhängigeRandbedingung = new ZeitabhängigeRandbedingung(knotenId, amplitude, frequenz, phasenWinkel)
                            { RandbedingungId = randbedingungId, VariationsTyp = 2, Vordefiniert = new double[1] };
                            break;
                        }
                    case "linear":
                        {
                            var interval = new double[teilStrings.Length - 3];
                            for (var j = 3; j < teilStrings.Length; j += 2)
                            {
                                interval[j - 3] = double.Parse(teilStrings[j]);
                                interval[j - 2] = double.Parse(teilStrings[j + 1]);
                            }
                            zeitabhängigeRandbedingung = new ZeitabhängigeRandbedingung(knotenId, interval)
                            { RandbedingungId = randbedingungId, VariationsTyp = 1, Vordefiniert = new double[1] };
                            break;
                        }
                    default:
                        throw new ParseAusnahme(i + 1 + ":\nZeitabhängige Randbedingungen, muss datei, harmonisch oder linear sein");
                }
                feModell.ZeitabhängigeRandbedingung.Add(randbedingungId, zeitabhängigeRandbedingung);
                i += 1;
            } while (lines[i].Length != 0);

            break;
        }

        // suche zeitabhängige Knotenlast (Temperaturen) Knotentemperaturen
        // datei:      Name, NodeId, datei
        // harmonisch: Name, NodeId, harmonisch, Amplitude, Frequenz, Phase 
        // linear:     Name, NodeId, linear, Wertepaare für stückweise linearen Verlauf
        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i] != "Zeitabhängige Knotenlasten") continue;
            FeParser.EingabeGefunden += "\nZeitabhängige Knotenlasten";
            i++;
            do
            {
                var teilStrings = lines[i].Split(_delimiters);
                if (teilStrings.Length < 4)
                    throw new ParseAusnahme(i + ":\nZeitabhängige Knotenlast, falsche Anzahl Parameter");
                var lastId = teilStrings[0];
                var knotenId = teilStrings[1];

                ZeitabhängigeKnotenLast zeitabhängigeKnotenLast;

                switch (teilStrings[2])
                {
                    case "datei":
                        var datei = teilStrings[3];
                        zeitabhängigeKnotenLast = new ZeitabhängigeKnotenLast(knotenId, datei)
                        { LastId = lastId };
                        break;

                    case "harmonisch":
                        var amplitude = double.Parse(teilStrings[3]);
                        var frequenz = double.Parse(teilStrings[4]);
                        var phasenWinkel = double.Parse(teilStrings[5]);
                        zeitabhängigeKnotenLast = new ZeitabhängigeKnotenLast(knotenId, amplitude, frequenz, phasenWinkel)
                        { LastId = lastId };
                        break;

                    case "linear":
                        var intervall = new double[teilStrings.Length - 3];
                        for (var j = 3; j < teilStrings.Length; j += 2)
                        {
                            intervall[j - 3] = double.Parse(teilStrings[j]);
                            intervall[j - 2] = double.Parse(teilStrings[j + 1]);
                        }
                        zeitabhängigeKnotenLast = new ZeitabhängigeKnotenLast(knotenId, intervall)
                        { LastId = lastId };
                        break;

                    default:
                        throw new ParseAusnahme(i + 2 + ":\nZeitabhängige Knotenlast, muss datei, harmonisch oder linear sein");
                }

                feModell.ZeitabhängigeKnotenLasten.Add(lastId, zeitabhängigeKnotenLast);
                i += 1;
            } while (lines[i].Length != 0);

            break;
        }

        // suche zeitabhängigeElementLast auf Dreieckselementen
        // 6: Name, ElementId, konstant, Knotenwert1, Knotenwert2, Knotenwert3 
        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i] != "Zeitabhängige Elementtemperaturen") continue;
            FeParser.EingabeGefunden += "\nZeitabhängige Elementtemperaturen";
            var knotenWerte = new double[3];
            i += 1;
            do
            {
                var teilStrings = lines[i].Split(_delimiters);
                var loadId = teilStrings[0];
                var elementId = teilStrings[1];
                ZeitabhängigeElementLast zeitabhängigeElementLast;

                if (teilStrings.Length < 4)
                    throw new ParseAusnahme(i + 1 + ":\nZeitabhängige Elementtemperaturen, falsche Anzahl Parameter");

                switch (teilStrings[2])
                {
                    case "konstant":
                        try
                        {
                            for (var k = 3; k < teilStrings.Length; k++)
                                knotenWerte[k - 3] = double.Parse(teilStrings[k]);
                            zeitabhängigeElementLast = new ZeitabhängigeElementLast(elementId, knotenWerte)
                            { LastId = loadId, VariationsTyp = 1 };
                            break;
                        }
                        catch (FormatException)
                        {
                            throw new ParseAusnahme((i + 2) + ":\nZeitabhängige Elementtemperaturen, ungültiges Eingabeformat");
                        }
                    default:
                        throw new ParseAusnahme(i + 1 + ":\nZeitabhängige Elementtemperaturen, muss konstant sein");
                }

                feModell.ZeitabhängigeElementLasten.Add(loadId, zeitabhängigeElementLast);
                i += 1;
            } while (lines[i].Length != 0);
            break;
        }
    }

    [Serializable]
    private class ParseException : Exception
    {
        public ParseException() { }

        public ParseException(string message) : base(message) { }

        public ParseException(string message, Exception innerException) : base(message, innerException) { }

    }
}