using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Services
{
    public interface IDepartmentService
    {
        List<Department> GetAllDepartments();

        Department? GetDepartmentById(int id);

        void AddDepartment(Department department);

        void UpdateDepartment(Department department);

        void DeleteDepartment(int id);

        bool HasEmployees(int departmentId);
    }
}