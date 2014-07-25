using NTierRankingKeyword;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RankCorrelation
{
  static public class Utilities
  {
    public static object lock_paper = new object();
    public static object lock_keyword = new object();
    public static object lock_author = new object();
    public static object lock_conference = new object();

    public static Dictionary<int, Author> dicAllAuthors = new Dictionary<int, Author>();
    public static Dictionary<int, Conference> dicAllConferences = new Dictionary<int, Conference>();
    public static Dictionary<int, Paper> dicAllPapers = new Dictionary<int, Paper>();
    public static Dictionary<int, Keyword> dicAllKeywords = new Dictionary<int, Keyword>();

    public static void ReadConferenceFromResultFile(string str)
    {
      IEnumerable<string> lines = File.ReadLines(Path.GetFullPath(str));
      var line = lines.Where(x => (x != ""));
      var results = new ConcurrentQueue<Conference>();
      Parallel.ForEach(line, l =>
      {
        Conference conference = Conference.FromResultLine(l);
        if (conference != null)
        {
          results.Enqueue(conference);
        }
      });
      dicAllConferences = results.ToDictionary(x => x.idConference, x => x);
    }
    public static void ReadConferenceFromResultFileNew(string str)
    {
      IEnumerable<string> lines = File.ReadLines(Path.GetFullPath(str));
      var line = lines.Where(x => (x != ""));
      var results = new ConcurrentQueue<Conference>();
      Parallel.ForEach(line, l =>
      {
        Conference conference = Conference.FromResultLineNew(l);
        if (conference != null)
        {
          results.Enqueue(conference);
        }
      });
      dicAllConferences = results.ToDictionary(x => x.idConference, x => x);
    }
    public static void ReadAuthorFromResultFileNew(string str)
    {
      IEnumerable<string> lines = File.ReadLines(Path.GetFullPath(str));
      var line = lines.Where(x => (x != ""));
      var results = new ConcurrentQueue<Author>();
      Parallel.ForEach(line, l =>
      {
        Author author = Author.FromResultLineNew(l);
        if (author != null)
        {
          results.Enqueue(author);
        }
      });
      dicAllAuthors = results.ToDictionary(x => x.idAuthor, x => x);
    }
    public static void ReadAuthorFromResultFile(string str)
    {
      IEnumerable<string> lines = File.ReadLines(Path.GetFullPath(str));
      var line = lines.Where(x => (x != ""));
      var results = new ConcurrentQueue<Author>();
      Parallel.ForEach(line, l =>
      {
        Author author = Author.FromResultLineNew(l);
        if (author != null)
        {
          results.Enqueue(author);
        }
      });
      dicAllAuthors = results.ToDictionary(x => x.idAuthor, x => x);
    }

    public static void MakeConferenceAvgRank(string directory)
    {
      List<KeyValuePair<int, Conference>> myList = dicAllConferences.ToList();
      myList.Sort(
        delegate(KeyValuePair<int, Conference> firstPair,
          KeyValuePair<int, Conference> nextPair)
        {
          return nextPair.Value.curValue.CompareTo(firstPair.Value.curValue);
        });
      for (int i = 0; i < myList.Count; i++)
      {
        dicAllConferences[myList[i].Key].curRank = i + 1;
      }
      double sumRank = 0;
      int countSameRank = 0;
      for (int i = 0; i < myList.Count; i++)
      {
        if ((i < (myList.Count - 1)) && (dicAllConferences[myList[i].Key].curValue == dicAllConferences[myList[i + 1].Key].curValue))
        {
          sumRank += dicAllConferences[myList[i].Key].curRank;
          countSameRank++;
        }
        else
        {
          sumRank += dicAllConferences[myList[i].Key].curRank;
          countSameRank++;
          for (int k = 0; k < countSameRank; k++)
          {
            dicAllConferences[myList[i - k].Key].curRank = sumRank / countSameRank;
          }
          sumRank = countSameRank = 0;
        }
      }

      myList.Sort(
        delegate(KeyValuePair<int, Conference> firstPair,
          KeyValuePair<int, Conference> nextPair)
        {
          return nextPair.Value.NPC.CompareTo(firstPair.Value.NPC);
        });
      for (int i = 0; i < myList.Count; i++)
      {
        dicAllConferences[myList[i].Key].oldRank = i + 1;
      }

      sumRank = 0;
      countSameRank = 0;
      for (int i = 0; i < myList.Count - 1; i++)
      {
        if ((i < (myList.Count - 1)) && (dicAllConferences[myList[i].Key].NPC == dicAllConferences[myList[i + 1].Key].NPC))
        {
          sumRank += dicAllConferences[myList[i].Key].oldRank;
          countSameRank++;
        }
        else
        {
          sumRank += dicAllConferences[myList[i].Key].oldRank;
          countSameRank++;
          for (int k = 0; k < countSameRank; k++)
          {
            dicAllConferences[myList[i - k].Key].oldRank = sumRank / countSameRank;
          }
          sumRank = countSameRank = 0;
        }
      }
      StringBuilder sb = new StringBuilder();
      sb.AppendLine("Kendal Tau=" + Utilities.KendalTauConference().ToString());
      sb.AppendLine("idConference, conferenceName, curRank, curValue, oldRank, NPC");
      foreach (KeyValuePair<int, Conference> conf in dicAllConferences)
      {
        sb.AppendLine(string.Format("{0},{1},{2},{3},{4},{5}",
          conf.Value.idConference, conf.Value.conferenceName,
          conf.Value.curRank, conf.Value.curValue,
          conf.Value.oldRank, conf.Value.NPC));
      }
      File.WriteAllText(Path.GetFullPath(directory + "conf-rank-" + dicAllConferences.Count.ToString() + ".csv"), sb.ToString());

    }

    public static void MakeAuthorAvgRank(string directory)
    {
      List<KeyValuePair<int, Author>> myList = dicAllAuthors.ToList();
      myList.Sort(
        delegate(KeyValuePair<int, Author> firstPair,
          KeyValuePair<int, Author> nextPair)
        {
          return nextPair.Value.curValue.CompareTo(firstPair.Value.curValue);
        });
      for (int i = 0; i < myList.Count; i++)
      {
        dicAllAuthors[myList[i].Key].curRank = i + 1;
      }
      double sumRank = 0;
      int countSameRank = 0;
      for (int i = 0; i < myList.Count; i++)
      {
        if ((i < (myList.Count - 1)) && (dicAllAuthors[myList[i].Key].curValue == dicAllAuthors[myList[i + 1].Key].curValue))
        {
          sumRank += dicAllAuthors[myList[i].Key].curRank;
          countSameRank++;
        }
        else
        {
          sumRank += dicAllAuthors[myList[i].Key].curRank;
          countSameRank++;
          for (int k = 0; k < countSameRank; k++)
          {
            dicAllAuthors[myList[i - k].Key].curRank = sumRank / countSameRank;
          }
          sumRank = countSameRank = 0;
        }
      }

      myList.Sort(
        delegate(KeyValuePair<int, Author> firstPair,
          KeyValuePair<int, Author> nextPair)
        {
          return nextPair.Value.NPC.CompareTo(firstPair.Value.NPC);
        });
      for (int i = 0; i < myList.Count; i++)
      {
        dicAllAuthors[myList[i].Key].oldRank = i + 1;
      }

      sumRank = 0;
      countSameRank = 0;
      for (int i = 0; i < myList.Count - 1; i++)
      {
        if ((i < (myList.Count - 1)) && (dicAllAuthors[myList[i].Key].NPC == dicAllAuthors[myList[i + 1].Key].NPC))
        {
          sumRank += dicAllAuthors[myList[i].Key].oldRank;
          countSameRank++;
        }
        else
        {
          sumRank += dicAllAuthors[myList[i].Key].oldRank;
          countSameRank++;
          for (int k = 0; k < countSameRank; k++)
          {
            dicAllAuthors[myList[i - k].Key].oldRank = sumRank / countSameRank;
          }
          sumRank = countSameRank = 0;
        }
      }
      StringBuilder sb = new StringBuilder();
 //     sb.AppendLine("Kendal Tau=" + Utilities.KendalTauAuthors().ToString());
      sb.AppendLine("idauthor, authorname, curRank, curValue, oldRank, NPC");
      foreach (KeyValuePair<int, Author> author in dicAllAuthors)
      {
        sb.AppendLine(string.Format("{0},{1},{2},{3},{4},{5}",
          author.Value.idAuthor, author.Value.authorName,
          author.Value.curRank, author.Value.curValue,
          author.Value.oldRank, author.Value.NPC));
      }
      File.WriteAllText(Path.GetFullPath(directory + "authors-rank-" + dicAllAuthors.Count.ToString() + ".csv"), sb.ToString());

    }

    public static double KendalTauA()
    {
      int[] npc = new int[dicAllAuthors.Count];
      int[] sd3r = new int[dicAllAuthors.Count];
      int i=0;
      foreach (KeyValuePair<int, Author> conf in dicAllAuthors)
      {
        npc[i] = (int) conf.Value.curRank-1;
        sd3r[i] = (int) conf.Value.oldRank-1;
        i++;
      }
      return Inversions.Kendalldistance(npc,sd3r);
    }
    public static void KendalTauConference(out double k1,out double k2,out double k3)
    {
      object objc = new object(), objc2 = new object(), objc3 = new object();
      object objd = new object();
      int countConcordant1 = 0, countConcordant2 = 0,countConcordant3 = 0;
      int countDisConcordant1 = 0, countDisConcordant2 = 0, countDisConcordant3 = 0;
      List<int> confKeys = dicAllConferences.Keys.ToList();
      for (int i = 0; i < confKeys.Count - 1; i++)
      {
        //Parallel.For(i + 1, confKeys.Count, j =>
        for (int j = i + 1; j < confKeys.Count; j++)
        {
          if (((dicAllConferences[confKeys[i]].curValue - dicAllConferences[confKeys[j]].curValue) *
            (dicAllConferences[confKeys[i]].NCC - dicAllConferences[confKeys[j]].NCC)) >= 0)
          {
            countConcordant1++;
          }
          else
          {
            countDisConcordant1++;
          }
          if (((dicAllConferences[confKeys[i]].curValue - dicAllConferences[confKeys[j]].curValue) *
            (dicAllConferences[confKeys[i]].NPC - dicAllConferences[confKeys[j]].NPC)) >= 0)
          {
            countConcordant2++;
          }
          else
          {
            countDisConcordant2++;
          }
          if (((dicAllConferences[confKeys[i]].NCC - dicAllConferences[confKeys[j]].NCC) *
           (dicAllConferences[confKeys[i]].NPC - dicAllConferences[confKeys[j]].NPC)) >= 0)
          {
            countConcordant3++;
          }
          else
          {
            countDisConcordant3++;
          }
        }
        //);
      }
      double num= confKeys.Count * (confKeys.Count-1);
      k1 = 2.0* (countConcordant1 - countDisConcordant1) / num;
      k2 = 2.0 * (countConcordant2 - countDisConcordant2) / num;
      k3 = 2.0 * (countConcordant3 - countDisConcordant3) / num; 
    }

  
    public static double KendalTauConference()
    {
      object objc = new object();
      object objd = new object();
      int countConcordant = 0;
      int countDisConcordant = 0;
      List<int> confKeys = dicAllConferences.Keys.ToList();
      for (int i = 0; i < confKeys.Count - 1; i++)
      {
        Parallel.For(i + 1, confKeys.Count, j =>
        //for (int j = i + 1; j < confKeys.Count; j++)
        {
          if (((dicAllConferences[confKeys[i]].curRank - dicAllConferences[confKeys[j]].curRank) *
            (dicAllConferences[confKeys[i]].oldRank - dicAllConferences[confKeys[j]].oldRank)) > 0)
          {
            lock (objc)
            {
              countConcordant++;
            }
          }
          if (((dicAllConferences[confKeys[i]].curRank - dicAllConferences[confKeys[j]].curRank) *
           (dicAllConferences[confKeys[i]].oldRank - dicAllConferences[confKeys[j]].oldRank)) < 0)
          {
            lock (objd)
            {
              countDisConcordant++;
            }
          }
        }
        );
      }
      return 2.0 * (countConcordant - countDisConcordant) / confKeys.Count / (confKeys.Count - 1);
    }

    public static void KendalTauAuthors(out double k1, out double k2, out double k3)
    {
      ParallelOptions po = new ParallelOptions()
      {
        MaxDegreeOfParallelism = Environment.ProcessorCount
      };
      object objc = new object(), objc2 = new object(), objc3 = new object();
      object objd = new object(), objd2 = new object(), objd3 = new object();
      long countConcordant1 = 0, countConcordant2 = 0, countConcordant3 = 0;
      long countDisConcordant1 = 0, countDisConcordant2 = 0, countDisConcordant3 = 0;
      List<int> authorKeys = dicAllAuthors.Keys.ToList();
      Stopwatch sw = new Stopwatch();
      for (int i = 0; i < authorKeys.Count - 1; i++)
      {
        sw.Start();
        Parallel.For(i + 1, authorKeys.Count, po,j =>
        //for (int j = i + 1; j < authorKeys.Count; j++)
        {
          if (((dicAllAuthors[authorKeys[i]].curValue - dicAllAuthors[authorKeys[j]].curValue) *
            (dicAllAuthors[authorKeys[i]].NCC - dicAllAuthors[authorKeys[j]].NCC)) >= 0)
          {
            lock (objc)
            {
              countConcordant1++;
            }
          }
          else
          {
            lock (objd)
            {
              countDisConcordant1++;
            }
          }
          if (((dicAllAuthors[authorKeys[i]].curValue - dicAllAuthors[authorKeys[j]].curValue) *
            (dicAllAuthors[authorKeys[i]].NPC - dicAllAuthors[authorKeys[j]].NPC)) >= 0)
          {
            lock (objc2)
            {
              countConcordant2++;
            }
          }
          else
          {
            lock (objd2)
            {
              countDisConcordant2++;
            }
          }
          if (((dicAllAuthors[authorKeys[i]].curValue - dicAllAuthors[authorKeys[j]].curValue) *
           (dicAllAuthors[authorKeys[i]].hIndex - dicAllAuthors[authorKeys[j]].hIndex)) >= 0)
          {
            lock (objc3)
            {
              countConcordant3++;
            }
          }
          else
          {
            lock (objd3)
            {
              countDisConcordant3++;
            }
          }
        }
        );
        if (i % 100 == 0)
        {
          sw.Stop();
          Console.WriteLine("loop {0}: {1}", i, sw.Elapsed);
        }
      }
      long num = ((long)authorKeys.Count) * (authorKeys.Count - 1);
      Console.WriteLine("ncc:{0}-{1}-npc:{2}-{3}-h-index:{4}-{5}-num:{6}",
        countConcordant1,countDisConcordant1,
        countConcordant2, countDisConcordant2,
        countConcordant3,countDisConcordant3,
        num);
     
      k1 = 2.0 * (countConcordant1 - countDisConcordant1) / num;
      k2 = 2.0 * (countConcordant2 - countDisConcordant2) / num;
      k3 = 2.0 * (countConcordant3 - countDisConcordant3) / num;
    }
    public static double KendalTauAuthors()
    {
      object objc = new object();
      object objd = new object();
      int countConcordant = 0;
      int countDisConcordant = 0;
      List<int> authorKeys = dicAllAuthors.Keys.ToList();
      ParallelOptions po = new ParallelOptions()
      {
          MaxDegreeOfParallelism = Environment.ProcessorCount
      };
      Stopwatch sw = new Stopwatch();

      for (int i = 0; i < authorKeys.Count - 1; i++)
      {
        sw.Start();
        Parallel.For(i + 1, authorKeys.Count, po, j =>
        //for (int j = i + 1; j < authorKeys.Count; j++)
        {
          if (((dicAllAuthors[authorKeys[i]].curRank - dicAllAuthors[authorKeys[j]].curRank) *
            (dicAllAuthors[authorKeys[i]].oldRank - dicAllAuthors[authorKeys[j]].oldRank)) > 0)
          {
            lock (objc)
            {
              countConcordant++;
            }
          }
          if (((dicAllAuthors[authorKeys[i]].curRank - dicAllAuthors[authorKeys[j]].curRank) *
           (dicAllAuthors[authorKeys[i]].oldRank - dicAllAuthors[authorKeys[j]].oldRank)) < 0)
          {
            lock (objd)
            {
              countDisConcordant++;
            }
          }
        }
        );
        if (i % 100 == 0)
        {
          sw.Stop();
          Console.WriteLine("loop {0}: {1}", i, sw.Elapsed);
        }
      }
      return 2.0 * (countConcordant - countDisConcordant) / authorKeys.Count / (authorKeys.Count - 1);
    }
  }
}
