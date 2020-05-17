using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cw3_apbd.DTOs.Request
{
    public class RequestAccount
    {
        public EnrollStudentRequest Student { get; set; }
        public string Password   { get; set; }
        // public string Salt { get; set; }

    }
}
