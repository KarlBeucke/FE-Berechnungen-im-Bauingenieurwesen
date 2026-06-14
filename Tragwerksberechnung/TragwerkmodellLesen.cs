using FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

namespace FE_Berechnungen.Tragwerksberechnung;

internal class TragwerkmodellLesen
{
    private FeModell _tragwerkModell;
    private string[] _dateiZeilen;
    private FeParser _parse;

    public FeModell TragwerksdatenLesen(string[] zeilen)
    {
        _dateiZeilen = zeilen;
        try
        {
            _parse = new FeParser();
            _parse.ParseModell(_dateiZeilen);
            _tragwerkModell = _parse.FeModell;
            _parse.ParseNodes(_dateiZeilen);

            var parseTragwerk = new TragwerksParser();
            parseTragwerk.ParseTragwerk(_dateiZeilen, _tragwerkModell);

            _tragwerkModell.Berechnet = false;
        }
        catch (ParseAusnahme e2)
        {
            _ = MessageBox.Show(e2.Message);
        }
        return _tragwerkModell;
    }
}