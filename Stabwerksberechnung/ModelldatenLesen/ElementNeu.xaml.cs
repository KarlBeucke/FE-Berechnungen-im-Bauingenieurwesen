using FE_Berechnungen.Stabwerksberechnung.Modelldaten;
using FE_Berechnungen.Stabwerksberechnung.ModelldatenAnzeigen;

namespace FE_Berechnungen.Stabwerksberechnung.ModelldatenLesen;

public partial class ElementNeu
{
    private readonly FeModell _modell;
    private ElementKeys _elementKeys;
    private MaterialKeys _materialKeys;
    private QuerschnittKeys _querschnittKeys;
    private string _aktuelleId;

    public ElementNeu(FeModell modell)
    {
        InitializeComponent();
        _modell = modell;
        Show();
    }

    private void FachwerkChecked(object sender, RoutedEventArgs e)
    {
        Gelenk1.IsChecked = true;
        Gelenk2.IsChecked = true;
        BalkenCheck.IsChecked = false;
        FederCheck.IsChecked = false;
    }

    private void BalkenChecked(object sender, RoutedEventArgs e)
    {
        Gelenk1.IsChecked = false;
        Gelenk2.IsChecked = false;
        FachwerkCheck.IsChecked = false;
        FederCheck.IsChecked = false;
    }

