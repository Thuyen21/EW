using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class Faculty
    {

        public SignUpModel marketingCoordinatorId { get; set; }

        public SignUpModel[] Student { get; set; }

    }
}