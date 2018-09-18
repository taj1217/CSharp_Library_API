using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using adodblib;
using C = System.Console;

namespace asg4client
{
    class Client
    {
        static void Main(string[] args)
        {

            try
            {
                Reservations rs = new Reservations("ism6236", "ism6236bo");

                List<string> listOfCustomers = rs.listCustomers();

                C.WriteLine("Press L to List Reservations, B to Book, Q to quit");

                string input = C.ReadLine();

                while (!input.ToLower().Equals("q"))
                {
                    if (input.ToLower().Equals("l"))
                    {
                        C.WriteLine("Enter Customer ID:");
                        string cid = C.ReadLine();
                        int cidInt = Int32.Parse(cid);
                        List<string> res = rs.listReservations(cidInt);
                        foreach (string s in res)
                        {
                            C.WriteLine(s);
                        }
                    }
                    else if (input.ToLower().Equals("b"))
                    {
                        C.WriteLine("Please enter a check in date - formatted as 4/20/18");
                        string datein = C.ReadLine();
                        C.WriteLine("Please enter a leaving date - formatted as 4/20/18");
                        String dateout = C.ReadLine();
                        List<string> avrooms = rs.listavailable(datein, dateout);
                        if (avrooms.Capacity > 0)
                        {
                            List<string> roomnos = new List<string>();
                            C.WriteLine(string.Format("Rooms available between {0} and {1}", datein, dateout));
                            C.WriteLine("Press B to book or C to Cancel");
                            input = C.ReadLine();

                            if (input.ToLower().Equals("b"))
                            {
                                C.WriteLine("Please enter available room");
                                input = C.ReadLine();

                                while (!avrooms.Contains(input.Trim()))
                                {
                                    C.WriteLine("Please enter available room");
                                    input = C.ReadLine();
                                }

                                string roomno = input;
                                C.WriteLine("Enter Customer ID:");
                                input = C.ReadLine();
                                string cid = input;

                                float p = rs.Book(cid, roomno, datein, dateout);
                                if (p > 0)
                                {
                                    C.WriteLine(string.Format("Reservations Total Cost:{0}", p));
                                }
                                else
                                {
                                    C.WriteLine("Something is Wrong - Try Again!");
                                }
                            }
                        }
                        else
                            C.WriteLine("No Rooms are available for the entered dates");
                    }
                    C.WriteLine("Press L to List Reservations, B to Book, Q to quit");
                    input = C.ReadLine();

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }

        }
    }
}

