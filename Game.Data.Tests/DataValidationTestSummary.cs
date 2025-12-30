using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Game.Data.Tests;

/// <summary>
/// Summary test to display test coverage statistics across all JSON data validation suites
/// </summary>
[Trait("Category", "DataValidation")]
[Trait("Type", "Summary")]
public class DataValidationTestSummary
{
  private readonly string _dataPath;

  public DataValidationTestSummary()
  {
    var baseDir = AppDomain.CurrentDomain.BaseDirectory;
    var solutionRoot = Directory.GetParent(baseDir)?.Parent?.Parent?.Parent?.Parent?.FullName;
    if (solutionRoot == null)
      throw new DirectoryNotFoundException($"Could not find solution root from: {baseDir}");

    _dataPath = Path.Combine(solutionRoot, "Game.Data", "Data", "Json");
  }

  [Fact]
  public void Should_Display_Test_Coverage_Summary()
  {
    // Discover all file types
    var catalogFiles = Directory.GetFiles(_dataPath, "catalog.json", SearchOption.AllDirectories).Length;
    var namesFiles = Directory.GetFiles(_dataPath, "names.json", SearchOption.AllDirectories).Length;
    var configFiles = Directory.GetFiles(_dataPath, ".cbconfig.json", SearchOption.AllDirectories).Length;

    var allJsonFiles = Directory.GetFiles(_dataPath, "*.json", SearchOption.AllDirectories);
    var componentFiles = allJsonFiles
        .Count(f => !f.EndsWith("catalog.json") &&
                    !f.EndsWith("names.json") &&
                    !f.EndsWith(".cbconfig.json"));

    var totalFiles = catalogFiles + namesFiles + configFiles + componentFiles;

    // Output coverage statistics
    Console.WriteLine("==========================================");
    Console.WriteLine("JSON Data Validation Test Coverage");
    Console.WriteLine("==========================================");
    Console.WriteLine($"Catalog files (catalog.json):      {catalogFiles,3} files");
    Console.WriteLine($"Names files (names.json):          {namesFiles,3} files");
    Console.WriteLine($"Config files (.cbconfig.json):     {configFiles,3} files");
    Console.WriteLine($"Component files (other *.json):    {componentFiles,3} files");
    Console.WriteLine("------------------------------------------");
    Console.WriteLine($"Total JSON files:                  {totalFiles,3} files");
    Console.WriteLine("==========================================");
    Console.WriteLine();
    Console.WriteLine("Test Suites:");
    Console.WriteLine("  - CatalogJsonComplianceTests");
    Console.WriteLine("  - NamesJsonComplianceTests");
    Console.WriteLine("  - ConfigJsonComplianceTests");
    Console.WriteLine("  - ComponentJsonComplianceTests");
    Console.WriteLine("==========================================");
  }
}
