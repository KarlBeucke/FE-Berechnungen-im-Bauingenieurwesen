using FE_Berechnungen.Stabwerksberechnung.Modelldaten;
using FE_Berechnungen.Stabwerksberechnung.ModelldatenAnzeigen;

namespace FE_Berechnungen.Stabwerksberechnung.ModelldatenLesen;

public partial class ZeitKnotenlastNeu
{
    private readonly FeModell _modell;
    private ZeitEinflussKeys _einflussKeys;
    public string AktuelleId;

    public ZeitKnotenlastNeu(FeModell modell)
    {
        InitializeComponent();
        _modell = modell;
        AktuelleId = "";
        Show();
    }
    public ZeitKnotenlastNeu(FeModell modell, AbstraktZeitabhängigeKnotenlast zeitKnotenlast)
    {
        InitializeComponent();
        _modell = modell;
        LastId.Text = zeitKnotenlast.LastId;
        AktuelleId = zeitKnotenlast.LastId;
        KnotenId.Text = zeitKnotenlast.KnotenId;
        KnotenDof.Text = zeitKnotenlast.KnotenFreiheitsgrad.ToString();
        if (zeitKnotenlast.Bodenanregung) Bodenanregung.IsChecked = true;
        if (zeitKnotenlast.Datei != "") Datei.IsChecked = true;
        Amplitude.Text = zeitKnotenlast.Amplitude.ToString("G2");
        Frequenz.Text = zeitKnotenlast.Frequenz.ToString("G2");
        Winkel.Text = zeitKnotenlast.PhasenWinkel.ToString("G2");
        if (zeitKnotenlast.Intervall != null)
        {
            var knotenlinear = "";
            for (var i = 0; i < zeitKnotenlast.Intervall.Length; i += 2)
            {
                knotenlinear += zeitKnotenlast.Intervall[i].ToString("G2") + ";";
                knotenlinear += zeitKnotenlast.Intervall[i + 1].ToString("G2") + "  ";
            }
            Linear.Text = knotenlinear;
        }
        Show();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        string knotenlastId;
        if (LastId.Text.Length > 0) knotenlastId = LastId.Text;
        else { _ = MessageBox.Show("ZeitKnotenlast Id muss definiert sein", "neue ZeitKnotenlast"); return; }

        // vorhandene zeitabhängige Knotenlast
        if (_modell.ZeitabhängigeKnotenLasten.TryGetValue(knotenlastId, out var vorhandeneZeitKnotenlast))
        {
            vorhandeneZeitKnotenlast.Datei = "";
            vorhandeneZeitKnotenlast.Amplitude = 0;
            vorhandeneZeitKnotenlast.Frequenz = 0;
            vorhandeneZeitKnotenlast.PhasenWinkel = 0;
            vorhandeneZeitKnotenlast.Intervall = null;
            if (KnotenId.Text.Length > 0)
                vorhandeneZeitKnotenlast.KnotenId = KnotenId.Text.ToString(CultureInfo.CurrentCulture);
            try
            {
                if (KnotenDof.Text.Length > 0) vorhandeneZeitKnotenlast.KnotenFreiheitsgrad = int.Parse(KnotenDof.Text);
                if (Bodenanregung.IsChecked == true) vorhandeneZeitKnotenlast.Bodenanregung = true;
                if (Datei.IsChecked == true)
                {
                    vorhandeneZeitKnotenlast.Datei = "";
                    vorhandeneZeitKnotenlast.VariationsTyp = 0;
                }
                else if (Amplitude.Text.Length > 0)
                {
                    vorhandeneZeitKnotenlast.Amplitude = double.Parse(Amplitude.Text);
                    if (Frequenz.Text.Length > 0) vorhandeneZeitKnotenlast.Frequenz = double.Parse(Frequenz.Text);
                    if (Winkel.Text.Length > 0) vorhandeneZeitKnotenlast.PhasenWinkel = double.Parse(Winkel.Text);
                    vorhandeneZeitKnotenlast.VariationsTyp = 2;
                }
                else if (Linear.Text.Length > 0)
                {
                    var knotenlinear = Linear.Text;
                    char[] delimiters = [' ', ';'];
                    var substrings = knotenlinear.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                    var interval = new double[substrings.Length];
                    for (var j = 0; j < substrings.Length; j += 2)
                    {
                        interval[j] = double.Parse(substrings[j]);
                        interval[j + 1] = double.Parse(substrings[j + 1]);
                    }
                    vorhandeneZeitKnotenlast.Intervall = interval;
                    vorhandeneZeitKnotenlast.VariationsTyp = 1;
                }
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("ungültiges Format in der Eingabe", "Neue zeitabhängige Knotenlast");
                return;
            }
        }

        // neue zeitabhängige Knotenlast
        else
        {
            var knotenId = "";
            var knotenDof = 0;
            var boden = false;
            ZeitabhängigeKnotenLast zeitKnotenlast = null;

            // test, ob Lastknoten definiert ist im Benutzerdialog
            if (KnotenId.Text.Length > 0) knotenId = KnotenId.Text.ToString(CultureInfo.CurrentCulture);
            else { _ = MessageBox.Show("Lastknoten nicht definiert im Benutzerdialog", "neue zeitabhängige Knotenlast"); return; }
            // test, ob Lastknoten im Modell (Dictionary) vorhanden ist
            if (_modell.Knoten.TryGetValue(knotenId, out _)) { }
            else { _ = MessageBox.Show("Lastknoten im Modell nicht gefunden", "neue zeitabhängige Knotenlast"); return; }

            try
            {
                if (KnotenDof.Text.Length > 0) knotenDof = int.Parse(KnotenDof.Text);
                if (Bodenanregung.IsChecked == true) boden = true;
                if (Datei.IsChecked == true)
                {
                    zeitKnotenlast = new ZeitabhängigeKnotenLast(LastId.Text, knotenId, knotenDof, "", boden)
                    {
                        LastId = knotenlastId,
                        VariationsTyp = 0
                    };
                }
                else if (Amplitude.Text.Length > 0 && Frequenz.Text.Length > 0 && Winkel.Text.Length > 0)
                {
                    zeitKnotenlast = new ZeitabhängigeKnotenLast(LastId.Text, knotenId, knotenDof, boden)
                    {
                        LastId = knotenlastId,
                        Amplitude = double.Parse(Amplitude.Text),
                        Frequenz = double.Parse(Frequenz.Text),
                        PhasenWinkel = double.Parse(Winkel.Text),
                        VariationsTyp = 2
                    };
                }

                if (Linear.Text.Length > 0)
                {
                    var knotenlinear = Linear.Text;
                    char[] delimiters = [' ', ';'];
                    var substrings = knotenlinear.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                    var intervall = new double[substrings.Length];
                    for (var j = 0; j < substrings.Length; j += 2)
                    {
                        intervall[j] = double.Parse(substrings[j]);
                        intervall[j + 1] = double.Parse(substrings[j + 1]);
                    }

                    zeitKnotenlast = new ZeitabhängigeKnotenLast(LastId.Text, knotenId, knotenDof, "", boden)
                    {
                        LastId = knotenlastId,
                        Intervall = intervall,
                        VariationsTyp = 3
                    };
                }
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("ungültiges Format in der Eingabe", "neue zeitabhängige Knotenlast");
                return;
            }

            _modell.ZeitabhängigeKnotenLasten.Add(knotenlastId, zeitKnotenlast);
            StartFenster.StabwerkVisual.IsZeitKnotenlast = true;
        }

        if (AktuelleId != LastId.Text) _modell.ZeitabhängigeKnotenLasten.Remove(AktuelleId);

        Close();
        StartFenster.StabwerkVisual.Close();
        StartFenster.StabwerkVisual = new StabwerkmodellVisualisieren(_modell);
        StartFenster.StabwerkVisual.Show();
        _modell.Berechnet = false;
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        StartFenster.StabwerkVisual.IsZeitKnotenlast = false;
        Close();
    }

    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        if (!_modell.ZeitabhängigeKnotenLasten.Remove(LastId.Text, out _)) return;
        Close();
        StartFenster.StabwerkVisual.Close();

