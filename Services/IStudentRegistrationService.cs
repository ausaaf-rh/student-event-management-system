using StudentEventAPI.Models;

namespace StudentEventAPI.Services
{
    public interface IStudentRegistrationService
    {
        Task<IEnumerable<Student>> GetAllStudentsAsync();
        Task<Student?> GetStudentByIdAsync(int studentId);
        Task<Student> CreateStudentProfileAsync(Student studentData);
        Task<Student?> UpdateStudentProfileAsync(int studentId, Student studentData);
        Task<bool> DeleteStudentProfileAsync(int studentId);
        Task<IEnumerable<Student>> SearchStudentsAsync(string searchTerm);
        Task<Student?> GetStudentByEmailAsync(string email);
        Task<IEnumerable<Student>> GetStudentsByDepartmentAsync(string department);
        Task<bool> IsEmailUniqueAsync(string email, int? excludeStudentId = null);
    }
}
