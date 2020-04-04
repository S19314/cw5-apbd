using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cw3_apbd.DTOs.Responde
{
    public class EnrollStudentResponde
    {
        public int Semester { get; set; }
        public int IdEnrollment { get; set; }
        public int IdStudy { get; set; }

        public string StartDate { get; set; }
    }

}
