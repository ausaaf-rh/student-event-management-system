using Microsoft.EntityFrameworkCore;
using StudentEventAPI.Data;
using StudentEventAPI.Models;

namespace StudentEventAPI.Services
{
    public class StudentRegistrationService : IStudentRegistrationService
    {
        private readonly AppDbContext _dataRepository;

        public StudentRegistrationService(AppDbContext dataRepository)
        {
            _dataRepository = dataRepository;
        }

        public async Task<IEnumerable<Student>> GetAllStudentsAsync()
        {
            return await _dataRepository.Students
                .Include(s => s.Registrations)
                .ThenInclude(r => r.Event)
                .OrderBy(s => s.FullName)
                .ToListAsync();
        }

        public async Task<Student?> GetStudentByIdAsync(int studentId)
        {
            return await _dataRepository.Students
                .Include(s => s.Registrations)
                .ThenInclude(r => r.Event)
                .FirstOrDefaultAsync(s => s.Id == studentId);
        }

        public async Task<Student> CreateStudentProfileAsync(Student studentData)
        {
            // Validate email uniqueness
            var emailExists = await _dataRepository.Students
                .AnyAsync(s => s.Email.ToLower() == studentData.Email.ToLower());

            if (emailExists)
                throw new InvalidOperationException("A student with this email address already exists.");

            studentData.EnrollmentDate = DateTime.UtcNow;
            _dataRepository.Students.Add(studentData);
            await _dataRepository.SaveChangesAsync();
            return studentData;
        }

        public async Task<Student?> UpdateStudentProfileAsync(int studentId, Student studentData)
        {
            var existingStudent = await _dataRepository.Students.FindAsync(studentId);
            if (existingStudent == null)
                return null;

            // Check email uniqueness (excluding current student)
            var emailExists = await _dataRepository.Students
                .AnyAsync(s => s.Email.ToLower() == studentData.Email.ToLower() && s.Id != studentId);

            if (emailExists)
                throw new InvalidOperationException("Another student with this email address already exists.");

            existingStudent.FullName = studentData.FullName;
            existingStudent.Email = studentData.Email;
            existingStudent.PhoneNumber = studentData.PhoneNumber;
            existingStudent.StudentIdentifier = studentData.StudentIdentifier;
            existingStudent.Department = studentData.Department;
            existingStudent.YearOfStudy = studentData.YearOfStudy;
            existingStudent.Status = studentData.Status;

            await _dataRepository.SaveChangesAsync();
            return existingStudent;
        }

        public async Task<bool> DeleteStudentProfileAsync(int studentId)
        {
            var studentToDelete = await _dataRepository.Students.FindAsync(studentId);
            if (studentToDelete == null)
                return false;

            _dataRepository.Students.Remove(studentToDelete);
            await _dataRepository.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Student>> SearchStudentsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllStudentsAsync();

            var searchQuery = searchTerm.ToLower().Trim();

            return await _dataRepository.Students
                .Where(s => s.FullName.ToLower().Contains(searchQuery) ||
                           s.Email.ToLower().Contains(searchQuery) ||
                           s.StudentIdentifier.ToLower().Contains(searchQuery) ||
                           s.Department.ToLower().Contains(searchQuery))
                .Include(s => s.Registrations)
                .OrderBy(s => s.FullName)
                .ToListAsync();
        }

        public async Task<Student?> GetStudentByEmailAsync(string email)
        {
            return await _dataRepository.Students
                .Include(s => s.Registrations)
                .ThenInclude(r => r.Event)
                .FirstOrDefaultAsync(s => s.Email.ToLower() == email.ToLower());
        }

        public async Task<IEnumerable<Student>> GetStudentsByDepartmentAsync(string department)
        {
            return await _dataRepository.Students
                .Where(s => s.Department.ToLower() == department.ToLower())
                .Include(s => s.Registrations)
                .OrderBy(s => s.FullName)
                .ToListAsync();
        }

        public async Task<bool> IsEmailUniqueAsync(string email, int? excludeStudentId = null)
        {
            var query = _dataRepository.Students
                .Where(s => s.Email.ToLower() == email.ToLower());

            if (excludeStudentId.HasValue)
                query = query.Where(s => s.Id != excludeStudentId.Value);

            return !await query.AnyAsync();
        }
    }
}
