using System;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using cw3_apbd.DTOs.Request;
using cw3_apbd.DTOs.Responde;
using System.Data;

namespace cw3_apbd.Services 
{
    public class ServerDbService : IStudentsDbService
    {// Не мешало бы через IConfigure хранить информацию об connectParametr
        private const string connectParametr = "Data Source=db-mssql;Initial Catalog=s19314;Integrated Security=True ";
      
        public string writeStudentIntoSemester(EnrollStudentRequest request)
        {
            string semester = "",
                   numberIdEnrollment = "",
                   idStudyDlaException = "",
                   responde = "ObjEnrollment\n";

            using (SqlConnection connection = new SqlConnection(connectParametr))
            using (SqlCommand command = new SqlCommand())
            {
                SqlDataReader dataReader = null;
                SqlTransaction transaction = null;
                try
                {
                    command.Connection = connection;

                    connection.Open();

                    transaction = connection.BeginTransaction();
                    command.Transaction = transaction;
                    
                    command.CommandText = $"SELECT IdStudy FROM Studies WHERE name = @studies ;";
                    
                    command.Parameters.AddWithValue("studies", request.Studies);
                    

                    dataReader = command.ExecuteReader();

                    if (!dataReader.Read()) throw new Exception("W tabeli \"Studies\" nie ma potrzebnego rekorku z atrybutem \"name\" = " + request.Studies
                                                              + "\n Command " + command.CommandText); 

                    idStudyDlaException = dataReader["IdStudy"].ToString();
                    int idStudies = Convert.ToInt32(idStudyDlaException); 
                    dataReader.Close();

                    command.CommandText = $"SELECT IdEnrollment	" +
                                                " FROM Enrollment " +
                                                " WHERE ( Enrollment.IdStudy = @IdStudies1 ) AND " +
                                                        " (semester = 1) AND " +
                                                        " StartDate = (SELECT  Max(Enrollment.StartDate) " +
                                                                        " FROM Enrollment " +
                                                                        " WHERE IdEnrollment = (SELECT IdEnrollment " +
                                                                                            " FROM Enrollment " +
                                                                                            " WHERE Enrollment.IdStudy = @IdStudies2 AND semester = 1 " +
                                                                                            " ) " +
                                                                    " );"
                    ;
                    command.Parameters.AddWithValue("IdStudies1", idStudies);
                    command.Parameters.AddWithValue("IdStudies2", idStudies);
                    
                    dataReader = command.ExecuteReader();
                    int idEnrollment;
                    if (!dataReader.Read())
                    {
                        dataReader.Close();
                        command.CommandText = "SELECT (1 + ISNULL(Max(IdEnrollment), 0))  FROM Enrollment;";
                        
                        dataReader = command.ExecuteReader();
                        dataReader.Read();
                        numberIdEnrollment = dataReader.GetValue(0).ToString();
                        idEnrollment = Convert.ToInt32(numberIdEnrollment);

                        dataReader.Close();
                        command.CommandText = $"INSERT INTO Enrollment(IdEnrollment, Semester, IdStudy, StartDate)" +
                                                "VALUES(@IdEnrollment, 1, @IdStudies, GETDATE());"
                        ;

                        command.Parameters.AddWithValue("IdEnrollment", idEnrollment);
                        command.Parameters.AddWithValue("IdStudies", idStudies);
                        command.ExecuteNonQuery();
                    
                        dataReader.Close();
                    }
                    else
                    {
                        idEnrollment = Convert.ToInt32(dataReader["IdEnrollment"].ToString());
                        dataReader.Close();
                    }

                    command.CommandText = $"SELECT * FROM Student WHERE student.IndexNumber = @IndexStudenta;";
                    command.Parameters.AddWithValue("IndexStudenta", request.IndexNumber);
                    dataReader = command.ExecuteReader();
                    if (dataReader.Read()) throw new Exception("Student z takim indexem już istnieje! "); // SqlException

                    dataReader.Close();
                    command.CommandText = "INSERT INTO Student(IndexNumber, FirstName, LastName, BirthDate, IdEnrollment)" +
                                            "VALUES(@IndexNumber, @FirstName, @LastName, @BirthDate, @IdEnrollment_my);";
                    command.Parameters.AddWithValue("IndexNumber", request.IndexNumber);
                    command.Parameters.AddWithValue("FirstName", request.FirstName);
                    command.Parameters.AddWithValue("LastName", request.LastName);
                    command.Parameters.AddWithValue("BirthDate", Convert.ToDateTime(request.BirthDate));
                    command.Parameters.AddWithValue("IdEnrollment_my", idEnrollment);
                    
                    command.ExecuteNonQuery();
                    
                    dataReader.Close();

                    command.CommandText = $" SELECT IdEnrollment, Semester, IdStudy, StartDate " +
                                           " FROM Enrollment WHERE IdEnrollment = @IdEnrollment ;";
                    command.Parameters.AddWithValue("IdEnrollment", idEnrollment);
                    dataReader = command.ExecuteReader();
                    dataReader.Read();

                    responde = responde +
                               "IdEnrollment: " + dataReader["IdEnrollment"] + "\n" +
                               "Semester: " + dataReader["Semester"] + "\n" +
                               "IdStudy: " + dataReader["IdStudy"] + "\n" +
                               "StartDate: " + dataReader["StartDate"] + "\n";

                    dataReader.Close();
                    transaction.Commit();
                }
                catch (SqlException sqlExc)
                {
                    dataReader.Close();
                    transaction.Rollback();
                    return "SqlException \n " 
                    + "StackTrace " + sqlExc.StackTrace + "\n"
                        + "NumberIdEnrollment " + numberIdEnrollment +
                        " idStudyDlaException " + idStudyDlaException;

                }
                catch (Exception myExc)
                {
                    dataReader.Close();
                    transaction.Rollback();

                    return "Exception\n" +
                         myExc.Message;
                }

            }


            return responde;
        }

