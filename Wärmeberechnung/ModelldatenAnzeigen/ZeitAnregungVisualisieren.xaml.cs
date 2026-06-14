namespace FE_Berechnungen.Wärmeberechnung.ModelldatenAnzeigen
{
    public partial class ZeitAnregungVisualisieren
    {
        private static string _typ = string.Empty;
        public ZeitAnregungVisualisieren(FeModell feModell)
        {
            Language = XmlLanguage.GetLanguage("de-DE");

            // Festlegung der Zeitachse
            var dt = feModell.Zeitintegration.Dt;
            const double tmin = 0;
            var tmax = feModell.Zeitintegration.Tmax;
            var nZeitschritte = (int)(tmax / dt) + 1;
            var funktion = new double[nZeitschritte];
            var lastId = "";
            var knotenId = "";

            foreach (var item in feModell.ZeitabhängigeKnotenLasten)
            {
                switch (item.Value.VariationsTyp)
                {
                    case 0:
                        var eingabe = item.Value.Datei;
                        string pfad;
                        if (eingabe.Length > 0)
                        {
                            //pfad = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                            pfad = StartFenster.Speicherort
                                   + @"\FE Berechnungen\Beispiele\Wärmeberechnung\instationär\Anregungsdateien\" + eingabe;
                        }
                        else
                        {
                            var datei = new OpenFileDialog
                            {
                                InitialDirectory = StartFenster.Speicherort
                                                   + @"\FE Berechnungen\Beispiele\Wärmeberechnung\instationär\Anregungsdateien\",
                                RestoreDirectory = true
                            };
                            if (datei.ShowDialog() != true) return;
                            pfad = datei.FileName;
                        }
                        const int spalte = -1; // ALLE Values in Datei
                        // Ordinatenwerte im Zeitintervall dt aus Datei lesen
                        Berechnung.AusDatei(pfad, spalte, funktion, feModell);
                        break;

                    case 1:
                        var intervall = item.Value.Intervall;
                        Berechnung.StückweiseLinear(intervall, funktion, feModell);
                        break;
                    case 2:
                        var amplitude = item.Value.Amplitude;
                        var frequenz = item.Value.Frequenz;
                        var winkel = item.Value.PhasenWinkel;
                        Berechnung.Periodisch(amplitude, frequenz, winkel, funktion, feModell);
                        break;
                }

                lastId = item.Value.LastId;
                knotenId = item.Value.KnotenId;
                break;
            }

            foreach (var item in feModell.ZeitabhängigeRandbedingung)
            {
                _typ = "zeitabhängige Randbedingung ";
                switch (item.Value.VariationsTyp)
                {
                    case 0:
                        var eingabe = item.Value.Datei;
                        string pfad;
                        if (eingabe.Length > 0)
                        {
                            pfad = StartFenster.Speicherort
                                   + @"\FE Berechnungen\Beispiele\Wärmeberechnung\instationär\Anregungsdateien\" + eingabe;
                        }
                        else
                        {
                            var datei = new OpenFileDialog
                            {
                                InitialDirectory = StartFenster.Speicherort
                                                   + @"\FE Berechnungen\Beispiele\Wärmeberechnung\instationär\Anregungsdateien\",
                                RestoreDirectory = true
                            };
                            if (datei.ShowDialog() != true) return;
                            pfad = datei.FileName;
                        }
                        const int spalte = 0; // ALLE Values in Datei
                        // Ordinatenwerte im Zeitintervall dt aus Datei lesen
                        Berechnung.AusDatei(pfad, spalte, funktion, feModell);
                        break;
                    case 1:
                        var intervall = item.Value.Intervall;
                        Berechnung.StückweiseLinear(intervall, funktion, feModell);
                        break;
                    case 2:
                        var amplitude = item.Value.Amplitude;
                        var frequenz = item.Value.Frequenz;
                        var winkel = item.Value.PhasenWinkel;
                        Berechnung.Periodisch(amplitude, frequenz, winkel, funktion, feModell);
                        break;
                }

                lastId = item.Value.RandbedingungId;
                knotenId = item.Value.KnotenId;
                break;
            }
            if (funktion.Length == 0)
            {
                MessageBox.Show("Keine Anregungswerte gefunden.");
                return;
            }

            var anregungMax = funktion.Max();
            var anregungMin = -anregungMax;

            // Initialisierung der Zeichenfläche
            InitializeComponent();
            Show();
            var darstellung = new Darstellung(feModell, VisualAnregung);

            //var funktion = new double[werte.Count];
            darstellung.Koordinatensystem(tmin, tmax, anregungMax, anregungMin);
            darstellung.ZeitverlaufZeichnen(dt, tmin, tmax, anregungMax, funktion);
            // Textdarstellung der Anregungsdauer mit Anzahl Datenpunkten und Zeitintervall
            AnregungText(lastId, knotenId, funktion.Length * dt, funktion.Length, dt, VisualAnregung);
        }

        private static void AnregungText(string id, string knoten, double dauer, int nSteps, double dt, Canvas anregung)
        {
            var tage = (dauer / 60 / 60 / 24).ToString("N2");
            var anregungsWerte = _typ + "'" + id + "'" + " am Knoten " + "'" + knoten + "', "
                                 + dauer.ToString("N0") + "s (" + tage + "Tage) Anregung  mit "
                                 + nSteps + " Anregungswerten im Zeitschritt dt = "
                                 + dt.ToString("N2");

            var anregungTextBlock = new TextBlock
            {
                FontSize = 12,
                Foreground = Brushes.Black,
                FontWeight = FontWeights.Bold,
                Text = anregungsWerte
            };
            Canvas.SetTop(anregungTextBlock, 10);
            Canvas.SetLeft(anregungTextBlock, 20);
            anregung.Children.Add(anregungTextBlock);
        }
    }
}