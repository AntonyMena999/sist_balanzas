using System.Text.RegularExpressions;

namespace BalanzaService.Measure;

public class BalanzaLP7516 : IBalanza
{
    public string Nombre => "LP7516";

    public bool PuedeParsesr(string data)
    {
        // Formato comando (C18=3): ST,GS,+   0.75kg
        if (data.StartsWith("ST,") || data.StartsWith("US,") || data.StartsWith("OL,"))
            return true;

        // Formato continuo/big display: STX + espacios + peso + kg
        // El carácter STX (0x02) puede estar al inicio
        if (data.Length > 0 && (data[0] == '\u0002' || data[0] == (char)0x02) && data.Contains("kg"))
            return true;

        return false;
    }

    public double? ParsearPeso(string data)
    {
        try
        {
            // Intentar formato comando: ST,GS,+   0.75kg
            var matchComando = Regex.Match(data, @"(\w+),(\w+),([+-])\s*([\d.]+)(\w+)");

            if (matchComando.Success)
            {
                string signo = matchComando.Groups[3].Value;
                string pesoStr = matchComando.Groups[4].Value;

                if (double.TryParse(pesoStr, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out double peso))
                {
                    return signo == "-" ? -peso : peso;
                }
            }

            // Intentar formato continuo/big display: \u0002     1.30 kg PT
            // Remover caracteres de control al inicio (STX, etc)
            var dataSinControl = data.TrimStart('\u0002', '\u0003', (char)0x02, (char)0x03);
            
            // Buscar patrón: espacios + número decimal + espacios + "kg"
            var matchContinuo = Regex.Match(dataSinControl, @"\s*([\d.]+)\s*kg");

            if (matchContinuo.Success)
            {
                string pesoStr = matchContinuo.Groups[1].Value;

                if (double.TryParse(pesoStr, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out double peso))
                {
                    return peso;
                }
            }
        }
        catch
        {
        }

        return null;
    }
}
