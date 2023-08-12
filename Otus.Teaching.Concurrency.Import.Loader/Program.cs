using Otus.Teaching.Concurrency.Import.Core.Loaders;
using Otus.Teaching.Concurrency.Import.DataGenerator.Generators;
using Otus.Teaching.Concurrency.Import.Loader.Loaders;
using System;
using System.Diagnostics;
using System.IO;


namespace Otus.Teaching.Concurrency.Import.Loader
{
    class Program
    {
        private static string _dataFileName = "customers.xml";
        private static string _dataFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _dataFileName);
        private static int _numberRows = 10000;
        private static string _genType = "M"; // P- через процесс, M - вызов метода (M - по умолчанию)
        private static int _threadCount = 10; // количество потоков 

        private static string _dbConfig = "Host=localhost;Port=5432;Database=customers;Username=postgres;Password=admin";
        static void Main(string[] args)
        {
            string _generatorPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Generator", "Otus.Teaching.Concurrency.Import.DataGenerator.App.exe");
            string _generatorPath1 = Environment.CurrentDirectory;
            string _generatorPath2 = Directory.GetCurrentDirectory();
            if (args != null && args.Length >= 1)
            {
                if (args[0] == "M" || args[0] == "P")
                {
                    _genType = args[0];
                }
            }

            Generation generation = new Generation(_dataFilePath, _dataFileName, _numberRows);

            if (_genType =="M")
            {
                generation.GenerationInMethod();
            }

            if (_genType == "P")
            {
                generation.GenerationInProccess();
            }

            // Console.WriteLine($"Loader started with process Id {Process.GetCurrentProcess().Id}...");

            // GenerateCustomersDataFile();

            // var loader = new FakeDataLoader();

            // loader.LoadData();
            var loader = new MyDataLoader(_dataFileName);
            loader.LoadData();  
        }

        static void GenerateCustomersDataFile()
        {
            var xmlGenerator = new XmlGenerator(_dataFilePath, _numberRows);
            xmlGenerator.Generate();
        }

       
    }
}