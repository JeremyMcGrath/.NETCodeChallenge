using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using challenge.Services;
using challenge.Models;

namespace challenge.Controllers
{
    [Route("api/employee")]
    public class EmployeeController : Controller
    {
        private readonly ILogger _logger;
        private readonly IEmployeeService _employeeService;

        public EmployeeController(ILogger<EmployeeController> logger, IEmployeeService employeeService)
        {
            _logger = logger;
            _employeeService = employeeService;
        }

        [HttpPost]
        public IActionResult CreateEmployee([FromBody] Employee employee)
        {
            _logger.LogDebug($"Received employee create request for '{employee.FirstName} {employee.LastName}'");

            _employeeService.Create(employee);

            return CreatedAtRoute("getEmployeeById", new { id = employee.EmployeeId }, employee);
        }

        [HttpGet("{id}", Name = "getEmployeeById")]
        public IActionResult GetEmployeeById(String id)
        {
            _logger.LogDebug($"Received employee get request for '{id}'");

            var employee = _employeeService.GetById(id);

            if (employee == null)
                return NotFound();

            return Ok(employee);
        }

        [HttpPut("{id}")]
        public IActionResult ReplaceEmployee(String id, [FromBody]Employee newEmployee)
        {
            _logger.LogDebug($"Recieved employee update request for '{id}'");

            var existingEmployee = _employeeService.GetById(id);
            if (existingEmployee == null)
                return NotFound();

            _employeeService.Replace(existingEmployee, newEmployee);

            return Ok(newEmployee);
        }

        // *** REPORTING STRUCTURE ***

        [HttpGet("reporting-structure/{id}", Name = "getReportingStructure")]
        public IActionResult GetReportingStructure(String id)
        {
            _logger.LogDebug($"Received Reporting Structure get request for '{id}'");

            ReportingStructure numberOfReports = _employeeService.GetReportingStructure(id);

            return Ok(numberOfReports);
        }

        // *** COMPENSATION ***

        [HttpPost("compensation")]
        public IActionResult CreateCompensation([FromBody] Compensation compensation)
        {
            // Get Employee
            Employee employee = _employeeService.GetById(compensation.employee.EmployeeId);
            // if employee exists
            if (employee != null)
            {
                // Make sure all of the information is filled in 
                compensation.employee = employee;
            }
            // if datetime is null
            if (compensation.effectiveDate == null)
            {
                // Set Datetime to now
                compensation.effectiveDate = DateTime.Now;
            }

            _logger.LogDebug($"Received Compensation create request for '{compensation.employee.FirstName} {compensation.employee.LastName}'");

            _employeeService.CreateCompensation(compensation);

            return CreatedAtRoute("getCompensation", new { id = compensation.employee.EmployeeId }, compensation);
        }

        [HttpGet("compensation/{id}", Name = "getCompensation")]
        public IActionResult GetCompensation(String id)
        {
            _logger.LogDebug($"Received Compensation get request for employee '{id}'");

            Compensation compensation = _employeeService.GetCompensation(id);

            if (compensation == null)
                return NotFound();

            return Ok(compensation);
        }

    }
}
