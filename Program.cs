using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.IO;

namespace Expertise_Rank
{
    class Program
    {
        static int g_count = 0;
        static void Main(string[] args)
        {
            SqlConnection myConnection = new SqlConnection(GetConnectionString());

            // Open database
            try
            {
                myConnection.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine("Database open error: " + e.ToString());
            }

            // start caculating expertise
            calculate_expertise(myConnection);

            // Close database
            try
            {
                myConnection.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("====================== Done ==========================");
            Console.ReadKey();

        }

        static private string GetConnectionString()
        {
            return "user id=cfa18;server=localhost;" +
                "Trusted_Connection=yes;database=Trustlet_2;connection timeout=30";
        }

        static private void calculate_expertise(SqlConnection connection)
        {

            
            SqlDataReader myReader = null;
            SqlCommand myCommand = new SqlCommand("select * from [reviews-all]", connection);

            /*=====================Calculate Expertise ==================*/

            Dictionary<String, User> users = new Dictionary<String, User>();
            myReader = myCommand.ExecuteReader();        
            while (myReader.Read())
            {
                //Console.WriteLine(myReader["member_since"].ToString());
                Insert(myReader, users);
            }

            //
            foreach (KeyValuePair<String, User> kvp in users)
            {
                User u = kvp.Value;
                u.CalculateExpertise();
                //rank.Add(u);
            }
            myReader.Close();
           


            /*=====================Calculate Trustworthiness ==================*/

            // Create web of trust
            myCommand = new SqlCommand("select * from [trusts]", connection);
            myReader = myCommand.ExecuteReader();
            while (myReader.Read())
            {
                String source_user = myReader["source_user"].ToString();
                String target_user = myReader["target_user"].ToString();
                if (users.ContainsKey(target_user))
                    users[target_user].AddTrustBy(source_user);
            }
            myReader.Close();

            // Calculate trustworthiness
            //List<User> rank = new List<User>();
            //TextWriter file = new StreamWriter("TrustBy.txt");
            foreach (KeyValuePair<String, User> kvp in users)
            {
                User u = kvp.Value;
                foreach (String category in User.CategoryName)
                {
                    foreach (String trusteeStr in u.TrustBy)
                    {
                        if (users.ContainsKey(trusteeStr))
                        {
                            User trustee = users[trusteeStr];
                            if (trustee.Credibility[category].Expertise != 0.0d)
                            {
                                u.Credibility[category].TotalTrusteeExpertise +=
                                    trustee.Credibility[category].Expertise;
                                u.Credibility[category].TrusteeCount++;
                            }
                        }
                    }
                }

                u.CalculateTrustwhorthiness();
                InsertIntoTable(u, "Credibility", connection);
                //rank.Add(u);
                //file.WriteLine("{0} {1}", kvp.Key, kvp.Value.TrustBy.Count);
            }
            //rank.Sort(userComparator);
            //WriteToFile( rank);
            //file.Close();
        }

        private static int userTrComparator(User u1, User u2)
        {
            CredibilityInfo info1 = u1.GetCredibilityInfo("Movies");
            CredibilityInfo info2 = u2.GetCredibilityInfo("Movies");
            if (info1.Trustworthiness < info2.Trustworthiness)
                return 1;
            else if (info1.Trustworthiness > info2.Trustworthiness)
                return -1;
            else
            {
                if (info1.TrusteeCount > info2.TrusteeCount)
                    return -1;
                else if (info1.TrusteeCount < info2.TrusteeCount)
                    return 1;
                else
                    return 0;
            }
        }

        private static int userExpComparator(User u1, User u2)
        {
            CredibilityInfo info1 = u1.GetCredibilityInfo("Movies");
            CredibilityInfo info2 = u2.GetCredibilityInfo("Movies");
            if (info1.Expertise < info2.Expertise)
                return 1;
            else if (info1.Expertise > info2.Expertise)
                return -1;
            else
            {
                if (info1.RatingCount > info2.RatingCount)
                    return -1;
                else if (info1.RatingCount < info2.RatingCount)
                    return 1;
                else
                    return 0;
            }
        }

        private static void WriteToFile(List<User> l)
        {
            TextWriter expFile = new StreamWriter("Movies_Exp.txt");
            TextWriter trFile = new StreamWriter("Movies_Tr.txt");
            expFile.WriteLine("user_id\tExpertise\tTrustee_Count");
            trFile.WriteLine("user_id\tTrustworthiness\tReview_Count");

            l.Sort(userExpComparator);
            foreach (User u in l)
            {
                CredibilityInfo info = u.GetCredibilityInfo("Movies");
                if(info.RatingCount != 0)
                    expFile.WriteLine("{0}\t{1}\t{2}", u.Name, 
                        info.Expertise, info.RatingCount);

                
            }

            l.Sort(userTrComparator);
            foreach (User u in l)
            {
                CredibilityInfo info = u.GetCredibilityInfo("Movies");
                if (info.TrusteeCount != 0)
                    trFile.WriteLine("{0}\t{1}\t{2}", u.Name,
                        info.Trustworthiness, info.TrusteeCount);
            }

            expFile.Close();
            trFile.Close();
        }

        private static void InsertIntoTable(User u, String table, SqlConnection connection)
        {
            foreach(String category in User.CategoryName)
            {
                CredibilityInfo info = u.Credibility[category];
                if(info != null)
                {
                    String query = "insert into [" + table + "] ";
                    query += "values (" + g_count++ + ", "
                        + "'" + u.Name + "', "
                        + "'" + category + "', "
                        + info.Expertise + ", "
                        + info.RatingCount + ", "
                        + info.Trustworthiness + ", "
                        + info.TrusteeCount + ")"
                        ;
                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static void Insert(SqlDataReader reader,
           Dictionary<String, User> users)
        {
            String user_id = reader["user_id"].ToString();
            user_id = user_id.ToLower();
            if (!reader["review_helpfulness"].Equals(DBNull.Value))
            {
                int rating = (int)reader["review_helpfulness"];
                if (rating != -1)
                {
                    if (!users.ContainsKey(user_id))
                    {
                        User newUser = new User(user_id);
                        newUser.Update(reader["product_category"].ToString(),
                            rating);
                        users.Add(user_id, newUser);
                    }
                    else
                    {
                        User curUser = users[user_id];
                        curUser.Update(reader["product_category"].ToString(),
                            rating);
                    }
                }
            }
        }
    }
    
}
