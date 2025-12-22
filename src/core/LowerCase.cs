using System.Text.Json;

namespace LowerCase
{
    public class LowerCaseNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name)
        => name.ToLowerInvariant();
}
    public class CamelCaseNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name)
        => name.ToUpperInvariant();
}

}