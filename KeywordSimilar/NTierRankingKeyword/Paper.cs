using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NTierRankingKeyword
{
  public class Paper : IEquatable<Paper>
  {
    public int idPaper
    {
      get;
      set;
    }
    public string doi
    {
      get;
      set;
    }
    public string title
    {
      get;
      set;
    }
    public int year
    {
      get;
      set;
    }
    public int conference
    {
      get;
      set;
    }
    public double curValue
    {
      get;
      set;
    }
    public double oldValue
    {
      get;
      set;
    }
    public Dictionary<int, Keyword> dicKeywords
    {
      get;
      set;
    }
    public Dictionary<int, Author> dicAuthors
    {
      get;
      set;
    }
    public Dictionary<int, Paper> dicCitePapers
    {
      get;
      set;
    }
    public Dictionary<int, Paper> dicRefPapers
    {
      get;
      set;
    }
    public bool Equals(Paper other)
    {
      return other.idPaper.Equals(this.idPaper);
    }

    public override bool Equals(object obj)
    {
      return this.Equals(obj as Paper);
    }

    public override int GetHashCode()
    {
      return idPaper.GetHashCode();
    }

    public Paper(int id, string doix, string titles, int y, int idConf)
    {
      idPaper = id;
      doi = doix;
      title = titles;
      conference = idConf;
      year = y;
      dicCitePapers = new Dictionary<int, Paper>();
      dicKeywords = new Dictionary<int, Keyword>();
      dicRefPapers = new Dictionary<int, Paper>();
      dicAuthors = new Dictionary<int, Author>();
      oldValue = 100;
      curValue = 100;
    }

    public static Paper FromLine(string line)
    {
      string[] datas = line.Split(',');
      Paper paper = null;
      try
      {
        int id = int.TryParse(datas[0].Trim('"'), out id) ? id : -1;
        string doi = datas[1].Trim('"');
        string title = datas[4].Trim('"');
        int year = int.TryParse(datas[8].Trim('"'), out year) ? year : -1;
        int conf = int.TryParse(datas[13].Trim('"'), out conf) ? conf : -1;
        paper = new Paper(id, doi, title, year, conf);
      }
      catch (Exception ex)
      {
        Debug.WriteLine("Error {0} at line:{1}", ex.Message, line);
        Console.WriteLine("Error {0} at line:{1}", ex.Message, line);
      }
      return paper;
    }

    public double SetValueFromAuthorConference(double a1, double a2, double a3, 
      Dictionary<int,Conference> dicAllConferences)
    {
      oldValue = curValue;
      double authorsum = dicAuthors.AsParallel().Sum(x => x.Value.curValue/ x.Value.dicPapers.Count);
      double conferencesum = dicAllConferences[conference].curValue / dicAllConferences[conference].dicPapers.Count;
      double citesum = dicRefPapers.Count > 0 ? dicRefPapers.AsParallel().Sum(x => x.Value.oldValue /
        x.Value.dicCitePapers.Count) : 0;
      curValue = a1 * authorsum + a2 * conferencesum + a3 * citesum;// +(1 - a1 - a2 - a3) * oldValue; 
      return curValue;
    }
  }
}