        public string promocjaStudentaNaNowySemestr(EnrollSemesterRequest request)
        {
            string response = "",
                   infoForException = "";
            using (SqlConnection connection = new SqlConnection(connectParametr))
            using (SqlCommand command = new SqlCommand())
            {
                SqlDataReader dataReader = null;
                SqlTransaction transaction = null;
                try
                {
                    command.Connection = connection;
                    connection.Open();

                    transaction = connection.BeginTransaction();
                    command.Transaction = transaction;

                    command.CommandText = $" SELECT IdEnrollment, Semester, IdStudy, StartDate " +
                                            " FROM Enrollment WHERE Semester = @Semester AND " +
                                            " IdStudy = (SELECT IdStudy FROM Studies WHERE Name = @Studies );";
                    command.Parameters.AddWithValue("Studies", request.Studies);
                    command.Parameters.AddWithValue("Semester", request.Semester);


                    dataReader = command.ExecuteReader();
                    if (!dataReader.Read()) throw new Exception("Nie ma takiego rekordu");
                    
                    dataReader.Close();
                 
                    string executeProcedure = $" DECLARE @return_status int;  " +
                                                " EXEC @return_status = promocjaStudentaNaNastepnySemestr @StudiesName, @CurrentSemester; " +
                                                " SELECT 'IdEnrollment ' = @return_status; " +
                                                "  ";

                    command.CommandText = executeProcedure;
                    command.Parameters.AddWithValue("StudiesName", request.Studies);
                    command.Parameters.AddWithValue("CurrentSemester", request.Semester);
                    dataReader = command.ExecuteReader(); 
                                                          

                    
                    dataReader.Read();
                    string result = dataReader.GetValue(0).ToString();
                    int v_IdEnrollment = Convert.ToInt32(result);
                    infoForException = infoForException + "ExcReader " + result;
                    dataReader.Close();

                    string selectObjEnrollment = " SELECT IdEnrollment, Semester, IdStudy, StartDate " +
                                                 " FROM Enrollment WHERE IdEnrollment = @v_IdEnrollment ; ";

                    command.CommandText = selectObjEnrollment;
                    command.Parameters.AddWithValue("v_IdEnrollment", v_IdEnrollment);
                    dataReader = command.ExecuteReader();
                    dataReader.Read();
                    response = "ObjEnrollment \n" +
                                "IdEnrollment: " + (dataReader["IdEnrollment"].ToString()) + "\n" +
                                "Semester: " + (dataReader["Semester"].ToString()) + "\n" +
                                "IdStudy: " + (dataReader["IdStudy"].ToString()) + "\n" +
                                "StartDate: " + (dataReader["StartDate"].ToString()) + "\n";


                    // End transaction
                    dataReader.Close();
                    transaction.Commit();
                }
                catch (SqlException sqlExc)
                {
                    dataReader.Close();
                    transaction.Rollback();
                    return "SqlException \n " //; 
                    ///*        
                    + "StackTrace " + sqlExc.StackTrace
                    + "_____\n" + infoForException
                    ;
                    // */
                }
                catch (Exception myExc)
                {
                    dataReader.Close();
                    transaction.Rollback();

                    return "Exception\n" +
                         myExc.Message +
                    "\nStackTrace\n " + myExc.StackTrace + "\n"
                    + "_____\n" + infoForException
                     ;
                    // + "NumberIdEnrollment " + numberIdEnrollment +
                    // " idStudyDlaException " + idStudyDlaException; //.ToString();

                }
            }
            return response + "\n ZrobionnoPPPPp";
        }



