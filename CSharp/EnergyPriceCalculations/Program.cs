using CsvHelper;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using UnitsNet.Units;
using System;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Text.Json.Nodes;
using Newtonsoft.Json.Linq;

Newtonsoft.Json.Linq.JObject summary = new Newtonsoft.Json.Linq.JObject();

foreach (string f in Directory.GetFiles("C:/Projects/Code/EnergyPriceCalculations/Dataset"))
{
    var filename = f.Split("/").Last().Split("\\").Last();
    if (filename.StartsWith("production"))
    {
        Newtonsoft.Json.Linq.JObject j = new Newtonsoft.Json.Linq.JObject();
        Stopwatch stopwatch = Stopwatch.StartNew();
        var generationFields = Util.LoadGenerationFieldsFromCsv(f);
        stopwatch.Stop();
        j["generationFieldsIoElapsedMicroseconds"] = stopwatch.Elapsed.TotalMicroseconds;

        stopwatch.Restart();
        var generationDictionaries = Util.LoadGenerationDictionariesFromCsv(f);
        stopwatch.Stop();
        j["generationDictionariesIoElapsedMicroseconds"] = stopwatch.Elapsed.TotalMicroseconds;

        stopwatch.Restart();
        var generationArrays = Util.LoadGenerationArraysFromCsv(f);
        stopwatch.Stop();
        j["generationArraysIoElapsedMicroseconds"] = stopwatch.Elapsed.TotalMicroseconds;

        stopwatch.Restart();
        JObject resultFields = Util.GetMeanGenerationPerProvince(generationFields);
        stopwatch.Stop();
        j["fieldsResult"] = resultFields;
        j["generationFieldsCalculationElapsedMicroseconds"] = stopwatch.Elapsed.TotalMicroseconds;

        stopwatch.Restart();
        JObject resultArray = Util.GetMeanGenerationPerProvince(generationArrays);
        stopwatch.Stop();
        j["arraysResult"] = resultArray;
        j["generationArraysCalculationElapsedMicroseconds"] = stopwatch.Elapsed.TotalMicroseconds;

        stopwatch.Restart();
        JObject resultDictionary = Util.GetMeanGenerationPerProvince(generationDictionaries);
        stopwatch.Stop();
        j["dictionariesResult"] = resultDictionary;
        j["generationDictionariesCalculationElapsedMicroseconds"] = stopwatch.Elapsed.TotalMicroseconds;

        summary[filename.Split(".").First()] = j;
    }
}

using (StreamWriter file = File.CreateText(@"C:/Projects/Code/EnergyPriceCalculations/Results/csharp.json"))
using (JsonTextWriter writer = new JsonTextWriter(file))
{
    summary.WriteTo(writer);
}

enum EnergyProductionMethod
{
    Oil,
    Nuclear,
    Wind,
    Biomass,
    Solar,
    Hydro,
    Geothermal,
    MAX
}
enum Province
{
    Ontario,
    Quebec,
    NovaScotia,
    NewBrunswick,
    BritishColumbia,
    MAX
}

class EnergyAnalysisBase
{
    public ulong id = 0;
    public DateTime dateTime = new DateTime();
    public Province province = Province.Ontario;
}
class GenerationArray : EnergyAnalysisBase
{
    public double[] generation = new double[(int)EnergyProductionMethod.MAX]; // initialized to number of items in enum or greater, but this is imaginary-ish
}
class GenerationFields : EnergyAnalysisBase
{
    public double Oil;
    public double Nuclear;
    public double Wind;
    public double Biomass;
    public double Solar;
    public double Hydro;
    public double Geothermal;
}

class GenerationDictionary : EnergyAnalysisBase
{
    private Dictionary<EnergyProductionMethod, double> generation = new Dictionary<EnergyProductionMethod, double>();

    public void SetGeneration(EnergyProductionMethod method, double value)
    {
        generation[method] = value;
    }

    public double GetGeneration(EnergyProductionMethod method)
    {
        if (generation.ContainsKey(method))
        {
            return generation[method];
        }
        else
        {
            return 0;
        }
    }
}

