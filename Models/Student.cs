using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentEventAPI.Models
{
    [Table("StudentProfiles")]
    public class Student
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Student name specification is essential")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must contain 2-100 characters")]
        [Display(Name = "Complete Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address specification is essential")]
        [EmailAddress(ErrorMessage = "Please specify a valid email address")]
        [StringLength(255, ErrorMessage = "Email must not exceed 255 characters")]
        [Display(Name = "Electronic Mail")]
        public string Email { get; set; } = string.Empty;

        [StringLength(15, ErrorMessage = "Phone number must not exceed 15 characters")]
        [Phone(ErrorMessage = "Please specify a valid phone number")]
        [Display(Name = "Contact Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Student ID must not exceed 20 characters")]
        [Display(Name = "Student Identification")]
        public string StudentIdentifier { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Department name must not exceed 100 characters")]
        [Display(Name = "Academic Faculty")]
        public string Department { get; set; } = string.Empty;

        [Range(1, 8, ErrorMessage = "Study year must range from 1 to 8")]
        [Display(Name = "Current Year")]
        public int YearOfStudy { get; set; } = 1;

        [Display(Name = "Enrollment Date")]
        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Account Status")]
        public StudentStatus Status { get; set; } = StudentStatus.Active;

        // Navigation property
        public virtual ICollection<Registration> Registrations { get; set; }

        public Student()
        {
            Registrations = new HashSet<Registration>();
        }

        // Computed property
        [NotMapped]
        public int TotalEventsAttended => Registrations?.Count ?? 0;
    }

    public enum StudentStatus
    {
        Active = 1,
        Inactive = 2,
        Suspended = 3,
        Graduated = 4
    }
}
