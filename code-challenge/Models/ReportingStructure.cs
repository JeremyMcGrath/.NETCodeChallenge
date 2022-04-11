using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace challenge.Models
{
    public class ReportingStructure 
    {
        public ReportingStructure(Employee inputEmployee)
        {
            employee = inputEmployee;
        }
        public Employee employee { get; set; }
        public int numberOfReports 
        { 
            /*
            Get function to call helper starting at 0 direct reports
            */
            get
            {                
                return _countDirectReports(employee, 0);
            }
        }

        /*
        Helper function to recursivly check all direct reports for their own direct reports.

        Does not check for duplicates.
        */
        private int _countDirectReports(Employee emp, int count)
        {
            if (emp.DirectReports != null)
            {
                List<Employee> directReportList = emp.DirectReports.ToList();

                foreach (var directReport in directReportList)
                {
                    count++;
                    // Check if current directReport has direct reports
                    if (directReport.DirectReports != null)
                    {
                        return _countDirectReports(directReport, count);
                    }
                }
            }
            return count;
        }
    }
}
