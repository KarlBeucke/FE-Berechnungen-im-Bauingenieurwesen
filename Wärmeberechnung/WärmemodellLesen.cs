namespace FE_Berechnungen.Wärmeberechnung;

internal class WärmemodellLesen
{
    private FeModell _wärmeModell;
    private string[] _dateiZeilen;
    private FeParser _parse;

    public FeModell WärmedatenLesen(string[] zeilen)
    {
        _dateiZeilen = zeilen;
        try
        {
            _parse = new FeParser();
            _parse.ParseModell(_dateiZeilen);
            _wärmeModell = _parse.FeModell;
            _parse.ParseNodes(_dateiZeilen);

            var wärmeMaterial = new ModelldatenLesen.MaterialParser();
            wärmeMaterial.ParseMaterials(_dateiZeilen, _wärmeModell);

            var wärmeElemente = new ModelldatenLesen.ElementParser();
            wärmeElemente.ParseWärmeElements(_dateiZeilen, _wärmeModell);

            var wärmeLasten = new ModelldatenLesen.LastParser();
            wärmeLasten.ParseLasten(_dateiZeilen, _wärmeModell);

            var wärmeRandbedingungen = new ModelldatenLesen.RandbedingungParser();
            wärmeRandbedingungen.ParseRandbedingungen(_dateiZeilen, _wärmeModell);

            var wärmeTransient = new ModelldatenLesen.TransientParser();
            wärmeTransient.ParseZeitintegration(_dateiZeilen, _wärmeModell);

            _wärmeModell.ZeitintegrationDaten = wärmeTransient.ZeitintegrationDaten;
            _wärmeModell.Berechnet = false;
            _wärmeModell.ZeitintegrationBerechnet = false;
        }
        catch (ParseAusnahme e2)
        {
            _ = MessageBox.Show(e2.Message);
        }

        return _wärmeModell;
    }
}