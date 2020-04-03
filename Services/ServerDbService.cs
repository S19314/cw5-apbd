using System;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using cw3_apbd.DTOs.Request;

namespace cw3_apbd.Services 
{
    public class ServerDbService : IStudentsDbService
    {
        private const string connectParametr = "Data Source=db-mssql;Initial Catalog=s19314;Integrated Security=True ";
        public string writeStudentIntoSemester(EnrollStudentRequest request) {
            SqlTransaction transaction = null;
            try
            {
                using (SqlConnection connection = new SqlConnection(connectParametr))
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;

                    connection.Open();

                    transaction = connection.BeginTransaction();

                    // Падазрение, что дело всё в этих одиноких скобках и как следствие, при добавлении параметра 
                    // программа проаускает слово studies и выбивает null
                    // Хотя нелогично, ибо exception никакого нет. Просто считывает и имеет пустой результат.
                    // DataReader с пустым резултаттом, но не null в DataReader
                    command.CommandText = $"SELECT IdStudy FROM Studies WHERE name = ' @studies ' ;";
                    command.Parameters.AddWithValue("studies", request.Studies);

                    var dataReader = command.ExecuteReader();
                    if (!dataReader.Read()) return ""; // SqlException 
                    int idStudies = Convert.ToInt32(dataReader["IdStudies"].ToString());

                    command.CommandText = $"SELECT IdEnrollment	" +
                                                "FROM Enrollment " +
                                                "WHERE Enrollment.IdStudy = @IdStudies AND " +
                                                      "semester = 1 AND " +
                                                      "StartDate = (SELECT  Max(Enrollment.StartDate)" +
                                                                     "FROM Enrollment" +
                                                                     "WHERE IdEnrollment = (SELECT IdEnrollment" +
                                                                                           "FROM Enrollment" +
                                                                                           "WHERE Enrollment.IdStudy = @IdStudies AND semester = 1" +
                                                                                            ")" +
                                                                    ");"
                    ;
                    command.Parameters.AddWithValue("IdStudies", idStudies);

                    dataReader = command.ExecuteReader();
                    int idEnrollment;
                    if (!dataReader.Read())
                    {
                        command.CommandText = "SELECT 1 + Max(IdEnrollment) FROM Enrollment";
                        var dataReaderMaxIdEnrollmetn = command.ExecuteReader();
                        if (!dataReader.Read()) ; // SqlExccepton
                        idEnrollment = Convert.ToInt32(dataReaderMaxIdEnrollmetn["IdEnrollment"].ToString());

                        command.CommandText = $"INSERT INTO Enrollment(IdEnrollment, Semester, IdStudy, StartDate)" +
                                               "VALUES(@IdEnrollment, 1, @IdStudies, GETDATE());"
                        ;

                        command.Parameters.AddWithValue("IdEnrollment", idEnrollment);
                        command.Parameters.AddWithValue("IdStudies", idStudies);

                        if (command.ExecuteNonQuery() <= 0) transaction.Rollback(); // SqlException
                    }
                    idEnrollment = Convert.ToInt32(dataReader["IdEnrollment"].ToString());


                    command.CommandText = $"SELECT * FROM Student WHERE student.IndexNumber = @IndexStudenta;";
                    command.Parameters.AddWithValue("IndexStudenta", request.IndexNumber);
                    dataReader = command.ExecuteReader();
                    if (!dataReader.Read()) ; // SqlException

                    command.CommandText = "INSERT INTO Student(IndexNumber, FirstName, LastName, BirthDate, IdEnrollment)" +
                                            "VALUES(@IndexNumber, '@FirstName', '@LastName', '@BirtDate', @IdEnrollment);";
                    command.Parameters.AddWithValue("IndexNumber", request.IndexNumber);
                    command.Parameters.AddWithValue("FirstName", request.FirstName);
                    command.Parameters.AddWithValue("LastName", request.LastName);
                    command.Parameters.AddWithValue("BirthDate", request.BirthDate);
                    command.Parameters.AddWithValue("IdEnrollment", idEnrollment);
                    int countRowAffected = command.ExecuteNonQuery();
                    if (countRowAffected <= 0) transaction.Rollback(); // SqlException


                    // command.CommandText = 
                }
            }
            catch (SqlException sqlExc) {
                transaction.Rollback();
                return "";

            }

                // try{} catch(){} - rollback 


           
            return "";
        }

        public void dodanieKoncowkeDoPromocjiStudentow() { 
        
        }

        public Boolean isExistStudies(string studiesName)
        {
            string request = $"SELECT studies.name FROM Studies WHERE {studiesName} = studies.Name;";
            var dataReader = sendRequestGetSqlDataReader(request);
            string result = "";
            while (dataReader.Read()) {
                result = dataReader["Name"].ToString();
            }

            result.Trim();
            // Nie znam na 100 procent czy w momencie pustej odpowiedzi od Bazy Danych
            // Ona będzie na 100% pust bez różch tabulacij i t.d.
            if (result.Equals(""))
                return false;
            return true;
        
        }

        private  SqlDataReader sendRequestGetSqlDataReader(string request) {
            // if(request.Equals("")) return "";
            using (var connection = new SqlConnection(connectParametr))
            using (var command = new SqlCommand()) {
                command.Connection = connection;
                command.CommandText = request;
                return command.ExecuteReader();
            }
        
        }



    }
}
