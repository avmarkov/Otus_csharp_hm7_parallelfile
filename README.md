## Общая структура проекта для домашнего задания по параллельной загрузке данных

### Постановка задачи

Цель: Сделать параллельный обработчик файла с данными клиентов на основе подготовленного проекта с архитектурой. 
Задание поможет отработать основные инструменты параллелизма на реалистичной задаче.
Каждая строка файла содержит: 
- id (целое число) 
- ФИО (строка), 
- Email (строка) 
- Телефон (строка). 

Данные отсортированы по id. Нужно десериализовать данные клиента в объект и передать объект в метод класса, который сохраняет его в БД.

### Задание
1. Запуск генератора файла через создание процесса, сделать возможность выбора в коде, как запускать генератор, процессом или через вызов метода.
2. Распараллеливаем обработку файла по набору диапазонов Id, то есть нужно, чтобы файл разбивался на диапазоны по Id и обрабатывался параллельно через Thread, обработка предполагает сохранение в БД через вызов репозитория.  Хорошо сделать настройку с количеством потоков, чтобы можно было настроить оптимальное количество потоков под размер файла с данными. Предусмотреть обработку ошибок и механизм попыток для повторного сохранения в случае ошибки. Проверить обработку на файле, в котором 1 млн. записей, при сдаче задания написать время, за которое был обработан файл и количество потоков.
3. Добавить сохранение в реальную БД, в качестве БД выбрать SQL Lite для простоты тестирования и реализации.
4. По желанию вместо SQL Lite проверить работу приложения на полноценной БД, например, MS SQL Server или PostgreSQL, увеличив размер файла до 3 млн записей. При сдаче работы написать о результатах, возникали ли длительные блокировки при записи в таблицу и за какое время происходила загрузка.(*) 
5. Дать обратную связь по 1-му домашнему заданию других студентов на курсе.

### Инструкция
1. Сделать форк этого репозитория.
2. Реализовать 1 пункт задания, сделав в main проекта запуск процесса-генератора файла, его нужно будет собрать отдельно и передать в программу путь к .exe файлу, также сделать в `Main` вызов кода генератора из подключенного проекта, выбор между процессом или вызовом метода сделать настройкой (например аргумент командной строки или файл с настройками) со значением по умолчанию для метода.
3. Реализовать 2 пункт задания, сделав свои реализации для `IDataLoader` и `IDataParser`. Лучше всего десериализовать данные из файла в коллекцию и передать в конструктор реализации `IDataLoader`, а затем уже в реализации разбить коллекцию на набор подколлекций согласно количеству потоков, чтобы каждую подколлекцию обрабатывал свой поток. Предусмотреть обработку ошибок в обработчике потока и перезапуск по ошибке с указанием числа попыток. При обработке поток должен вызывать сохранение данных через репозитории.
4. Реализовать 3 пункт задания, сделав дополнительную реализацию для `ICustomerRepository` и инициализацию БД при старте приложения, можно использовать EF, в этом случае DbContext должен создаваться на поток, чтобы не было проблем с конкуренцией, так как DbContext не потокобезопасен. При многопоточной записи в SQLite могут быть проблемы с блокировкой файла базы, чтобы этого избежать нужно не забывать про using при создании connection к БД, если это делается через EF, то DbContext должен создаваться в using. При активной записи все равно могут быть блокировки, для этого мы реализуем механизм попыток, когда блокировка будет снята, даже если было исключение, то при следующей попытке поток запишет данные, это актуально и для больших баз по нагрузкой. 
5. По желанию реализовать 4 пункт задания.
5. По желанию дать обратную связь по 1-му домашнему заданию других студентов на курсе, можно найти репозитории по форкам к этому репозиторию. Обратную связь можно описать, создав issue к репозиторию, например, 
https://gitlab.com/devgrav/Otus.Teaching.Concurrency.Queue/issues/1. Чтобы обратная связь была качественной обязательно нужно похвалить работу, написав, что сделано хорошо и написать, что можно улучшить с пояснениями почему это сделает работу более качественной. Эти рекомендации работают и для code review, так как позволяют более конструктивно обсуждать коммиты.


### Решение
#### 1. Запуск генератора файла через создание процесса, сделать возможность выбора в коде, как запускать генератор, процессом или через вызов метода.

Сделал класс Generation. С двумя методами GenerationInMethod - генерация файла через вызов метода, GenerationInProccess генерация с помощью процесса:
```cs
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
            
        process.StartInfo.FileName = programmName;           
        process.StartInfo.Arguments = "\"" + fileName + "\" " + dataCount.ToString();
        process.Start();
        Console.WriteLine($"Generation in process. process.Id = {process.Id}...");
        process.WaitForExit();
    }
}
```

Вызов в зависимости от параметров командной строки такой:
```cs
if (_genType =="M")
{
    generation.GenerationInMethod();
}

if (_genType == "P")
{
    generation.GenerationInProccess();
}      
```

#### 2. Распараллеливаем обработку файла по набору диапазонов Id

Сначала десериализовал данные из XML-файла в List. Т.е в классе XmlParser реализовал IDataParser:
```cs
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

```

Затем создал класс MyDataLoader, который реализует интерфейс IDataLoader. В этом классе сделал загрузку данных в БД без потоков и с потоками. 
Для реализации параллельной загрузки я использовал не напрямую потоки, а Task. Количество Task - threadCount инициализируется в конструкторе класса MyDataLoader. Количество запускаемых Task - 10.
В случае ошибок записи в БД, я делаю до трех повторов.

```cs
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
            // Повторы, в случае ошибки
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
```

#### 3. Добавить сохранение в реальную БД, в качестве БД выбрать SQL Lite для простоты тестирования и реализации

Для решения задачи я сразу использовал БД Postgres. Классы CustomerRepository и MyDbContext для работы с БД Postgres такие:

```cs
public class CustomerRepository : ICustomerRepository
{
    private MyDbContext _context;
    protected DbSet<Customer> _dbSet;
    public CustomerRepository(MyDbContext context)
    {
        _context = context;
        _dbSet = context.Set<Customer>();
    }
    public void AddCustomer(Customer customer)
    {
        //Add customer to data source
        if (customer != null)
        {
            _dbSet.Add(customer);
               
        }
    }

    public void Clear()
    {
        _dbSet.RemoveRange(_dbSet);
        SaveChange();
    }


    public void SaveChange()
    {
        _context.SaveChanges();
    }
}
```

```cs
public class MyDbContext : DbContext
{
    public DbSet<Customer> Customers { get; set; }
    private string _config;

    public MyDbContext(string config)
    {
        _config = config;
        Database.EnsureCreated();
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_config);
    }
}
```

DbContext, в случае загрузки в БД параллельно, я создаю отдельно на каждую Task, чтобы избежать проблем с многопоточностью.

4. По желанию вместо SQL Lite проверить работу приложения на полноценной БД, например, MS SQL Server или PostgreSQL.
Сделано

5. Результат:

<image src="images/result.png" alt="result">

Как видно из результатов загрузка без потоков заняла около 3 мин. Закрузка с 10 Task заняла - 8 сек.