class ProductionCsvRecord
{
    public double Oil { get; set; }
    public double Nuclear { get; set; }
    public double Wind { get; set; }
    public double Biomass { get; set; }
    public double Solar { get; set; }
    public double Hydro { get; set; }
    public double Geothermal { get; set; }
    public string Province { get; set; }
}

class Util
{
    public static ProductionCsvRecord[] LoadProductionDataFromCsv(string path)
    {
        using (var reader = new StreamReader(path))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            return csv.GetRecords<ProductionCsvRecord>().ToArray();
        }
    }

    public static List<GenerationFields> LoadGenerationFieldsFromCsv(string path)
    {
        var ProductionCsvRecords = LoadProductionDataFromCsv(path);
        List<GenerationFields> result = new List<GenerationFields> { };
        foreach (var record in ProductionCsvRecords)
        {
            GenerationFields row = new GenerationFields();
            row.Oil = record.Oil;
            row.Nuclear = record.Nuclear;
            row.Wind = record.Wind;
            row.Biomass = record.Biomass;
            row.Solar = record.Solar;
            row.Hydro = record.Hydro;
            row.Geothermal = record.Geothermal;
            Enum.TryParse(record.Province, out Province p);
            row.province = p;
            result.Add(row);
        }
        return result;
    }

    public static List<GenerationDictionary> LoadGenerationDictionariesFromCsv(string path)
    {
        var ProductionCsvRecords = LoadProductionDataFromCsv(path);
        var result = new List<GenerationDictionary> { };
        foreach (var record in ProductionCsvRecords)
        {
            GenerationDictionary row = new GenerationDictionary();
            row.SetGeneration(EnergyProductionMethod.Oil, record.Oil);
            row.SetGeneration(EnergyProductionMethod.Nuclear, record.Nuclear);
            row.SetGeneration(EnergyProductionMethod.Wind, record.Wind);
            row.SetGeneration(EnergyProductionMethod.Biomass, record.Biomass);
            row.SetGeneration(EnergyProductionMethod.Solar, record.Solar);
            row.SetGeneration(EnergyProductionMethod.Hydro, record.Hydro);
            row.SetGeneration(EnergyProductionMethod.Geothermal, record.Geothermal);
            Enum.TryParse(record.Province, out Province p);
            row.province = p;
            result.Add(row);
        }
        return result;
    }

    public static List<GenerationArray> LoadGenerationArraysFromCsv(string path)
    {
        var ProductionCsvRecords = LoadProductionDataFromCsv(path);
        var result = new List<GenerationArray> { };
        foreach (var record in ProductionCsvRecords)
        {
            GenerationArray row = new GenerationArray();
            row.generation[(ushort)EnergyProductionMethod.Oil] = record.Oil;
            row.generation[(ushort)EnergyProductionMethod.Nuclear] = record.Nuclear;
            row.generation[(ushort)EnergyProductionMethod.Wind] = record.Wind;
            row.generation[(ushort)EnergyProductionMethod.Biomass] = record.Biomass;
            row.generation[(ushort)EnergyProductionMethod.Solar] = record.Solar;
            row.generation[(ushort)EnergyProductionMethod.Hydro] = record.Hydro;
            row.generation[(ushort)EnergyProductionMethod.Geothermal] = record.Geothermal;
            Enum.TryParse(record.Province, out Province p);
            row.province = p;
            result.Add(row);
        }
        return result;
    }

    public static JObject GetMeanGenerationPerProvince(List<GenerationFields> data)
    {
        var result = new JObject();
        var generationPerProvince = new Dictionary<Province, GenerationFields>();
        for (ushort i = 0; i < (ushort)Province.MAX; ++i)
        {
            generationPerProvince[(Province)i] = new GenerationFields();
        }
        var counts = new uint[(ushort)Province.MAX];
        foreach (var record in data)
        {
            ++counts[(ushort)record.province];
            generationPerProvince[record.province].Oil += record.Oil;
            generationPerProvince[record.province].Nuclear += record.Nuclear;
            generationPerProvince[record.province].Wind += record.Wind;
            generationPerProvince[record.province].Biomass += record.Biomass;
            generationPerProvince[record.province].Solar += record.Solar;
            generationPerProvince[record.province].Hydro += record.Hydro;
            generationPerProvince[record.province].Geothermal += record.Geothermal;
        }
        foreach (var pair in generationPerProvince)
        {
            pair.Value.Oil /= counts[(ushort)pair.Key];
            pair.Value.Nuclear /= counts[(ushort)pair.Key];
            pair.Value.Wind /= counts[(ushort)pair.Key];
            pair.Value.Biomass /= counts[(ushort)pair.Key];
            pair.Value.Solar /= counts[(ushort)pair.Key];
            pair.Value.Hydro /= counts[(ushort)pair.Key];
            pair.Value.Geothermal /= counts[(ushort)pair.Key];
        }
        foreach (var pair in generationPerProvince)
        {
            var j = new JObject();
            j["Oil"] = pair.Value.Oil;
            j["Nuclear"] = pair.Value.Nuclear;
            j["Wind"] = pair.Value.Wind;
            j["Biomass"] = pair.Value.Biomass;
            j["Solar"] = pair.Value.Solar;
            j["Hydro"] = pair.Value.Hydro;
            j["Geothermal"] = pair.Value.Geothermal;
            string? provinceName = Enum.GetName<Province>(pair.Key);
            if (provinceName != null)
            {
                result[provinceName] = j;
            }
        }
        return result;
    }
    public static JObject GetMeanGenerationPerProvince(List<GenerationArray> data)
    {
        var result = new JObject();
        var means = new double[(ushort)Province.MAX, (ushort)EnergyProductionMethod.MAX];
        var counts = new uint[(ushort)Province.MAX];
        foreach (var record in data)
        {
            ++counts[(ushort)record.province];
            for (ushort i = 0; i < (ushort)EnergyProductionMethod.MAX; ++i)
            {
                means[(ushort)record.province, i] += record.generation[i];
            }
        }
        for (ushort p = 0; p < (ushort)Province.MAX; ++p)
        {
            var j = new JObject();
            for (ushort m = 0; m < (ushort)EnergyProductionMethod.MAX; ++m)
            {
                string? methodName = Enum.GetName<EnergyProductionMethod>((EnergyProductionMethod)m);
                if (methodName != null)
                {
                    j[methodName] = means[p, m] / counts[p];
                }
            }
            string? provinceName = Enum.GetName<Province>((Province)p);
            if (provinceName != null)
            {
                result[provinceName] = j;
            }
        }
        return result;
    }
    public static JObject GetMeanGenerationPerProvince(List<GenerationDictionary> data)
    {
        var result = new JObject();
        var generationPerProvince = new Dictionary<Province, GenerationDictionary>();
        for (ushort p = 0; p < (ushort)Province.MAX; ++p)
        {
            var generation = new GenerationDictionary();
            // we will also initialise the value per energy production method
            // you'd typically see this somewhere in the loop checking if the value is already there
            // but again that seems unnecessarily brutal given that this method will already be struggling
            for (ushort m = 0; m < (ushort)EnergyProductionMethod.MAX; ++m)
            {
                generation.SetGeneration((EnergyProductionMethod)m, 0.0);
            }
            generationPerProvince[(Province)p] = generation;
        }
        var counts = new uint[(ushort)Province.MAX];
        for (int r = data.Count - 1; r != 0; --r)
        {
            ++counts[(int)data[r].province];
            for (ushort i = 0; i < (ushort)EnergyProductionMethod.MAX; ++i)
            {
                // yup that's painful
                generationPerProvince[data[r].province].SetGeneration((EnergyProductionMethod)i, generationPerProvince[data[r].province].GetGeneration((EnergyProductionMethod)i)
                    + data[r].GetGeneration((EnergyProductionMethod)i));
            }
        }
        for (ushort p = 0; p < (ushort)Province.MAX; ++p)
        {
            var j = new JObject();
            for (ushort m = 0; m < (ushort)EnergyProductionMethod.MAX; ++m)
            {
                string? methodName = Enum.GetName<EnergyProductionMethod>((EnergyProductionMethod)m);
                if (methodName != null)
                {
                    j[methodName] = generationPerProvince[(Province)p].GetGeneration((EnergyProductionMethod)m) / counts[p];
                }
            }
            string? provinceName = Enum.GetName<Province>((Province)p);
            if (provinceName != null)
            {
                result[provinceName] = j;
            }
        }
        return result;
    }
}