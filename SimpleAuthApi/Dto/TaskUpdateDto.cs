namespace SimpleAuthApi.Dto
{
    public class TaskUpdateDto
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required DateTime DueDate { get; set; }
        public IFormFile File { get; set; }
        //serveur supprime l'ancien fichier 
        public bool DeleteExistingDocument { get; set; } = false;
    }
}
