using Models.Employee;

namespace ExampleLibrary;

public interface IEmployee
{
    Task<List<Employees>> GetEmployeesTask();
    Task<List<Employees>> FilterEmployeesTask(string phone, string zipCode);
    Task<Employees?> CreateEmployee(Employees employees);
    Task<Employees?> EditEmployee(Employees data);
    Task<bool> DeleteEmployee(int id);
}