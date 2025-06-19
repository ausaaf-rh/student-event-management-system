using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentEventAPI.Models
{
    [Table("UniversityGatherings")]
    public class Event
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Gathering title is essential")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Gathering title must contain 3-200 characters")]
        [Display(Name = "Gathering Title")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Gathering description must not exceed 500 characters")]
        [Display(Name = "Gathering Description")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Location specification is essential")]
        [StringLength(300, MinimumLength = 2, ErrorMessage = "Location must contain 2-300 characters")]
        [Display(Name = "Physical Location")]
        public string Venue { get; set; }

        [Required(ErrorMessage = "Gathering date and time specification is essential")]
        [Display(Name = "Planned Date & Time")]
        [DataType(DataType.DateTime)]
        public DateTime Date { get; set; }

        [Range(1, 10000, ErrorMessage = "Participant limit must range from 1 to 10,000")]
        [Display(Name = "Participant Limit")]
        public int MaxCapacity { get; set; } = 100;

        [Display(Name = "Enrollment Deadline")]
        [DataType(DataType.DateTime)]
        public DateTime? RegistrationDeadline { get; set; }

        [Display(Name = "Gathering Classification")]
        public EventCategory Category { get; set; } = EventCategory.Academic;

        [Display(Name = "Establishment Date")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Last Revision")]
        public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;        // Navigation properties
        public virtual ICollection<Registration> Registrations { get; set; }
        public virtual ICollection<Feedback> Feedbacks { get; set; }

        public Event()
        {
            Name = string.Empty;
            Venue = string.Empty;
            Registrations = new HashSet<Registration>();
            Feedbacks = new HashSet<Feedback>();
        }

        // Computed property
        [NotMapped]
        public int CurrentParticipantCount => Registrations?.Count ?? 0;

        [NotMapped]
        public bool HasRemainingSpots => CurrentParticipantCount < MaxCapacity;
    }

    public enum EventCategory
    {
        Academic = 1,
        Workshop = 2,
        Seminar = 3,
        Conference = 4,
        Sports = 5,
        Cultural = 6,
        Social = 7
    }
}

