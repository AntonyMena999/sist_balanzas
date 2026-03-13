namespace BalanzaService.Measure;

public class BalanzaGross : IBalanza
{
    public string Nombre => "Gross";

    public bool PuedeParsesr(string data)
    {
        return data.StartsWith("GROSS", StringComparison.OrdinalIgnoreCase);
    }

    public double? ParsearPeso(string data)
    {
        try
        {
            // Formato: GROSS    6.5 kg
            // Eliminar "GROSS", luego espacios, quedarse con el peso
            var sinGross = data.Substring(5).Trim();

            int kgPos = sinGross.IndexOf("kg", StringComparison.OrdinalIgnoreCase);
            if (kgPos == -1)
                return null;

            string weightValue = sinGross.Substring(0, kgPos).Trim();

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
