using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly EmployeeDbContext _context;

        public DepartmentService(EmployeeDbContext context)
        {
            _context = context;
        }

        public List<Department> GetAllDepartments()
        {
            return _context.Departments.ToList();
        }

        public Department? GetDepartmentById(int id)
        {
            return _context.Departments
                .FirstOrDefault(d => d.Id == id);
        }

        public void AddDepartment(Department department)
        {
            _context.Departments.Add(department);
            _context.SaveChanges();
        }

        public void UpdateDepartment(Department department)
        {
            _context.Departments.Update(department);
            _context.SaveChanges();
        }

        public void DeleteDepartment(int id)
        {
            var department = _context.Departments
                .FirstOrDefault(d => d.Id == id);

            if (department != null)
            {
                _context.Departments.Remove(department);
                _context.SaveChanges();
            }
        }
        public bool HasEmployees(int departmentId)
        {
            return _context.Employees
                .Any(e => e.DepartmentId == departmentId);
        }
    }
}