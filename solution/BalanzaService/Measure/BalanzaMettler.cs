namespace BalanzaService.Measure;

public class BalanzaMettler : IBalanza
{
    public string Nombre => "Mettler";

    public bool PuedeParsesr(string data)
    {
        return data.StartsWith("Date");
    }

    public double? ParsearPeso(string data)
    {
        try
        {
            int startPos = data.IndexOf("Gross");
            int endPos = data.IndexOf("Tare");

            if (startPos == -1 || endPos == -1 || startPos >= endPos)
                return null;

            string weightLine = data.Substring(startPos + 5, endPos - startPos - 5);
            int weightEndPos = weightLine.IndexOf("kg");

            if (weightEndPos == -1)
                return null;

            string weightValue = weightLine.Substring(0, weightEndPos).Trim();

            if (double.TryParse(weightValue, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double peso))
            {
                return peso;
            }
        }
        catch
        {
        }

        return null;
    }
}
