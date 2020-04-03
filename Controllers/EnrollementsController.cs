using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using cw3_apbd.Services;
using cw3_apbd.Models;
using cw3_apbd.DTOs.Request;

namespace cw3_apbd.Controllers
{
    [Route("api/enrollments")]
    [ApiController]
    public class EnrollementsController : Controller
    {
        private readonly IStudentsDbService _dbStudentServices;

        public EnrollementsController(IStudentsDbService dbService) {
            _dbStudentServices = dbService;
        }

        [HttpPost]
        public IActionResult addStudentIntoSemester(EnrollStudentRequest request) {
            string responde = _dbStudentServices.writeStudentIntoSemester(request);
            // Można stworzyć "switch", który w zależości od zwracanego metodą "writeStudentIntoSemester " parametru 
            //          będzie zwracać BadRequest() z odpowiednim opisem.
            if (responde.Equals("")) return BadRequest("In addStudentIntoSemester");
                return Ok("In addStudentIntoSemester");
            
            // Null verification 
            /*
            if( !(
                student.BirthDate.Equals("") &&
                student.FirstName.Equals("") &&
                student.LastName.Equals("") &&
                student.IdEnrollment.Equals("") &&
                student.IndexNumber.Equals("") 
                )
            ) return BadRequest("Nie wszystki dane są zapisane");
            */

            // if(_dbStudentServices.isExistStudies(student.))
            // return Ok();

            // return Ok("Работает");
        }


        /*
        public IActionResult Index(
        {
            return View();
        }
        */
    }
}