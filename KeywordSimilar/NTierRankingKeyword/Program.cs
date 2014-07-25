using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTierRankingKeyword
{
  class Program
  {
    static int parallelLoop = 300;
    static string directory = @"G:\dblp\2014\db\";



    static void Main(string[] args)
    {
      directory = @"G:\dblp\2014\db\";
      int stoploop = 1000;
      int stopval = 1;
      int nsequence = 3;
      double a1 = 0.45, a2 = 0.45, a3 = 0.1;
      string fileconference = "X_J_C_Conference.csv";
      string filekeyword = "X_J_C_Keyword.csv";
      string filepaper = "X_J_C_Paper.csv";
      string fileauthor = "X_J_C_Author.csv";
      string fileauthorpaper = "X_J_C_Author_Paper.csv";
      string filekeywordpaper = "X_J_C_Paper_Keyword.csv";
      string filepaperpaper = "X_J_C_Paper_Paper.csv";
      string filestatevalue = "-";
      try
      {
        if (args.Length > 0)
        {
          directory = args[0];
          stoploop = Convert.ToInt32(args[1]);
          stopval = Convert.ToInt32(args[2]);
          fileauthor = args[3];
          filepaper = args[4];
          fileconference = args[5];
          fileauthorpaper = args[6];
          filepaperpaper = args[7];         
          a1 = Convert.ToDouble(args[8]);
          a2 = Convert.ToDouble(args[9]);
          a3 = Convert.ToDouble(args[10]);
        }

        Utilities.ReadAuthorFromFile(directory + fileauthor);
        Utilities.ReadConferenceFromFile(directory + fileconference);
        Utilities.ReadPaperFromFile(directory + filepaper);
        Utilities.ReadAuthorPaperFromFile(directory + fileauthorpaper);

        Console.WriteLine("Paper before remove:{0}", Utilities.dicAllPapers.Count);
        List<KeyValuePair<int, Paper>> lremove = Utilities.dicAllPapers.Where(x => x.Value.dicAuthors.Count == 0).ToList();
        Parallel.ForEach(lremove, item =>
        {
          lock (Utilities.lock_paper)
          {
            Utilities.dicAllPapers.Remove(item.Key);
          }
        });
        Console.WriteLine("Paper after remove:{0}", Utilities.dicAllPapers.Count);
        //Add conferences
        Parallel.ForEach(Utilities.dicAllPapers, dp =>
        {
          lock (Utilities.lock_conference)
          {
            Utilities.dicAllConferences[dp.Value.conference].dicPapers.Add(dp.Key, dp.Value);
          }
        }
        );
        Console.WriteLine("Conference before remove (<300papers):{0}", Utilities.dicAllConferences.Count);
        List<KeyValuePair<int, Conference>> cremove = Utilities.dicAllConferences.Where(x => x.Value.dicPapers.Count < 300).ToList();
        Parallel.ForEach(cremove, item =>
        {
          lock (Utilities.lock_conference)
          {
            Utilities.dicAllConferences.Remove(item.Key);
          }
        });
        Console.WriteLine("Conference after remove:{0}", Utilities.dicAllConferences.Count);
        Console.WriteLine("Paper before remove conference:{0}", Utilities.dicAllPapers.Count);
        lremove = Utilities.dicAllPapers.Where(x => !Utilities.dicAllConferences.Keys.Contains(x.Value.conference)).ToList();
        Parallel.ForEach(lremove, item =>
        {
          lock (Utilities.lock_paper)
          {
            Utilities.dicAllPapers.Remove(item.Key);
          }
        });
        Console.WriteLine("Paper after remove conference:{0}", Utilities.dicAllPapers.Count);

        Console.WriteLine("Author before remove:{0}", Utilities.dicAllAuthors.Count);
       
        //remove dicpaper of author which is not in alldicpaper
        Parallel.ForEach(Utilities.dicAllAuthors, author =>
        {
          {
            List<int> paperids = author.Value.dicPapers.Keys.ToList();
            foreach (int id in paperids){
              if (!Utilities.dicAllPapers.Keys.Contains(id))
                author.Value.dicPapers.Remove(id);
            }
          }
        });

        List<KeyValuePair<int, Author>> aremove = Utilities.dicAllAuthors.Where(x => x.Value.dicPapers.Count == 0).ToList();
        Parallel.ForEach(aremove, item =>
        {
          lock (Utilities.lock_author)
          {
            Utilities.dicAllAuthors.Remove(item.Key);
          }
        });
        Console.WriteLine("Author after remove:{0}", Utilities.dicAllAuthors.Count);
        
        Utilities.ReadPaperCiteFromFile(directory + filepaperpaper);

        Utilities.DoCaculation(directory, filepaper, stoploop, stopval, a1, a2, a3);
        Console.WriteLine("DONE!");
      }
      catch (Exception ex)
      {
        Console.WriteLine("Error:{0} - {1}", ex.Message, ex.StackTrace);
        Console.WriteLine("Usage: NTierRankingKeyword.exe \"filepath\" \"stoploop\" \"stopval\"" +
          " \"authorfile\" \"paperfile\" \"conferencefile\" \"author_paper\" \"paper_paper\"  \"a1\" \"a2\" \"a3\"");
      }
    }
  }
}
