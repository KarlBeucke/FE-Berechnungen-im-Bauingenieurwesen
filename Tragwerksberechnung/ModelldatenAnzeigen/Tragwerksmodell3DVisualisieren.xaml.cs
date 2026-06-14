using FE_Berechnungen.Tragwerksberechnung.Ergebnisse;
using FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Media3D;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenAnzeigen;

public partial class Tragwerksmodell3DVisualisieren
{
    // Veränderung des Kippwinkels, wenn hoch/runter Taste gedrückt wird
    //private const double CameraDPhi = 0.1;
    // Veränderung des Drehwinkels, wenn links/rechts Taste gedrückt wird
    //private const double CameraDTheta = 0.1;
    // Veränderung des Abstands, wenn BildHoch/BildRunter Taste gedrückt wird
    private const double CameraDr = 10;

    // Horizontalverschiebung li/re
    private const double CameraDx = 10;

    // Vertikalverschiebung hoch/runter
    private const double CameraDy = 5;

    private readonly Darstellung3D _darstellung3D;

    // 3D Modellgruppe
    private readonly Model3DGroup _model3DGroup = new();

    // Anfangsposition der Kamera
    private double _cameraPhi = 0.13; // 7,45 Grad
    private double _cameraR = 20.0;
    private double _cameraTheta = 1.65; // 94,5 Grad
    private double _cameraX;
    private double _cameraY;
    private ModelVisual3D _modelVisual;
    private PerspectiveCamera _theCamera;
    private readonly FeModell _tragwerksModell;
    private MaterialNeu _materialNeu;
    private KnotenlastNeu _knotenlastNeu;
    //private LagerNeu _lagerNeu;
    private Berechnung _modellBerechnung;

    public Tragwerksmodell3DVisualisieren(FeModell feModell)
    {
        _tragwerksModell = feModell;
        _darstellung3D = new Darstellung3D(feModell);
        InitializeComponent();
        Koordinaten.IsChecked = true;
        Oberflächen.IsChecked = true;
        Drahtmodell.IsChecked = true;
        RandbedingungenFest.IsChecked = true;
        RandbedingungenVor.IsChecked = true;
        Knotenlasten.IsChecked = true;
        ErstellSzene();
    }

    // Erstellung einer 3D-Szene
    // Viewport ist definiert als Viewport3D im XAML-Code, der alles darstellt 
    private void ErstellSzene()
    {
        // Festlegung der Anfangsposition der Kamera
        _theCamera = new PerspectiveCamera { FieldOfView = 100 };
        View3D.Camera = _theCamera;
        PositionierKamera();

        // Festlegung der Beleuchtung
        FestlegungBeleuchtung();

        // Koordinatensystem
        _darstellung3D.Koordinatensystem(_model3DGroup);

        // Erzeugung des Modells
        _darstellung3D.UnverformteGeometrie(_model3DGroup, true);

        _darstellung3D.Randbedingungen(_model3DGroup);

        _darstellung3D.Knotenlasten(_model3DGroup);

        // Hinzufügen der Modellgruppe (mainModel3DGroup) zu einem neuen ModelVisual3D
        _modelVisual = new ModelVisual3D { Content = _model3DGroup };

        // Darstellung des "modelVisual" im Viewport
        View3D.Children.Add(_modelVisual);
    }

