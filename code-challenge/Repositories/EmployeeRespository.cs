using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using challenge.Models;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using challenge.Data;

namespace challenge.Repositories
{
    public class EmployeeRespository : IEmployeeRepository
    {
        private readonly EmployeeContext _employeeContext;
        private readonly ILogger<IEmployeeRepository> _logger;

        public EmployeeRespository(ILogger<IEmployeeRepository> logger, EmployeeContext employeeContext)
        {
            _employeeContext = employeeContext;
            _logger = logger;
        }

        public Employee Add(Employee employee)
        {
            employee.EmployeeId = Guid.NewGuid().ToString();
            _employeeContext.Employees.Add(employee);
            return employee;
        }

        public Employee GetById(string id)
        {
            // I had to add this code to this function because an employee would return before this update
            // but no matter what their DirectReports value was it was always returned as 'null'. I figured 
            // out that if that information was accessed before the return then the DirectReports would be 
            // queried and return the correct values. So this current loop does nothing but access the info
            // before its returned. I'm not sure what the cause of this is
            var employees = _employeeContext.Employees.ToList();
            // Used for debugging but commenting out now
            // Console.WriteLine("\nHello from Inside the EmployeeRespository File! Specifically the GetById Function!\n");
            foreach (var employee in employees)
            {
                var accessingEmployeeId = employee.EmployeeId;
                var accessingEmployeeLastName = employee.LastName;
                var accessingEmployeeFirstName = employee.FirstName;
                var accessingEmployeeDirectReports = employee.DirectReports;
                // Used for debugging but commenting out now
                // Console.WriteLine(String.Format("Employee: {0}, {1} | EmployeeId: {2} | DirectReports: {3}", employee.LastName, employee.FirstName, employee.EmployeeId, employee.DirectReports));
            }
            return employees.SingleOrDefault(e => e.EmployeeId == id);
        }

        public Task SaveAsync()
        {
            return _employeeContext.SaveChangesAsync();
        }

        public Employee Remove(Employee employee)
        {
            return _employeeContext.Remove(employee).Entity;
        }
    }
}
