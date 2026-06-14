using FE_Berechnungen.Stabwerksberechnung.Modelldaten;

namespace FE_Berechnungen.Stabwerksberechnung.ModelldatenLesen;

public partial class ZeitEinflussKeys
{
    public ZeitEinflussKeys(FeModell modell)
    {
        InitializeComponent();
        Left = 2 * Width;
        Top = Height;
        var lasten = modell.ZeitabhängigeKnotenLasten.Where(item => item.Value is ZeitabhängigeKnotenLast).
            Select(item => item.Value).ToList();
        ZeitlastenKeys.ItemsSource = lasten;

        ZeitAnfangKeys.Items.Clear();
        var anfang = modell.Zeitintegration.Anfangsbedingungen;
        ZeitAnfangKeys.ItemsSource = anfang;
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}