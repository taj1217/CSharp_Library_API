using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.Globalization;
namespace adodblib
{
    public class Reservations
    {
        public SqlConnection dbConnection;
        private const string dbName = "reservations";



        public Reservations(string userID, string UserPass)
        {
            dbConnection = new SqlConnection();
            dbConnection.ConnectionString = String.Format("Data Source = (local); Initial Catalog =reservations; Integrated Security = True;");
            dbConnection.Open();

        }


        public List<string> listCustomers()
        {
            List<string> allCustomers = new List<string>();
            try
            {
                String sqlQuery = String.Format("Select * from reservations.dbo.Customer");
                DataSet dataSet = new DataSet();
                SqlDataAdapter dataAdapter = new SqlDataAdapter();
                SqlCommand cmd = new SqlCommand(sqlQuery, dbConnection);
                dataAdapter.SelectCommand = cmd;
                dataAdapter.Fill(dataSet);
                DataTable dataTable = dataSet.Tables[0];

                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    String row = "";
                    for (int j = 0; j < dataTable.Columns.Count; j++)
                        if (dataTable.Rows[i][j] != null)
                        {
                            row += dataTable.Rows[i][j].ToString() + "\t";
                            allCustomers.Add(dataTable.Rows[i][j].ToString());
                        }
                        else
                            row += "\t"; Console.WriteLine(row);
                }
            }
            catch (Exception sqle)
            {
                Console.WriteLine(sqle.StackTrace);
            }
            return allCustomers;
        }

        public int TransactSQL(String[] sql, SqlConnection cnn)
        {
            SqlCommand cmd = new SqlCommand();
            SqlTransaction trans = cnn.BeginTransaction();
            int n = 0;
            cmd.Connection = cnn;
            try
            {
                cmd.Transaction = trans;
                for (int i = 0; i < sql.Length; i++)
                {
                    cmd.CommandText = sql[i];
                    n += cmd.ExecuteNonQuery();
                }
                trans.Commit();
            }
            catch (InvalidOperationException ex)
            {
                trans.Rollback();
                throw ex;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                throw ex;
            }
            return n;
        }


        public List<string> listReservations(int customerId)
        {
            List<string> allReservationsByCustomerId = new List<string>();
            try
            {
                String sql = String.Format("SELECT roomno,datein,dateout,totalprice FROM reservation WHERE CustomerId  = '{0}'", customerId);
                DataSet ds = new DataSet();
                SqlDataAdapter da = new SqlDataAdapter();
                SqlCommand cmd = new SqlCommand(sql, dbConnection);
                da.SelectCommand = cmd;
                da.Fill(ds);
                DataTable dt = ds.Tables[0];

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    String row = "";
                    for (int j = 0; j < dt.Columns.Count; j++)
                        if (dt.Rows[i][j] != null)
                        {
                            row += dt.Rows[i][j].ToString() + "\t";
                            allReservationsByCustomerId.Add(dt.Rows[i][j].ToString());
                        }
                        else
                            row += "\t"; Console.WriteLine(row);
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
            catch (Exception ex2)
            {
                Console.WriteLine(ex2.StackTrace);
            }

            return allReservationsByCustomerId;
        }

        public List<string> listavailable(String datein, String dateout)
        {
            List<string> rv = new List<string>();
            List<int> roomNumber = new List<int>();
            List<string> roomType = new List<string>();
            List<int> numberBeds = new List<int>();
            List<double> pricing = new List<double>();
            try
            {
                String sql = String.Format("select Max(Date),Min(Date) from pricing \n"
                        + "HAVING Max(Date) >= '{0}' and Min(Date) <= '{1}'", dateout, datein);

                SqlCommand s = new SqlCommand(sql, dbConnection);
                SqlDataReader rs = s.ExecuteReader();
                int count = 0;
                if (rs.HasRows)
                {
                    while (rs.Read())
                    {
                        count++;
                    }
                }

                if (count == 1)
                {
                    sql = String.Format("SELECT r.RoomNo, rd.RoomType, rd.numbeds, SUM(Price) "
                            + "FROM RoomInventory r INNER JOIN RoomDetails rd INNER JOIN Pricing p ON p.RoomType= rd.RoomType "
                            + "ON r.RoomType = rd.RoomType "
                            + "WHERE p.Date >= '{0}' AND p.Date < '{1}' and RoomNo NOT IN "
                            + "(SELECT RoomNo FROM Reservation "
                            + " WHERE DateIn < '{2}' AND DateOut > '{3}' ) "
                            + "GROUP BY r.RoomNo,rd.RoomType, rd.numbeds;", datein, dateout, dateout, datein);

                    SqlCommand s2 = new SqlCommand(sql, dbConnection);
                    rs.Close();
                    DataSet ds = new DataSet();
                    SqlDataAdapter da = new SqlDataAdapter();
                    SqlCommand cmd = new SqlCommand(sql, dbConnection);
                    da.SelectCommand = cmd;
                    da.Fill(ds);
                    DataTable dt = ds.Tables[0];
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        String row = "";
                        for (int j = 0; j < dt.Columns.Count; j++)
                            if (dt.Rows[i][j] != null)
                            {
                                row += dt.Rows[i][j].ToString() + "\t";
                            }
                            else
                                row += "\t"; Console.WriteLine(row); rv.Add(row);
                    }
                    ds.Clear();
                }
                rs.Close();


            }
            catch (SqlException ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
            List<string> rooms = new List<string>();
            foreach (var item in rv)
            {
                List<string> myList = item.Split('\t').ToList();
                string x = myList[0];
                rooms.Add(x);
            }
            return rooms;
        }


        public float Book(String cid, String roomno, String datein, String dateout)
        {
            List<string> sum = new List<string>();
            float finalPrice = 0;
            try
            {
                String sql = String.Format("SELECT SUM(Price) "
                        + "FROM RoomInventory r INNER JOIN RoomDetails rd INNER JOIN Pricing p ON p.RoomType= rd.RoomType "
                        + "ON r.RoomType = rd.RoomType WHERE p.Date >= '{0}' AND p.Date < '{1}' and RoomNo = {2} ", datein, dateout, roomno);

                SqlCommand s = new SqlCommand(sql, dbConnection);
                DataSet ds = new DataSet();
                SqlDataAdapter da = new SqlDataAdapter();
                SqlCommand cmd = new SqlCommand(sql, dbConnection);
                da.SelectCommand = cmd;

                da.Fill(ds);
                DataTable dt = ds.Tables[0];
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    String row = "";
                    for (int j = 0; j < dt.Columns.Count; j++)
                        if (dt.Rows[i][j] != null)
                        {
                            row += dt.Rows[i][j].ToString() + "\t";
                        }
                        else
                            row += "\t"; Console.WriteLine(row); sum.Add(row);
                }



                String[] Arraysql = new String[1];
                Arraysql[0] = String.Format("INSERT INTO Reservation(CustomerID, RoomNo,Datein,Dateout, TotalPrice) "
                        + "VALUES({0},{1},'{2}','{3}',{04});", cid, roomno, datein, dateout, finalPrice);
                TransactSQL(Arraysql, dbConnection);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
            List<string> totalPrice = new List<string>();
            foreach (var item in sum)
            {
                float price = float.Parse(item);
                finalPrice += price;
            }
            return finalPrice;
        }

    }
}
