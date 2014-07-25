using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTierRankingKeyword
{
  static class Utilities
  {

    public static object lock_paper = new object();
    public static object lock_keyword = new object();
    public static object lock_author = new object();
    public static object lock_conference = new object();

    public static Dictionary<int, Author> dicAllAuthors = new Dictionary<int, Author>();
    public static Dictionary<int, Conference> dicAllConferences = new Dictionary<int, Conference>();
    public static Dictionary<int, Paper> dicAllPapers = new Dictionary<int, Paper>();
    public static Dictionary<int, Keyword> dicAllKeywords = new Dictionary<int, Keyword>();
    //public static Dictionary<KeyValuePair<int, int>, double> dicSpaceStateValues = new Dictionary<KeyValuePair<int, int>, double>();
    //public static Dictionary<KeyValuePair<int, int>, double> dicSpaceStateOldValues = new Dictionary<KeyValuePair<int, int>, double>();
    //public static Dictionary<KeyValuePair<int, int>, Dictionary<int, double>> dicSpaceStateResults = new Dictionary<KeyValuePair<int, int>, Dictionary<int, double>>();
  
    public static void ReadConferenceFromFile(string str)
    {
      IEnumerable<string> lines = File.ReadLines(Path.GetFullPath(str));
      var line = lines.Where(x => (x != ""));
      var results = new ConcurrentQueue<Conference>();
      Parallel.ForEach(line, l =>
      {
        Conference conference = Conference.FromLine(l);
        if (conference != null)
        {
          results.Enqueue(conference);
        }
      });
      dicAllConferences = results.ToDictionary(x => x.idConference, x => x);
    }

    public static void ReadAuthorFromFile(string str)
    {
      IEnumerable<string> lines = File.ReadLines(Path.GetFullPath(str));
      var line = lines.Where(x => (x != ""));
      var results = new ConcurrentQueue<Author>();
      Parallel.ForEach(line, l =>
      {
        Author author = Author.FromLine(l);
        if (author != null)
        {
          results.Enqueue(author);
        }
      });
      dicAllAuthors = results.ToDictionary(x => x.idAuthor, x => x);
    }

    public static void ReadKeywordFromFile(string str)
    {
      IEnumerable<string> lines = File.ReadLines(Path.GetFullPath(str));
      var line = lines.Where(x => (x != ""));
      var results = new ConcurrentQueue<Keyword>();
      Parallel.ForEach(line, l =>
      {
        Keyword keyword = Keyword.FromLine(l);
        if (keyword != null)
        {
          results.Enqueue(keyword);
        }
      });
      dicAllKeywords = results.ToDictionary(x => x.idKeyword, x => x);
    }

    public static void ReadPaperFromFile(string str)
    {
      IEnumerable<string> lines = File.ReadLines(Path.GetFullPath(str));
      var line = lines.Where(x => (x != ""));
      var results = new ConcurrentQueue<Paper>();
      Parallel.ForEach(line, l =>
      {
        Paper paper = Paper.FromLine(l);
        if (paper != null)
        {
          results.Enqueue(paper);
        }
      });
      dicAllPapers = results.ToDictionary(x => x.idPaper, x => x);      
    }

    public static void ReadKeywordPaperRelationFromFile(string str)
    {
      IEnumerable<string> lines = File.ReadLines(Path.GetFullPath(str));
      var line = lines.Where(x => (x != ""));
      var results = new ConcurrentQueue<KeyValuePair<int, int>>();
      Parallel.ForEach(line, l =>
      {
        string[] datas = l.Split(',');
        try
        {
          int idPaper = Convert.ToInt32(datas[0].Trim('"'));
          int idKeyword = Convert.ToInt32(datas[1].Trim('"'));
          lock (lock_paper)
          {
            dicAllPapers[idPaper].dicKeywords.Add(idKeyword, dicAllKeywords[idKeyword]);
          }
          lock (lock_keyword)
          {
            dicAllKeywords[idKeyword].dicPapers.Add(idPaper, dicAllPapers[idPaper]);
          }
          results.Enqueue(new KeyValuePair<int, int>(idKeyword, idPaper));
        }
        catch (Exception ex)
        {
          Console.WriteLine("Error {0} at line:{1}", ex.Message, l);
        }
      });
      //dicSpaceStateValues = results.ToDictionary(x => x, x => 0.0);
      // dicSpaceStateOldValues = results.ToDictionary(x => x, x => 0.0);
      //dicSpaceStateResults = results.ToDictionary(x => x, x => new Dictionary<int, double>());
    }

    public static void ReadAuthorPaperFromFile(string str)
    {
      IEnumerable<string> lines = File.ReadLines(Path.GetFullPath(str));
      var line = lines.Where(x => (x != ""));
      var results = new ConcurrentQueue<KeyValuePair<int, int>>();
      Parallel.ForEach(line, l =>
      {
        string[] datas = l.Split(',');
        try
        {
          int idAuthor = Convert.ToInt32(datas[0].Trim('"'));
          int idPaper = Convert.ToInt32(datas[1].Trim('"'));
          lock (lock_paper)
          {
            dicAllPapers[idPaper].dicAuthors.Add(idAuthor, dicAllAuthors[idAuthor]);
          }
          lock (lock_author)
          {
            dicAllAuthors[idAuthor].dicPapers.Add(idPaper, dicAllPapers[idPaper]);
          }
          results.Enqueue(new KeyValuePair<int, int>(idAuthor, idPaper));
        }
        catch (Exception ex)
        {
          Console.WriteLine("Error {0} at line:{1}", ex.Message, l);
        }
      });
      //dicSpaceStateValues = results.ToDictionary(x => x, x => 0.0);
      // dicSpaceStateOldValues = results.ToDictionary(x => x, x => 0.0);
      //dicSpaceStateResults = results.ToDictionary(x => x, x => new Dictionary<int, double>());
    }

    public static void ReadPaperCiteFromFile(string str)
    {
      IEnumerable<string> lines = File.ReadLines(Path.GetFullPath(str));
      var line = lines.Where(x => (x != ""));
      var results = new ConcurrentQueue<KeyValuePair<int, int>>();
      Parallel.ForEach(line, l =>
      {
        string[] datas = l.Split(',');
        try
        {
          int idPaper = Convert.ToInt32(datas[0].Trim('"'));
          int idCitePaper = Convert.ToInt32(datas[1].Trim('"'));
          lock (lock_paper)
          {
            if (dicAllPapers.Keys.Contains(idCitePaper) && dicAllPapers.Keys.Contains(idPaper))
            {
              dicAllPapers[idPaper].dicCitePapers.Add(idCitePaper, dicAllPapers[idCitePaper]);
              dicAllPapers[idCitePaper].dicRefPapers.Add(idPaper, dicAllPapers[idPaper]);
            }
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine("Error {0} at line:{1}", ex.Message, l);
        }
      });
    }

    internal static void DoCaculation(string directory, string file, 
      int stoploop, double stopval, double a1, double a2, double a3)
    {
      
      double suma = dicAllAuthors.AsParallel().Sum(x => x.Value.curValue);
      double sump1 = dicAllPapers.AsParallel().Sum(x => x.Value.curValue);
      double sumc = dicAllConferences.AsParallel().Sum(x => x.Value.curValue);
      double sum = 0, curmax =0 , max=1000;
      int step = 0;

      Stopwatch sw = new Stopwatch();
      while (step < stoploop && max > stopval)
      {
        sw.Start();
        //Parallel.ForEach(dicAllAuthors, da =>
        foreach (KeyValuePair<int, Author> da in dicAllAuthors)
        {
          da.Value.SetValueFromPaper();
        }
        //);
        suma = dicAllAuthors.AsParallel().Sum(x => x.Value.curValue);

        //Parallel.ForEach(dicAllConferences, dc =>
        foreach (KeyValuePair<int, Conference> dc in dicAllConferences)
        {
          dc.Value.SetValueFromPaper();
        }
        //);
        sumc = dicAllConferences.AsParallel().Sum(x => x.Value.curValue);

        foreach (KeyValuePair<int, Paper> dp in dicAllPapers)
        {
          dp.Value.SetValueFromAuthorConference(a1, a2, a3, dicAllConferences);
        }

        double sump2 = dicAllPapers.AsParallel().Sum(x => x.Value.curValue);
        double delta = sump1 - sump2;
        if (delta > 0)
        {
          foreach (KeyValuePair<int, Paper> dp in dicAllPapers)
          {
            dp.Value.curValue += delta / dicAllPapers.Count;
          }
        }
        var keys = dicAllPapers.Keys.ToList();
        sump2 = dicAllPapers.AsParallel().Sum(x => x.Value.curValue);
        curmax = 0;
        Parallel.ForEach(keys, paper =>
        {
          var diff = Math.Abs(dicAllPapers[paper].curValue - dicAllPapers[paper].oldValue);
          lock (lock_paper)
          {
            if (curmax < diff)
            {
              curmax = diff;
            }
          }
        });
        max = curmax;
        sw.Stop();
        Console.WriteLine("Loop - 1: {0} - Elapse:{1}", step, sw.Elapsed);
        WriteResultToFile(directory, file + "-" + step.ToString() + "-" + max.ToString());
        step++;
      }
      Console.WriteLine("Done");
    }

    private static void WriteResultToFile(string directory, string file)
    {
      StringBuilder sb = new StringBuilder();
      var results1 = new ConcurrentQueue<string>();
      Parallel.ForEach(dicAllPapers, x =>
      {
        results1.Enqueue(string.Format("\"{0}\",\"{1}\",\"{2}\"\n", x.Key, x.Value.curValue, x.Value.conference));
      });
      File.WriteAllText(Path.GetFullPath(directory + file + "-papers.csv"), string.Concat(results1.ToList()));
      var results2 = new ConcurrentQueue<string>();
      Parallel.ForEach(dicAllAuthors, x =>
      {
        results2.Enqueue(string.Format("\"{0}\",\"{1}\",\"{2}\"\n", x.Key, x.Value.curValue, x.Value.authorName));
      });
      File.WriteAllText(Path.GetFullPath(directory + file + "-authors.csv"), string.Concat(results2.ToList()));
      var results3 = new ConcurrentQueue<string>();
      Parallel.ForEach(dicAllConferences, x =>
      {
        results3.Enqueue(string.Format("\"{0}\",\"{1}\",\"{2}\"\n", x.Key, x.Value.curValue, x.Value.conferenceName));
      });
      File.WriteAllText(Path.GetFullPath(directory + file + "-conferences.csv"), string.Concat(results3.ToList()));

    }
  }
}
