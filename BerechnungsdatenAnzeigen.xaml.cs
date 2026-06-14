namespace FE_Berechnungen;

public partial class BerechnungsdatenAnzeigen
{
    private Berechnung _modellBerechnung;
    public BerechnungsdatenAnzeigen(Berechnung modellBerechnung)
    {
        InitializeComponent();
        _modellBerechnung = modellBerechnung;
    }
    public void ShowMatrix()
    {
        var dt = MatrizenAlgebra.ViewMatrix(_modellBerechnung.SystemGleichungen.Matrix);
        MatrixGrid.ItemsSource = dt.DefaultView;
    }
    public void ShowVektor()
    {
        var dt = MatrizenAlgebra.ViewVektor(_modellBerechnung.SystemGleichungen.Vektor);
        VektorGrid.ItemsSource = dt.DefaultView;
    }
    public void ShowPrimal()
    {
        var dt = MatrizenAlgebra.ViewVektor(_modellBerechnung.SystemGleichungen.Primal);
        PrimalGrid.ItemsSource = dt.DefaultView;
    }
    public void ShowDual()
    {
        var dt = MatrizenAlgebra.ViewVektor(_modellBerechnung.SystemGleichungen.Dual);
        DualGrid.ItemsSource = dt.DefaultView;
    }
}