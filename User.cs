using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Expertise_Rank
{
    class User
    {
        class CategoryInfo
        {
            public String Name;
            public int MaxRatingCount;

            public CategoryInfo(String name, int maxRatingCount)
            {
                Name = name;
                MaxRatingCount = maxRatingCount;
            }
        }

        Books	1078
Business & Technology	515
Cars & Motorsports	485
Computer Hardware	258
Education	36
Electronics	525
Games	499
Home and Garden	692
Hotels & Travel	597
Kids & Family	1544
Magazines & Newspapers	240
Movies	1336
Music	953
Musical Instruments	279
Personal Finance	159
Pets	214
Restaurants & Gourmet	1145
Software	103
Sports & Outdoors	519
Wellness & Beauty	908

        static public CategoryInfo[] Catetories = new CategoryInfo[] 
        {
            new CategoryInfo("Books", 1078), 
            new CategoryInfo("Business & Technology", 515),
            new CategoryInfo("Cars & Motorsports", 485),
            
        };

        static public String[] CategoryName = new String[]
        {
            "Movies",
            "Books",
            "Music",
            "Wellness & Beauty",
            "Kids & Family",
            "Home and Garden",
            "Hotels & Travel",
            "Electronics",
            "Restaurants & Gourmet",
            "Games",
            "Computer Hardware",
            "Sports & Outdoors",
            "Cars & Motorsports",
            "Pets",
            "Business & Technology",
            "Magazines & Newspapers",
            "Personal Finance",
            "Software",
            "Musical Instruments",
            "Education",
        };
        public String Name = "";
        public Dictionary<String, CredibilityInfo> Credibility =
            new Dictionary<string, CredibilityInfo>();
        public List<String> TrustBy;

        public User(String n)
        {
            Name = n;
            foreach (String category in CategoryName)
            {
                Credibility.Add(category, new CredibilityInfo());
            }

            TrustBy = new List<string>();
        }

        public void Update(String category, int rating)
        {
            if (Credibility.ContainsKey(category))
            {
                Credibility[category].RatingCount++;
                Credibility[category].TotalRating += rating;
            }
        }

        public void AddTrustBy(String friend)
        {
            TrustBy.Add(friend);
        }

        public void CalculateExpertise()
        {
            foreach (KeyValuePair<String, CredibilityInfo> kvp in Credibility)
            {
                CredibilityInfo cr = kvp.Value;
                if (cr.RatingCount != 0)
                {
                    double average = (double)cr.TotalRating / (double)cr.RatingCount;
                    cr.Expertise = average * f((double)cr.RatingCount);
                }
            }
        }

        public void CalculateTrustwhorthiness()
        {
            foreach (KeyValuePair<String, CredibilityInfo> kvp in Credibility)
            {
                CredibilityInfo cr = kvp.Value;
                if(cr.TrusteeCount != 0)
                {
                    double average = (double)cr.TotalTrusteeExpertise / (double)cr.TrusteeCount;
                    cr.Trustworthiness = average * f((double)cr.TrusteeCount);
                   
                }
            }

            //if (Name == "stephen_murray" || Name == "shopaholic_man")
            //    Console.WriteLine(ToString("Movies"));
        
        }

        public CredibilityInfo GetCredibilityInfo(String category)
        {
            if (Credibility.ContainsKey(category))
                return Credibility[category];
            else
                return null;
        }

        static private double f(double n)
        {
            if (n > 1500.0d)
                return 1.0d;
            else
            {
                // BASELINE
                double en = Math.Pow(Math.E, 0.02d * n);
                return en / (en + 1);

                // CIRCULAR
                //return Math.Sqrt(1.0d - Math.Pow(n / 1500.0d - 1, 2.0d));

                // INVERSE
                //return Math.Pow(5.0d, -(1.0d / 0.06d * n));
            }
        }

        public String ToString(String category)
        {
            String str = Name;
            CredibilityInfo cr = Credibility[category];
            if (cr != null)
            {
                str += "\n\tTotalRating: " + cr.TotalRating.ToString()
                    + "\n\tRatingCount: " + cr.RatingCount.ToString()
                    + "\n\tTotalTrusteeExp: " + cr.TotalTrusteeExpertise.ToString()
                    + "\n\tTrusteeCount: " + cr.TrusteeCount.ToString()
                    + "\n\tExpertise: " + cr.Expertise.ToString()
                    + "\n\tTrustworthiness: " + cr.Trustworthiness.ToString()
                    ;
            }
            str += "\n";
            return str;
        }

       
    }

    /* Credibility information for a category */
     class CredibilityInfo
     {
         public int TotalRating = 0;
         public int RatingCount = 0;
         public double TotalTrusteeExpertise = 0.0d;
         public int TrusteeCount = 0;
         public double Expertise = 0.0d;
         public double Trustworthiness = 0.0d;

         public CredibilityInfo()
         {
             TotalRating = 0;
             RatingCount = 0;
             Expertise = 0.0d;
             Trustworthiness = 0.0d;
         }

         public CredibilityInfo(int totalRating, int ratingCount, double exp)
         {
             TotalRating = totalRating;
             RatingCount = ratingCount;
             Expertise = exp;
         }
     }
}
