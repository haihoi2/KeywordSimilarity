using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arnetminer
{
  public class Citation : IEquatable<Citation>
  {
    public static Dictionary<Tuple<int, int>, Citation> dicAllCitations = new Dictionary<Tuple<int, int>, Citation>();
    public int idArnetFrom { get; set; }
    public int idArnetTo { get; set; }
    public int idDBLPFrom { get; set; }
    public int idDBLPTo { get; set; }
    public double curValue { get; set; }
    public double oldValue { get; set; }

    public bool Equals(Citation other)
    {
      return other.idArnetFrom.Equals(this.idArnetFrom) && other.idArnetTo.Equals(this.idArnetTo);
    }
    public override string ToString()
    {
      return string.Format("{0}\t{1}", idArnetFrom, idArnetTo);
    }
  }
}
