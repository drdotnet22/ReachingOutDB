using Microsoft.EntityFrameworkCore;
using Sylvan.Data.Csv;

using System.Data;

namespace ReachingOutDB.Data
{
    public class CustomerServices
    {
        #region Private members
        private IDbContextFactory<AppDbContext> contextFactory;
        private OrderAuditLogServices auditLogServices;
        #endregion

        #region Constructor
        public CustomerServices(IDbContextFactory<AppDbContext> contextFactory)
        {
            this.contextFactory = contextFactory;
        }
        #endregion

        public async Task<IEnumerable<Customer>> GetCustomersAsync()
        {
            var dbContext = await contextFactory.CreateDbContextAsync();
            return await dbContext.Customers.OrderBy(c => c.CustomerName).ToListAsync();
        }

        public async Task AddCustomerAsync(Customer customer)
        {
            var dbContext = await contextFactory.CreateDbContextAsync();
            try
            {
                dbContext.Customers.Add(customer);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        public async Task UpdateCustomerAsync(Customer customer)
        {
            var dbContext = await contextFactory.CreateDbContextAsync();
            try
            {
                dbContext.Update(customer);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> ArchiveCustomerAsync(Customer customer)
        {
            var dbContext = await contextFactory.CreateDbContextAsync();
            if (customer.Active)
            {
                customer.Active = false;
                await dbContext.SaveChangesAsync();
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task InitialImportFromCSV(string path)
        {
            var dbContext = await contextFactory.CreateDbContextAsync();

            CsvDataReader reader = CsvDataReader.Create(path, new CsvDataReaderOptions
            {
                Delimiter = ',',
                HasHeaders = true
            });
            DataTable dataTable = new DataTable();
            dataTable.Load(reader);
            foreach (DataRow row in dataTable.Rows)
            {
                var customer = new Customer
                {
                    CustomerId = int.Parse(row["CustomerId"].ToString()),
                    CustomerName = row["CustomerName"].ToString(),
                    Location = row["Location"].ToString(),
                    Active = true,
                    QtyQ1 = int.Parse(row["QtyQ1"].ToString()),
                    QtyQ2 = int.Parse(row["QtyQ2"].ToString()),
                    QtyQ3 = int.Parse(row["QtyQ3"].ToString()),
                    QtyQ4 = int.Parse(row["QtyQ4"].ToString()),
                    VariableQty = bool.Parse(row["VariableQty"].ToString()),
                    NotesQ1 = string.IsNullOrWhiteSpace(row["NotesQ1"].ToString()) ? null : row["NotesQ1"].ToString(),
                    NotesQ2 = string.IsNullOrWhiteSpace(row["NotesQ2"].ToString()) ? null : row["NotesQ2"].ToString(),
                    NotesQ3 = string.IsNullOrWhiteSpace(row["NotesQ3"].ToString()) ? null : row["NotesQ3"].ToString(),
                    NotesQ4 = string.IsNullOrWhiteSpace(row["NotesQ4"].ToString()) ? null : row["NotesQ4"].ToString(),
                    CustomBP = bool.Parse(row["CustomBP"].ToString()),
                    DmQty = string.IsNullOrWhiteSpace(row["DmQty"].ToString()) ? null : int.Parse(row["DmQty"].ToString()),
                    DmQtyQ1 = string.IsNullOrWhiteSpace(row["DmQtyQ1"].ToString()) ? null : int.Parse(row["DmQtyQ1"].ToString()),
                    DmQtyQ2 = string.IsNullOrWhiteSpace(row["DmQtyQ2"].ToString()) ? null : int.Parse(row["DmQtyQ2"].ToString()),
                    DmQtyQ3 = string.IsNullOrWhiteSpace(row["DmQtyQ3"].ToString()) ? null : int.Parse(row["DmQtyQ3"].ToString()),
                    DmQtyQ4 = string.IsNullOrWhiteSpace(row["DmQtyQ4"].ToString()) ? null : int.Parse(row["DmQtyQ4"].ToString()),
                    UpsQty = string.IsNullOrWhiteSpace(row["UpsQty"].ToString()) ? null : int.Parse(row["UpsQty"].ToString()),
                    UpsQtyQ1 = string.IsNullOrWhiteSpace(row["UpsQtyQ1"].ToString()) ? null : int.Parse(row["UpsQtyQ1"].ToString()),
                    UpsQtyQ2 = string.IsNullOrWhiteSpace(row["UpsQtyQ2"].ToString()) ? null : int.Parse(row["UpsQtyQ2"].ToString()),
                    UpsQtyQ3 = string.IsNullOrWhiteSpace(row["UpsQtyQ3"].ToString()) ? null : int.Parse(row["UpsQtyQ3"].ToString()),
                    UpsQtyQ4 = string.IsNullOrWhiteSpace(row["UpsQtyQ4"].ToString()) ? null : int.Parse(row["UpsQtyQ4"].ToString()),
                    PostalQty = string.IsNullOrWhiteSpace(row["PostalQty"].ToString()) ? null : int.Parse(row["PostalQty"].ToString()),
                    PostalQtyQ1 = string.IsNullOrWhiteSpace(row["PostalQtyQ1"].ToString()) ? null : int.Parse(row["PostalQtyQ1"].ToString()),
                    PostalQtyQ2 = string.IsNullOrWhiteSpace(row["PostalQtyQ2"].ToString()) ? null : int.Parse(row["PostalQtyQ2"].ToString()),
                    PostalQtyQ3 = string.IsNullOrWhiteSpace(row["PostalQtyQ3"].ToString()) ? null : int.Parse(row["PostalQtyQ3"].ToString()),
                    PostalQtyQ4 = string.IsNullOrWhiteSpace(row["PostalQtyQ4"].ToString()) ? null : int.Parse(row["PostalQtyQ4"].ToString()),
                    LtlQty = string.IsNullOrWhiteSpace(row["LtlQty"].ToString()) ? null : int.Parse(row["LtlQty"].ToString()),
                    LtlQtyQ1 = string.IsNullOrWhiteSpace(row["LtlQtyQ1"].ToString()) ? null : int.Parse(row["LtlQtyQ1"].ToString()),
                    LtlQtyQ2 = string.IsNullOrWhiteSpace(row["LtlQtyQ2"].ToString()) ? null : int.Parse(row["LtlQtyQ2"].ToString()),
                    LtlQtyQ3 = string.IsNullOrWhiteSpace(row["LtlQtyQ3"].ToString()) ? null : int.Parse(row["LtlQtyQ3"].ToString()),
                    LtlQtyQ4 = string.IsNullOrWhiteSpace(row["LtlQtyQ4"].ToString()) ? null : int.Parse(row["LtlQtyQ4"].ToString()),
                    IntlQty = string.IsNullOrWhiteSpace(row["IntlQty"].ToString()) ? null : int.Parse(row["IntlQty"].ToString()),
                    IntlQtyQ1 = string.IsNullOrWhiteSpace(row["IntlQtyQ1"].ToString()) ? null : int.Parse(row["IntlQtyQ1"].ToString()),
                    IntlQtyQ2 = string.IsNullOrWhiteSpace(row["IntlQtyQ2"].ToString()) ? null : int.Parse(row["IntlQtyQ2"].ToString()),
                    IntlQtyQ3 = string.IsNullOrWhiteSpace(row["IntlQtyQ3"].ToString()) ? null : int.Parse(row["IntlQtyQ3"].ToString()),
                    IntlQtyQ4 = string.IsNullOrWhiteSpace(row["IntlQtyQ4"].ToString()) ? null : int.Parse(row["IntlQtyQ4"].ToString()),
                    YearlyBillingQuarter = string.IsNullOrWhiteSpace(row["YearlyBillingQuarter"].ToString()) ? null : Enum.Parse<Quarter>(row["YearlyBillingQuarter"].ToString().Trim())
                };
                await dbContext.Customers.AddAsync(customer);
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
