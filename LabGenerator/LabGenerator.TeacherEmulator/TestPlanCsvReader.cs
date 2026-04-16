using System.Globalization;

namespace LabGenerator.TeacherEmulator;

internal static class TestPlanCsvReader
{
    public static List<TestPlanCase> Load(string csvPath)
    {
        if (!File.Exists(csvPath))
        {
            throw new FileNotFoundException("Test plan CSV not found.", csvPath);
        }

        var lines = File.ReadAllLines(csvPath);
        if (lines.Length == 0)
        {
            throw new InvalidOperationException($"Test plan CSV is empty: {csvPath}");
        }

        var cases = new List<TestPlanCase>();

        for (var i = 1; i < lines.Length; i++)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var parts = line.Split(';');
            var col = new string[7];
            for (var j = 0; j < col.Length; j++)
            {
                col[j] = j < parts.Length ? parts[j].Trim() : string.Empty;
            }

            var testNumber = ParsePositiveInt(col[0], "test number", csvPath, i + 1);
            var discipline = col[1];
            if (string.IsNullOrWhiteSpace(discipline))
            {
                throw new InvalidOperationException($"Discipline name is empty at {csvPath}:{i + 1}");
            }

            var labNumber = ParsePositiveInt(col[2], "lab number", csvPath, i + 1);
            var param1 = ParseOptionalPositiveInt(col[3], "param1", csvPath, i + 1);
            var preserve1 = ParseYesNo(col[4], csvPath, i + 1);
            var param2 = ParseOptionalPositiveInt(col[5], "param2", csvPath, i + 1);
            var preserve2 = ParseYesNo(col[6], csvPath, i + 1);

            cases.Add(new TestPlanCase
            {
                TestNumber = testNumber,
                DisciplineName = discipline,
                LabNumber = labNumber,
                Param1 = param1,
                PreserveParam1 = param1 is not null && preserve1,
                Param2 = param2,
                PreserveParam2 = param2 is not null && preserve2
            });
        }

        return cases;
    }

    private static int ParsePositiveInt(string value, string field, string path, int line)
    {
        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) || parsed <= 0)
        {
            throw new InvalidOperationException($"Invalid {field} at {path}:{line}: '{value}'");
        }

        return parsed;
    }

    private static int? ParseOptionalPositiveInt(string value, string field, string path, int line)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return ParsePositiveInt(value, field, path, line);
    }

    private static bool ParseYesNo(string value, string path, int line)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var normalized = value.Trim();
        if (normalized.Equals("да", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (normalized.Equals("нет", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        throw new InvalidOperationException($"Invalid yes/no value at {path}:{line}: '{value}'");
    }
}
