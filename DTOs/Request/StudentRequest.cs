using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cw3_apbd.DTOs.Request
{
    public class StudentRequest
    {

          public string IndexNumber { get; set;  }
        public string  FirstName { get; set; }
        public string  LastName { get; set; }
        public DateTime  BirthDate { get; set; }
        public int IdEnrollment { get; set; }
     
    }
}
