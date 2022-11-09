using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class Submissions
    {

        [DatabaseGenerated(DatabaseGeneratedOption.None)]

        public string? DesignationName { get; set; }

        public string? Token { get; set; }
    }
}