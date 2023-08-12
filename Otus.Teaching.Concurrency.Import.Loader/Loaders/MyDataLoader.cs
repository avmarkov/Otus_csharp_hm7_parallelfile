using Otus.Teaching.Concurrency.Import.Core.Loaders;
using Otus.Teaching.Concurrency.Import.DataAccess.Parsers;

namespace Otus.Teaching.Concurrency.Import.Loader.Loaders
{
    public class MyDataLoader : IDataLoader
    {
        private string fileName;
        public MyDataLoader(string fileName)
        {
            this.fileName = fileName;
        }
        public void LoadData()
        {
            var parser = new XmlParser(fileName);
            var list = parser.Parse();

        }


    }
}
