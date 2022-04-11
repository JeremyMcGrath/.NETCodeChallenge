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
    public class CompensationRespository : ICompensationRepository
    {
        private readonly EmployeeContext _employeeContext;
        private readonly ILogger<ICompensationRepository> _logger;

        public CompensationRespository(ILogger<ICompensationRepository> logger, EmployeeContext employeeContext)
        {
            _employeeContext = employeeContext;
            _logger = logger;
        }

        public Compensation GetById(String id)
        {
            // This one runs into the same issue where the employee field inside Compensations will be null unless accessed first.
            // I'm starting to this that this is because of the way that I'm retrieving the data but I haven't found a solid way to fix it
            if (_employeeContext.Compensations != null)
            {
                var employees = _employeeContext.Employees.ToList();
                foreach (var employee in employees)
                {
                    var accessingEmployeeId = employee.EmployeeId;
                    var accessingEmployeeLastName = employee.LastName;
                    var accessingEmployeeFirstName = employee.FirstName;
                    var accessingEmployeeDirectReports = employee.DirectReports;
                    // Used for debugging but commenting out now
                    // Console.WriteLine(String.Format("Employee: {0}, {1} | EmployeeId: {2} | DirectReports: {3}", employee.LastName, employee.FirstName, employee.EmployeeId, employee.DirectReports));
                }
                var compensations = _employeeContext.Compensations.ToList();
                // Used for debugging
                // Console.WriteLine("Hello from Inside the CompensationRespository File! Specifically the GetById Function!\n");
                foreach (var compensation in compensations)
                {
                    var accessingCompensationEmployee = compensation.employee;
                    var accessingCompensationSalary = compensation.salary;
                    var accessingCompensationEffectiveDate = compensation.effectiveDate;
                    // Used for debugging
                    // Console.WriteLine(String.Format("Employee: {0} | Salary: {2} | EffectiveDate: {3}", compensation.employee, compensation.salary, compensation.effectiveDate));
                }
                return compensations.SingleOrDefault(c => c.employee.EmployeeId == id);
            }
            return null;
        }
        
        public Compensation Add(Compensation compensation)
        {
            compensation.CompensationId = Guid.NewGuid().ToString();
            _employeeContext.Compensations.Add(compensation);
            return compensation;
        }

        public Task SaveAsync()
        {
            return _employeeContext.SaveChangesAsync();
        }
    }
}
