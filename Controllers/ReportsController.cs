using ClosedXML.Excel;
using EmployeeManagementSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EmployeeManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReportsController : Controller
    {
        private readonly EmployeeDbContext _context;

        public ReportsController(
            EmployeeDbContext context)
        {
            _context = context;
        }
 
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
 
        [HttpGet]
        public IActionResult Employees()
        {
            var employees = _context.Employees
                .OrderBy(e => e.Name)
                .ToList();

            return View(employees);
        }

 
        [HttpGet]
        public IActionResult ExportEmployeesExcel()
        {
            var employees = _context.Employees
                .OrderBy(e => e.Name)
                .ToList();

            using var workbook = new XLWorkbook();

            var worksheet =
                workbook.Worksheets.Add(
                    "Employee Report");


            // Title

            worksheet.Cell("A1")
                .Value = "Employee Report";

            worksheet.Range("A1:G1")
                .Merge();

            worksheet.Cell("A1")
                .Style.Font.Bold = true;

            worksheet.Cell("A1")
                .Style.Font.FontSize = 18;

            worksheet.Cell("A1")
                .Style.Alignment.Horizontal =
                    XLAlignmentHorizontalValues.Center;


            // Generated date

            worksheet.Cell("A2")
                .Value =
                $"Generated: {DateTime.Now:dd-MM-yyyy HH:mm}";

            worksheet.Range("A2:G2")
                .Merge();


            // Headers

            worksheet.Cell("A4")
                .Value = "ID";

            worksheet.Cell("B4")
                .Value = "Name";

            worksheet.Cell("C4")
                .Value = "Email";

            worksheet.Cell("D4")
                .Value = "Phone";

            worksheet.Cell("E4")
                .Value = "Department ID";

            worksheet.Cell("F4")
                .Value = "Salary";

            worksheet.Cell("G4")
                .Value = "Joining Date";


            worksheet.Range("A4:G4")
                .Style.Font.Bold = true;


            // Data

            int row = 5;

            foreach (var employee in employees)
            {
                worksheet.Cell(row, 1)
                    .Value = employee.Id;

                worksheet.Cell(row, 2)
                    .Value = employee.Name;

                worksheet.Cell(row, 3)
                    .Value = employee.Email;

                worksheet.Cell(row, 4)
                    .Value = employee.Phone;

                worksheet.Cell(row, 5)
                    .Value = employee.DepartmentId;

                worksheet.Cell(row, 6)
                    .Value = employee.Salary;

                worksheet.Cell(row, 7)
                    .Value =
                    employee.JoiningDate
                        .ToString("dd-MM-yyyy");

                row++;
            }


            // Formatting

            worksheet.Column("F")
                .Style.NumberFormat.Format =
                    "₹#,##0.00";

            worksheet.Columns()
                .AdjustToContents();


            using var stream =
                new MemoryStream();

            workbook.SaveAs(stream);

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"EmployeeReport_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }
 
        [HttpGet]
        public IActionResult ExportEmployeesPdf()
        {
            var employees = _context.Employees
                .OrderBy(e => e.Name)
                .ToList();

            var document =
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4.Landscape());

                        page.Margin(30);

                        page.Header()
                            .Text("Employee Report")
                            .Bold()
                            .FontSize(22);


                        page.Content()
                            .Table(table =>
                            {
                                table.ColumnsDefinition(
                                    columns =>
                                    {
                                        columns.ConstantColumn(35);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                    });


                                table.Header(header =>
                                {
                                    header.Cell()
                                        .Element(HeaderCell)
                                        .Text("ID");

                                    header.Cell()
                                        .Element(HeaderCell)
                                        .Text("Name");

                                    header.Cell()
                                        .Element(HeaderCell)
                                        .Text("Email");

                                    header.Cell()
                                        .Element(HeaderCell)
                                        .Text("Phone");

                                    header.Cell()
                                        .Element(HeaderCell)
                                        .Text("Department");

                                    header.Cell()
                                        .Element(HeaderCell)
                                        .Text("Salary");

                                    header.Cell()
                                        .Element(HeaderCell)
                                        .Text("Joining Date");
                                });


                                foreach (var employee in employees)
                                {
                                    table.Cell()
                                        .Element(DataCell)
                                        .Text(employee.Id.ToString());

                                    table.Cell()
                                        .Element(DataCell)
                                        .Text(employee.Name);

                                    table.Cell()
                                        .Element(DataCell)
                                        .Text(employee.Email);

                                    table.Cell()
                                        .Element(DataCell)
                                        .Text(employee.Phone);

                                    table.Cell()
                                        .Element(DataCell)
                                        .Text(employee.DepartmentId.ToString());

                                    table.Cell()
                                        .Element(DataCell)
                                        .Text(
                                            $"₹{employee.Salary:N2}");

                                    table.Cell()
                                        .Element(DataCell)
                                        .Text(
                                            employee.JoiningDate
                                                .ToString("dd-MM-yyyy"));
                                }
                            });


                        page.Footer()
                            .AlignCenter()
                            .Text(text =>
                            {
                                text.Span("Generated: ");
                                text.Span(
                                    DateTime.Now
                                        .ToString(
                                            "dd-MM-yyyy HH:mm"));
                            });
                    });
                });


            var pdfBytes =
                document.GeneratePdf();

            return File(
                pdfBytes,
                "application/pdf",
                $"EmployeeReport_{DateTime.Now:yyyyMMddHHmmss}.pdf");
        }

 
        [HttpGet]
        public IActionResult Departments()
        {
            var departments = _context.Departments
                .Select(d => new
                {
                    d.Id,
                    d.Name,
                    d.Description,
                    EmployeeCount =
                        d.Employees.Count()
                })
                .OrderBy(d => d.Name)
                .ToList();

            return View(departments);
        }

 
        [HttpGet]
        public IActionResult ExportDepartmentsExcel()
        {
            var departments = _context.Departments
                .Select(d => new
                {
                    d.Id,
                    d.Name,
                    d.Description,
                    EmployeeCount =
                        d.Employees.Count()
                })
                .OrderBy(d => d.Name)
                .ToList();


            using var workbook =
                new XLWorkbook();

            var worksheet =
                workbook.Worksheets.Add(
                    "Department Report");


            worksheet.Cell("A1")
                .Value = "Department Report";

            worksheet.Range("A1:D1")
                .Merge();

            worksheet.Cell("A1")
                .Style.Font.Bold = true;

            worksheet.Cell("A1")
                .Style.Font.FontSize = 18;

            worksheet.Cell("A1")
                .Style.Alignment.Horizontal =
                    XLAlignmentHorizontalValues.Center;


            worksheet.Cell("A2")
                .Value =
                $"Generated: {DateTime.Now:dd-MM-yyyy HH:mm}";

            worksheet.Range("A2:D2")
                .Merge();


            worksheet.Cell("A4")
                .Value = "ID";

            worksheet.Cell("B4")
                .Value = "Department";

            worksheet.Cell("C4")
                .Value = "Description";

            worksheet.Cell("D4")
                .Value = "Employee Count";


            worksheet.Range("A4:D4")
                .Style.Font.Bold = true;


            int row = 5;

            foreach (var department in departments)
            {
                worksheet.Cell(row, 1)
                    .Value = department.Id;

                worksheet.Cell(row, 2)
                    .Value = department.Name;

                worksheet.Cell(row, 3)
                    .Value =
                    department.Description ?? "";

                worksheet.Cell(row, 4)
                    .Value =
                    department.EmployeeCount;

                row++;
            }


            worksheet.Columns()
                .AdjustToContents();


            using var stream =
                new MemoryStream();

            workbook.SaveAs(stream);

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"DepartmentReport_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }
 
        [HttpGet]
        public IActionResult ExportDepartmentsPdf()
        {
            var departments = _context.Departments
                .Select(d => new
                {
                    d.Id,
                    d.Name,
                    d.Description,
                    EmployeeCount =
                        d.Employees.Count()
                })
                .OrderBy(d => d.Name)
                .ToList();


            var document =
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);

                        page.Margin(30);

                        page.Header()
                            .Text("Department Report")
                            .Bold()
                            .FontSize(22);


                        page.Content()
                            .Table(table =>
                            {
                                table.ColumnsDefinition(
                                    columns =>
                                    {
                                        columns.ConstantColumn(40);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(4);
                                        columns.RelativeColumn(2);
                                    });


                                table.Header(header =>
                                {
                                    header.Cell()
                                        .Element(HeaderCell)
                                        .Text("ID");

                                    header.Cell()
                                        .Element(HeaderCell)
                                        .Text("Department");

                                    header.Cell()
                                        .Element(HeaderCell)
                                        .Text("Description");

                                    header.Cell()
                                        .Element(HeaderCell)
                                        .Text("Employees");
                                });


                                foreach (var department in departments)
                                {
                                    table.Cell()
                                        .Element(DataCell)
                                        .Text(
                                            department.Id.ToString());

                                    table.Cell()
                                        .Element(DataCell)
                                        .Text(
                                            department.Name);

                                    table.Cell()
                                        .Element(DataCell)
                                        .Text(
                                            department.Description
                                            ?? "");

                                    table.Cell()
                                        .Element(DataCell)
                                        .Text(
                                            department.EmployeeCount
                                            .ToString());
                                }
                            });


                        page.Footer()
                            .AlignCenter()
                            .Text(
                                $"Generated: {DateTime.Now:dd-MM-yyyy HH:mm}");
                    });
                });


            var pdfBytes =
                document.GeneratePdf();

            return File(
                pdfBytes,
                "application/pdf",
                $"DepartmentReport_{DateTime.Now:yyyyMMddHHmmss}.pdf");
        }

 
        [HttpGet]
        public IActionResult Leaves()
        {
            var leaves = _context.Leaves
                .Include(l => l.Employee)
                .OrderByDescending(l => l.Id)
                .ToList();

            return View(leaves);
        }
 
        [HttpGet]
        public IActionResult ExportLeavesExcel()
        {
            var leaves = _context.Leaves
                .Include(l => l.Employee)
                .OrderByDescending(l => l.Id)
                .ToList();


            using var workbook =
                new XLWorkbook();

            var worksheet =
                workbook.Worksheets.Add(
                    "Leave Report");


            worksheet.Cell("A1")
                .Value = "Leave Report";

            worksheet.Range("A1:H1")
                .Merge();

            worksheet.Cell("A1")
                .Style.Font.Bold = true;

            worksheet.Cell("A1")
                .Style.Font.FontSize = 18;

            worksheet.Cell("A1")
                .Style.Alignment.Horizontal =
                    XLAlignmentHorizontalValues.Center;


            worksheet.Cell("A2")
                .Value =
                $"Generated: {DateTime.Now:dd-MM-yyyy HH:mm}";

            worksheet.Range("A2:H2")
                .Merge();


            worksheet.Cell("A4")
                .Value = "ID";

            worksheet.Cell("B4")
                .Value = "Employee";

            worksheet.Cell("C4")
                .Value = "Leave Type";

            worksheet.Cell("D4")
                .Value = "Start Date";

            worksheet.Cell("E4")
                .Value = "End Date";

            worksheet.Cell("F4")
                .Value = "Duration";

            worksheet.Cell("G4")
                .Value = "Reason";

            worksheet.Cell("H4")
                .Value = "Status";


            worksheet.Range("A4:H4")
                .Style.Font.Bold = true;


            int row = 5;

            foreach (var leave in leaves)
            {
                worksheet.Cell(row, 1)
                    .Value = leave.Id;

                worksheet.Cell(row, 2)
                    .Value =
                    leave.Employee?.Name ?? "N/A";

                worksheet.Cell(row, 3)
                    .Value = leave.LeaveType;

                worksheet.Cell(row, 4)
                    .Value =
                    leave.StartDate
                        .ToString("dd-MM-yyyy");

                worksheet.Cell(row, 5)
                    .Value =
                    leave.EndDate
                        .ToString("dd-MM-yyyy");

                worksheet.Cell(row, 6)
                    .Value =
                    leave.DurationDays;

                worksheet.Cell(row, 7)
                    .Value =
                    leave.Reason ?? "";

                worksheet.Cell(row, 8)
                    .Value = leave.Status;

                row++;
            }


            worksheet.Columns()
                .AdjustToContents();


            using var stream =
                new MemoryStream();

            workbook.SaveAs(stream);

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"LeaveReport_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }

 
        [HttpGet]
        public IActionResult ExportLeavesPdf()
        {
            var leaves = _context.Leaves
                .Include(l => l.Employee)
                .OrderByDescending(l => l.Id)
                .ToList();


            var document =
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4.Landscape());

                        page.Margin(30);

                        page.Header()
                            .Text("Leave Report")
                            .Bold()
                            .FontSize(22);


                        page.Content()
                            .Table(table =>
                            {
                                table.ColumnsDefinition(
                                    columns =>
                                    {
                                        columns.ConstantColumn(35);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                        columns.ConstantColumn(55);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(1.5f);
                                    });


                                table.Header(header =>
                                {
                                    header.Cell()
                                        .Element(HeaderCell)
                                        .Text("ID");

                                    header.Cell()
                                        .Element(HeaderCell)
                                        .Text("Employee");

                                    header.Cell()
                                        .Element(HeaderCell)
                                        .Text("Leave Type");

                                    header.Cell()
                                        .Element(HeaderCell)
                                        .Text("Start");

                                    header.Cell()
                                        .Element(HeaderCell)
                                        .Text("End");

                                    header.Cell()
                                        .Element(HeaderCell)
                                        .Text("Days");

                                    header.Cell()
                                        .Element(HeaderCell)
                                        .Text("Reason");

                                    header.Cell()
                                        .Element(HeaderCell)
                                        .Text("Status");
                                });


                                foreach (var leave in leaves)
                                {
                                    table.Cell()
                                        .Element(DataCell)
                                        .Text(
                                            leave.Id.ToString());

                                    table.Cell()
                                        .Element(DataCell)
                                        .Text(
                                            leave.Employee?.Name
                                            ?? "N/A");

                                    table.Cell()
                                        .Element(DataCell)
                                        .Text(
                                            leave.LeaveType);

                                    table.Cell()
                                        .Element(DataCell)
                                        .Text(
                                            leave.StartDate
                                                .ToString(
                                                    "dd-MM-yyyy"));

                                    table.Cell()
                                        .Element(DataCell)
                                        .Text(
                                            leave.EndDate
                                                .ToString(
                                                    "dd-MM-yyyy"));

                                    table.Cell()
                                        .Element(DataCell)
                                        .Text(
                                            leave.DurationDays
                                            .ToString());

                                    table.Cell()
                                        .Element(DataCell)
                                        .Text(
                                            leave.Reason
                                            ?? "");

                                    table.Cell()
                                        .Element(DataCell)
                                        .Text(
                                            leave.Status);
                                }
                            });


                        page.Footer()
                            .AlignCenter()
                            .Text(
                                $"Generated: {DateTime.Now:dd-MM-yyyy HH:mm}");
                    });
                });


            var pdfBytes =
                document.GeneratePdf();

            return File(
                pdfBytes,
                "application/pdf",
                $"LeaveReport_{DateTime.Now:yyyyMMddHHmmss}.pdf");
        }
 
        private static IContainer HeaderCell(
            IContainer container)
        {
            return container
                .Background(Colors.Grey.Lighten2)
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Medium)
                .Padding(5)
                .DefaultTextStyle(
                    x => x.Bold()
                          .FontSize(9));
        }


        private static IContainer DataCell(
            IContainer container)
        {
            return container
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten3)
                .Padding(5)
                .DefaultTextStyle(
                    x => x.FontSize(8));
        }
    }
}