    private void FederChecked(object sender, RoutedEventArgs e)
    {
        Gelenk1.IsChecked = false;
        Gelenk2.IsChecked = false;
        FachwerkCheck.IsChecked = false;
        BalkenCheck.IsChecked = false;
    }
    private void Gelenk1Checked(object sender, RoutedEventArgs e)
    {
        if (Gelenk1.IsChecked != true || Gelenk2.IsChecked != true) return;
        FachwerkCheck.IsChecked = true;
        BalkenCheck.IsChecked = false;
    }
    private void Gelenk2Checked(object sender, RoutedEventArgs e)
    {
        if (Gelenk1.IsChecked != true || Gelenk2.IsChecked != true) return;
        FachwerkCheck.IsChecked = true;
        BalkenCheck.IsChecked = false;
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        AbstraktElement element = null;
        if (ElementId.Text == "") ElementId.Text = _aktuelleId;

        // vorhandenes Element wird komplett entfernt, da Elementdefinition
        // (Fachwerk, Biegebalken, BiegebalkenGelenk) geändert werden kann
        // neues Element wird angelegt und unter vorhandenem Key gespeichert
        _modell.Elemente.Remove(ElementId.Text);
        var knotenIds = new string[2];
        // test, ob Elementknoten definiert sind im Benutzerdialog
        if (StartknotenId.Text.Length > 0) knotenIds[0] = StartknotenId.Text;
        else { _ = MessageBox.Show("Startknoten nicht definiert", "neues Element"); return; }
        // test, ob Elementknoten im Modell (Dictionary) vorhanden sind
        if (_modell.Knoten.TryGetValue(knotenIds[0], out var startKnoten)) { }
        else { _ = MessageBox.Show("Startknoten im Modell nicht gefunden", "neues Element"); return; }

        Knoten endKnoten = null;
        if (FederCheck.IsChecked != null && (bool)!FederCheck.IsChecked)
        {
            if (EndknotenId.Text.Length > 0) knotenIds[1] = EndknotenId.Text;
            else { _ = MessageBox.Show("Endknoten nicht definiert", "neues Element"); return; }

            if (_modell.Knoten.TryGetValue(knotenIds[1], out endKnoten)) { }
            else { _ = MessageBox.Show("Endknoten im Modell nicht gefunden", "neues Element"); return; }
        }

        try
        {
            if (FachwerkCheck.IsChecked != null && (bool)FachwerkCheck.IsChecked)
            {
                element = new Fachwerk(knotenIds, MaterialId.Text, QuerschnittId.Text, _modell);
                // check, ob ein Knoten des Fachwerkstabs ein eingespannter Knoten ist
                if (endKnoten != null && (Gelenkstab(startKnoten.Id) || (Gelenkstab(endKnoten.Id))))
                    _ = MessageBox.Show("\nGelenkstab '" + ElementId.Text + "' mit Gelenk an Einspannung");
            }
            else if (BalkenCheck.IsChecked != null && (bool)BalkenCheck.IsChecked)
            {
                if (startKnoten.AnzahlKnotenfreiheitsgrade < 3 && endKnoten is { AnzahlKnotenfreiheitsgrade: < 3 })
                {
                    _ = MessageBox.Show("Biegebalken kann nicht an 2 Gelenkknoten angeschlossen werden", "neues Element");
                    return;
                }

                if (_modell.Querschnitt.TryGetValue(QuerschnittId.Text, out var querschnitt))
                {
                    if (querschnitt.QuerschnittsWerte.Length < 2 && Trägheitsmoment.Text.Length == 0)
                    {
                        _ = MessageBox.Show("Trägheitsmoment ist nicht definiert", "neues Element");
                        return;
                    }
                }

                switch (Gelenk1.IsChecked != null && (bool)Gelenk1.IsChecked)
                {
                    // falls Biegebalken angewählt ist und der Biegebalken an Knoten mit <3 Freiheitsgraden angeschlossen ist,
                    // wird Biegebalken automatisch als BiegebalkenGelenk bzw. Fachwerk eingefügt
                    case false when Gelenk2.IsChecked != null && (bool)Gelenk2.IsChecked:   // kein Startknotengelenk, Endknotengelenk, Biegebalken Gelenk 2
                        startKnoten.AnzahlKnotenfreiheitsgrade = 3;
                        element = new BiegebalkenGelenk(knotenIds, MaterialId.Text, QuerschnittId.Text, _modell, 2);
                        break;
                    case true when Gelenk2.IsChecked != null && !(bool)Gelenk2.IsChecked:   // Startknotengelenk, kein Endknotengelenk, Biegebalken Gelenk 1
                        if (Gelenkstab(startKnoten.Id))
                            _ = MessageBox.Show("\nGelenkstab '" + ElementId.Text + "' mit Gelenk an Startknoten");
                        endKnoten?.AnzahlKnotenfreiheitsgrade = 3;
                        element = new BiegebalkenGelenk(knotenIds, MaterialId.Text, QuerschnittId.Text, _modell, 1);
                        break;
                    case false when Gelenk2.IsChecked != null && !(bool)Gelenk2.IsChecked:  // kein Startknotengelenk, kein Endknotengelenk, Biegebalken
                        if (endKnoten != null && Gelenkstab(endKnoten.Id))
                            _ = MessageBox.Show("\nGelenkstab '" + ElementId.Text + "' mit Gelenk an Endknoten");
                        startKnoten.AnzahlKnotenfreiheitsgrade = 3;
                        endKnoten?.AnzahlKnotenfreiheitsgrade = 3;
                        element = new Biegebalken(knotenIds, MaterialId.Text, QuerschnittId.Text, _modell);
                        break;
                    case true when Gelenk2.IsChecked != null && (bool)Gelenk2.IsChecked:    // Startknotengelenk, Endknotengelenk, Fachwerk
                        if (Gelenkstab(startKnoten.Id))
                            _ = MessageBox.Show("\nGelenkstab '" + ElementId.Text + "' mit Gelenk an Startknoten");
                        if (Gelenkstab(endKnoten?.Id))
                            _ = MessageBox.Show("\nGelenkstab '" + ElementId.Text + "' mit Gelenk an Endknoten");
                        element = new Fachwerk(knotenIds, MaterialId.Text, QuerschnittId.Text, _modell);
                        break;
                }
            }
            else if (FederCheck.IsChecked != null && (bool)FederCheck.IsChecked)
            {
                element = new FederElement(knotenIds, MaterialId.Text, _modell);
            }
            else
            {
                _ = MessageBox.Show("Elementtyp muss definiert sein", "neues Element");
                return;
            }
        }
        catch (ModellAusnahme elementNeu)
        {
            _ = MessageBox.Show(elementNeu.Message);
        }

        if (element != null)
        {
            switch (MaterialId.Text)
            {
                case "" when element.E == 0:
                    _ = MessageBox.Show("Element Material nicht definiert", "neues Element");
                    return;
                default:
                    try
                    {
                        element.ElementId = ElementId.Text;
                        if (EModul.Text != string.Empty) element.E = double.Parse(EModul.Text);
                        if (Masse.Text != string.Empty) element.M = double.Parse(Masse.Text);
                    }
                    catch (FormatException)
                    {
                        _ = MessageBox.Show("ungültiges  Eingabeformat", "neues Element");
                        return;
                    }
                    break;
            }
            switch (QuerschnittId.Text)
            {
                case "" when (bool)FederCheck.IsChecked:
                    break;
                case "" when Fläche.Text == "":
                    _ = MessageBox.Show("Element Querschnittsfläche nicht definiert", "neues Element");
                    return;
                case "" when Trägheitsmoment.Text == "" && element is Biegebalken or BiegebalkenGelenk:
                    _ = MessageBox.Show("Element Trägheitsmoment nicht definiert", "neues Element");
                    return;
                default:
                    try
                    {
                        element.ElementId = ElementId.Text;
                        if (Fläche.Text != string.Empty) element.A = double.Parse(Fläche.Text);
                        if (Trägheitsmoment.Text != string.Empty) element.I = double.Parse(Trägheitsmoment.Text);
                    }
                    catch (FormatException)
                    {
                        _ = MessageBox.Show("ungültiges  Eingabeformat", "neues Element");
                        return;
                    }
                    break;
            }
            _modell.Elemente.Add(ElementId.Text, element);
        }

        StartFenster.StabwerkVisual.Close();
        _elementKeys?.Close();
        Close();

        StartFenster.StabwerkVisual = new StabwerkmodellVisualisieren(_modell);
        StartFenster.StabwerkVisual.Show();
        _modell.Berechnet = false;
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        _elementKeys?.Close();
        StartFenster.StabwerkVisual.IsElement = false;
        Close();
    }

    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        if (!_modell.Elemente.Remove(ElementId.Text, out _)) return;
        Close();
        StartFenster.StabwerkVisual.Close();

