using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cw3_apbd.Models_2;
using cw3_apbd.DTOs.Request;

namespace cw3_apbd.Services
{
    public interface IDbServicesCwieczenie10
    {
        public ICollection<Student> GetStudents();

        public Boolean UpdateStudent(StudentRequest request);

        public Boolean DeleteStudent(string indexNumber);
    }
}
