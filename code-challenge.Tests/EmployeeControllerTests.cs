using challenge.Controllers;
using challenge.Data;
using challenge.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using code_challenge.Tests.Integration.Extensions;

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using code_challenge.Tests.Integration.Helpers;
using System.Text;
using System.Collections.Generic;

namespace code_challenge.Tests.Integration
{
    [TestClass]
    public class EmployeeControllerTests
    {
        private static HttpClient _httpClient;
        private static TestServer _testServer;

        [ClassInitialize]
        public static void InitializeClass(TestContext context)
        {
            _testServer = new TestServer(WebHost.CreateDefaultBuilder()
                .UseStartup<TestServerStartup>()
                .UseEnvironment("Development"));

            _httpClient = _testServer.CreateClient();
        }

        [ClassCleanup]
        public static void CleanUpTest()
        {
            _httpClient.Dispose();
            _testServer.Dispose();
        }

        [TestMethod]
        public void CreateEmployee_Returns_Created()
        {
            // Arrange
            var employee = new Employee()
            {
                Department = "Complaints",
                FirstName = "Debbie",
                LastName = "Downer",
                Position = "Receiver",
            };

            var requestContent = new JsonSerialization().ToJson(employee);

            // Execute
            var postRequestTask = _httpClient.PostAsync("api/employee",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var response = postRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            var newEmployee = response.DeserializeContent<Employee>();
            Assert.IsNotNull(newEmployee.EmployeeId);
            Assert.AreEqual(employee.FirstName, newEmployee.FirstName);
            Assert.AreEqual(employee.LastName, newEmployee.LastName);
            Assert.AreEqual(employee.Department, newEmployee.Department);
            Assert.AreEqual(employee.Position, newEmployee.Position);
        }

        [TestMethod]
        public void GetEmployeeById_Returns_Ok()
        {
            // Arrange
            var employeeId = "16a596ae-edd3-4847-99fe-c4518e82c86f";
            var expectedFirstName = "John";
            var expectedLastName = "Lennon";

            // Execute
            var getRequestTask = _httpClient.GetAsync($"api/employee/{employeeId}");
            var response = getRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var employee = response.DeserializeContent<Employee>();
            Assert.AreEqual(expectedFirstName, employee.FirstName);
            Assert.AreEqual(expectedLastName, employee.LastName);
        }

        [TestMethod]
        public void UpdateEmployee_Returns_Ok()
        {
            // Arrange
            var employee = new Employee()
            {
                EmployeeId = "03aa1462-ffa9-4978-901b-7c001562cf6f",
                Department = "Engineering",
                FirstName = "Pete",
                LastName = "Best",
                Position = "Developer VI",
            };
            var requestContent = new JsonSerialization().ToJson(employee);

            // Execute
            var putRequestTask = _httpClient.PutAsync($"api/employee/{employee.EmployeeId}",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var putResponse = putRequestTask.Result;
            
            // Assert
            Assert.AreEqual(HttpStatusCode.OK, putResponse.StatusCode);
            var newEmployee = putResponse.DeserializeContent<Employee>();

            Assert.AreEqual(employee.FirstName, newEmployee.FirstName);
            Assert.AreEqual(employee.LastName, newEmployee.LastName);
        }

        [TestMethod]
        public void UpdateEmployee_Returns_NotFound()
        {
            // Arrange
            var employee = new Employee()
            {
                EmployeeId = "Invalid_Id",
                Department = "Music",
                FirstName = "Sunny",
                LastName = "Bono",
                Position = "Singer/Song Writer",
            };
            var requestContent = new JsonSerialization().ToJson(employee);

            // Execute
            var postRequestTask = _httpClient.PutAsync($"api/employee/{employee.EmployeeId}",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var response = postRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        // *** REPORTING STRUCTURE TESTS ***

        /*
        First Reporting Structure test to get the reporting structure from an existing employee

        If this test is run with all other tests in order then the expectedNumberOfReports will be 1 since 
        Ringo was updated to Pete in the previous commands, this causes Ringo to be deleted from Johns directReports list as well
        But if this test is run on its own, the new build won't have that update so the expectedNumberOfReports would be 4
        */
        [TestMethod]
        public void GetReportingStructure_Returns_Ok()
        {
            // Arrange
            var employeeId = "16a596ae-edd3-4847-99fe-c4518e82c86f";
            var expectedFirstName = "John";
            var expectedLastName = "Lennon";
            var expectedNumberOfReports = 1;

            // Execute
            var getRequestTask = _httpClient.GetAsync($"api/employee/reporting-structure/{employeeId}");
            var response = getRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var reportingStructure = response.DeserializeContent<ReportingStructure>();
            // Comment to debug
            //Console.WriteLine("\nNeil: Number of Reports Expected = {0}, Number of Reports = {1}", expectedNumberOfReports, reportingStructure.numberOfReports);
            Assert.AreEqual(expectedNumberOfReports, reportingStructure.numberOfReports);
            Assert.AreEqual(expectedFirstName, reportingStructure.employee.FirstName);
            Assert.AreEqual(expectedLastName, reportingStructure.employee.LastName);
        }

        /*
        Second Reporting Structure test to create a new employee, check their structure to verify that it is 0,
        then update that employee to have 3 employees under them so that there structure count updates to 3
        */
        [TestMethod]
        public void GetReportingStructure_Returns_Ok_After_Update()
        {
            // Construct Neil who will have 0 direct reports
            var employee_Neil = new Employee()
            {
                Department = "Engineering",
                FirstName = "Neil",
                LastName = "Young",
                Position = "Sr. Developer",
            };
            var requestContent_Neil = new JsonSerialization().ToJson(employee_Neil);

            var expectedNumberOfReports_NeilTest1 = 0;

            //Execute Neil to create
            var postRequestTask_Neil = _httpClient.PostAsync("api/employee",
               new StringContent(requestContent_Neil, Encoding.UTF8, "application/json"));
            var postResponse_Neil = postRequestTask_Neil.Result;
            employee_Neil = postResponse_Neil.DeserializeContent<Employee>();

            // Dispose of previous client
            postRequestTask_Neil.Dispose();

            // Execute Neil Get reporting structure
            var getRequestTask_Neil = _httpClient.GetAsync($"api/employee/reporting-structure/{employee_Neil.EmployeeId}");
            var getResponse_Neil = getRequestTask_Neil.Result;

            // Assert Neil has 0 direct reports
            Assert.AreEqual(HttpStatusCode.OK, getResponse_Neil.StatusCode);
            var reportingStructure_NeilTest1 = getResponse_Neil.DeserializeContent<ReportingStructure>();
            // Comment to debug
            // Console.WriteLine("\nNeil: Number of Reports Expected = {0}, Number of Reports = {1}", expectedNumberOfReports_NeilTest1, reportingStructure_NeilTest1.numberOfReports);
            Assert.AreEqual(expectedNumberOfReports_NeilTest1, reportingStructure_NeilTest1.numberOfReports);
            Assert.AreEqual(employee_Neil.FirstName, reportingStructure_NeilTest1.employee.FirstName);
            Assert.AreEqual(employee_Neil.LastName, reportingStructure_NeilTest1.employee.LastName);

            // Update Neil to have 3 new employees under him
            var employee_Van = new Employee()
            {
                Department = "Engineering",
                FirstName = "Van",
                LastName = "Morrison",
                Position = "Jr. Developer"
            };
            
            var employee_Jerry = new Employee()
            {
                Department = "Engineering",
                FirstName = "Jerry",
                LastName = "Garcia",
                Position = "Jr. Developer"
            };

            List<Employee> directReports_Stevie = new List<Employee>()
            {
                employee_Van,
                employee_Jerry
            };

            var employee_Stevie = new Employee()
            {
                Department = "Engineering",
                FirstName = "Stevie",
                LastName = "Nicks",
                Position = "Developer",
                DirectReports = directReports_Stevie
            };
            
            List<Employee> directReports_Neil = new List<Employee>()
            {
                employee_Stevie
            };

            // Update Neil to have Stevie as a DirectReport
            var up_employee_Neil = new Employee()
            {
                EmployeeId = employee_Neil.EmployeeId,
                Department = "Engineering",
                FirstName = "Neil",
                LastName = "Young",
                Position = "Sr. Developer",
                DirectReports = directReports_Neil
            };
            var up_requestContent_Neil = new JsonSerialization().ToJson(up_employee_Neil);
            // Execute Neil Update
            var putRequestTask_Neil = _httpClient.PutAsync($"api/employee/{up_employee_Neil.EmployeeId}",
               new StringContent(up_requestContent_Neil, Encoding.UTF8, "application/json"));
            var putResponse_Neil = putRequestTask_Neil.Result;

            putRequestTask_Neil.Dispose();

            /*
            After the updates, the hierarchy now looks like this:
                            Neil Young
                                |
                            Stevie Nicks
                             /        \
                   Jerry Garcia     Van Morrison
            

            Neil = 3
            */
            var expectedNumberOfReports_NeilTest2 = 3;

            // Execute Neil 
            var up_getRequestTask_Neil = _httpClient.GetAsync($"api/employee/reporting-structure/{up_employee_Neil.EmployeeId}");
            var up_getResponse_Neil = up_getRequestTask_Neil.Result;

            // Assert John Test 1
            Assert.AreEqual(HttpStatusCode.OK, up_getResponse_Neil.StatusCode);
            var reportingStructure_NeilTest2 = up_getResponse_Neil.DeserializeContent<ReportingStructure>();
            // Comment to debug
            // Console.WriteLine("\nNeil: Number of Reports Expected = {0}, Number of Reports = {1}", expectedNumberOfReports_NeilTest2, reportingStructure_NeilTest2.numberOfReports);
            Assert.AreEqual(expectedNumberOfReports_NeilTest2, reportingStructure_NeilTest2.numberOfReports);
        }

        // *** COMPENSATION TESTS *** 

        /*
        First Compensation test to create a new employee with a new compensation
        */
        [TestMethod]
        public void CreateCompensation_Returns_Created()
        {
            // Arrange
            Employee employee_Neil = new Employee()
            {
                Department = "Engineering",
                FirstName = "Neil",
                LastName = "Young",
                Position = "Developer",
            };

            var compensation = new Compensation()
            {
                employee = employee_Neil,
                salary = 100000,
                effectiveDate = DateTime.Now
            };

            var requestContent = new JsonSerialization().ToJson(compensation);

            // Execute
            var postRequestTask = _httpClient.PostAsync("api/employee/compensation",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var response = postRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            var newCompensation = response.DeserializeContent<Compensation>();
            Assert.IsNotNull(newCompensation.CompensationId);
            Assert.AreEqual(employee_Neil.FirstName, newCompensation.employee.FirstName);
            Assert.AreEqual(employee_Neil.LastName, newCompensation.employee.LastName);
            Assert.AreEqual(employee_Neil.Department, newCompensation.employee.Department);
            Assert.AreEqual(employee_Neil.Position, newCompensation.employee.Position);
            Assert.AreEqual(compensation.salary, newCompensation.salary);
            Assert.AreEqual(compensation.effectiveDate, newCompensation.effectiveDate);
        }

        /*
        Second Compensation test to create a new compensation with an existing employee
        */
        [TestMethod]
        public void CreateCompensation_Returns_Created_ExistingEmp()
        {
            // Arrange
            Employee employee_John = new Employee()
            {
                EmployeeId = "16a596ae-edd3-4847-99fe-c4518e82c86f"
            };
            var employeeExpectingFirstName = "John";
            var employeeExpectingLastName = "Lennon";
            var employeeExpectingDepartment = "Engineering";
            var employeeExpectingLPosition = "Development Manager";

            var compensation = new Compensation()
            {
                employee = employee_John,
                salary = 100000,
                effectiveDate = DateTime.Now
            };

            var requestContent = new JsonSerialization().ToJson(compensation);

            // Execute
            var postRequestTask = _httpClient.PostAsync("api/employee/compensation",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var response = postRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            var newCompensation = response.DeserializeContent<Compensation>();
            Assert.IsNotNull(newCompensation.CompensationId);
            Assert.AreEqual(employeeExpectingFirstName, newCompensation.employee.FirstName);
            Assert.AreEqual(employeeExpectingLastName, newCompensation.employee.LastName);
            Assert.AreEqual(employeeExpectingDepartment, newCompensation.employee.Department);
            Assert.AreEqual(employeeExpectingLPosition, newCompensation.employee.Position);
            Assert.AreEqual(compensation.salary, newCompensation.salary);
            Assert.AreEqual(compensation.effectiveDate, newCompensation.effectiveDate);
        }

        /*
        Third Compensation test to create a new employee and then access the compensation data
        */
        [TestMethod]
        public void GetCompensation_Returns_Created()
        {
            // Arrange Neil Setup
            Employee employee_Neil = new Employee()
            {
                Department = "Engineering",
                FirstName = "Neil",
                LastName = "Young",
                Position = "Developer",
            };

            var createCompensation = new Compensation()
            {
                employee = employee_Neil,
                salary = 100000,
                effectiveDate = DateTime.Now
            };

            var requestContent = new JsonSerialization().ToJson(createCompensation);

            // Execute Neil Setup
            var postRequestTask = _httpClient.PostAsync("api/employee/compensation",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var response = postRequestTask.Result;
            var compensation_Neil = response.DeserializeContent<Compensation>();
            employee_Neil = compensation_Neil.employee;

            postRequestTask.Dispose();

            // Execute Neil Get Request
            var getRequestTask = _httpClient.GetAsync($"api/employee/compensation/{employee_Neil.EmployeeId}");
            var getResponse = getRequestTask.Result;

            // Assert 
            Assert.AreEqual(HttpStatusCode.OK, getResponse.StatusCode);
            var getCompensation = getResponse.DeserializeContent<Compensation>();
            Assert.AreEqual(createCompensation.salary, getCompensation.salary);
            Assert.AreEqual(createCompensation.effectiveDate, getCompensation.effectiveDate);
        }

        /*
        Fourth Compensation test to test with invalid Id
        */
        [TestMethod]
        public void GetCompensation_Returns_NotFound()
        {
            // Arrange 
            var Invalid_Id = "Invalid_Id";

            // Execute 
            var getRequestTask = _httpClient.GetAsync($"api/employee/compensation/{Invalid_Id}");
            var getResponse = getRequestTask.Result;

            // Assert 
            Assert.AreEqual(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

    }
}