        StartFenster.StabwerkVisual = new StabwerkmodellVisualisieren(_modell);
        StartFenster.StabwerkVisual.Show();
        _modell.Berechnet = false;
    }

    private void ElementIdGotFocus(object sender, RoutedEventArgs e)
    {
        _aktuelleId = ElementId.Text;
        _elementKeys = new ElementKeys(_modell) { Topmost = true, Owner = (Window)Parent };
        _elementKeys.Show();
        ElementId.Focus();
    }
    private void ElementIdLostFocus(object sender, RoutedEventArgs e)
    {
        _elementKeys?.Close();
        if (!_modell.Elemente.TryGetValue(ElementId.Text, out var vorhandenesElement)) return;

        // Elementtyp aus vorhandener Elementdefinition
        ElementId.Text = vorhandenesElement.ElementId;
        switch (vorhandenesElement)
        {
            case Fachwerk:
                FachwerkCheck.IsChecked = true;
                Gelenk1.IsChecked = true;
                Gelenk2.IsChecked = true;
                BalkenCheck.IsChecked = false;
                EndknotenId.Text = vorhandenesElement.KnotenIds[1];
                break;
            case Biegebalken:
                FachwerkCheck.IsChecked = false;
                BalkenCheck.IsChecked = true;
                EndknotenId.Text = vorhandenesElement.KnotenIds[1];
                break;
            case BiegebalkenGelenk:
                {
                    BalkenCheck.IsChecked = true;
                    switch (vorhandenesElement.Typ)
                    {
                        case 1:
                            Gelenk1.IsChecked = true;
                            break;
                        case 2:
                            Gelenk2.IsChecked = true;
                            break;
                    }

                    FachwerkCheck.IsChecked = false;
                    EndknotenId.Text = vorhandenesElement.KnotenIds[1];
                    break;
                }
            case FederElement:
                FederCheck.IsChecked = true;
                break;
        }

        // Elementeigenschaften aus identifiziertem, vorhandenem Element
        StartknotenId.Text = vorhandenesElement.KnotenIds[0];
        MaterialId.Text = vorhandenesElement.ElementMaterialId;
        EModul.Text = vorhandenesElement.E == 0
            ? string.Empty
            : vorhandenesElement.E.ToString("E2", CultureInfo.CurrentCulture);
        Masse.Text = vorhandenesElement.M == 0
            ? string.Empty
            : vorhandenesElement.M.ToString("E2", CultureInfo.CurrentCulture);
        QuerschnittId.Text = vorhandenesElement.ElementQuerschnittId;
        Fläche.Text = vorhandenesElement.A == 0
            ? string.Empty
            : vorhandenesElement.A.ToString("E2", CultureInfo.CurrentCulture);
        Trägheitsmoment.Text = vorhandenesElement.I == 0
            ? string.Empty
            : vorhandenesElement.I.ToString("E2", CultureInfo.CurrentCulture);
    }

    private void MaterialIdGotFocus(object sender, RoutedEventArgs e)
    {
        _materialKeys = new MaterialKeys(_modell) { Topmost = true, Owner = (Window)Parent };
        _materialKeys.Show();
        MaterialId.Focus();
    }
    private void MaterialIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (_materialKeys.Id != null) MaterialId.Text = _materialKeys.Id;
        _materialKeys.Close();
        //Show();
    }

    private void QuerschnittIdGotFocus(object sender, RoutedEventArgs e)
    {
        _querschnittKeys = new QuerschnittKeys(_modell) { Topmost = true, Owner = (Window)Parent };
        _querschnittKeys.Show();
        QuerschnittId.Focus();
    }

    private void QuerschnittIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (_querschnittKeys.Id != null) QuerschnittId.Text = _querschnittKeys.Id;
        _querschnittKeys.Close();
        //Show();
    }

    private bool Gelenkstab(string knotenId)
    {
        return _modell.Randbedingungen.Any(lager
            => lager.Value.KnotenId == knotenId && lager.Value.Typ == 7);
    }
}