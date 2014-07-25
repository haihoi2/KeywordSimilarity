using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RankCorrelation
{
  class Program
  {
    static string directory = @"F:\vohoanghai2\Dropbox-vohoanghai2-gmail\Dropbox\LeAnhVu\HungaryJournal\experiment result\";
    static void Main(string[] args)
    {
      //string fileconference = "result-3351dblp-conferences-3351.csv";
      //string fileauthor = "result-845295dblp.xml-www-845295.csv";
      string fileconference = "c3-conferences.csv";
      string fileauthor = "c3-authors.csv";
      
      try
      {
        if (args.Length > 0)
        {
          directory = args[0];
          fileconference = args[1];
          fileauthor = args[2];
        }
       // Utilities.ReadConferenceFromResultFile(directory + fileconference);
       // Utilities.ReadConferenceFromResultFileNew(directory + fileconference);
       // Utilities.MakeConferenceAvgRank(directory);
        double k1=0,k2=0,k3=0;
       // Utilities.KendalTauConference(out k1,out k2, out k3);
       // Console.WriteLine("{0}-{1}-{2}",k1,k2,k3);
       // Utilities.KendalTauConference();
       // Utilities.ReadAuthorFromResultFile(directory + fileauthor);
        Utilities.ReadAuthorFromResultFileNew(directory + fileauthor);
        Utilities.KendalTauAuthors(out k1, out k2, out k3);
        Console.WriteLine("{0}-{1}-{2}",k1,k2,k3);
        //Utilities.MakeAuthorAvgRank(directory);
        
      }
      catch (Exception ex)
      {
        Console.WriteLine("Error:{0} - {1}", ex.Message, ex.StackTrace);
        Console.WriteLine("Usage: RankCorrelation.exe \"filepath\" "+
          "\"conferencefile\" \"authorfile\"");
      }
      Console.WriteLine("DONE!");
      Console.ReadLine();
    }
  }
}
