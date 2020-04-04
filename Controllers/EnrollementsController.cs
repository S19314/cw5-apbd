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
                var enrollmentResponde = convertParametrsIntoEnrollStudentResponde(responde); 
                return StatusCode(201, enrollmentResponde);
                
            }

                return BadRequest(responde);
        }

        [HttpPost("promotions")]
        public IActionResult promocjaStudentaNaNowySemestr(EnrollSemesterRequest request) {
            string responde = _dbStudentServices.promocjaStudentaNaNowySemestr(request);
            
            if (responde.StartsWith("ObjEnrollment"))
            {
                var enrollmentResponde = convertParametrsIntoEnrollStudentResponde(responde);
                return StatusCode(201, enrollmentResponde);
            }

            return NotFound(responde);
            
        }


        private EnrollStudentResponde convertParametrsIntoEnrollStudentResponde(string responde) {
            string[] parametrs = responde.Split("\n");
            string semester = "";

            var enrollmentResponde = new EnrollStudentResponde();
            enrollmentResponde.IdEnrollment = Convert.ToInt32(parametrs[1].Split(" ")[1]);
            enrollmentResponde.Semester = Convert.ToInt32(parametrs[2].Split(" ")[1]);
            enrollmentResponde.IdStudy = Convert.ToInt32(parametrs[3].Split(" ")[1]);
            enrollmentResponde.StartDate = parametrs[4].Split(" ")[1];


            return enrollmentResponde;
        }


    }
}