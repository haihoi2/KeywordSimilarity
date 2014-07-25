using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeywordSimilar
{
  public class Paper : IEquatable<Paper>
  {
    public double PageRank { get; set; }
    public double refCount { get; set; }
    public double keywordCount { get; set; }
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
    public string strabstract
    {
      get;
      set;
    }
    public double oldValue
    {
      get;
      set;
    }
    public double curValue
    {
      get;
      set;
    }
    public Dictionary<int, Keyword> dicKeywords
    {
      get;
      set;
    }
    public Dictionary<int, Paper> dicCitePaper
    {
      get;
      set;
    }
    public Dictionary<int, Paper> dicRefPaper
    {
      get;
      set;
    }
    public Dictionary<int, State> dicState
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

    public Paper(int id, string doix, string titles)
    {
      idPaper = id;
      doi = doix;
      title = titles;
      dicCitePaper = new Dictionary<int, Paper>();
      dicKeywords = new Dictionary<int, Keyword>();
      dicRefPaper = new Dictionary<int, Paper>();
      dicState = new Dictionary<int, State>();
      curValue = oldValue = 100;
    }
    public Paper(int id, string doix, string titles, string strabstracts)
    {
      idPaper = id;
      doi = doix;
      title = titles;
      strabstract = strabstracts;
      dicCitePaper = new Dictionary<int, Paper>();
      dicKeywords = new Dictionary<int, Keyword>();
      dicRefPaper = new Dictionary<int, Paper>();
      dicState = new Dictionary<int, State>();
      curValue = oldValue = 100;
    }

    public static Paper FromLine(string line)
    {
      string[] datas = line.Split(',','#');
      Paper paper = null;
      try
      {
        int id = Convert.ToInt32(datas[0].Trim('"'));
        string doi = datas[1].Trim('"');
        string title = datas[4].Trim('"');
        string strabstract = datas[5].Trim('"');
        paper = new Paper(id, doi, title,strabstract);
      }
      catch (Exception ex)
      {
        Console.WriteLine("Error {0} at line:{1}", ex.Message, line);
      }
      return paper;
    }
    public static Paper FromStemmingLine(string line, char split)
    {
      string[] datas = line.Split(split);
      Paper paper = null;
      try
      {
        int id = Convert.ToInt32(datas[0].Trim('"'));
        string doi = datas[1].Trim('"');
        string title = datas[4].Trim('"');
        string strabstract = datas[5].Trim('"');
        paper = new Paper(id, doi, title, strabstract);
      }
      catch (Exception ex)
      {
        Console.WriteLine("Error {0} at line:{1}", ex.Message, line);
      }
      return paper;
    }
    public double SetValueFromStates(Dictionary<KeyValuePair<int, int>, State> dicStates)
    {
      oldValue = curValue;
      curValue = dicStates.Where(x => x.Key.Value == idPaper).Sum(x => x.Value.curValue);
      return curValue;
    }
    public double SetValueFromStates()
    {
      oldValue = curValue;
      curValue = dicState.Values.Sum(x => x.curValue);
      return curValue;
    }

    
  }
}
