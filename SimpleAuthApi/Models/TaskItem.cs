using System.ComponentModel.DataAnnotations;

namespace SimpleAuthApi.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;
        [StringLength(1000)]
        public string Description { get; set; }

        public bool IsCompleted { get; set; } = false;
        public DateTime DueDate { get; set; }
        //Admin qui a assigné la tache 
        public string? AssignedByAdminId { get; set; }
        public string? DocumentPath { get; set; }
        //Employers assignés 
        public ICollection<TaskAssignement>? Assignements { get; set; }
    }
}
