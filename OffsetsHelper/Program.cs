using System.Text.Json;

var offsetsService = new OffsetsService();
var updatedOffsets = await offsetsService.GetCurrentOffsets();

OffsetsService.Log("Updating txt file.");
offsetsService.CreateOffsetsFile(updatedOffsets);

OffsetsService.Log("Finished.");
Console.ReadKey();

public class OffsetsService
{
    string OutputDir = @"output";
    string OutputDumperName = @"offsets.txt";

    public async Task<Offsets> GetCurrentOffsets()
    {
        try
        {
            var values = ReadOffsets();
            var offsets = GetConfigFromDictionary(values);
            //CreateOffsetsFile(offsets);
            return offsets;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n[ERRO] {ex.Message}");
            Console.ResetColor();
            return null;
        }
    }

    public Offsets? TryGetOffsets()
    {
        try
        {
            var json = File.ReadAllText(OutputDumperName);
            return JsonSerializer.Deserialize<Offsets>(json);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n[ERRO] {ex.Message}");
            Console.ResetColor();
            return null;
        }
    }

    public void CreateOffsetsFile(Offsets offsets)
    {
        var json = JsonSerializer.Serialize(offsets, new JsonSerializerOptions { WriteIndented = true });

        // Caminho absoluto para a raiz do projeto
        string projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\"));
        string filePath = Path.Combine(projectRoot, "offsets.txt");

        File.WriteAllText(filePath, json);
    }


    Dictionary<string, long> ReadOffsets()
    {
        Log($"Reading JSONs of '{OutputDir}' ...");

        var clientFile = Path.Combine(OutputDir, "client_dll.json");
        var offsetsFile = Path.Combine(OutputDir, "offsets.json");

        foreach (var f in new[] { clientFile, offsetsFile })
            if (!File.Exists(f))
                throw new FileNotFoundException($"File not found: {f}");

        var clientDoc = JsonDocument.Parse(File.ReadAllText(clientFile));
        var offsetsDoc = JsonDocument.Parse(File.ReadAllText(offsetsFile));

        long ClientField(string className, string fieldName)
        {
            var classes = clientDoc.RootElement
                .GetProperty("client.dll")
                .GetProperty("classes");

            if (!classes.TryGetProperty(className, out var cls))
                throw new KeyNotFoundException($"Class '{className}' not found on client.dll.json");

            if (!cls.GetProperty("fields").TryGetProperty(fieldName, out var val))
                throw new KeyNotFoundException($"Field '{fieldName}' not found on '{className}'");

            return val.GetInt64();
        }

        long ClientOffset(string key)
        {
            var clientOffsets = offsetsDoc.RootElement.GetProperty("client.dll");
            if (!clientOffsets.TryGetProperty(key, out var val))
                throw new KeyNotFoundException($"Offset '{key}' not found on offsets.json");

            return val.GetInt64();
        }

        return new Dictionary<string, long>
        {
            [nameof(Offsets.LocalPlayerPawnOffset)] = ClientField("CCSPlayerController", "m_hPlayerPawn"),
            [nameof(Offsets.PlayerNameOffset)] = ClientField("CBasePlayerController", "m_iszPlayerName"),
            [nameof(Offsets.EntityListOffset)] = ClientOffset("dwEntityList"),
            [nameof(Offsets.HealthOffset)] = ClientField("C_BaseEntity", "m_iHealth"),
            [nameof(Offsets.TeamOffset)] = ClientField("C_BaseEntity", "m_iTeamNum"),
            [nameof(Offsets.PositionOffset)] = ClientField("C_BasePlayerPawn", "m_vOldOrigin"),
            [nameof(Offsets.LocalPlayerPawn)] = ClientOffset("dwLocalPlayerPawn"),
            [nameof(Offsets.MatrixOffset)] = ClientOffset("dwViewMatrix"),
            [nameof(Offsets.GameSceneNodeOffset)] = ClientField("C_BaseEntity", "m_pGameSceneNode"),
            [nameof(Offsets.SkeletonInstanceOffset)] = ClientField("CBodyComponentSkeletonInstance", "m_skeletonInstance"),
            [nameof(Offsets.ModelStateOffset)] = ClientField("CSkeletonInstance", "m_modelState"),
            [nameof(Offsets.LifeStateOffset)] = ClientField("C_BaseEntity", "m_lifeState")
        };
    }

    Offsets GetConfigFromDictionary(Dictionary<string, long> v)
    {
        return new Offsets
        {
            LocalPlayerPawnOffset = (int)v[nameof(Offsets.LocalPlayerPawnOffset)],
            PlayerNameOffset = (int)v[nameof(Offsets.PlayerNameOffset)],
            EntityListOffset = (int)v[nameof(Offsets.EntityListOffset)],
            HealthOffset = (int)v[nameof(Offsets.HealthOffset)],
            TeamOffset = (int)v[nameof(Offsets.TeamOffset)],
            PositionOffset = (int)v[nameof(Offsets.PositionOffset)],
            LocalPlayerPawn = (int)v[nameof(Offsets.LocalPlayerPawn)],
            MatrixOffset = (int)v[nameof(Offsets.MatrixOffset)],
            GameSceneNodeOffset = (int)v[nameof(Offsets.GameSceneNodeOffset)],
            SkeletonInstanceOffset = (int)v[nameof(Offsets.SkeletonInstanceOffset)],
            ModelStateOffset = (int)v[nameof(Offsets.ModelStateOffset)],
            LifeStateOffset = (int)v[nameof(Offsets.LifeStateOffset)]
        };
    }

    public static void Log(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"[*] {msg}");
        Console.ResetColor();
    }
}

public class Offsets
{
    public int LocalPlayerPawnOffset { get; set; } = 2316;
    public int PlayerNameOffset { get; set; } = 1780;
    public int EntityListOffset { get; set; } = 38688144;
    public int HealthOffset { get; set; } = 844;
    public int TeamOffset { get; set; } = 1003;
    public int PositionOffset { get; set; } = 5008;
    public int LocalPlayerPawn { get; set; } = 36959896;
    public int MatrixOffset { get; set; } = 36981552;
    public int GameSceneNodeOffset { get; set; } = 816;
    public int SkeletonInstanceOffset { get; set; } = 128;
    public int ModelStateOffset { get; set; } = 336;
    public int LifeStateOffset { get; set; } = 852;
}

