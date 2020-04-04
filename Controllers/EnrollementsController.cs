using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using cw3_apbd.Services;
using cw3_apbd.Models;
using cw3_apbd.DTOs.Request;
using cw3_apbd.DTOs.Responde;

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
            if (responde.StartsWith("ObjEnrollment")) {
                // создание Enrolment
                string[] respondeParametrs = responde.Split("\n");
                string semester = "";
                for (int i = 0; i < respondeParametrs.Length; i++) {
                    if (respondeParametrs[i].StartsWith("Semester:"))
                    {
                        semester = respondeParametrs[i].Split(" ")[1];
                        break;
                    }
                }
                var enrollmentResponde = new EnrollStudentResponde();
                enrollmentResponde.Semester = Convert.ToInt32(semester);
                
                return StatusCode(201, enrollmentResponde);
                // return Created(new (201));
            }
            //if (responde.Equals("Exception")) 
                return BadRequest(responde);

            
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