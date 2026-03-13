namespace BalanzaService.Measure;

public class BalanzaDix : IBalanza
{
    public string Nombre => "Dix";

    public bool PuedeParsesr(string data)
    {
        return !data.StartsWith("Date") &&
               !data.StartsWith("ST,") &&
               !data.StartsWith("US,") &&
               !data.StartsWith("OL,") &&
               !data.StartsWith("GROSS", StringComparison.OrdinalIgnoreCase) &&
               data.Contains("kg");
    }

    public double? ParsearPeso(string data)
    {
        try
        {
            int startPos = data.IndexOf("0 ");
            int endPos = data.IndexOf("kg");

            if (startPos == -1 || endPos == -1 || startPos >= endPos)
                return null;

            string weightValue = data.Substring(startPos + 1, endPos - startPos - 1).Trim();

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