    private void PositionierKamera()
    {
        // z-Blickrichtung, y-up, x-seitlich, _cameraR=Abstand
        // ermittle die Kameraposition in kartesischen Koordinaten
        // y=Abstand*sin(Kippwinkel) (hoch, runter)
        // hypotenuse = Abstand*cos(Kippwinkel)
        // x= hypotenuse * cos(Drehwinkel) (links, rechts)
        // z= hypotenuse * sin(Drehwinkel)
        var y = _cameraR * Math.Sin(_cameraPhi);
        var hyp = _cameraR * Math.Cos(_cameraPhi);
        var x = hyp * Math.Cos(_cameraTheta);
        var z = hyp * Math.Sin(_cameraTheta);
        // Setzen der Kameraposition
        // _cameraX und _cameraY sind die horizontalen und vertikalen Verschiebungen
        _theCamera.Position = new Point3D(x + _cameraX, y + _cameraY, z);
        double offset = 0;

        // Blick in Richtung Koordinatenursprung (0; 0; 0) zentriert, falls
        // Koordinatenursprung links oben, versetz Darstellung um offset
        if (_darstellung3D.MinX >= 0) offset = 10;
        _theCamera.LookDirection = new Vector3D(-(x - offset), -(y + offset), -z);

        // Setzen der Up Richtung
        _theCamera.UpDirection = new Vector3D(0, 1, 0);

        //_ = MessageBox.Show("Camera.Position: (" + x + ", " + y + ", " + z + ")", "3D Wireframe");
    }

    private void FestlegungBeleuchtung()
    {
        var ambientLight = new AmbientLight(Colors.Gray);
        var directionalLight =
            new DirectionalLight(Colors.Gray, new Vector3D(-1.0, -3.0, -2.0));
        _model3DGroup.Children.Add(ambientLight);
        _model3DGroup.Children.Add(directionalLight);
    }

    // Veränderung der Kameraposition mit Tasten hoch/runter, links/rechts, BildUHoch/BildRunter
    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Up: // Vertikalverschiebung
                //cameraPhi -= CameraDPhi;
                //if (cameraPhi > Math.PI / 2.0) cameraPhi = Math.PI / 2.0;
                //ScrPhi.Value = cameraPhi;
                _cameraY -= CameraDy;
                break;
            case Key.Down:
                //cameraPhi += CameraDPhi;
                //if (cameraPhi < -Math.PI / 2.0) cameraPhi = -Math.PI / 2.0;
                //ScrPhi.Value = cameraPhi;
                _cameraY += CameraDy;
                break;

            case Key.Left: // Horizontalverschiebung
                //cameraTheta -= CameraDTheta;
                //ScrTheta.Value = cameraTheta;
                _cameraX += CameraDx;
                break;
            case Key.Right:
                //cameraTheta += CameraDTheta;
                //ScrTheta.Value = cameraTheta;
                _cameraX -= CameraDx;
                break;

            case Key.Add: //  + Ziffernblock
            case Key.OemPlus: //  + alphanumerisch
            case Key.PageUp:
                _cameraR -= CameraDr;
                if (_cameraR < CameraDr) _cameraR = CameraDr;
                break;

