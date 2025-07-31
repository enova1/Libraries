using DataAccess;
using Microsoft.EntityFrameworkCore;
using Models.Employee;

namespace ExampleLibrary
{
    /// <inheritdoc />
    public class Employee(IApplicationDbContext employeeDbContext) : IEmployee
    {
        public async Task<List<Employees>> GetEmployeesTask()
        {
            List<Employees> employees = await employeeDbContext.Employees!
                .Include(e => e.EmployeePhones)
                .Include(e => e.EmployeeAddresses)
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .Distinct()
                .ToListAsync();

            return employees;
        }

        public async Task<List<Employees>> FilterEmployeesTask(string phone, string zipCode)
        {
            List<Employees> employees = await employeeDbContext.Employees!
                .Include(e => e.EmployeePhones)
                .Include(e => e.EmployeeAddresses)
                .Where(e => e.EmployeePhones != null && e.EmployeePhones.Any(p => p.PhoneNumber.Contains(phone)))
                .Where(e => e.EmployeeAddresses != null && e.EmployeeAddresses.Any(a => a.ZipCode.Contains(zipCode)))
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .ToListAsync();

            return employees;
        }

        public async Task<Employees?> CreateEmployee(Employees employees)
        {
            employeeDbContext.Add(employees);
            await employeeDbContext.SaveChangesAsync();

            if (employeeDbContext.Employees != null)
            {
                var saveData = await employeeDbContext.Employees
                    .Include(e => e.EmployeePhones)
                    .Include(e => e.EmployeeAddresses)
                    .FirstOrDefaultAsync(m => m.EmployeeId == employees.EmployeeId);

                if (saveData == null)
                {
                    return (null);
                }

                if (employees.EmployeePhones != null)
                {
                    foreach (var number in employees.EmployeePhones)
                    {
                        saveData.EmployeePhones!.Add(number);
                    }
                }

                if (employees.EmployeeAddresses != null)
                {
                    foreach (var addy in employees.EmployeeAddresses)
                    {
                        saveData.EmployeeAddresses!.Add(addy);
                    }
                }

                employees = saveData;
            }

            await employeeDbContext.SaveChangesAsync();

            return (employees);
        }

        public async Task<Employees?> EditEmployee(Employees data)
        {
            if (employeeDbContext.Employees == null) return null;
            var saveData = await employeeDbContext.Employees.FindAsync(data.EmployeeId);

            if (saveData == null)
            {
                return null;
            }

            employeeDbContext.Update(data);
            await employeeDbContext.SaveChangesAsync();

            return data;
        }

        public async Task<bool> DeleteEmployee(int id)
        {
            if (employeeDbContext.Employees == null)
            {
                return false;
            }

            var employee = await employeeDbContext.Employees.FindAsync(id);
            if (employee == null)
            {
                return false;
            }

            employeeDbContext.Employees.Remove(employee);

            await employeeDbContext.SaveChangesAsync();

            return true;
        }
    }
}
