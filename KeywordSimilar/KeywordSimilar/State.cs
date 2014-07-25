using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeywordSimilar
{
  public class State : IEquatable<State>
  {
    public int idKeyword { get; set; }
    public int idPaper { get; set; }
    //public Dictionary<KeyValuePair<int, int>, State> dicStateSameKeywords
    //{
    //  get;
    //  set;
    //}
    //public Dictionary<KeyValuePair<int, int>, State> dicStateSamePapers
    //{
    //  get;
    //  set;
    //}
    //public Dictionary<KeyValuePair<int, int>, State> dicStateFromCitations
    //{
    //  get;
    //  set;
    //}
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
    public List<Link> listLinks = new List<Link>();
    public bool Equals(State other)
    {
      return other.idPaper.Equals(this.idPaper) && other.idKeyword.Equals(this.idKeyword);
    }

    public override bool Equals(object obj)
    {
      return this.Equals(obj as State);
    }
    public override int GetHashCode()
    {
      return (this.idPaper * this.idKeyword).GetHashCode();
    }

    public State(int idKeyword, int idPaper, double value)
    {
      this.idKeyword = idKeyword;
      this.idPaper = idPaper;
      this.curValue = value;
      this.oldValue = value;
      //dicStateSameKeywords = new Dictionary<KeyValuePair<int, int>, State>();
      //dicStateSamePapers = new Dictionary<KeyValuePair<int, int>, State>();
      //dicStateFromCitations = new Dictionary<KeyValuePair<int, int>, State>();
    }
    public double SetValue(Dictionary<int, Keyword> dicKeywords, 
      Dictionary<int,Paper> dicPapers, 
      Dictionary<KeyValuePair<int,int>,Link> dicLinks, double[] alpha)
    {
      oldValue = curValue;
      curValue = alpha[0] * dicPapers[idPaper].curValue / dicPapers[idPaper].dicKeywords.Count();
      curValue += alpha[1] * dicKeywords[idKeyword].curValue / dicKeywords[idKeyword].dicPapers.Count();
      double sumlink = 
          dicLinks.Where(x => x.Value.idPaperRef == idPaper).Sum(x => x.Value.curValue);
      curValue += alpha[2] * sumlink / dicPapers[idPaper].dicKeywords.Count;
      //curValue += alpha[3] * oldValue;
      return curValue;
    }
    public double SetValue(Dictionary<int, Keyword> dicKeywords,
      Dictionary<int, Paper> dicPapers, double[] alpha)
    {
      oldValue = curValue;
      curValue = alpha[0] * dicPapers[idPaper].curValue / dicPapers[idPaper].dicKeywords.Count();
      curValue += alpha[1] * dicKeywords[idKeyword].curValue / dicKeywords[idKeyword].dicPapers.Count();
      double sumlink =  listLinks.Sum(x => x.curValue);
      curValue += alpha[2] * sumlink / dicPapers[idPaper].dicKeywords.Count;
      //curValue += alpha[3] * oldValue;
      return curValue;
    }
  }
}
