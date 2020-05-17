using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using cw3_apbd.Models_2;
using cw3_apbd.DTOs.Request;
using cw3_apbd.Services;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace cw3_apbd.Controllers
{
    [Route("api/StudentsCw10")]
    [ApiController]
    public class StudentsCw10Controller : Controller
    {

        private IDbServicesCwieczenie10 _context;

        public StudentsCw10Controller(IDbServicesCwieczenie10 context) {
            _context = context;
        }
        // GET: /<controller>/
        [HttpGet]
        public IActionResult GetStudents()
        {
            return Ok(_context.GetStudents());
        }

        [Route("api/changeStudent")]
        [HttpPost]
        public IActionResult UpdateStudent(StudentRequest request){
            // new s19314Context().Student.Remove();

            if (_context.UpdateStudent(request))
                return Ok("Student " + request.IndexNumber + " was updated! ");
            else return NotFound("Student " + request.IndexNumber +  " Not Found!");
        }

        [Route("delete")]
        [HttpGet]
        public IActionResult DeleteStudent(string indexNumber) {
            if (_context.DeleteStudent(indexNumber)) return Ok("Student " + indexNumber + " was deleted! ");
            else return NotFound("Student " + indexNumber + " Not Found!");
        }
        

    }
}
