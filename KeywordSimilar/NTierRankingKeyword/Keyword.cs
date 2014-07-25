using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTierRankingKeyword
{
  public class Keyword : IEquatable<Keyword>
  {
    public int idKeyword
    {
      get;
      set;
    }
    public string keyword
    {
      get;
      set;
    }
    public string stemmingVariations
    {
      get;
      set;
    }
    public string url
    {
      get;
      set;
    }
    public Dictionary<int, Paper> dicPapers
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
    public bool Equals(Keyword other)
    {
      return other.idKeyword.Equals(this.idKeyword);
    }

    public override bool Equals(object obj)
    {
      return this.Equals(obj as Keyword);
    }

    public override int GetHashCode()
    {
      return idKeyword.GetHashCode();
    }
    public Keyword(int id, string key, string stem, string urls)
    {
      idKeyword = id;
      keyword = key;
      stemmingVariations = stem;
      url = urls;
      curValue = 100;
      oldValue = 0;
      dicPapers = new Dictionary<int, Paper>();
    }

    public static Keyword FromLine(string line)
    {
      string[] datas = line.Split(',');      
      Keyword keyword = null;
      try
      {
        int id=Convert.ToInt32(datas[0].Trim('"'));
        string key = datas[1].Trim('"');
        string stem = datas[2].Trim('"');
        string url = datas[3].Trim('"');
        keyword = new Keyword(id,key,stem,url);
      }
      catch (Exception ex)
      {
        Console.WriteLine("Error {0} at line:{1}", ex.Message,line);
      }
      return keyword;
    }
  }
}
