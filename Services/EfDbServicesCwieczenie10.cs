using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cw3_apbd.Models_2;
using cw3_apbd.DTOs.Request;

namespace cw3_apbd.Services
{
    public class EfDbServicesCwieczenie10 : IDbServicesCwieczenie10
    {

        public s19314Context _context { get; set; }
        public EfDbServicesCwieczenie10(s19314Context s19314Conext) {
            _context = s19314Conext;
        }


        public ICollection<Student> GetStudents() {
            return _context.Student.ToList();
        }


        public Boolean UpdateStudent(StudentRequest request) {
            /*
            ICollection<Student> students = _context.Student.ToList();
            var oldStudents = students.Where((emp) => emp.IndexNumber == request.IndexNumber);
            if (oldStudents.Count() == 0)    return false;
            var oldStudt = oldStudents.ElementAt(0);
            students.Remove(oldStudt);
            oldStudt.BirthDate = request.BirthDate;
            oldStudt.FirstName = request.FirstName;
            oldStudt.LastName = request.LastName;
            oldStudt.IdEnrollment = request.IdEnrollment;
            students.Add(oldStudt);
            */
            var student = _context.Student.Find(request.IndexNumber);
            if (student == null) return false;
            _context.Student.Remove(student);
            _context.SaveChanges();
            student.BirthDate = request.BirthDate;
            student.FirstName = request.FirstName;
            student.LastName = request.LastName;
            student.IdEnrollment = request.IdEnrollment;
            _context.Student.Add(student);
            _context.SaveChanges();

            return true;
        }

        public Boolean DeleteStudent(string indexNumber) {
            var student = _context.Student.Find(indexNumber); //.Remove();
            if (student == null) return false;
            _context.Student.Remove(student);
            _context.SaveChanges();
            return true;
        }

    }
}
