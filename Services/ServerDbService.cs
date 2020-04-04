using System;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using cw3_apbd.DTOs.Request;
using System.Data;

namespace cw3_apbd.Services 
{
    public class ServerDbService : IStudentsDbService
    {
        private const string connectParametr = "Data Source=db-mssql;Initial Catalog=s19314;Integrated Security=True ";
        public string writeStudentIntoSemester(EnrollStudentRequest request) {

             string semester = "",
                    numberIdEnrollment = "", 
                    idStudyDlaException = "";
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
                    transaction.Commit();
               
                
                }catch (SqlException sqlExc)
                {
                    dataReader.Close();
                    transaction.Rollback();
                    return "SqlException \n "; 
                    /*        
                    + "StackTrace " + sqlExc.StackTrace + "\n"
                        + "NumberIdEnrollment " + numberIdEnrollment +
                        " idStudyDlaException " + idStudyDlaException;
                        */
                } catch (Exception myExc) {
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


           
            return "ObjEnrollment\n" + 
                   "Semester: 1";
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
