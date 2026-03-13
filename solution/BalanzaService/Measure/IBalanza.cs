namespace BalanzaService.Measure;

public interface IBalanza
{
    string Nombre { get; }
    bool PuedeParsesr(string data);
    double? ParsearPeso(string data);
}
