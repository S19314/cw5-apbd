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
    {
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
                    // Падазрение, что дело всё в этих одиноких скобках и как следствие, при добавлении параметра 
                    // программа проаускает слово studies и выбивает null
                    // Хотя нелогично, ибо exception никакого нет. Просто считывает и имеет пустой результат.
                    // DataReader с пустым резултаттом, но не null в DataReader

                    command.CommandText = $"SELECT IdStudy FROM Studies WHERE name = @studies ;";
                    // command.CommandText = $"SELECT IdStudy FROM Studies WHERE name = 'IT' ;";
                    command.Parameters.AddWithValue("studies", request.Studies);
                    // name = '@studies'  траблы с этой херней

                    dataReader = command.ExecuteReader();

                    if (!dataReader.Read()) throw new Exception("W tabeli \"Studies\" nie ma potrzebnego rekorku z atrybutem \"name\" = " + request.Studies
                                                              + "\n Command " + command.CommandText); // SqlException...

                    idStudyDlaException = dataReader["IdStudy"].ToString();
                    int idStudies = Convert.ToInt32(idStudyDlaException); // Convert.ToInt32(dataReader["IdStudy"].ToString());
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
                    // dataReader.Close();
                    /* ТУТ */
                    dataReader = command.ExecuteReader();
                    int idEnrollment;
                    if (!dataReader.Read())
                    {
                        dataReader.Close();
                        command.CommandText = "SELECT (1 + ISNULL(Max(IdEnrollment), 0))  FROM Enrollment;";
                        // var dataReaderMaxIdEnrollment = command.ExecuteReader();
                        // Тут. Тоже проверка не нужна. 
                        // if (! 
                        //dataReaderMaxIdEnrollment.Read();
                        // ) throw new Exception(); // SqlExccepton
                        dataReader = command.ExecuteReader();
                        dataReader.Read();
                        numberIdEnrollment = dataReader.GetValue(0).ToString();
                        idEnrollment = Convert.ToInt32(numberIdEnrollment);

                        //idEnrollment =  Convert.ToInt32(dataReader[0].ToString());
                        dataReader.Close();
                        command.CommandText = $"INSERT INTO Enrollment(IdEnrollment, Semester, IdStudy, StartDate)" +
                                                "VALUES(@IdEnrollment, 1, @IdStudies, GETDATE());"
                        ;

                        command.Parameters.AddWithValue("IdEnrollment", idEnrollment);
                        command.Parameters.AddWithValue("IdStudies", idStudies);
                        command.ExecuteNonQuery();
                        // Проверка, вполне возможно что и не нужна вовсе
                        // if (command.ExecuteNonQuery() <= 0) new Exception("Z jakiegoś powodu nie było wstawionno do tabeli Enrollment nowe Studia"); // SqlException
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
                    // int countRowAffected =
                    command.ExecuteNonQuery();
                    // Не нужно
                    // if (countRowAffected <= 0) throw new Exception(); // SqlException
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
                    return "SqlException \n " //; 
                    ///*        
                    + "StackTrace " + sqlExc.StackTrace + "\n"
                        + "NumberIdEnrollment " + numberIdEnrollment +
                        " idStudyDlaException " + idStudyDlaException;
                    // */
                }
                catch (Exception myExc)
                {
                    dataReader.Close();
                    transaction.Rollback();

                    return "Exception\n" +
                         myExc.Message;
                    /* + 
                        "\nStackTrace\n " + myExc.StackTrace + "\n"
                        + "NumberIdEnrollment " + numberIdEnrollment +
                        " idStudyDlaException " + idStudyDlaException; //.ToString();
                    */
                }

                // command.CommandText = 
            }


            // try{} catch(){} - rollback 



            return responde;
            // "ObjEnrollment\n" + 
            // "Semester: 1";
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
                    /*
                    $" SELECT IdEnrollment, Semester, IdStudy, StartDate " +
                          " FROM Enrollment WHERE IdStudy = @IdStudy AND Semester = @Semester ; ";
                    */
                    command.Parameters.AddWithValue("Studies", request.Studies);
                    command.Parameters.AddWithValue("Semester", request.Semester);


                    dataReader = command.ExecuteReader();
                    if (!dataReader.Read()) throw new Exception("Nie ma takiego rekordu");
                    // bEGIN TRANSACTION
                    dataReader.Close();
                    ///*
                    /*
                    string deleteProcedureIfExist =
                        " IF( SELECT object_id('promocjaStudentaNaNastepnySemestr') ) IS NOT NULL " +
                        " BEGIN " +
                        " DROP PROCEDURE promocjaStudentaNaNastepnySemestr; " +
                        " END; " +
                        "  ";
                    */
                    // command.CommandText = deleteProcedureIfExist;
                    // command.ExecuteNonQuery();
                    // dataReader.Close();
                    //*/
                    // Tutaj jest problem 
                    // dataReader = command.ExecuteReader();
                    // dataReader.Read();



                    /*
                    string createProcedure = 
                        " CREATE PROCEDURE promocjaStudentaNaNastepnySemestr  @Studies Varchar(20), @Semester INT " +
                        " AS " + 
                        " BEGIN " + 
                        " DECLARE @v_idEnrollment_current_Students INT, " +
		                " @v_idEnrollment_new_Students INT, " +
                        " @v_idStudy INT; " +

                        " SET @v_idStudy = (SELECT IdStudy " +
                                          " FROM Studies " + 
                                          " WHERE Name = @Studies); " + 

                        " SET @v_idEnrollment_current_Students = " + 
                                         " (SELECT IdEnrollment--, Semester, IdStudy, StartDate " +
                                           " FROM Enrollment " +
                                           " WHERE Semester = @Semester AND " +
                                                 " IdStudy = @v_idStudy " +
						                   " ); " +
                        " SET @v_idEnrollment_new_Students = (SELECT IdEnrollment " +
                                           " FROM Enrollment " +
                                           " WHERE Semester > 1 AND " +
                                                 " IdStudy = @v_idStudy " +
						" ); " +
                    
                        " IF(@v_idEnrollment_new_Students IS NULL) " + 
                        " BEGIN " +
                        "   SET @v_idEnrollment_new_Students = (SELECT(1 + MAX(IdEnrollment))  FROM Enrollment); " +
                        "   INSERT INTO Enrollment(IdEnrollment, Semester, IdStudy, StartDate) " +
                        "   VALUES(@v_idEnrollment_new_Students, (1 + @Semester), @v_idStudy, GETDATE()); " +
                        " END " +
                        " ELSE " +
                        " BEGIN " +
                        "    UPDATE Enrollment " +
                        "    SET Semester = (1 + @Semester) " +
                        "    WHERE IdEnrollment = @v_idEnrollment_new_Students " +
                        " END; " + 

                        " UPDATE Student " + 
                        " SET IdEnrollment = @v_idEnrollment_new_Students " + 
                        " WHERE IdEnrollment = @v_idEnrollment_current_Students; " +
                        " RETURN @v_idEnrollment_new_Students; " +
                        " END; " + 
                        " ";
                   */
                    // command.CommandText = createProcedure;
                    // dataReader = 
                    //     command.BeginExecuteReader();  
                    // command.ExecuteScalar();
                    // dataReader.Close();


                    string executeProcedure = $" DECLARE @return_status int;  " +
                                                " EXEC @return_status = promocjaStudentaNaNastepnySemestr @StudiesName, @CurrentSemester; " +
                                                " SELECT 'IdEnrollment ' = @return_status; " +
                                                "  ";

                    command.CommandText = executeProcedure;
                    command.Parameters.AddWithValue("StudiesName", request.Studies);
                    command.Parameters.AddWithValue("CurrentSemester", request.Semester);
                    /// ExecuteScalar() ?
                    // dataReader = command.ExecuteReader();
                    // dataReader.Read();
                    // string respondeIdEnrollment = command.ExecuteScalar().ToString(); // dataReader.GetValue(0).ToString();
                    dataReader = command.ExecuteReader(); // Convert.ToInt32(respondeIdEnrollment.Split(" ")[1]);
                                                          //infoForException = infoForException + "ResultProcedure " + resultProcedure;

                    //int v_IdEnrollment = Convert.ToInt32(resultProcedure.ToString());
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
    }

    }
