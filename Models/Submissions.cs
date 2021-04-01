using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class Submissions
    {
        
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        
        public string DesignationName { get; set; }

        public string Token { get; set; }
    }
}