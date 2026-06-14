using FE_Berechnungen.Dateieingabe;
using FE_Berechnungen.Stabwerksberechnung;
using FE_Berechnungen.Stabwerksberechnung.Ergebnisse;
using FE_Berechnungen.Stabwerksberechnung.ModelldatenAnzeigen;
using FE_Berechnungen.Tragwerksberechnung;
using FE_Berechnungen.Tragwerksberechnung.Ergebnisse;
using FE_Berechnungen.Tragwerksberechnung.ModelldatenAnzeigen;
using FE_Berechnungen.Wärmeberechnung;
using FE_Berechnungen.Wärmeberechnung.Ergebnisse;
using FE_Berechnungen.Wärmeberechnung.Modelldaten;
using FE_Berechnungen.Wärmeberechnung.ModelldatenAnzeigen;

namespace FE_Berechnungen;

public partial class StartFenster
{
    private FeModell _stabwerkModell;
    private FeModell _tragwerkModell;
    private FeModell _wärmeModell;
    private Berechnung _modellBerechnung;
    public static StabwerkmodellVisualisieren StabwerkVisual { get; set; }
    public static ErgebnisseVisualisieren StatikErgebnisse { get; private set; }

    public static TragwerksmodellVisualisieren TragwerkVisual { get; set; }
    public static Tragwerksmodell3DVisualisieren TragwerkVisual3D { get; set; }
    private static StatikErgebnisseVisualisieren TragwerksErgebnisse { get; set; }

    public static WärmemodellVisualisieren WärmeVisual { get; set; }
    public static StationäreErgebnisseVisualisieren StationäreErgebnisse { get; private set; }

    public static readonly string Speicherort = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    private OpenFileDialog _dateiDialog;
    private string _dateiPfad;
    private string[] _dateiZeilen;

    public StartFenster()
    {
        InitializeComponent();
        //Speicherort = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    }

    // aktuelle Sicherungsdatei "Beispiele\Sicherungsdatei.bak" laden mit Taste L
    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        var name = Speicherort;
        _dateiPfad = name + "\\FE Berechnungen\\Beispiele\\Sicherungsdatei.bak";
        _dateiZeilen = File.ReadAllLines(_dateiPfad, Encoding.UTF8);
        switch (e.Key)
        {
            case Key.L: // Lesen der aktuellen Sicherungsdatei
                {
                    switch (_dateiZeilen[0])
                    {
                        case "Stabwerksberechnung":
                            {
                                var modellLesen = new StabmodellLesen();
                                _stabwerkModell = modellLesen.StabwerksdatenLesen(_dateiZeilen);
                                StabwerkVisual = new StabwerkmodellVisualisieren(_stabwerkModell);
                                StabwerkVisual.Show();
                                break;
                            }
                        case "Tragwerksberechnung":
                            {
                                var modellLesen = new TragwerkmodellLesen();
                                _tragwerkModell = modellLesen.TragwerksdatenLesen(_dateiZeilen);

                                switch (_tragwerkModell.Raumdimension)
                                {
                                    case 2:
                                        {
                                            TragwerkVisual = new TragwerksmodellVisualisieren(_tragwerkModell);
                                            TragwerkVisual.Show();
                                            break;
                                        }
                                    case 3:
                                        {
                                            TragwerkVisual3D = new Tragwerksmodell3DVisualisieren(_tragwerkModell);
                                            TragwerkVisual3D.Show();
                                            break;
                                        }
                                }

                                break;
                            }
                        case "Wärmeberechnung":
                            {
                                var modellLesen = new WärmemodellLesen();
                                _wärmeModell = modellLesen.WärmedatenLesen(_dateiZeilen);
                                WärmeVisual = new WärmemodellVisualisieren(_wärmeModell);
                                WärmeVisual.Show();
                                break;
                            }
                    }
                    break;
                }
        }
    }

