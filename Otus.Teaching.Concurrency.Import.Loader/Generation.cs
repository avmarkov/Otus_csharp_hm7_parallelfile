using Otus.Teaching.Concurrency.Import.DataGenerator.Generators;
using System;
using System.Diagnostics;
using System.IO;

namespace Otus.Teaching.Concurrency.Import.Loader
{
    public class Generation
    {
        private string fileName;
        private int dataCount;
        private string programmName;

        public Generation(string programmName, string fileName, int dataCount)
        {
            this.programmName = programmName;
            this.fileName = fileName;
            this.dataCount = dataCount;
        }

        public void GenerationInMethod()
        {
            Console.WriteLine($"Generation in method...");
            var xmlGenerator = new XmlGenerator(fileName, dataCount);           
            xmlGenerator.Generate();
        }

        public void GenerationInProccess()
        {
            var process = new Process();
            Console.WriteLine($"Generation in process. process.Id = {process.Id}...");
            
            process.StartInfo.FileName = programmName;
            process.StartInfo.Arguments = "\"" + Path.GetFileNameWithoutExtension(fileName) + "\" " + dataCount.ToString();
            process.Start();
        }


    }
}
