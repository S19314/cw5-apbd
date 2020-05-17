using System;
using System.Collections.Generic;

namespace cw3_apbd.Models_2
{
    public partial class Patient
    {
        public Patient()
        {
            Prescription = new HashSet<Prescription>();
        }

        public int IdPatient { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime Birthdate { get; set; }

        public virtual ICollection<Prescription> Prescription { get; set; }
    }
}
