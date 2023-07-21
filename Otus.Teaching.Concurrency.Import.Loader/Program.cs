using Otus.Teaching.Concurrency.Import.Core.Loaders;
using Otus.Teaching.Concurrency.Import.DataGenerator.Generators;
using System;
using System.Diagnostics;
using System.IO;


namespace Otus.Teaching.Concurrency.Import.Loader
{
    class Program
    {
        private static string _dataFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "customers.xml");
        private static int _numberRows = 10000;
        private static string _genType = "P"; // P- через процесс, M - вызов вызовом метода
        private static int _threadCount = 10; // количество потоков 

        private static string _dbConfig = "Host=localhost;Port=5432;Database=customers;Username=postgres;Password=admin";
        static void Main(string[] args)
        {
            string _generatorPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Generator", "Otus.Teaching.Concurrency.Import.DataGenerator.App.exe");
            string _generatorPath1 = Environment.CurrentDirectory;
            string _generatorPath2 = Directory.GetCurrentDirectory();
            if (args != null && args.Length == 1)
            {
                _dataFilePath = args[0];
            }

            Console.WriteLine($"Loader started with process Id {Process.GetCurrentProcess().Id}...");

            GenerateCustomersDataFile();

            var loader = new FakeDataLoader();

            loader.LoadData();
        }

        static void GenerateCustomersDataFile()
        {
            var xmlGenerator = new XmlGenerator(_dataFilePath, _numberRows);
            xmlGenerator.Generate();
        }
    }
}