            case Key.Subtract: //  - Ziffernblock
            case Key.OemMinus: //  - alphanumerisch
            case Key.PageDown:
                _cameraR += CameraDr;
                if (_cameraR < CameraDr) _cameraR = CameraDr;
                break;
            case Key.S: // sichern der aktuellen Modelldefinition
                var name = StartFenster.Speicherort;
                var dateiPfad = name + "\\FE Berechnungen\\Beispiele\\Sicherungsdatei.bak";
                var modellSchreiben = new TragwerkmodellSchreiben();
                var zeilen = modellSchreiben.TragwerksdatenSchreiben(_tragwerksModell);
                zeilen.Insert(0, "Tragwerksberechnung");
                File.WriteAllLines(dateiPfad, zeilen);
                _ = MessageBox.Show("aktuelle Modelldaten gesichert in " + dateiPfad, "3D Tragwerksmodell visualisieren");
                break;
            default:
                return;
        }

        // Neufestlegung der Kameraposition
        PositionierKamera();
    }

    // Veränderung der Kameraposition mit Scrollbars
    private void ScrThetaScroll(object sender, ScrollEventArgs e)
    {
        _cameraTheta = ScrTheta.Value;
        PositionierKamera();
    }

    private void ScrPhiScroll(object sender, ScrollEventArgs e)
    {
        _cameraPhi = ScrPhi.Value;
        PositionierKamera();
    }

    // An- und Abschalten der einzelnen Modelldarstellungen (GeometryModel3Ds)
    private void ShowKoordinaten(object sender, RoutedEventArgs e)
    {
        foreach (var koordinaten in _darstellung3D.Koordinaten) _model3DGroup.Children.Add(koordinaten);
    }

    private void RemoveKoordinaten(object sender, RoutedEventArgs e)
    {
        foreach (var koordinaten in _darstellung3D.Koordinaten) _model3DGroup.Children.Remove(koordinaten);
    }

    private void ShowOberflächen(object sender, RoutedEventArgs e)
    {
        foreach (var oberflächen in _darstellung3D.Oberflächen) _model3DGroup.Children.Add(oberflächen);
    }

    private void RemoveOberflächen(object sender, RoutedEventArgs e)
    {
        foreach (var oberflächen in _darstellung3D.Oberflächen) _model3DGroup.Children.Remove(oberflächen);
    }

    private void ShowDrahtmodell(object sender, RoutedEventArgs e)
    {
        foreach (var kanten in _darstellung3D.Kanten) _model3DGroup.Children.Add(kanten);
    }

    private void RemoveDrahtmodell(object sender, RoutedEventArgs e)
    {
        foreach (var kanten in _darstellung3D.Kanten) _model3DGroup.Children.Remove(kanten);
    }

    private void ShowRandbedingungenFest(object sender, RoutedEventArgs e)
    {
        foreach (var randbedingungenFest in _darstellung3D.RandbedingungenFest)
            _model3DGroup.Children.Add(randbedingungenFest);
    }

    private void RemoveRandbedingungenFest(object sender, RoutedEventArgs e)
    {
        foreach (var randbedingungenFest in _darstellung3D.RandbedingungenFest)
            _model3DGroup.Children.Remove(randbedingungenFest);
    }

    private void ShowRandbedingungenVor(object sender, RoutedEventArgs e)
    {
        foreach (var randbedingungenVor in _darstellung3D.RandbedingungenVor)
            _model3DGroup.Children.Add(randbedingungenVor);
    }

    private void RemoveRandbedingungenVor(object sender, RoutedEventArgs e)
    {
        foreach (var randbedingungenVor in _darstellung3D.RandbedingungenVor)
            _model3DGroup.Children.Remove(randbedingungenVor);
    }

    private void ShowKnotenlasten(object sender, RoutedEventArgs e)
    {
        foreach (var knotenlasten in _darstellung3D.KnotenLasten) _model3DGroup.Children.Add(knotenlasten);
    }

    private void RemoveKnotenlasten(object sender, RoutedEventArgs e)
    {
        foreach (var knotenlasten in _darstellung3D.KnotenLasten) _model3DGroup.Children.Remove(knotenlasten);
    }

    // Button statische Berechnung
    private void OnBtnBerechnen_Click(object sender, RoutedEventArgs e)
    {
        if (_tragwerksModell == null)
        {
            _ = MessageBox.Show("Modelldaten für Elastizitätsberechnung sind noch nicht spezifiziert",
                "Elastizitätsberechnung");
            return;
        }

        try
        {
            _modellBerechnung = new Berechnung(_tragwerksModell);
            _modellBerechnung.BerechneSystemMatrix();
            _modellBerechnung.BerechneSystemVektor();
            _modellBerechnung.LöseGleichungen();
            _tragwerksModell.Berechnet = true;
        }
        catch (BerechnungAusnahme e2)
        {
            _ = MessageBox.Show(e2.Message);
        }

        var tragwerk = new StatikErgebnisse3DVisualisieren(_tragwerksModell);
        tragwerk.Show();
    }

    // Modell
    private void BtnClickModell(object sender, RoutedEventArgs e)
    {
        var modellNeu = new ModellNeu(_tragwerksModell)
        {
            Topmost = true,
            Owner = (Window)Parent,
            Id = { Text = _tragwerksModell.ModellId },
            Dimension = { Text = _tragwerksModell.Raumdimension.ToString() },
            Ndof = { Text = _tragwerksModell.AnzahlKnotenfreiheitsgrade.ToString() },
            MinX = { Text = _tragwerksModell.MinX.ToString(CultureInfo.CurrentCulture) },
            MaxX = { Text = _tragwerksModell.MaxX.ToString(CultureInfo.CurrentCulture) },
            MinY = { Text = _tragwerksModell.MinY.ToString(CultureInfo.CurrentCulture) },
            MaxY = { Text = _tragwerksModell.MaxY.ToString(CultureInfo.InvariantCulture) }
        };
        if (_tragwerksModell.Raumdimension == 3)
        {
            modellNeu.MinZ.Text = _tragwerksModell.MinZ.ToString(CultureInfo.InvariantCulture);
            modellNeu.MaxZ.Text = _tragwerksModell.MaxZ.ToString(CultureInfo.InvariantCulture);
        }
        else
        {
            modellNeu.MinZ.Text = 0.0.ToString(CultureInfo.CurrentCulture);
            modellNeu.MaxZ.Text = 0.0.ToString(CultureInfo.CurrentCulture);
        }
        modellNeu.Show();
    }

    // Knoten
    private void MenuKnotenNeu(object sender, RoutedEventArgs e)
    {
        _ = new Knoten3DNeu(_tragwerksModell) { Topmost = true, Owner = (Window)Parent };
        _tragwerksModell.Berechnet = false;
    }

    private void MenuKnotenNetzÄquidistant(object sender, RoutedEventArgs e)
    {
        _ = new Knoten3DNetzÄquidistant(_tragwerksModell) { Topmost = true, Owner = (Window)Parent };
    }

    private void MenuKnotenNetzVariabel(object sender, RoutedEventArgs e)
    {
        _ = new Knoten3DNetzVariabel(_tragwerksModell) { Topmost = true, Owner = (Window)Parent };
    }

    // Elemente
    private void MenuElement3D8Neu(object sender, RoutedEventArgs e)
    {
        _ = new Element3D8Neu(_tragwerksModell) { Topmost = true, Owner = (Window)Parent };
        _tragwerksModell.Berechnet = false;
    }
    private void MenuElement3D8Netz(object sender, RoutedEventArgs e)
    {
        _ = new Element3D8Netz(_tragwerksModell) { Topmost = true, Owner = (Window)Parent };
        _tragwerksModell.Berechnet = false;
    }
    private void MenuMaterialNeu(object sender, RoutedEventArgs e)
    {
        _materialNeu = new MaterialNeu(_tragwerksModell) { Topmost = true, Owner = (Window)Parent };
        _materialNeu.AktuelleMaterialId = _materialNeu.MaterialId.Text;
        _materialNeu.AktuelleQuerschnittId = _materialNeu.QuerschnittId.Text;
        _tragwerksModell.Berechnet = false;
    }

    // Lasten
    private void MenuKnotenlastNeu(object sender, RoutedEventArgs e)
    {
        _knotenlastNeu = new KnotenlastNeu(_tragwerksModell);
        _knotenlastNeu.AktuelleId = _knotenlastNeu.LastId.Text;
        _tragwerksModell.Berechnet = false;
    }

    // Randbedingungen
    private void MenuKnotenRandbedingungNeu(object sender, RoutedEventArgs e)
    {
        _ = new Randbedingung3DKnoten(_tragwerksModell) { Topmost = true, Owner = (Window)Parent };
        _tragwerksModell.Berechnet = false;
    }

    private void MenuFlächenRandbedingungNeu(object sender, RoutedEventArgs e)
    {
        _ = new Randbedingung3DFlächen(_tragwerksModell) { Topmost = true, Owner = (Window)Parent };
        _tragwerksModell.Berechnet = false;
    }

    private void MenuBoussinesqRandbedingungNeu(object sender, RoutedEventArgs e)
    {
        _ = new Randbedingung3DBoussinesq(_tragwerksModell) { Topmost = true, Owner = (Window)Parent };
        _tragwerksModell.Berechnet = false;
    }
}