using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cw3_apbd.DTOs.Request;
using Microsoft.AspNetCore.Mvc;

namespace cw3_apbd.Services
{
    public interface IStudentsDbService
    {
        public string writeStudentIntoSemester(EnrollStudentRequest request);

        public string promocjaStudentaNaNowySemestr(EnrollSemesterRequest request);

        public bool isExistStudies(string indexNumber);

        public bool isPassedAuthorization(String login, String password);

        public bool addRefreshToken(string refreshToken);

        public bool deleteRefreshToken(string refreshToken);

        public bool updateRefreshToken(string oldRefreshToken, string newRefreshToken);
    }
}
