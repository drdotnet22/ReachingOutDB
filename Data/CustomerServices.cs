using Microsoft.EntityFrameworkCore;
using Sylvan.Data.Csv;

using System.Data;

namespace ReachingOutDB.Data
{
    public class CustomerServices
    {
        #region Private members
        private IDbContextFactory<AppDbContext> contextFactory;
        private CustomerStateServices customerStateServices;
        #endregion

        #region Constructor
        public CustomerServices(IDbContextFactory<AppDbContext> contextFactory, CustomerStateServices customerStateServices)
        {
            this.contextFactory = contextFactory;
            this.customerStateServices = customerStateServices;
        }
        #endregion

        public async Task<IEnumerable<Customer>> GetCustomersAsync()
        {
            await using var dbContext = await contextFactory.CreateDbContextAsync();
            return await dbContext.Customers.OrderBy(c => c.CustomerName).ToListAsync();
        }

        public async Task<Customer> GetCustomerByIdAsync(int id)
        {
            await using var dbContext = await contextFactory.CreateDbContextAsync();
            return await dbContext.Customers.FirstOrDefaultAsync(c => c.CustomerId == id);
        }

        public async Task AddCustomerAsync(Customer customer)
        {
            await using var dbContext = await contextFactory.CreateDbContextAsync();
            await CalculateQty(customer);
            dbContext.Customers.Add(customer);
            await dbContext.SaveChangesAsync();
        }

        public async Task UpdateCustomerAsync(Customer customer)
        {
            await using var dbContext = await contextFactory.CreateDbContextAsync();
            await CalculateQty(customer);
            dbContext.Update(customer);
            await dbContext.SaveChangesAsync();
            await customerStateServices.NotifyCustomerUpdated(customer.CustomerId);
        }

        // VariableOrders customers get different quantities each quarter, so totals
        // are tracked per-quarter (QtyQ1-Q4); others use a single flat Qty for all quarters.
        private async Task CalculateQty(Customer customer)
        {
            if (customer.VariableOrders)
            {
                customer.QtyQ1 = (customer.DmQtyQ1 ?? 0) + (customer.LtlQtyQ1 ?? 0) +
                    (customer.UpsQtyQ1 ?? 0) + (customer.PostalQtyQ1 ?? 0) + (customer.IntlQtyQ1 ?? 0);

                customer.QtyQ2 = (customer.DmQtyQ2 ?? 0) + (customer.LtlQtyQ2 ?? 0) +
                    (customer.UpsQtyQ2 ?? 0) + (customer.PostalQtyQ2 ?? 0) + (customer.IntlQtyQ2 ?? 0);

                customer.QtyQ3 = (customer.DmQtyQ3 ?? 0) + (customer.LtlQtyQ3 ?? 0) +
                    (customer.UpsQtyQ3 ?? 0) + (customer.PostalQtyQ3 ?? 0) + (customer.IntlQtyQ3 ?? 0);

                customer.QtyQ4 = (customer.DmQtyQ4 ?? 0) + (customer.LtlQtyQ4 ?? 0) +
                    (customer.UpsQtyQ4 ?? 0) + (customer.PostalQtyQ4 ?? 0) + (customer.IntlQtyQ4 ?? 0);
            }
            else
            {
                customer.Qty = (customer.DmQty ?? 0) + (customer.LtlQty ?? 0) +
                    (customer.UpsQty ?? 0) + (customer.PostalQty ?? 0) + (customer.IntlQty ?? 0);
            }
        }

        public async Task<bool> ArchiveCustomerAsync(Customer customer)
        {
            if (customer.Active)
            {
                customer.Active = false;
                await UpdateCustomerAsync(customer);
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task InitialImportFromCSV(string path)
        {
            await using var dbContext = await contextFactory.CreateDbContextAsync();

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
                    VariableOrders = bool.Parse(row["VariableOrders"].ToString()),
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

        public async Task<IEnumerable<Customer>> GetMailingNotesAsync()
        {
            await using var dbContext = await contextFactory.CreateDbContextAsync();
            return await dbContext.Customers
                .Where(c => c.MailingNotes != null)
                .ToListAsync();
        }
    }
}