        public Boolean isExistStudies(string indexNumber)
        {

            using (var connection = new SqlConnection(connectParametr))
            using (var command = new SqlCommand())
            {
                string request = $"SELECT indexNumber FROM Student WHERE @indexNumber  = IndexNumber;";

                command.Connection = connection;
                connection.Open();
                command.CommandText = request;
                command.Parameters.AddWithValue("indexNumber", indexNumber);
                var dataReader = command.ExecuteReader();
                if (dataReader.Read()) return true;
                return false;

            }
        }

        public bool isPassedAuthorization(string login, string password)
        {
            using (SqlConnection sqlConnection = new SqlConnection(connectParametr))
            using (SqlCommand sqlCommand = new SqlCommand()) {
                sqlCommand.Connection = sqlConnection;
                sqlConnection.Open();
                String request = $"SELECT IndexNumber FROM Student WHERE IndexNumber = @login AND PASSWORD = @password";
                sqlCommand.CommandText = request;
                sqlCommand.Parameters.AddWithValue("login", login);
                sqlCommand.Parameters.AddWithValue("password", password);

                var dataReader = sqlCommand.ExecuteReader();
                if (dataReader.Read()) return true;
            }
                return false;
        }

        public bool addRefreshToken(string refreshToken)
        { 
            try{
            using (SqlConnection sqlConnection = new SqlConnection(connectParametr))
            using (SqlCommand sqlCommand = new SqlCommand()) {
                sqlCommand.Connection = sqlConnection;
                sqlConnection.Open();
                sqlCommand.CommandText = "INSERT INTO RefreshToken(id) VALUES(@refreshToken);";
                sqlCommand.Parameters.AddWithValue("refreshToken", refreshToken);

                    if (sqlCommand.ExecuteNonQuery() == 0) return false;            
            }
            }
            catch (SqlException sqlException) {
                return false; 
            }
            return true;
        }

        public bool deleteRefreshToken(string refreshToken)
        {
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(connectParametr))
                using (SqlCommand sqlCommand = new SqlCommand())
                {
                    sqlCommand.Connection = sqlConnection;
                    sqlConnection.Open();
                    sqlCommand.CommandText = "DELETE FROM RefreshToken WHERE id = @refreshToken;";
                    sqlCommand.Parameters.AddWithValue("refreshToken", refreshToken);

                    if (sqlCommand.ExecuteNonQuery() == 0) return false;
                }
            }
            catch (SqlException sqlException)
            {
                return false;
            }
            return true;
        }

        public bool updateRefreshToken(string oldRefreshToken, string newRefreshToken)
        {
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(connectParametr))
                using (SqlCommand sqlCommand = new SqlCommand())
                {
                    sqlCommand.Connection = sqlConnection;
                    sqlConnection.Open();
                    sqlCommand.CommandText = "UPDATE RefreshToken SET id = @newRefreshToken WHERE id = @oldRefreshToken; ";
                    sqlCommand.Parameters.AddWithValue("newRefreshToken", newRefreshToken);
                    sqlCommand.Parameters.AddWithValue("oldRefreshToken", oldRefreshToken);

                    if (sqlCommand.ExecuteNonQuery() == 0) return false;
                }
            }
            catch (SqlException sqlException)
            {
                return false;
            }
            return true;
        }
    }

    }
