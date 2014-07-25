using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arnetminer
{
  class Program
  {
    const string strTitle = "#*";
    const string strAuthors = "#@";
    const string strYear = "#year";
    const string strConfVenue = "#conf";
    const string strCitationNumber = "#citation";//(both -1 and 0 means none)
    const string strIndex = "#index";
    const string strArnetid = "#arnetid";
    const string strRefId = "#%";
    const string strAbstract = "#!";

    static private string directory = @"G:\dblp\2013\";
    /*public static Dictionary<int, Publication> dicAllPubs = Publication.dicAllPubs;
    public static Dictionary<int, Author> dicAllAuthors = Author.dicAllAuthors;
    public static Dictionary<int, Venue> dicAllVenues = Venue.dicAllVenues;
    */public static Dictionary<string, Venue> dicStrVenues = new Dictionary<string, Venue>();
    public static Dictionary<string, Author> dicStrAuthors = new Dictionary<string, Author>();
  
    public static int totalVenue;
    public static int totalAuthor;

    static void Main(string[] args)
    {
      totalAuthor = totalVenue = 0;
      
      string fileauthor = "test-author.csv";
      string filevenue = "test-venue.csv";
      string filepub = "test-pub.csv";

      ReadAuthorFromFile(directory + fileauthor);
      ReadVenueFromFile(directory + filevenue);
      ReadPublicationFromFile(directory + filepub);
      UpdatingCitation();
      int iCite = Publication.dicAllPubs.Values.Sum(x => x.liCitationIds.Count);
      int iRef = Publication.dicAllPubs.Values.Sum(x => x.liReferenceIds.Count);
      Console.WriteLine("Cite={0}, Ref={1}", iCite, iRef);
      DoNStarCalculation(directory, 1000, 1, 0.25, 0.25, 0.45);
      DoPageRankCalculation(directory, 1000, 1, 0.45);
       
      //string filename = "acm_output.txt";
      //ParseDataArnetMiner(directory + filename);
      /*string strPrefixfile = "publication-*";      
      string[] files = Directory.GetFiles(directory,strPrefixfile).ToArray();     
      foreach (string f in files)
      {
        ParseAuthorsVenues(f,true);              
      }
      UpdatingCitation();
      int iCite = Publication.dicAllPubs.Values.Sum(x => x.liCitationIds.Count);
      int iRef = Publication.dicAllPubs.Values.Sum(x => x.liReferenceIds.Count);
      WriteToFile(Author.dicAllAuthors, "authors-");
      WriteToFile(Venue.dicAllVenues, "venues-");
      WriteToFile(Publication.dicAllPubs, 1, "publications-");*/
      Console.WriteLine("DONE");
      Console.ReadLine();
    }

    private static void DoPageRankCalculation(string directory, int stoploop, int stopval, double damplingfactor)
    {
      Console.WriteLine("Start PAGERANK loop({0}) -stopval: {1}", stoploop, stopval);
      Console.WriteLine("Author: ({0}) -Venues:{1} -Publication:{2} ",
        Author.dicAllAuthors.Count, Venue.dicAllVenues.Count, Publication.dicAllPubs.Count);
      double sumvalue = Publication.dicAllPubs.Values.Sum(x => x.curValue);
      int step = 0;
      double max = 10000;
      Stopwatch sw = new Stopwatch();
      while (step < stoploop && max > stopval)
      {
        sw.Start();
        PageRankMove(step, out max, 0.45);
        sw.Stop();
        if (step % 10 == 0)
        {
          Console.WriteLine("Loop: {0} - Elapse:{1}", step, sw.Elapsed);
          WriteToFile(Publication.dicAllPubs, 1, string.Format("pub-pk-{0}-{1}",step,max));
        }
        step++;
      }
      WriteToFile(Publication.dicAllPubs, 1, string.Format("pub-pk-{0}-{1}", step, max));
      Console.WriteLine("Total Step:{0} Time: {1}- Done", step, sw.Elapsed);
    }

    private static void PageRankMove(int step, out double max, double damplingfactor)
    {
      object lock_paper = new object();
      ParallelOptions po = new ParallelOptions
      {
        MaxDegreeOfParallelism = Environment.ProcessorCount
      };
      var keys = Publication.dicAllPubs.Keys.ToList();
      Parallel.ForEach(keys, po, key =>
      {
        Publication.dicAllPubs[key].oldValue = Publication.dicAllPubs[key].curValue;
        Publication.dicAllPubs[key].curValue = 0;
      });
      double oldSum = Publication.dicAllPubs.AsParallel().Sum(x => x.Value.oldValue);


      Parallel.ForEach(keys, po, key =>
      {
        lock (lock_paper)
        {
          //sum_nocite += dicAllPapers[key].dicRefPaper.Count == 0 ? dicAllPapers[key].oldValue : 0;
          double temp = 0;
          foreach (int citepaper in Publication.dicAllPubs[key].liCitationIds)
          {
            temp += Publication.dicAllPubs[citepaper].oldValue / Publication.dicAllPubs[citepaper].liReferenceIds.Count;
          }
          Publication.dicAllPubs[key].curValue = damplingfactor * temp;
        }
      });
      double curSum = Publication.dicAllPubs.AsParallel().Sum(x => x.Value.curValue);
      double delta = oldSum - curSum;
      if (delta > 0)
      {
        Parallel.ForEach(keys, po, key =>
        {
          lock (lock_paper)
          {
            Publication.dicAllPubs[key].curValue += delta / Publication.dicAllPubs.Count;
          }
        }
        );
      }

      double curmax = 0;
      Parallel.ForEach(keys, p =>
      {
        var diff = Math.Abs(Publication.dicAllPubs[p].curValue - Publication.dicAllPubs[p].oldValue);
        lock (lock_paper)
        {
          if (curmax < diff)
          {
            curmax = diff;
          }
        }
      });
      max = curmax;
    }

    private static void UpdatingCitation()
    {
      foreach (Publication p in Publication.dicAllPubs.Values)
      {
        foreach(int refid in p.liReferenceIds)
           Publication.dicAllPubs[refid].liCitationIds.Add(p.idPaper);
      }
    }
    
    public static void DoNStarCalculation(string directory, 
     int stoploop, double stopval, double a1, double a2, double a3)
    {
      object lock_paper = new object();
      double suma = Author.dicAllAuthors.AsParallel().Sum(x => x.Value.curValue);
      double sump1 = Publication.dicAllPubs.AsParallel().Sum(x => x.Value.curValue);
      double sumc = Venue.dicAllVenues.AsParallel().Sum(x => x.Value.curValue);
      double sum = 0, curmax = 0, max = 1000;
      int step = 0;

      Stopwatch sw = new Stopwatch();
      while (step < stoploop && max > stopval)
      {
        sw.Start();
        //Parallel.ForEach(dicAllAuthors, da =>
        foreach (KeyValuePair<int, Author> da in Author.dicAllAuthors)
        {
          da.Value.SetValueFromPublication();
        }
        //);
        suma = Author.dicAllAuthors.AsParallel().Sum(x => x.Value.curValue);

        //Parallel.ForEach(dicAllConferences, dc =>
        foreach (KeyValuePair<int, Venue> dv in Venue.dicAllVenues)
        {
          dv.Value.SetValueFromPublication();
        }
        //);
        sumc = Venue.dicAllVenues.AsParallel().Sum(x => x.Value.curValue);

        foreach (KeyValuePair<int, Publication> dp in Publication.dicAllPubs)
        {
          dp.Value.SetValueFromAuthorVenue(a1, a2, a3);
        }

        double sump2 = Publication.dicAllPubs.AsParallel().Sum(x => x.Value.curValue);
        double delta = sump1 - sump2;
        if (delta > 0)
        {
          foreach (KeyValuePair<int, Publication> dp in Publication.dicAllPubs)
          {
            dp.Value.curValue += delta / Publication.dicAllPubs.Count;
          }
        }
        var keys = Publication.dicAllPubs.Keys.ToList();
        sump2 = Publication.dicAllPubs.AsParallel().Sum(x => x.Value.curValue);
        curmax = 0;
        Parallel.ForEach(keys, paper =>
        {
          var diff = Math.Abs(Publication.dicAllPubs[paper].curValue - Publication.dicAllPubs[paper].oldValue);
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
        if (step % 10 == 0)
        {
          WriteToFile(Author.dicAllAuthors, "author-" + step.ToString() + "-" + max.ToString() + "-");
          WriteToFile(Venue.dicAllVenues, "venue-" + step.ToString() + "-" + max.ToString());
          WriteToFile(Publication.dicAllPubs, 1, "pub-" + step.ToString() + "-" + max.ToString());
        }
        Console.WriteLine("Loop - 1: {0} - Elapse:{1}", step, sw.Elapsed);        
        step++;
      }
      WriteToFile(Author.dicAllAuthors, "author-" + step.ToString() + "-" + max.ToString() + "-");
      WriteToFile(Venue.dicAllVenues, "venue-" + step.ToString() + "-" + max.ToString());
      WriteToFile(Publication.dicAllPubs, 1, "pub-" + step.ToString() + "-" + max.ToString());
      Console.WriteLine("Done");
    }
    
    public static void ReadPublicationFromFile(string str)
    {
      IEnumerable<string> lines = File.ReadLines(Path.GetFullPath(str));
      var line = lines.Where(x => (x != ""));
      var results = new ConcurrentQueue<Publication>();
      Parallel.ForEach(line, l =>
      {
        Publication publication = Publication.FromLine(l);

        if (publication != null)
        {
          publication.curValue = 100;
          results.Enqueue(publication);
        }
      });
      Publication.dicAllPubs = results.ToDictionary(x => x.idPaper, x => x);
    }
    private static void ReadVenueFromFile(string str)
    {    
      IEnumerable<string> lines = File.ReadLines(Path.GetFullPath(str));
      var line = lines.Where(x => (x != ""));
      var results = new ConcurrentQueue<Venue>();
      Parallel.ForEach(line, l =>
      {
        Venue venue = Venue.FromLine(l);
        if (venue != null)
        {
          results.Enqueue(venue);
        }
      });
      Venue.dicAllVenues = results.ToDictionary(x => x.idVenue, x => x);
    }
    public static void ReadAuthorFromFile(string str)
    {
      IEnumerable<string> lines = File.ReadLines(Path.GetFullPath(str));
      var line = lines.Where(x => (x != ""));
      var results = new ConcurrentQueue<Author>();
      Parallel.ForEach(line, l =>
      {
        Author author = Author.FromLine(l);
        if (author != null )//&& author.idAuthor!=-1 )
        {
          results.Enqueue(author);
        }
      });
      Author.dicAllAuthors = results.ToDictionary(x => x.idAuthor, x => x);
    }

    public static void ParseAuthorsVenues(string f, bool withtitle)
    {
      Venue v= null;
      Author a=null;
      var lines =File.ReadLines(f);
      foreach (string l in lines)
      {
        try
        {
          Publication pub = Publication.ReadFromLine(l, withtitle);
          //add venues
          if (dicStrVenues.Keys.Contains(pub.venue))
          {
            v = dicStrVenues[pub.venue];
            if (!v.liPublications.Contains(pub.idPaper))
            {
              v.liPublications.Add(pub.idPaper);
            }
            pub.idVenue = v.idVenue;
          }
          else
          {
            totalVenue++;
            v = new Venue()
            {
              idVenue = pub.idVenue = totalVenue,
              venueName = pub.venue
            };
            v.liPublications.Add(pub.idPaper);
            Venue.dicAllVenues.Add(v.idVenue, v);
            dicStrVenues.Add(v.venueName, v);
          }
          //add authors
          foreach (string au in pub.strAuthors)
          {
            if (dicStrAuthors.Keys.Contains(au))
            {
              a = dicStrAuthors[au];
              if (!a.liPublications.Contains(pub.idPaper))
              {
                a.liPublications.Add(pub.idPaper);
              }
            }
            else
            {
              totalAuthor++;
              a = new Author()
              {
                idAuthor = totalAuthor,
                authorName = au
              };
              a.liPublications.Add(pub.idPaper);
              Author.dicAllAuthors.Add(a.idAuthor, a);
              dicStrAuthors.Add(a.authorName, a);
            }
            if (!pub.liAuthors.Contains(a.idAuthor))
            {
              pub.liAuthors.Add(a.idAuthor);
            }
          }
          Publication.dicAllPubs.Add(pub.idPaper, pub);
        }
        catch (Exception ex)
        {
          Console.WriteLine("Error:{0} at line {1}", ex.Message, l);
        }
      }
    }

    private static void ParseDataArnetMiner(string file)
    {
      //using (FileStream fs = File.Open(directory + filename, FileMode.Open, FileAccess.Read))
      //using (BufferedStream bs = new BufferedStream(fs))
      // using (StreamReader sr = new StreamReader(bs))

      using (StreamReader sr = File.OpenText(file))
      {
        string s;
        string strcurTitle = "#*";
        int curYear = 0;
        string strcurConfVenue = "#con";
        int curCitationNumber = 0;
        int curIndex = -1;
        int curArnetid = -1;
        string strcurAbstract = "#!";
        Publication curPublication = null;
        List<string> strcurAuthors = new List<string>();
        List<int> icurRefId = new List<int>();
        int i = 0;
        curPublication = new Publication();
        while ((s = sr.ReadLine()) != null)
        {
          if (s.StartsWith(strTitle))
          {
            //save current paper
            if (curPublication == null)
            {
              curPublication = new Publication(
                curIndex,
                100,
                strcurTitle,
                strcurAuthors.ToArray(),
                curYear,
                strcurConfVenue,
                curCitationNumber,
                curIndex,
                curArnetid,
                strcurAbstract,
                icurRefId);
              Publication.dicAllPubs.Add(curIndex, curPublication);
              if ((Publication.dicAllPubs.Count) % 500000 == 0)
              {
                Console.WriteLine("dic={0}", Publication.dicAllPubs.Count * i);
              }
              if ((Publication.dicAllPubs.Count) == 500000)
              {
                i++;
                WriteToFile(Publication.dicAllPubs, i, "pub-");
                Publication.dicAllPubs.Clear();
              }
            }
            //RESET create new paper
            curPublication = null;
            icurRefId.Clear();
            strcurTitle = s.Substring(strTitle.Length);
            strcurAuthors.Clear();
          }
          else if (s.StartsWith(strAuthors))
          {
            strcurAuthors.AddRange(s.Substring(strAuthors.Length).Split(','));
          }
          else if (s.StartsWith(strYear))
          {
            curYear = Convert.ToInt32(s.Substring(strYear.Length));
          }
          else if (s.StartsWith(strConfVenue))
          {
            strcurConfVenue = s.Substring(strConfVenue.Length);
          }
          else if (s.StartsWith(strCitationNumber))
          {
            curCitationNumber = Convert.ToInt32(s.Substring(strCitationNumber.Length));
          }
          else if (s.StartsWith(strIndex))
          {
            curIndex = Convert.ToInt32(s.Substring(strIndex.Length));
          }
          else if (s.StartsWith(strArnetid))
          {
            curArnetid = Convert.ToInt32(s.Substring(strArnetid.Length));
          }
          else if (s.StartsWith(strRefId))
          {
            icurRefId.Add(Convert.ToInt32(s.Substring(strRefId.Length)));
          }
          else if (s.StartsWith(strAbstract))
          {
            strcurAbstract = s.Substring(strAbstract.Length);
          }
          else
          {
            if (s.Length != 0)
              Console.WriteLine("Unknown tag: string=" + s);
          }
        }
        WriteToFile(Publication.dicAllPubs, 1, "pub-");
      }
    }

    private static void WriteToFile(Dictionary<int, Publication> dicPubs, int round, string filename)
    {            
      using (StreamWriter file = new System.IO.StreamWriter(Path.GetFullPath(directory + filename +
       (round * dicPubs.Count).ToString() + ".csv")))
      {
        file.Write("idPaper\tcurValue\ttitle\tstring.Join(liAuthors)\t"+
          "year\tvenue\tcitationnumber\tindex\tarnetid\tstrAbstract\t"+
          "string.Join(liReferenceIds)\tstring.Join(liCitationIds)\tidVenue\r\n");
        foreach (Publication p in dicPubs.Values)
        {
          file.Write(p.ToString());
        }
      }
      GC.Collect();
    }
    private static void WriteToFileSimple(Dictionary<int, Publication> dicPubs, int count, string filename)
    {
      using (StreamWriter file = new System.IO.StreamWriter(Path.GetFullPath(directory + filename +
      count.ToString() + ".csv")))
      {

        foreach (Publication p in dicPubs.Values)
        {
          file.Write(p.ToStringSimple());
        }
      }
      GC.Collect();
    }
    private static void WriteToFile(Dictionary<int, Venue> dicVenues, string filename)
    {
      using (StreamWriter file = new System.IO.StreamWriter(Path.GetFullPath(directory + filename +
      (Venue.dicAllVenues.Count).ToString() + ".csv")))
      {
        file.Write("idVenue\tcurValue\tvenueName\tiPublications.Count\tstring.Join(iPublications.ToArray())\r\n");
        foreach (Venue v in dicVenues.Values)
        {
          file.Write(v.ToString());
        }
      }     
    }
    private static void WriteToFile(Dictionary<int, Author> dicAuthors, string filename)
    {
      using (StreamWriter file = new System.IO.StreamWriter(Path.GetFullPath(directory + filename +
      (dicAuthors.Count).ToString() + ".csv")))
      {
        file.Write("idAuthor\tcurValue\tauthorName\tiPublications.Count\tstring.Join(iPublications.ToArray())\r\n");
        foreach (Author a in dicAuthors.Values)
        {
          file.Write(a.ToString());
        }
      }
    }
  }
}
