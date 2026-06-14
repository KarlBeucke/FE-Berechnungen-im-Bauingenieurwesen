namespace FE_Berechnungen.Stabwerksberechnung.ModelldatenAnzeigen
{
    public partial class ZeitAnregungVisualisieren
    {
        public ZeitAnregungVisualisieren(FeModell feModell)
        {
            Language = XmlLanguage.GetLanguage("de-DE");

            // Festlegung der Zeitachse
            const double tmin = 0;
            var tmax = feModell.Zeitintegration.Tmax;
            var nZeitschritte = (int)(tmax / feModell.Zeitintegration.Dt) + 1;
            var funktion = new double[nZeitschritte];

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
                                   + @"\FE Berechnungen\Beispiele\Stabwerksberechnung\Dynamik\Anregungsdateien\" + eingabe;
                        }
                        else
                        {
                            var datei = new OpenFileDialog
                            {
                                InitialDirectory = StartFenster.Speicherort
                                                   + @"\FE Berechnungen\Beispiele\Stabwerksberechnung\Dynamik\Anregungsdateien\",
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
                        var anregungDauer = item.Value.AnregungDauer;
                        var amplitude = item.Value.Amplitude;
                        var frequenz = item.Value.Frequenz;
                        var winkel = item.Value.PhasenWinkel;
                        if (anregungDauer == 0)
                            Berechnung.Periodisch(amplitude, frequenz, winkel, funktion, feModell);
                        else
                            Berechnung.Periodisch(anregungDauer, amplitude, frequenz, winkel, funktion, feModell);
                        break;
                }

                if (funktion is not { Length: not 0 })
                {
                    MessageBox.Show("Keine Anregungswerte gefunden.");
                    return;
                }

                var anregungMax = funktion.Max();
                //if (anregungMax < double.Epsilon) return;
                var anregungMin = -anregungMax;

                // Initialisierung der Zeichenfläche
                InitializeComponent();
                Show();
                var darstellung = new Darstellung(feModell, VisualAnregung);
                darstellung.Koordinatensystem(tmin, tmax, anregungMax, anregungMin);
                darstellung.ZeitverlaufZeichnen(feModell.Zeitintegration.Dt, tmin, tmax, anregungMax, funktion);
                // Textdarstellung der Anregungsdauer mit Anzahl Datenpunkten und Zeitintervall
                AnregungText(item.Value.LastId, item.Value.KnotenId, funktion.Length * feModell.Zeitintegration.Dt, funktion.Length, feModell.Zeitintegration.Dt, VisualAnregung);
                break;
            }
        }

        private static void AnregungText(string id, string knoten, double dauer, int nSteps, double dt, Canvas anregung)
        {
            var anregungsWerte = "zeitabhängige Knotenlast " + id + " am Knoten " + "'" + knoten + "', "
                                        + dauer.ToString("N2") + " [s] Anregung  mit "
                                        + nSteps + " Anregungswerten im Zeitschritt dt = "
                                        + dt.ToString("N3");
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
