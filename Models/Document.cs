using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Prog2bPOEPart2.Models
{
    public class Document
    {
        [Key]
        public int DocumentId { get; set; }
        
        public string? FileName { get; set; }
        public string? FileType { get; set; }
        public string? FilePath { get; set; }
        public long FileSize { get; set; }
        public DateTime UploadedDate { get; set; }
        public int ClaimId { get; set; }
        public Claim? Claim { get; set; }
        [NotMapped]
        public IFormFile? File { get; set; }

    }
}
//https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads?view=aspnetcore-8.0
//IFormFile "learning about fileuploads