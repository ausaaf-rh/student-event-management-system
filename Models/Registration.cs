using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentEventAPI.Models
{
    [Table("GatheringEnrollments")]
    public class Registration
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RegistrationId { get; set; }

        [Required]
        [Display(Name = "Gathering Reference")]
        public int EventId { get; set; }

        [Required]
        [Display(Name = "Student Reference")]
        public int StudentId { get; set; }

        [Display(Name = "Enrollment Date")]
        public DateTime RegistrationTimestamp { get; set; } = DateTime.UtcNow;

        [Display(Name = "Enrollment Status")]
        public RegistrationStatus Status { get; set; } = RegistrationStatus.Confirmed;

        [StringLength(500, ErrorMessage = "Special accommodations must not exceed 500 characters")]
        [Display(Name = "Special Accommodations")]
        public string SpecialRequirements { get; set; } = string.Empty;

        [Display(Name = "Attendance Status")]
        public bool HasCheckedIn { get; set; } = false;

        [Display(Name = "Attendance Time")]
        public DateTime? CheckInTime { get; set; }

        [StringLength(200, ErrorMessage = "Remarks must not exceed 200 characters")]
        [Display(Name = "Additional Remarks")]
        public string Notes { get; set; } = string.Empty;

        // Navigation properties
        [ForeignKey("EventId")]
        public virtual Event Event { get; set; } = null!;        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; } = null!;
    }

    public enum RegistrationStatus
    {
        Pending = 1,
        Confirmed = 2,
        Cancelled = 3,
        Waitlisted = 4,
        Completed = 5
    }
}
