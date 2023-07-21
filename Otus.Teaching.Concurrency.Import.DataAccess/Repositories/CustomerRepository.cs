using Microsoft.EntityFrameworkCore;
using Otus.Teaching.Concurrency.Import.Handler.Entities;
using Otus.Teaching.Concurrency.Import.Handler.Repositories;

namespace Otus.Teaching.Concurrency.Import.DataAccess.Repositories
{
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
                _dbSet.Add(customer);
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
}