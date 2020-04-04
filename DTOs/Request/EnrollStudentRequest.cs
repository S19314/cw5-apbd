using System;
using System.ComponentModel.DataAnnotations;
namespace cw3_apbd.DTOs.Request
{
    public class EnrollStudentRequest
    {
        [Required(ErrorMessage="FirstName nie muszę być pusty")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "LastName nie muszę być pusty")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "IndexNumber nie muszę być pusty")]
        public string IndexNumber { get; set; }

        [Required(ErrorMessage = "BirthDate nie muszę być pusty")]
        public string BirthDate { get; set; }
        // public DateTime BirthDate { get; set; }
        [Required(ErrorMessage = "Studies nie muszę być pusty")]
        public string Studies { get; set; }

        // [Required(ErrorMessage = "IdEnrollment nie muszę być pusty")]
        public int IdEnrollment { get; set; }


    }
}