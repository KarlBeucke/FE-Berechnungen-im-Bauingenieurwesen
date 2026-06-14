namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public class TragwerksParser : FeParser
{
    public static RandbedingungenParser ParseRandbedingungen;
    private FeModell _modell;
    private ElementParser _parseElastizitätsElemente;
    private LastParser _parseElastizitätsLasten;
    private MaterialParser _parseElastizitätsMaterial;

    // Eingabedaten für eine Tragwerksberechnung aus Datei lesen
    public void ParseTragwerk(string[] lines, FeModell feModell)
    {
        _modell = feModell;
        _parseElastizitätsElemente = new ElementParser();
        _parseElastizitätsElemente.ParseElements(lines, _modell);

        _parseElastizitätsMaterial = new MaterialParser();
        _parseElastizitätsMaterial.ParseMaterials(lines, _modell);

        _parseElastizitätsLasten = new LastParser();
        _parseElastizitätsLasten.ParseLasten(lines, _modell);

        ParseRandbedingungen = new RandbedingungenParser();
        ParseRandbedingungen.ParseRandbedingungen(lines, _modell);
    }
}