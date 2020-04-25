using cw3_apbd.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace cw3_apbd.Handlers
{
    public class BasicAuthorizationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private IStudentsDbService studentsDbService;

        public BasicAuthorizationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger, // Для добваления механизма логгирования
            UrlEncoder encoder, // Используется для декодирования сообщений
            ISystemClock clock, // Связанна со временем
            IStudentsDbService studentsDbService
            ) : base(options, logger, encoder, clock) // == super() in Java
        {
            this.studentsDbService = studentsDbService;
        }
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return AuthenticateResult.Fail("Missing authorization header");

            var autauthorizationHeader = AuthenticationHeaderValue.Parse(
                Request.Headers["Authorization"]);
            var credentialsBytes = Convert.FromBase64String(autauthorizationHeader.Parameter);
            var credentials = Encoding.UTF8.GetString(credentialsBytes).Split(":");

            if (credentials.Length != 2)
                return AuthenticateResult.Fail("Incorrect authorization header value");
            // Проверка в БД
            if (!studentsDbService.isPassedAuthorization(credentials[0], credentials[1]))
                return AuthenticateResult.Fail("Incorrect \"Login\" or \"Password\".");

            // Добавить ВРЕМЕННО проверку по Id, для определения роли студент или работник 
            String role = "employee"; // Lepej wyrzycyć do IStudentDbServices i zaimplementować meetode do 
            // sprawdzania roli

            var claims = new[] {
                new Claim(ClaimTypes.Name, credentials[0]),
                new Claim(ClaimTypes.Role, role)
            };
            // Pasporort, please!
            var identity = new ClaimsIdentity(claims, Scheme.Name); // Scheme.Name -  говорит об типе идентификации пользователя
                                                                   //Basic,.. 
            var claimsPrincipal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(claimsPrincipal, Scheme.Name);

                return AuthenticateResult.Success(ticket);
        }
    }
}
