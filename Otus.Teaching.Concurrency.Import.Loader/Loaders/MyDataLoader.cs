using Otus.Teaching.Concurrency.Import.Core.Loaders;
using Otus.Teaching.Concurrency.Import.DataAccess;
using Otus.Teaching.Concurrency.Import.DataAccess.Parsers;
using Otus.Teaching.Concurrency.Import.DataAccess.Repositories;
using Otus.Teaching.Concurrency.Import.Handler.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Otus.Teaching.Concurrency.Import.Loader.Loaders
{
    public class MyDataLoader : IDataLoader
    {

        private string fileName;
        private string dbConfig;
        private int threadCount;
        public MyDataLoader(string fileName, string dbConfig, int threadCount)
        {
            this.fileName = fileName;
            this.dbConfig = dbConfig;
            this.threadCount = threadCount;
        }
        public void LoadData()
        {
            var parser = new XmlParser(fileName);
            var customers = parser.Parse();

            var stopWatch = new Stopwatch();
            Console.WriteLine();
            Console.WriteLine($"Start data load without thread...");
            stopWatch.Start();
            WithoutThread(customers);
            stopWatch.Stop();
            Console.WriteLine($"Data load without thread finish after {stopWatch.Elapsed} second.");
            Console.WriteLine();

            Console.WriteLine($"Start data load with {threadCount} threads...");
            stopWatch = new Stopwatch();
            stopWatch.Start();
            WithThread(customers);
            stopWatch.Stop();
            Console.WriteLine($"Data Load with {threadCount} threads finish after {stopWatch.Elapsed} second.");
            Console.WriteLine();
        }

        public bool WithoutThread(List<Customer> customerList)
        {
            ClearRepository();
            WriteCustomersToDb(customerList);
            return true;

        }

        public void ClearRepository()
        {
            CustomerRepository repository = new CustomerRepository(new MyDbContext(dbConfig));
            repository.Clear();
            repository.SaveChange();
        }

        public bool WriteCustomersToDb(List<Customer> customerList)
        {
            CustomerRepository repository = new CustomerRepository(new MyDbContext(dbConfig));

            foreach (var item in customerList)
            {
                int retry = 0;
                while (retry <= 3)
                {
                    try
                    {
                        repository.AddCustomer(item);
                        retry = 4;
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Error in repository.AddCustomer. Retry...");
                        retry++;
                    }
                }

                retry = 0;
                while (retry <= 3)
                {
                    try
                    {
                        repository.SaveChange();
                        retry = 4;
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Error in repository.SaveChange. Retry...");
                        retry++;
                    }
                }
            }
            return true;
        }

        public async Task<bool> WriteCustomersToDbInTaskAsync(List<Customer> customerList, int threadInd)
        {
            Console.WriteLine("Task № " + threadInd.ToString() + " started");
            var t = Task.Run(() =>
            {
                bool res = WriteCustomersToDb(customerList);
                return res;

            });
            return await t;
        }

        public bool WithThread(List<Customer> customerList)
        {
            ClearRepository();
            var taskList = new List<Task<bool>>();
            int subListLen = customerList.Count / threadCount;
            for (int i = 0; i < threadCount; i++)
            {
                int lastInd;

                if (i == threadCount - 1)
                {
                    lastInd = customerList.Count;
                }
                else
                {
                    lastInd = (i + 1) * subListLen;
                }
                List<Customer> customerSubList = customerList.GetRange(i * subListLen, lastInd - i * subListLen);

                // добавляем и задачу в taskList, она будет запущена внутри WriteCustomersToDbInTaskAsync 
                taskList.Add(WriteCustomersToDbInTaskAsync(customerSubList, i));
            }

            try
            {
                // ожидание выполнения всех тасок из taskList
                Task.WaitAll(taskList.ToArray());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return true;
        }

    }
}
