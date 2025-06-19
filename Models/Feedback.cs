using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentEventAPI.Models
{
    [Table("GatheringFeedbacks")]
    public class Feedback
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Gathering Reference")]
        public int EventId { get; set; }

        [Required]
        [Display(Name = "Student Reference")]
        public int StudentId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must range from 1 to 5")]
        [Display(Name = "Gathering Rating")]
        public int Rating { get; set; }

        [StringLength(1000, ErrorMessage = "Comment must not exceed 1000 characters")]
        [Display(Name = "Feedback Commentary")]
        public string Comment { get; set; } = string.Empty;

        [Display(Name = "Submission Date")]
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Would Recommend")]
        public bool WouldRecommend { get; set; } = true;

        [Range(1, 5, ErrorMessage = "Organization rating must range from 1 to 5")]
        [Display(Name = "Organization Rating")]
        public int OrganizationRating { get; set; } = 3;

        [Range(1, 5, ErrorMessage = "Content rating must range from 1 to 5")]
        [Display(Name = "Content Quality Rating")]
        public int ContentRating { get; set; } = 3;

        [Range(1, 5, ErrorMessage = "Venue rating must range from 1 to 5")]
        [Display(Name = "Venue Rating")]
        public int VenueRating { get; set; } = 3;

        [StringLength(500, ErrorMessage = "Suggestions must not exceed 500 characters")]
        [Display(Name = "Enhancement Suggestions")]
        public string Suggestions { get; set; } = string.Empty;

        // Navigation properties
        [ForeignKey("EventId")]
        public virtual Event Event { get; set; } = null!;

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; } = null!;

        // Computed property
        [NotMapped]
        public double AverageScore => (Rating + OrganizationRating + ContentRating + VenueRating) / 4.0;
    }
}
