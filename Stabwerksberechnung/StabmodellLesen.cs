namespace FE_Berechnungen.Stabwerksberechnung;

internal class StabmodellLesen
{
    private FeModell _stabwerkModell;
    private string[] _dateiZeilen;
    private FeParser _parse;

    public FeModell StabwerksdatenLesen(string[] zeilen)
    {
        _dateiZeilen = zeilen;
        try
        {
            _parse = new FeParser();
            _parse.ParseModell(_dateiZeilen);
            _stabwerkModell = _parse.FeModell;
            _parse.ParseNodes(_dateiZeilen);

            var tragwerksMaterial = new ModelldatenLesen.MaterialParser();
            tragwerksMaterial.ParseMaterials(_dateiZeilen, _stabwerkModell);

            var tragwerksElemente = new ModelldatenLesen.ElementParser();
            tragwerksElemente.ParseElements(_dateiZeilen, _stabwerkModell);

            var tragwerksLasten = new ModelldatenLesen.LastParser();
            tragwerksLasten.ParseLasten(_dateiZeilen, _stabwerkModell);

            var tragwerksRandbedingungen = new ModelldatenLesen.RandbedingungParser();
            tragwerksRandbedingungen.ParseRandbedingungen(_dateiZeilen, _stabwerkModell);

            var tragwerksTransient = new ModelldatenLesen.TransientParser();
            tragwerksTransient.ParseZeitintegration(_dateiZeilen, _stabwerkModell);

            _stabwerkModell.ZeitintegrationDaten = tragwerksTransient.ZeitintegrationDaten;
            _stabwerkModell.Berechnet = false;
            _stabwerkModell.ZeitintegrationBerechnet = false;
        }
        catch (ParseAusnahme e2)
        {
            _ = MessageBox.Show(e2.Message);
        }

        return _stabwerkModell;
    }
}