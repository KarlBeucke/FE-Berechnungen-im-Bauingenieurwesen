using FE_Berechnungen.Stabwerksberechnung.ModelldatenAnzeigen;

namespace FE_Berechnungen.Stabwerksberechnung.ModelldatenLesen
{
    public partial class ModellNeueDaten
    {
        private readonly FeModell _modell;

        public ModellNeueDaten(FeModell modell)
        {
            InitializeComponent();
            _modell = modell;
            ModellName.Text = _modell.ModellId;
            Dimension.Text = _modell.Raumdimension.ToString("D");
            Ndof.Text = _modell.AnzahlKnotenfreiheitsgrade.ToString("D");
            MinX.Text = modell.MinX.ToString("G");
            MaxX.Text = modell.MaxX.ToString("G");
            MinY.Text = modell.MinY.ToString("G");
            MaxY.Text = modell.MaxY.ToString("G");

            if (modell.MaxX - modell.MinX < double.Epsilon && modell.MaxY - modell.MinY < double.Epsilon
                                               && modell.Knoten.Count > 0)
            {
                var x = new List<double>();
                var y = new List<double>();

                foreach (var item in modell.Knoten)
                {
                    x.Add(item.Value.Koordinaten[0]);
                    y.Add(item.Value.Koordinaten[1]);
                }

                var xMin = (int)x.Min();
                var xMax = (int)x.Max();
                var yMin = (int)y.Min();
                var yMax = (int)y.Max();
                MinX.Text = xMin.ToString("G");
                MaxX.Text = xMax.ToString("G");
                MinY.Text = yMin.ToString("G");
                MaxY.Text = yMax.ToString("G");

            }
            else
            {
                MinX.Text = modell.MinX.ToString("G");
                MaxX.Text = modell.MaxX.ToString("G");
                MinY.Text = modell.MinY.ToString("G");
                MaxY.Text = modell.MaxY.ToString("G");
            }
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(Dimension.Text, out _) || !int.TryParse(Ndof.Text, out _))
            {
                MessageBox.Show("Bitte geben Sie ganzzahlige Werte ein.", "Fehler", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            if (ModellName.Text.Length > 0) _modell.ModellId = ModellName.Text;
            else { _ = MessageBox.Show("Modell Name nicht definiert", "neue Modelldaten"); return; }
            if (Dimension.Text.Length > 0) _modell.Raumdimension = int.Parse(Dimension.Text);
            else { _ = MessageBox.Show("Raumdimension nicht definiert", "neue Modelldaten"); return; }
            if (Ndof.Text.Length > 0) _modell.AnzahlKnotenfreiheitsgrade = int.Parse(Ndof.Text);
            else { _ = MessageBox.Show("Anzahl der Knotenfreiheitsgrade im Modell nicht definiert", "neue Modelldaten"); return; }

            if (!double.TryParse(MinX.Text, out _) || !double.TryParse(MaxX.Text, out _) ||
                !double.TryParse(MinY.Text, out _) || !double.TryParse(MaxY.Text, out _))
            {
                MessageBox.Show("Bitte geben Sie gültige Werte für Abmessungen ein.", "Fehler", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            if (MinX.Text.Length > 0) _modell.MinX = double.Parse(MinX.Text);
            else { _ = MessageBox.Show("minimale x-Koordinate im Modell nicht definiert", "neue Modelldaten"); return; }
            if (MaxX.Text.Length > 0) _modell.MaxX = double.Parse(MaxX.Text);
            else { _ = MessageBox.Show("maximale x-Koordinate im Modell nicht definiert", "neue Modelldaten"); return; }
            if (MinY.Text.Length > 0) _modell.MinY = double.Parse(MinY.Text);
            else { _ = MessageBox.Show("minimale y-Koordinate im Modell nicht definiert", "neue Modelldaten"); return; }
            if (MaxY.Text.Length > 0) _modell.MaxY = double.Parse(MaxY.Text);
            else { _ = MessageBox.Show("maximale y-Koordinate im Modell nicht definiert", "neue Modelldaten"); return; }

            Close();
            StartFenster.StabwerkVisual.Close();

            StartFenster.StabwerkVisual = new StabwerkmodellVisualisieren(_modell);
            StartFenster.StabwerkVisual.Show();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}