        StartFenster.StabwerkVisual = new StabwerkmodellVisualisieren(_modell);
        StartFenster.StabwerkVisual.Show();
        _modell.Berechnet = false;
    }

    private void LastIdGotFocus(object sender, RoutedEventArgs e)
    {
        _einflussKeys = new ZeitEinflussKeys(_modell) { Topmost = true, Owner = (Window)Parent };
        _einflussKeys.Show();
        _einflussKeys.Focus();
    }
    private void LastIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!_modell.ZeitabhängigeKnotenLasten.TryGetValue(LastId.Text, out var vorhandeneZeitKnotenlast))
        {
            _einflussKeys.Close();
            return;
        }

        // vorhandene zeitabhängige Knotenlastdefinition
        LastId.Text = vorhandeneZeitKnotenlast.LastId;
        KnotenId.Text = vorhandeneZeitKnotenlast.KnotenId;
        KnotenDof.Text = vorhandeneZeitKnotenlast.KnotenFreiheitsgrad.ToString(CultureInfo.CurrentCulture);
        if (vorhandeneZeitKnotenlast.Bodenanregung.Equals(true)) Bodenanregung.IsChecked = true;
        switch (vorhandeneZeitKnotenlast.VariationsTyp)
        {
            case 0:
                Datei.IsChecked = true;
                break;
            case 1:
                {
                    if (vorhandeneZeitKnotenlast.Intervall != null)
                    {
                        var knotenlinear = "";
                        for (var i = 0; i < vorhandeneZeitKnotenlast.Intervall.Length; i += 2)
                        {
                            knotenlinear += vorhandeneZeitKnotenlast.Intervall[i].ToString("G2") + ";";
                            knotenlinear += vorhandeneZeitKnotenlast.Intervall[i + 1].ToString("G2") + "  ";
                        }
                        Linear.Text = knotenlinear;
                    }
                    break;
                }
            case 2:
                Amplitude.Text = vorhandeneZeitKnotenlast.Amplitude.ToString("G2");
                Frequenz.Text = (vorhandeneZeitKnotenlast.Frequenz / (2 * Math.PI)).ToString("G2");
                Winkel.Text = (vorhandeneZeitKnotenlast.PhasenWinkel * 180 / Math.PI).ToString("G2");
                break;
        }
        Show();
        _einflussKeys.Close();
    }
    private void KnotenIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!_modell.Knoten.TryGetValue(KnotenId.Text, out var vorhandenerKnoten))
        {
            if (KnotenId.Text == "boden") return;
            _ = MessageBox.Show("Knoten nicht im Modell gefunden", "neue zeitabhängige Knotenlast");
            LastId.Text = "";
            KnotenId.Text = "";
        }
        else
        {
            KnotenId.Text = vorhandenerKnoten.Id;
            if (LastId.Text != "") return;
            LastId.Text = "zkl_" + KnotenId.Text;
            //AktuelleId = LastId.Text;
        }
    }

    private void KnotenPositionNeu(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        _modell.Knoten.TryGetValue(KnotenId.Text, out var knoten);
        if (knoten == null) { _ = MessageBox.Show("Knoten nicht im Modell gefunden", "neue zeitabhängige Knotenlast"); return; }
        StartFenster.StabwerkVisual.KnotenEdit(knoten);
        Close();
        _modell.Berechnet = false;
    }
}