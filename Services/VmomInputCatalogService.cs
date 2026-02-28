namespace FusimAiAssiant.Services;

public class VmomInputCatalogService
{
    public (Dictionary<string, string> Fields, string TemplateInput) GetCatalog()
    {
        var fields = BuildFieldCatalog(DefaultEqinptTemplate);
        return (fields, DefaultEqinptTemplate);
    }

    private static Dictionary<string, string> BuildFieldCatalog(string eqinptTemplate)
    {
        var lines = eqinptTemplate.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        var result = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('&') || line == "/")
            {
                continue;
            }

            var eqIndex = line.IndexOf('=');
            if (eqIndex <= 0)
            {
                continue;
            }

            var key = line[..eqIndex].Trim();
            if (!string.IsNullOrWhiteSpace(key) && !result.ContainsKey(key))
            {
                result[key] = key;
            }
        }

        return result;
    }

    private const string DefaultEqinptTemplate = """
        &eqinpt
        mflxs = 31,
        mhrms = 3,
        ntheta = 32,
        iradxi = 31,
        eqprzi = 53570.5742, 53570.5742, 51987.7656, 49349.1133, 47106.0469,
                 45132.2461, 43313.0586, 41579.9727, 39783.043, 38105.0547,
                 36583.5312, 35178.8789, 33841.1211, 32476.1191, 30889.7969,
                 28990.8359, 26978.2676, 25038.9785, 23169.3457, 21357.2656,
                 19596.1699, 17877.7852, 16210.5713, 14602.4727, 13055.7363,
                 11576.124, 10159.1309, 8798.20117, 7492.41602, 6250.5918,
                 5020.9834,
        eqiotb = 0.933227777, 0.933227777, 0.817905962, 0.753005147,
                 0.706786156, 0.670540154, 0.640539169, 0.614880621,
                 0.592321098, 0.572136283, 0.553915203, 0.537324965, 0.5221439,
                 0.508198261, 0.495309263, 0.483306974, 0.471977204,
                 0.461156845, 0.450796455, 0.440826744, 0.431193471,
                 0.421913922, 0.412950844, 0.404258609, 0.395835668,
                 0.387739539, 0.379856974, 0.372037679, 0.364421993,
                 0.357147336, 0.350033522,
        rmajor = 7.94513845,
        rminor = 2.8,
        elong = 1.5,
        triang = -0.2,
        shift = 0.05,
        gamma1 = 0,
        ftol = 1E-5,
        itmom = 20000,
        nskip = 200,
        iradin = 11,
        initin = 0,
        lprt = 6,
        /
        """;
}
