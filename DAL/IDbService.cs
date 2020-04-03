using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cw3_apbd.Models;

namespace cw3_apbd.DAL
{
    public interface IDbService
    {
        public IEnumerable<Student> GetStudents();
    }
}
