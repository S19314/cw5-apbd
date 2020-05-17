using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using cw3_apbd.DAL;
using cw3_apbd.Models;
using Microsoft.AspNetCore.Mvc;
using cw3_apbd.Models_2;

namespace cw3_apbd.Controllers
{
    [Route("api/students")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly IDbService _dbService;

        public StudentsController(IDbService dbService)
        {
            _dbService = dbService;
        }

        [HttpGet]
        public IActionResult GetStudents(string orderBy) // action method
        {


            return  Ok(new s19314Context().Student.ToList());

            /*
            //s  var s = HttpContext.Request;
            // return $"Jan, Anna, Katarzyna sortowanie={orderBy}";
            //return Ok(_dbService.GetStudents());
            // New version. By Cw4.
            //IList<Student>
              ICollection<Student>  students = new List<Student>();
            // Data Source=db-mssql;Initial Catalog=s19314;Integrated Security=True 
            using (var connection = new SqlConnection("Data Source=db-mssql;Initial Catalog=s19314;Integrated Security=True ")) // Arguments to "CONNECTION STRING"
            using (var command = new SqlCommand())
            {
                command.Connection = connection;
                command.CommandText = "SELECT * FROM Student;";
                connection.Open();
                var dataReader = command.ExecuteReader();
                while (dataReader.Read()) {
                    var student = new Student();
                    student.FirstName = dataReader["FirstName"].ToString();
                    student.LastName = dataReader["LastName"].ToString();
                    student.IndexNumber = dataReader["IndexNumber"].ToString();
                    student.IdEnrollment = Convert.ToInt32(dataReader["IdEnrollment"].ToString());
                    student.BirthDate = dataReader["BirthDate"].ToString();
                    
                    students.Add(student);
                }
            }   
            return Ok(students);
        */
        }
        // Первый способ передачи данных
        [HttpGet("{id}")]
        public IActionResult GetStudent(string id){ //int id) {

            var student = new s19314Context().Student.Find(id);
            if (student == null) return  NotFound("Student " + id + " Not Found");

            return Ok(student);
            /*
            ICollection<Student> students = new List<Student>();

            using (var connection = new SqlConnection("Data Source=db-mssql;Initial Catalog=s19314;Integrated Security=True "))
            using (var command = new SqlCommand()) 
            {
                command.Connection = connection;
                command.CommandText = $"SELECT * FROM Student WHERE student.IndexNumber = @id;";
                command.Parameters.AddWithValue("id",id); 
                connection.Open();
                    var dataReader = command.ExecuteReader();
                while (dataReader.Read()) {
                    var student = new Student();
                    student.FirstName = dataReader["FirstName"].ToString();
                    student.LastName= dataReader["LastName"].ToString();
                    student.IndexNumber = dataReader["IndexNumber"].ToString();
                    student.IdEnrollment = Convert.ToInt32(dataReader["IdEnrollment"].ToString());
                    student.BirthDate = dataReader["BirthDate"].ToString();

                    students.Add(student);
                }
            }

            if (students.Count > 0) {
                return Ok(students);
            }
                return NotFound("Nie znaleziono studenta");

            */
        }

        /*
        [HttpGet("something/else")]
        public string GetStudent() {
            return "A";
        }
        */
        // 3 способ Передача данных в теле запроса
        [HttpPost]
        public IActionResult CreateStudent(cw3_apbd.Models.Student student) {
            // добавили в БД
            // ... generating index number
            student.IndexNumber = $"s{new Random().Next(1, 20000)}";
            return Ok(student);
        }

        [HttpPut("{id}")]
        public IActionResult LoadData(int  id) {
            return Ok($"Ąktualizacja dokończona. ID " + id);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteData(int id) {
            return Ok($"Usuwanie ukończone. ID " + id);
        }


        // [Route("api/students/semesters")]
        [HttpGet("{id}/semester")]
        public IActionResult GetStudentSemestr(int id) {
            var _context = new s19314Context();

            // _context.Student.Join(_context.);    

            var result = from students in _context.Student where students.IndexNumber == ""+id
                          join enroll in _context.Enrollment
                          on students.IdEnrollment equals enroll.IdEnrollment
                          select new {
                              students.FirstName,
                              students.IndexNumber,
                              enroll.Semester,
                              enroll.StartDate
                          };

            return Ok(result);
            String info = "";

            using (var connection = new SqlConnection("Data Source=db-mssql;Initial Catalog=s19314;Integrated Security=True "))
            using (var command = new SqlCommand()) {
                command.Connection = connection;
                command.CommandText = "SELECT * FROM Enrollment " +
                                       $" INNER JOIN( SELECT * FROM Student WHERE Student.IndexNumber = {id}) a " +
                                       " ON a.IdEnrollment = Enrollment.IdEnrollment; ";
                connection.Open();
                var dataReader = command.ExecuteReader();
                while (dataReader.Read()) { 
                info = info +
                    "FirstName: " +dataReader["FirstName"].ToString() + " " +
                    "LastName: " + dataReader["LastName"].ToString() + " " +
                    "IndexNumber: " + dataReader["IndexNumber"].ToString() + " " +
                    "BirthDate" + " " + dataReader["BirthDate"].ToString() + " " +
                    "IdEnrollment" + " " + dataReader["IdEnrollment"].ToString() + " " +
                    "Semester" + " " + dataReader["Semester"].ToString() + " " +
                    "IdStudy" + " " + dataReader["IdStudy"].ToString() + " " +
                    "StartDate" + " " + dataReader["StartDate"].ToString() + "\n";
                }
            }

            if(info.Equals(""))
                return NotFound("Nie znaleziono informacje o semestrze dannego studenta");
            
            return Ok(info);
        }
    }
}