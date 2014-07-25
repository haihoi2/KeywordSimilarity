using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeywordSimilar
{
  public class Link : IEquatable<Link>
  {
    public int idPaper{ get; set; }
    public int idPaperRef { get; set; }
    public double curValue { get; set; }
    public double oldValue { get; set; }
    public bool Equals(Link other)
    {
      return other.idPaper.Equals(this.idPaper) && other.idPaperRef.Equals(this.idPaperRef);
    }

    public override bool Equals(object obj)
    {
      return this.Equals(obj as Link);
    }
    public override int GetHashCode()
    {
      return (this.idPaper * this.idPaperRef).GetHashCode();
    }

    public Link(int idPaper, int idPaperRef, double value)
    {
      this.idPaper = idPaper;
      this.idPaperRef = idPaperRef;
      this.curValue = this.oldValue = value;
    }
    public double SetValueFromPapers(Dictionary<int, Paper> dicPapers)
    {
      oldValue = curValue;
      //IEnumerable<KeyValuePair<int, Paper>> inPapers =
      //  dicPapers.Where(x => x.Key == idPaper);      
      //curValue = inPapers.Sum(x => x.Value.curValue / x.Value.dicCitePaper.Count);      
      curValue = dicPapers[idPaper].curValue / dicPapers[idPaper].dicCitePaper.Count;
      return curValue;
    }
  }
}
