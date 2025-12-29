namespace SimpleAuthApi.Dto
{
    public class DocumentUploadDto
    {
        // Représente le fichier envoyé via un formulaire multipart/form-data
        public IFormFile File { get; set; }
    }
}
