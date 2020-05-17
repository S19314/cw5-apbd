using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using cw3_apbd.Services;    
using cw3_apbd.Models;
using cw3_apbd.DTOs.Request;
using cw3_apbd.DTOs.Responde;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Configuration;
using cw3_apbd.PasswordManager;

namespace cw3_apbd.Controllers
{
    [Route("api/enrollments")]
    [Authorize]
    [ApiController]
    public class EnrollementsController : Controller
    {
        private readonly IStudentsDbService _dbStudentServices;
        private IConfiguration Configuration { get; set; }
        public EnrollementsController(IStudentsDbService dbService, IConfiguration configuration) {
            _dbStudentServices = dbService;
            Configuration = configuration;
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
        [Authorize(Roles = "employee")]
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


        [AllowAnonymous]
        [HttpPost("sing-up")]
        public IActionResult createAccount(RequestAccount account) {
            if (_dbStudentServices.addAccount(account)) return Ok("Account was added");
            return NotFound("Wprowadzonno nie prawidlowy typ dannych    ");
        }






        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login(LoginRequestDto loginRequestDto ) {
             // PasswordHashing.Create();
            // PasswordHashing.PasswordHashing.
            if(!_dbStudentServices.isPassedAuthorization(loginRequestDto.Login, loginRequestDto.Haslo))
                return NotFound("Your username or password is incorrect. Please try again");
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "Klient"), //loginRequestDto.Login),
                new Claim(ClaimTypes.Role, "employee")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]));
            var signingCredentails = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken
            (  // Порядок имеет значаение? Думаю нет
                issuer: "CORP",
                audience : "Employee",
                claims: claims,
                expires: DateTime.Now.AddMinutes(10),
                signingCredentials : signingCredentails
            );
            var accessToken = new JwtSecurityTokenHandler().WriteToken(token); // Текстовая репрезентация
            var refreshToken = Guid.NewGuid();
            _dbStudentServices.addRefreshToken(refreshToken.ToString());
            return Ok( new { 
                accessToken,
                refreshToken
            });
        }


        // Лучше всего, реализовать еще триггер, который будет очищать таблицу RefreshToken по истечению определенного времени.
        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public IActionResult RefreshToken(string refreshToken)
        {
            string newRefreshToken = Guid.NewGuid().ToString();
            if (!_dbStudentServices.updateRefreshToken(refreshToken, newRefreshToken)) return Ok("Nie istnieje takiego RefreshTokenu w Bazie Danych");
            
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "Klient"),
                new Claim(ClaimTypes.Role, "employee")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]));
            var signingCredentails = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken
            (  // Порядок имеет значаение? Думаю нет
                issuer: "CORP",
                audience: "Employee",
                claims: claims,
                expires: DateTime.Now.AddMinutes(10),
                signingCredentials: signingCredentails
            );
            var accessToken = new JwtSecurityTokenHandler().WriteToken(token); // Текстовая репрезентация
            
            return Ok(new
            {
                accessToken,
                newRefreshToken
            });
        }

    }
}