using System;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Course
    {

        public string nameCourse { get; set; }

        public string Coordinator { get; set; }
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm:ss tt}", ApplyFormatInEditMode = true)]
        [Required]
        public DateTime dateEnd { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm:ss tt}", ApplyFormatInEditMode = true)]
        [Required]
        public DateTime dateFinal { get; set; }


        public string[] Student { get; set; }

    }
}