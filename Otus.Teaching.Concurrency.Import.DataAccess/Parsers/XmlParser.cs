using Otus.Teaching.Concurrency.Import.Core.Parsers;
using Otus.Teaching.Concurrency.Import.Handler.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Otus.Teaching.Concurrency.Import.DataAccess.Parsers
{
    public class XmlParser : IDataParser<List<Customer>>
    {
        public string DataFile { get; set; }

        public XmlParser(string dataFile)
        {
            DataFile = dataFile;
        }

        public List<Customer> Parse()
        {
            var result = new List<Customer>();

            var serializer = new XmlSerializer(typeof(CustomersList));

            using (FileStream fileStream = new FileStream(DataFile, FileMode.OpenOrCreate))
            {
                CustomersList customersList = serializer.Deserialize(fileStream) as CustomersList;
                //List<Customer>? item = (List<Customer>)serializer.Deserialize(fileStream);
                result = customersList.Customers;
            }

            return result;
        }
    }

    [XmlRoot("Customers")]
    public class CustomersList
    {
        public List<Customer> Customers { get; set; }
    }
}