    // Stabwerksberechnung
    private void StabwerksdatenEinlesen(object sender, RoutedEventArgs e)
    {
        //var initial = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\FE Berechnungen";
        var initial = Speicherort + "\\FE Berechnungen";
        _dateiDialog = new OpenFileDialog
        {
            Filter = "inp files (*.inp)|*.inp|All files (*.*)|*.*",
            InitialDirectory = initial
        };

        if (Directory.Exists(_dateiDialog.InitialDirectory))
        {
            _dateiDialog.InitialDirectory += "\\Beispiele\\Stabwerksberechnung";
            _dateiDialog.ShowDialog();
        }
        else
        {
            _ = MessageBox.Show("Directory für FE Berechnungen \"" + initial + "\" nicht gefunden," +
                                " Eingabedatei am Speicherort von \\FE Berechnungen\\Beispiele\\Stabwerksberechnung",
                         "Stabwerksberechnung");
            _dateiDialog.ShowDialog();
        }

        _dateiPfad = _dateiDialog.FileName;

        if (_dateiPfad.Length == 0)
        {
            _ = MessageBox.Show("Eingabedatei ist leer", "Stabwerksberechnung");
            return;
        }

        _dateiZeilen = File.ReadAllLines(_dateiPfad, Encoding.UTF8);

        var modellLesen = new StabmodellLesen();
        _stabwerkModell = modellLesen.StabwerksdatenLesen(_dateiZeilen);

        StabwerkVisual = new StabwerkmodellVisualisieren(_stabwerkModell);
        StabwerkVisual.Show();
    }
    private void StabwerksdatenEditieren(object sender, RoutedEventArgs e)
    {
        if (_dateiPfad == null)
        {
            var tragwerksdaten = new ModelldatenEditieren();
            tragwerksdaten.Show();
        }
        else
        {
            var tragwerksdaten = new ModelldatenEditieren(_dateiPfad);
            tragwerksdaten.Show();
        }
    }
    private void StabwerksdatenSichern(object sender, RoutedEventArgs e)
    {
        var tragwerksdatei = new NeuerDateiname();
        tragwerksdatei.ShowDialog();

        _dateiPfad = _dateiDialog.InitialDirectory + "\\" + tragwerksdatei.DateiName + ".inp";

        var modellSchreiben = new StabmodellSchreiben();
        var zeilen = modellSchreiben.StabwerksdatenSchreiben(_stabwerkModell);

        File.WriteAllLines(_dateiPfad, zeilen);
    }
    private void StabwerksdatenAnzeigen(object sender, RoutedEventArgs e)
    {
        if (_stabwerkModell != null)
        {
            var tragwerk = new StabwerkdatenAnzeigen(_stabwerkModell);
            tragwerk.Show();
        }
        else
        {
            _ = MessageBox.Show("Stabwerksmodelldaten müssen erst definiert werden", "statische Stabwerksanalyse");
        }
    }
    private void StabwerksdatenVisualisieren(object sender, RoutedEventArgs e)
    {
        if (_stabwerkModell != null)
        {
            StabwerkVisual = new StabwerkmodellVisualisieren(_stabwerkModell);
            StabwerkVisual.Show();
        }
        else
        {
            _ = MessageBox.Show("Stabwerksmodelldaten müssen erst definiert werden", "statische Stabwerksanalyse");
        }
    }
    private void StabwerksdatenBerechnen(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_stabwerkModell != null)
            {
                _modellBerechnung = new Berechnung(_stabwerkModell);
                _modellBerechnung.BerechneSystemMatrix();
                _modellBerechnung.BerechneSystemVektor();

                var berechnungsDaten = new BerechnungsdatenAnzeigen(_modellBerechnung) { Topmost = true };
                berechnungsDaten.ShowMatrix();
                berechnungsDaten.ShowVektor();
                berechnungsDaten.ShowDialog();

                _modellBerechnung.LöseGleichungen();
                var statikErgebnisse = new ErgebnisseVisualisieren(_stabwerkModell);
                statikErgebnisse.Show();
            }
            else
            {
                _ = MessageBox.Show("Stabwerksdaten müssen zuerst eingelesen werden", "statische Stabwerksberechnung");
            }
        }
        catch (BerechnungAusnahme e2)
        {
            _ = MessageBox.Show(e2.Message);
        }
    }
    private void StabwerkStatikErgebnisseAnzeigen(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_stabwerkModell != null)
            {
                if (!_stabwerkModell.Berechnet)
                {
                    _modellBerechnung = new Berechnung(_stabwerkModell);
                    _modellBerechnung.BerechneSystemMatrix();
                    _modellBerechnung.BerechneSystemVektor();
                    _modellBerechnung.LöseGleichungen();
                    //_stabwerkModell.Berechnet = true;
                }

                var ergebnisse = new Stabwerksberechnung.Ergebnisse.StatikErgebnisseAnzeigen(_stabwerkModell);
                ergebnisse.Show();
            }
            else
            {
                _ = MessageBox.Show("Stabwerksdaten müssen zuerst eingelesen werden", "statische Stabwerksberechnung");
            }
        }
        catch (BerechnungAusnahme e2)
        {
            _ = MessageBox.Show(e2.Message);
        }
    }
    private void StabwerkStatikErgebnisseVisualisieren(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_stabwerkModell != null)
            {
                if (!_stabwerkModell.Berechnet)
                {
                    _modellBerechnung = new Berechnung(_stabwerkModell);
                    _modellBerechnung.BerechneSystemMatrix();
                    _modellBerechnung.BerechneSystemVektor();
                    _modellBerechnung.LöseGleichungen();
                    _stabwerkModell.Berechnet = true;
                }

                StatikErgebnisse = new ErgebnisseVisualisieren(_stabwerkModell);
                StatikErgebnisse.Show();
            }
            else
            {
                _ = MessageBox.Show("Stabwerksdaten müssen zuerst eingelesen werden", "statische Stabwerksberechnung");
            }
        }
        catch (BerechnungAusnahme e2)
        {
            _ = MessageBox.Show(e2.Message);
        }
    }
    private void StabwerkEigenlösungBerechnen(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_stabwerkModell != null)
            {
                if (!_stabwerkModell.Berechnet)
                {
                    _modellBerechnung ??= new Berechnung(_stabwerkModell);
                    _modellBerechnung.BerechneSystemMatrix();
                    _stabwerkModell.Berechnet = true;
                }

                // default = 2 Eigenzustände, falls nicht anders spezifiziert
                _stabwerkModell.Eigenzustand ??= new Eigenzustände("default", 2);
                if (_stabwerkModell.Eigenzustand.Eigenwerte != null) return;
                _modellBerechnung.Eigenzustände();
                _stabwerkModell.EigenBerechnet = true;
                _ = MessageBox.Show("Eigenfrequenzen erfolgreich ermittelt", "Stabwerksberechnung");
            }
            else
            {
                _ = MessageBox.Show("Modelldaten sind noch nicht spezifiziert", "Stabwerksberechnung");
            }
        }
        catch (BerechnungAusnahme e2)
        {
            _ = MessageBox.Show(e2.Message);
        }
    }
    private void StabwerkEigenlösungAnzeigen(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_stabwerkModell != null)
            {
                if (!_stabwerkModell.Berechnet)
                {
                    _modellBerechnung ??= new Berechnung(_stabwerkModell);
                    _modellBerechnung.BerechneSystemMatrix();
                    _stabwerkModell.Berechnet = true;
                }

                // default = 2 Eigenzustände, falls nicht anders spezifiziert
                _stabwerkModell.Eigenzustand ??= new Eigenzustände("default", 2);
                if (_stabwerkModell.Eigenzustand.Eigenwerte == null) _modellBerechnung.Eigenzustände();
                var eigen = new Stabwerksberechnung.Ergebnisse.EigenlösungAnzeigen(_stabwerkModell);
                eigen.Show();
            }
            else
            {
                _ = MessageBox.Show("Modelldaten sind noch nicht spezifiziert", "Stabwerksberechnung");
            }
        }
        catch (BerechnungAusnahme e2)
        {
            _ = MessageBox.Show(e2.Message);
        }
    }
    private void StabwerkEigenlösungVisualisieren(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_stabwerkModell != null)
            {
                if (!_stabwerkModell.Berechnet)
                {
                    _modellBerechnung ??= new Berechnung(_stabwerkModell);
                    _modellBerechnung.BerechneSystemMatrix();
                    _stabwerkModell.Berechnet = true;
                }

                // default = 2 Eigenzustände, falls nicht anders spezifiziert
                _stabwerkModell.Eigenzustand ??= new Eigenzustände("default", 2);
                if (_stabwerkModell.Eigenzustand.Eigenwerte != null) _modellBerechnung.Eigenzustände();
                var visual = new Stabwerksberechnung.Ergebnisse.EigenlösungVisualisieren(_stabwerkModell);
                visual.Show();
            }
            else
            {
                _ = MessageBox.Show("Modelldaten sind noch nicht spezifiziert", "Stabwerksberechnung");
            }
        }
        catch (BerechnungAusnahme e2)
        {
            _ = MessageBox.Show(e2.Message);
        }
    }
    private void StabwerkDynamischeDaten(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_stabwerkModell == null)
            {
                _ = MessageBox.Show("Stabwerkmodell noch nicht spezifiziert", "Stabwerksberechnung");
                return;
            }

            if (_stabwerkModell.Zeitintegration == null)
            {
                _stabwerkModell.Zeitintegration = new Stabwerksberechnung.Modelldaten.Zeitintegration(0, 0, 0, 0)
                { VonStationär = false };
                _stabwerkModell.ZeitintegrationDaten = true;
            }

            var tragwerk = new DynamikDatenAnzeigen(_stabwerkModell);
            tragwerk.Show();
            _stabwerkModell.ZeitintegrationBerechnet = false;
        }
        catch (BerechnungAusnahme e2)
        {
            _ = MessageBox.Show(e2.Message);
        }
    }
    private void StabwerkAnregungVisualisieren(object sender, RoutedEventArgs e)
    {
        if (_stabwerkModell != null)
        {
            var anregung = new Stabwerksberechnung.ModelldatenAnzeigen.ZeitAnregungVisualisieren(_stabwerkModell);
            anregung.Show();
        }
        else
        {
            _ = MessageBox.Show("Modelldaten für Stabwerkberechnung sind noch nicht spezifiziert", "Stabwerkberechnung");
        }
    }
    private void StabwerkDynamischeBerechnung(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_stabwerkModell.ZeitintegrationDaten && _stabwerkModell != null)
            {
                _modellBerechnung = new Berechnung(_stabwerkModell);
                _modellBerechnung.BerechneSystemMatrix();
                _modellBerechnung.BerechneSystemVektor();
                _modellBerechnung.LöseGleichungen();
                //_stabwerkModell.Berechnet = true;

                _modellBerechnung.ZeitintegrationZweiterOrdnung();
                //_stabwerkModell.ZeitintegrationBerechnet = true;
            }
            else
            {
                _ = MessageBox.Show("Daten für Zeitintegration sind noch nicht spezifiziert", "Stabwerksberechnung");
            }
        }
        catch (BerechnungAusnahme e2)
        {
            _ = MessageBox.Show(e2.Message);
        }
    }
    private void StabwerkDynamischeErgebnisseAnzeigen(object sender, RoutedEventArgs e)
    {
        if (_stabwerkModell is { ZeitintegrationBerechnet: false })
            _ = new DynamischeErgebnisseAnzeigen(_stabwerkModell);
        else
            _ = MessageBox.Show("Zeitintegration noch nicht ausgeführt!!", "dynamische Stabwerksberechnung");
    }
    private void StabwerkDynamischeModellzuständeVisualisieren(object sender, RoutedEventArgs e)
    {
        if (_stabwerkModell is { ZeitintegrationBerechnet: false })
        {
            var dynamikErgebnisse = new DynamischeModellzuständeVisualisieren(_stabwerkModell);
            dynamikErgebnisse.Show();
        }
        else
        {
            _ = MessageBox.Show("Zeitintegration noch nicht ausgeführt!!", "dynamische Stabwerksberechnung");
        }
    }
    private void StabwerkKnotenzeitverläufeTragwerkVisualisieren(object sender, RoutedEventArgs e)
    {
        if (_stabwerkModell is { ZeitintegrationBerechnet: true })
        {
            var knotenzeitverläufe =
                new Stabwerksberechnung.Ergebnisse.KnotenzeitverläufeVisualisieren(_stabwerkModell);
            knotenzeitverläufe.Show();
        }
        else
        {
            _ = MessageBox.Show("Zeitintegration noch nicht ausgeführt!!", "dynamische Stabwerksberechnung");
        }
    }

    // Tragwerksberechnung   
    private void TragwerksdatenEinlesen(object sender, RoutedEventArgs e)
    {
        var initial = Speicherort + "\\FE Berechnungen";
        _dateiDialog = new OpenFileDialog
        {
            Filter = "inp files (*.inp)|*.inp|All files (*.*)|*.*",
            InitialDirectory = initial
        };

        if (Directory.Exists(_dateiDialog.InitialDirectory))
        {
            _dateiDialog.InitialDirectory += "\\Beispiele\\Tragwerksberechnung";
            _dateiDialog.ShowDialog();
        }
        else
        {
            _ = MessageBox.Show("Directory für FE Berechnungen \"" + initial + "\" nicht gefunden," +
                                " Eingabedatei am Speicherort von \\FE Berechnungen\\Beispiele\\Tragwerksberechnung",
                         "Tragwerksberechnung");
            _dateiDialog.ShowDialog();
        }

        _dateiPfad = _dateiDialog.FileName;

        if (_dateiPfad.Length == 0)
        {
            _ = MessageBox.Show("Eingabedatei ist leer", "Tragwerksberechnung");
            return;
        }

        _dateiZeilen = File.ReadAllLines(_dateiPfad, Encoding.UTF8);

        var modellLesen = new TragwerkmodellLesen();
        _tragwerkModell = modellLesen.TragwerksdatenLesen(_dateiZeilen);

        switch (_tragwerkModell.Raumdimension)
        {
            case 2:
                {
                    TragwerkVisual = new TragwerksmodellVisualisieren(_tragwerkModell);
                    TragwerkVisual.Show();
                    break;
                }
            case 3:
                {
                    TragwerkVisual3D = new Tragwerksmodell3DVisualisieren(_tragwerkModell);
                    TragwerkVisual3D.Show();
                    break;
                }
        }
    }
    private void TragwerksdatenEditieren(object sender, RoutedEventArgs e)
    {
        if (_dateiPfad == null)
        {
            var elastizitätsdaten = new ModelldatenEditieren();
            elastizitätsdaten.Show();
        }
        else
        {
            var elastizitätsdaten = new ModelldatenEditieren(_dateiPfad);
            elastizitätsdaten.Show();
        }
    }
    private void TragwerksdatenAnzeigen(object sender, RoutedEventArgs e)
    {
        if (_tragwerkModell == null)
        {
            _ = MessageBox.Show("Modelldaten sind noch nicht spezifiziert", "Tragwerksberechnung");
            return;
        }

        var tragwerk = new TragwerksdatenAnzeigen(_tragwerkModell);
        tragwerk.Show();
    }
    private void TragwerksdatenSichern(object sender, RoutedEventArgs e)
    {
        var elastizitätsdatei = new NeuerDateiname();
        elastizitätsdatei.ShowDialog();
        _dateiPfad = _dateiDialog.InitialDirectory + "\\" + elastizitätsdatei.DateiName + ".inp";

        var modellSchreiben = new TragwerkmodellSchreiben();
        var zeilen = modellSchreiben.TragwerksdatenSchreiben(_tragwerkModell);

        File.WriteAllLines(_dateiPfad, zeilen);
    }
    private void TragwerksdatenVisualisieren(object sender, RoutedEventArgs e)
    {
        if (_tragwerkModell == null)
        {
            _ = MessageBox.Show("Modelldaten sind noch nicht spezifiziert", "Tragwerksberechnung");
            return;
        }

        switch (_tragwerkModell.Raumdimension)
        {
            case 2:
                {
                    TragwerkVisual = new TragwerksmodellVisualisieren(_tragwerkModell);
                    TragwerkVisual.Show();
                    break;
                }
            case 3:
                {
                    TragwerkVisual3D = new Tragwerksmodell3DVisualisieren(_tragwerkModell);
                    TragwerkVisual3D.Show();
                    break;
                }
        }
    }
    private void TragwerksdatenBerechnen(object sender, RoutedEventArgs e)
    {
        if (_tragwerkModell == null)
        {
            _ = MessageBox.Show("Modelldaten für Tragwerksberechnung sind noch nicht spezifiziert",
                "Tragwerksberechnung");
            return;
        }

        try
        {
            _modellBerechnung = new Berechnung(_tragwerkModell);
            _modellBerechnung.BerechneSystemMatrix();
            _modellBerechnung.BerechneSystemVektor();
            _modellBerechnung.LöseGleichungen();
            _tragwerkModell.Berechnet = true;

            _ = MessageBox.Show("Systemgleichungen erfolgreich gelöst", "Tragwerksberechnung");
        }
        catch (BerechnungAusnahme e2)
        {
            _ = MessageBox.Show(e2.Message);
        }
    }
    private void TragwerksberechnungErgebnisse(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!_tragwerkModell.Berechnet)
            {
                if (_tragwerkModell == null)
                {
                    _ = MessageBox.Show("Modelldaten für Tragwerksberechnung sind noch nicht spezifiziert",
                        "Tragwerksberechnung");
                    return;
                }

                _modellBerechnung = new Berechnung(_tragwerkModell);
                _modellBerechnung.BerechneSystemMatrix();
                _modellBerechnung.BerechneSystemVektor();
                _modellBerechnung.LöseGleichungen();
                _tragwerkModell.Berechnet = true;
            }

            var ergebnisse = new Tragwerksberechnung.Ergebnisse.StatikErgebnisseAnzeigen(_tragwerkModell);
            ergebnisse.Show();
        }
        catch (BerechnungAusnahme e2)
        {
            _ = MessageBox.Show(e2.Message);
        }
    }
    private void TragwerksErgebnisseVisualisieren(object sender, RoutedEventArgs e)
    {
        var sb = new StringBuilder();
        try
        {
            if (!_tragwerkModell.Berechnet)
            {
                if (_tragwerkModell == null)
                {
                    _ = MessageBox.Show("Modelldaten für Tragwerksberechnung sind noch nicht spezifiziert",
                        "Tragwerksberechnung");
                    return;
                }

                _modellBerechnung = new Berechnung(_tragwerkModell);
                _modellBerechnung.BerechneSystemMatrix();
                _modellBerechnung.BerechneSystemVektor();
                _modellBerechnung.LöseGleichungen();
                _tragwerkModell.Berechnet = true;
            }
        }
        catch (BerechnungAusnahme e2)
        {
            _ = MessageBox.Show(e2.Message);
        }

        switch (_tragwerkModell!.Raumdimension)
        {
            case 2:
                {
                    TragwerksErgebnisse = new StatikErgebnisseVisualisieren(_tragwerkModell);
                    TragwerksErgebnisse.Show();
                    break;
                }
            case 3:
                {
                    var elastizitätsErgebnisse3D = new StatikErgebnisse3DVisualisieren(_tragwerkModell);
                    elastizitätsErgebnisse3D.Show();
                    break;
                }
            default:
                _ = MessageBox.Show(sb.ToString(), "falsche Raumdimension, muss 2 oder 3 sein");
                break;
        }
    }

    // Wärmeberechnung
    private void WärmedatenEinlesen(object sender, RoutedEventArgs e)
    {
        var initial = Speicherort + "\\FE Berechnungen";
        _dateiDialog = new OpenFileDialog
        {
            Filter = "inp files (*.inp)|*.inp|All files (*.*)|*.*",
            InitialDirectory = initial
        };

        if (Directory.Exists(_dateiDialog.InitialDirectory))
        {
            _dateiDialog.InitialDirectory += "\\Beispiele\\Wärmeberechnung";
            _dateiDialog.ShowDialog();
        }
        else
        {
            _ = MessageBox.Show("Directory für FE Berechnungen \"" + initial + "\" nicht gefunden," +
                                " Eingabedatei am Speicherort von \\FE Berechnungen\\Beispiele\\Wärmeberechnung",
                "Wärmeberechnung");
            _dateiDialog.ShowDialog();
        }

        _dateiPfad = _dateiDialog.FileName;

        if (_dateiPfad.Length == 0)
        {
            _ = MessageBox.Show("Eingabedatei ist leer", "Wärmeberechnung");
            return;
        }

        _dateiZeilen = File.ReadAllLines(_dateiPfad, Encoding.UTF8);

        var modellLesen = new WärmemodellLesen();
        _wärmeModell = modellLesen.WärmedatenLesen(_dateiZeilen);

        WärmeVisual = new WärmemodellVisualisieren(_wärmeModell);
        WärmeVisual.Show();
    }
    private void WärmedatenEditieren(object sender, RoutedEventArgs e)
    {
        if (_dateiPfad == null)
        {
            var wärmeDatenEdit = new ModelldatenEditieren();
            wärmeDatenEdit.Show();
        }
        else
        {
            var wärmeDatenEdit = new ModelldatenEditieren(_dateiPfad);
            wärmeDatenEdit.Show();
        }
    }
    private void WärmedatenSichern(object sender, RoutedEventArgs e)
    {
        var wärmedatei = new NeuerDateiname();
        wärmedatei.ShowDialog();

        _dateiPfad = _dateiDialog.InitialDirectory + "\\" + wärmedatei.DateiName + ".inp";

        var modellSchreiben = new WärmemodellSchreiben();
        var zeilen = modellSchreiben.WärmedatenSchreiben(_wärmeModell);

        File.WriteAllLines(_dateiPfad, zeilen);
    }
    private void WärmedatenAnzeigen(object sender, RoutedEventArgs e)
    {
        if (_wärmeModell != null)
        {
            var wärme = new WärmedatenAnzeigen(_wärmeModell);
            wärme.Show();
        }
        else
        {
            _ = MessageBox.Show("WärmeModelldaten müssen zuerst eingelesen werden", "Wärmeberechnung");
        }
    }
    private void WärmedatenVisualisieren(object sender, RoutedEventArgs e)
    {
        if (_wärmeModell != null)
        {
            WärmeVisual = new WärmemodellVisualisieren(_wärmeModell);
            WärmeVisual.Show();
        }
        else
        {
            _ = MessageBox.Show("WärmeModelldaten müssen zuerst eingelesen werden", "Wärmeberechnung");
        }
    }
    private void WärmedatenBerechnen(object sender, EventArgs e)
    {
        try
        {
            if (_wärmeModell != null)
            {
                _modellBerechnung = new Berechnung(_wärmeModell);
                _modellBerechnung.BerechneSystemMatrix();
                _modellBerechnung.BerechneSystemVektor();
                _modellBerechnung.LöseGleichungen();
                _wärmeModell.Berechnet = true;
                _ = MessageBox.Show("Systemgleichungen erfolgreich gelöst", "Wärmeberechnung");
            }
            else
            {
                _ = MessageBox.Show("WärmeModelldaten müssen zuerst eingelesen werden", "Wärmeberechnung");
            }
        }
        catch (BerechnungAusnahme e2)
        {
            _ = MessageBox.Show(e2.Message);
        }
    }
    private void WärmeberechnungErgebnisseAnzeigen(object sender, EventArgs e)
    {
        try
        {
            if (_wärmeModell != null)
            {
                if (!_wärmeModell.Berechnet)
                {
                    _modellBerechnung = new Berechnung(_wärmeModell);
                    _modellBerechnung.BerechneSystemMatrix();
                    _modellBerechnung.BerechneSystemVektor();
                    _modellBerechnung.LöseGleichungen();
                    _wärmeModell.Berechnet = true;
                }

                var ergebnisse = new StationäreErgebnisseAnzeigen(_wärmeModell);
                ergebnisse.Show();
            }
            else
            {
                _ = MessageBox.Show("Modelldaten für Wärmeberechnung sind noch nicht spezifiziert", "Wärmeberechnung");
            }
        }
        catch (BerechnungAusnahme e2)
        {
            _ = MessageBox.Show(e2.Message);
        }
    }
    private void WärmeberechnungErgebnisseVisualisieren(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_wärmeModell != null)
            {
                if (!_wärmeModell.Berechnet)
                {
                    _modellBerechnung = new Berechnung(_wärmeModell);
                    _modellBerechnung.BerechneSystemMatrix();
                    _modellBerechnung.BerechneSystemVektor();
                    _modellBerechnung.LöseGleichungen();
                    _wärmeModell.Berechnet = true;
                }

                StationäreErgebnisse = new StationäreErgebnisseVisualisieren(_wärmeModell);
                StationäreErgebnisse.Show();
            }
            else
            {
                _ = MessageBox.Show("Modelldaten für Wärmeberechnung sind noch nicht spezifiziert", "Wärmeberechnung");
            }
        }
        catch (BerechnungAusnahme e2)
        {
            _ = MessageBox.Show(e2.Message);
        }
    }
    private void InstationäreDaten(object sender, RoutedEventArgs e)
    {
        if (_wärmeModell != null)
        {
            var wärme = new InstationäreDatenAnzeigen(_wärmeModell);
            wärme.Show();
            _wärmeModell.ZeitintegrationBerechnet = false;
        }
        else
        {
            _ = MessageBox.Show("Modelldaten für Wärmeberechnung sind noch nicht spezifiziert", "Wärmeberechnung");
        }
    }
    private void WärmeAnregungVisualisieren(object sender, RoutedEventArgs e)
    {
        if (_wärmeModell != null)
        {
            var anregung = new Wärmeberechnung.ModelldatenAnzeigen.ZeitAnregungVisualisieren(_wärmeModell);
            anregung.Show();
        }
        else
        {
            _ = MessageBox.Show("Modelldaten für Wärmeberechnung sind noch nicht spezifiziert", "Wärmeberechnung");
        }
    }
    private void EigenlösungWärmeBerechnen(object sender, RoutedEventArgs e)
    {
        if (_wärmeModell != null)
        {
            _modellBerechnung = new Berechnung(_wärmeModell);
            if (!_wärmeModell.Berechnet)
            {
                _modellBerechnung.BerechneSystemMatrix();
                _wärmeModell.Berechnet = true;
            }

            // default = 2 Eigenstates, falls nicht anders spezifiziert
            _wärmeModell.Eigenzustand ??= new Eigenzustände("default", 2);
            if (_wärmeModell.Eigenzustand.Eigenwerte != null) return;
            _modellBerechnung.Eigenzustände();
            _wärmeModell.EigenBerechnet = true;
            _ = MessageBox.Show("Eigenlösung erfolgreich ermittelt", "Wärmeberechnung");
        }
        else
        {
            _ = MessageBox.Show("Modelldaten für Wärmeberechnung sind noch nicht spezifiziert", "Wärmeberechnung");
        }
    }
    private void EigenlösungWärmeAnzeigen(object sender, RoutedEventArgs e)
    {
        if (_wärmeModell != null)
        {
            _modellBerechnung ??= new Berechnung(_wärmeModell);
            if (!_wärmeModell.Berechnet)
            {
                _modellBerechnung.BerechneSystemMatrix();
                _wärmeModell.Berechnet = true;
            }

            // default = 2 Eigenstates, falls nicht anders spezifiziert
            _wärmeModell.Eigenzustand ??= new Eigenzustände("default", 2);
            if (_wärmeModell.Eigenzustand.Eigenwerte == null) _modellBerechnung.Eigenzustände();
            var eigen = new Wärmeberechnung.Ergebnisse.EigenlösungAnzeigen(_wärmeModell); //Eigenlösung.Eigenlösung(modell));
            eigen.Show();
        }
        else
        {
            _ = MessageBox.Show("Modelldaten für Wärmeberechnung sind noch nicht spezifiziert", "Wärmeberechnung");
        }
    }
    private void EigenlösungWärmeVisualisieren(object sender, RoutedEventArgs e)
    {
        if (_wärmeModell != null)
        {
            _modellBerechnung ??= new Berechnung(_wärmeModell);
            if (!_wärmeModell.ZeitintegrationBerechnet)
            {
                _modellBerechnung.BerechneSystemMatrix();
                // default = 2 Eigenzustände, falls nicht anders spezifiziert
                _wärmeModell.Eigenzustand ??= new Eigenzustände("default", 2);
            }

            // default = 2 Eigenzustände, falls nicht anders spezifiziert
            _wärmeModell.Eigenzustand ??= new Eigenzustände("default", 2);
            if (_wärmeModell.Eigenzustand.Eigenwerte == null) _modellBerechnung.Eigenzustände();
            var visual = new Wärmeberechnung.Ergebnisse.EigenlösungVisualisieren(_wärmeModell);
            visual.Show();
        }
        else
        {
            _ = MessageBox.Show("Modelldaten für Wärmeberechnung sind noch nicht spezifiziert", "Wärmeberechnung");
        }
    }
    private void InstationäreBerechnung(object sender, RoutedEventArgs e)
    {
        if (_wärmeModell.ZeitintegrationDaten && _wärmeModell != null)
        {
            if (!_wärmeModell.Berechnet)
            {
                _modellBerechnung = new Berechnung(_wärmeModell);
                _modellBerechnung.BerechneSystemMatrix();
                _modellBerechnung.BerechneSystemVektor();
                _modellBerechnung.LöseGleichungen();
                _wärmeModell.Berechnet = true;
            }

            _modellBerechnung.ZeitintegrationErsterOrdnung();
            _wärmeModell.ZeitintegrationBerechnet = true;
            _ = MessageBox.Show("Zeitintegration erfolgreich durchgeführt", "instationäre Wärmeberechnung");
        }
        else
        {
            _ = MessageBox.Show("Daten für Zeitintegration sind noch nicht spezifiziert", "Wärmeberechnung");
            const double tmax = 0;
            const double dt = 0;
            const double alfa = 0;
            if (_wärmeModell != null)
            {
                _wärmeModell.Zeitintegration = new Zeitintegration(tmax, dt, alfa) { VonStationär = false };
                _wärmeModell.ZeitintegrationDaten = true;
                var wärme = new InstationäreDatenAnzeigen(_wärmeModell);
                wärme.Show();
            }

            _wärmeModell.ZeitintegrationBerechnet = false;
        }
    }
    private void InstationäreErgebnisseAnzeigen(object sender, RoutedEventArgs e)
    {
        //WärmeVisual?.Close();
        if (_wärmeModell.ZeitintegrationBerechnet && _wärmeModell != null)
        {
            var ergebnisse = new InstationäreErgebnisseAnzeigen(_wärmeModell, WärmeVisual);
            ergebnisse.Show();
        }
        else
        {
            _ = MessageBox.Show("Zeitintegration noch nicht ausgeführt!!", "Wärmeberechnung");
        }
    }
    private void InstationäreModellzuständeVisualisieren(object sender, RoutedEventArgs e)
    {
        if (_wärmeModell.ZeitintegrationBerechnet && _wärmeModell != null)
        {
            var modellzuständeVisualisieren = new InstationäreModellzuständeVisualisieren(_wärmeModell);
            modellzuständeVisualisieren.Show();
        }
        else
        {
            _ = MessageBox.Show("Zeitintegration noch nicht ausgeführt!!", "Wärmeberechnung");
        }
    }
    private void KnotenzeitverläufeWärmeVisualisieren(object sender, RoutedEventArgs e)
    {
        if (_wärmeModell.ZeitintegrationBerechnet && _wärmeModell != null)
        {
            var knotenzeitverläufeVisualisieren =
                new Wärmeberechnung.Ergebnisse.KnotenzeitverläufeVisualisieren(_wärmeModell);
            knotenzeitverläufeVisualisieren.Show();
        }
        else
        {
            _ = MessageBox.Show("Zeitintegration noch nicht ausgeführt!!", "Wärmeberechnung");
        }
    }
}