using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace EPHRS
{
    class Employee
    {
        public int Id { get; set; }                  // 1
        public string FirstName { get; set; } = "";  // 2
        public string LastName { get; set; } = "";   // 3
        public string Department { get; set; } = ""; // 4
        public string JobTitle { get; set; } = "";   // 5
        public decimal MonthlySalary { get; set; }   // 6
        public double TaxRate { get; set; }          // 7 (0.12 = 12%)
        public DateTime HireDate { get; set; }       // 8
        public bool IsActive { get; set; }           // 9
        public string Email { get; set; } = "";      // 10
        public decimal NetSalary => MonthlySalary - (MonthlySalary * (decimal)TaxRate);
    }

    class Program
    {
        static List<Employee> employees = new List<Employee>();
        static int nextId = 1001; // auto-increment starting at 1001

        static void Main(string[] args)
        {
            bool running = true;
            while (running)
            {
                Console.WriteLine("\n===== EPHRS - Main Branch =====");
                Console.WriteLine("[1] Add Employee (Create)");
                Console.WriteLine("[2] View All Employees (Read)");
                Console.WriteLine("[3] Search / Filter Employees");
                Console.WriteLine("[4] Update Employee");
                Console.WriteLine("[5] Delete Employee");
                Console.WriteLine("[0] Exit");
                Console.Write("Choose an option: ");

                switch (Console.ReadLine()?.Trim())
                {
                    case "1": AddEmployee(); break;
                    case "2": ViewAll(); break;
                    case "3": SearchMenu(); break;
                    case "4": UpdateEmployee(); break;
                    case "5": DeleteEmployee(); break;
                    case "0": running = false; Console.WriteLine("Goodbye!"); break;
                    default: Console.WriteLine("Invalid option. Try again."); break;
                }
            }
        }

        static void AddEmployee()
        {
            Console.WriteLine("\n--- Add New Employee ---");
            var emp = new Employee
            {
                Id = nextId++,
                FirstName = ReadText("First Name: "),
                LastName = ReadText("Last Name: "),
                Department = ReadText("Department (IT/HR/Sales/Finance): "),
                JobTitle = ReadText("Job Title: "),
                MonthlySalary = ReadDecimal("Monthly Salary: "),
                TaxRate = ReadTaxRate("Tax Rate (e.g. 0.12 for 12%): "),
                HireDate = ReadDate("Hire Date (yyyy-MM-dd): "),
                IsActive = ReadBool("Is Active? (y/n): "),
                Email = ReadEmail("Email: ")
            };
            employees.Add(emp);
            Console.WriteLine($"Employee added with ID {emp.Id}.");
        }

        static void ViewAll()
        {
            Console.WriteLine("\n--- All Employees ---");
            var sorted = employees.OrderBy(e => e.Id).ToList(); // LINQ
            PrintTable(sorted);
        }
        static void SearchMenu()
        {
            Console.WriteLine("\n--- Search / Filter ---");
            Console.WriteLine("[1] By ID");
            Console.WriteLine("[2] By Name (first or last)");
            Console.WriteLine("[3] By Department");
            Console.WriteLine("[4] Active employees only");
            Console.WriteLine("[5] Salary above an amount");
            Console.Write("Choose: ");

            List<Employee> results;
            switch (Console.ReadLine()?.Trim())
            {
                case "1":
                    int id = ReadInt("Enter ID: ");
                    results = employees.Where(e => e.Id == id).ToList();
                    break;
                case "2":
                    string name = ReadText("Enter name: ").ToLower();
                    results = employees.Where(e => e.FirstName.ToLower().Contains(name)
                                                || e.LastName.ToLower().Contains(name)).ToList();
                    break;
                case "3":
                    string dept = ReadText("Enter department: ");
                    results = employees.Where(e => e.Department.Equals(dept, StringComparison.OrdinalIgnoreCase)).ToList();
                    break;
                case "4":
                    results = employees.Where(e => e.IsActive).ToList();
                    break;
                case "5":
                    decimal min = ReadDecimal("Minimum salary: ");
                    results = employees.Where(e => e.MonthlySalary > min)
                                       .OrderByDescending(e => e.MonthlySalary).ToList();
                    break;
                default:
                    Console.WriteLine("Invalid option.");
                    return;
            }
            PrintTable(results);
        }

        static void UpdateEmployee()
        {
            Console.WriteLine("\n--- Update Employee ---");
            int id = ReadInt("Enter Employee ID to update: ");
            var emp = employees.FirstOrDefault(e => e.Id == id); // LINQ
            if (emp == null) { Console.WriteLine("Employee not found."); return; }

            Console.WriteLine("Press ENTER to keep the current value.");
            emp.FirstName = KeepOrNew($"First Name [{emp.FirstName}]: ", emp.FirstName);
            emp.LastName = KeepOrNew($"Last Name [{emp.LastName}]: ", emp.LastName);
            emp.Department = KeepOrNew($"Department [{emp.Department}]: ", emp.Department);
            emp.JobTitle = KeepOrNew($"Job Title [{emp.JobTitle}]: ", emp.JobTitle);

            string s = KeepOrNew($"Monthly Salary [{emp.MonthlySalary}]: ", "");
            if (decimal.TryParse(s, out decimal sal) && sal >= 0) emp.MonthlySalary = sal;

            s = KeepOrNew($"Tax Rate [{emp.TaxRate}]: ", "");
            if (double.TryParse(s, out double tax) && tax >= 0 && tax <= 1) emp.TaxRate = tax;

            s = KeepOrNew($"Hire Date [{emp.HireDate:yyyy-MM-dd}]: ", "");
            if (DateTime.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime d))
                emp.HireDate = d;

            s = KeepOrNew($"Is Active (y/n) [{(emp.IsActive ? "y" : "n")}]: ", "").ToLower();
            if (s == "y") emp.IsActive = true; else if (s == "n") emp.IsActive = false;

            s = KeepOrNew($"Email [{emp.Email}]: ", "");
            if (s.Contains("@") && s.Contains(".")) emp.Email = s;

            Console.WriteLine("Employee updated.");
        }

        static void DeleteEmployee()
        {
            Console.WriteLine("\n--- Delete Employee ---");
            int id = ReadInt("Enter Employee ID to delete: ");
            var emp = employees.FirstOrDefault(e => e.Id == id); // LINQ
            if (emp == null) { Console.WriteLine("Employee not found."); return; }

            if (ReadBool($"Delete {emp.FirstName} {emp.LastName}? (y/n): "))
            {
                employees.Remove(emp);
                Console.WriteLine("Employee deleted.");
            }
            else Console.WriteLine("Delete cancelled.");
        }

        static void PrintTable(List<Employee> list)
        {
            if (!list.Any()) { Console.WriteLine("No records found."); return; }

            Console.WriteLine($"{"ID",-6}{"First",-12}{"Last",-12}{"Dept",-10}{"Title",-20}{"Salary",12}{"Tax",6}{"Hired",12}  {"Active",-7}{"Email"}");
            Console.WriteLine(new string('-', 125));
            foreach (var e in list)
            {
                Console.WriteLine($"{e.Id,-6}{e.FirstName,-12}{e.LastName,-12}{e.Department,-10}{e.JobTitle,-20}{e.MonthlySalary,12:N2}{e.TaxRate,6:P0}{e.HireDate,12:yyyy-MM-dd}  {(e.IsActive ? "Yes" : "No"),-7}{e.Email}");
            }
            Console.WriteLine($"Total records: {list.Count}");
        }

        static string ReadText(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine()?.Trim() ?? "";
                if (input.Length > 0) return input;
                Console.WriteLine("This field cannot be empty.");
            }
        }

        static int ReadInt(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), out int v)) return v;
                Console.WriteLine("Please enter a whole number.");
            }
        }

        static decimal ReadDecimal(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                if (decimal.TryParse(Console.ReadLine(), out decimal v) && v >= 0) return v;
                Console.WriteLine("Please enter a valid non-negative amount.");
            }
        }

        static double ReadTaxRate(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                if (double.TryParse(Console.ReadLine(), out double v) && v >= 0 && v <= 1) return v;
                Console.WriteLine("Enter a value between 0 and 1 (e.g. 0.12).");
            }
        }

        static DateTime ReadDate(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                if (DateTime.TryParseExact(Console.ReadLine(), "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                           DateTimeStyles.None, out DateTime v)) return v;
                Console.WriteLine("Use the format yyyy-MM-dd (e.g. 2024-06-15).");
            }
        }

        static bool ReadBool(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string s = Console.ReadLine()?.Trim().ToLower() ?? "";
                if (s == "y" || s == "yes") return true;
                if (s == "n" || s == "no") return false;
                Console.WriteLine("Please type y or n.");
            }
        }

        static string ReadEmail(string prompt)
        {
            while (true)
            {
                string s = ReadText(prompt);
                if (s.Contains("@") && s.Contains(".")) return s;
                Console.WriteLine("Please enter a valid email address.");
            }
        }

        static string KeepOrNew(string prompt, string current)
        {
            Console.Write(prompt);
            string s = Console.ReadLine()?.Trim() ?? "";
            return s.Length == 0 ? current : s;
        }
    }
}