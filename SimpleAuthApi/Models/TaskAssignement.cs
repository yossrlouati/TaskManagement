namespace SimpleAuthApi.Models
{
    public class TaskAssignement
    {
        public int TaskId { get; set; }
        //public TaskItem Task { get; set; } = null!;
        public TaskItem? Task { get; set; }
        public string UserId { get; set; }
        public DateTime AssignedDate { get; set; } = DateTime.Now;
    }
}
