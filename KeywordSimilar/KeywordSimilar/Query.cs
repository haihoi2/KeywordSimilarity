using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeywordSimilar
{
  public class Query1 : IEquatable<Query1>
  {
    public double kenDalUKP_LKP { get; set; }
    public long kenDalUKP_LKP_concord { get; set; }
    public long kenDalUKP_LKP_discord { get; set; }
    public double kenDalUKP_Pagerank { get; set; }
    public long kenDalUKP_Pagerank_concord { get; set; }
    public long kenDalUKP_Pagerank_discord { get; set; }
    public double kenDalpagerank_LKP { get; set; }
    public long kenDalpagerank_LKP_concord { get; set; }
    public long kenDalpagerank_LKP_discord { get; set; }
    public double kenDalpagerank_RCM { get; set; }
    public long kenDalpagerank_RCM_concord { get; set; }
    public long kenDalpagerank_RCM_discord { get; set; }
    public double kenDalLKP_RCM { get; set; }
    public long kenDalLKP_RCM_concord { get; set; }
    public long kenDalLKP_RCM_discord { get; set; }
    public int keyword { get; set; }
    public Dictionary<int, double> dicResultsLKP { get; set; }
    public Dictionary<int, double> dicResultsUKP { get; set; }
    public Dictionary<int, double> dicResultsPagerank { get; set; }
    public Dictionary<int, double> dicResultsRCM { get; set; }
    public Query1(int k1)
    {
      keyword = k1;
      dicResultsLKP = new Dictionary<int, double>();
      dicResultsUKP = new Dictionary<int, double>();
      dicResultsPagerank = new Dictionary<int, double>();
      dicResultsRCM = new Dictionary<int, double>();
    }
    
    public bool Equals(Query1 q)
    {
      return keyword == q.keyword;
    }
    public override bool Equals(object obj)
    {
      return this.Equals(obj as Query1);
    }
    public override int GetHashCode()
    {
      return keyword.GetHashCode();
    }
    public override string ToString()
    {
      return string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}\t{11}\t{12}\t{13}\t{14}\t{15}\t{16}\r\n",
        keyword, dicResultsLKP.Count,
        kenDalUKP_Pagerank, kenDalUKP_LKP, kenDalpagerank_LKP,
        kenDalLKP_RCM, kenDalpagerank_RCM,
        kenDalUKP_Pagerank_concord, kenDalUKP_Pagerank_discord,
        kenDalUKP_LKP_concord, kenDalUKP_LKP_discord,
        kenDalpagerank_LKP_concord, kenDalpagerank_LKP_discord,
        kenDalLKP_RCM_concord, kenDalLKP_RCM_discord,
        kenDalpagerank_RCM_concord, kenDalpagerank_RCM_discord);
    }
  }
  public class Query2: IEquatable<Query2>
  {
    public double kenDalUKP_LKP { get; set; }
    public long kenDalUKP_LKP_concord { get; set; }
    public long kenDalUKP_LKP_discord { get; set; }
    public double kenDalUKP_Pagerank { get; set; }
    public long kenDalUKP_Pagerank_concord { get; set; }
    public long kenDalUKP_Pagerank_discord { get; set; }
    public double kenDalpagerank_LKP { get; set; }
    public long kenDalpagerank_LKP_concord { get; set; }
    public long kenDalpagerank_LKP_discord { get; set; }
    public double kenDalpagerank_RCM { get; set; }
    public long kenDalpagerank_RCM_concord { get; set; }
    public long kenDalpagerank_RCM_discord { get; set; }
    public double kenDalLKP_RCM { get; set; }
    public long kenDalLKP_RCM_concord { get; set; }
    public long kenDalLKP_RCM_discord { get; set; }
    public Tuple<int, int> keywords {get;set;}
    public Dictionary<int, double> dicResultsLKP { get; set; }
    public Dictionary<int, double> dicResultsUKP { get; set; }
    public Dictionary<int, double> dicResultsPagerank { get; set; }
    public Dictionary<int, double> dicResultsRCM { get; set; }
    public Query2(int k1, int k2)
    {
      keywords = Tuple.Create(k1, k2);
      dicResultsLKP = new Dictionary<int, double>();
      dicResultsUKP = new Dictionary<int, double>();
      dicResultsPagerank = new Dictionary<int, double>();
      dicResultsRCM = new Dictionary<int, double>();
    }
    static public Query2 ReadFromLine(string line)
    {
      Query2 q = null;
      try{
      string[] datas= line.Split('\t',',');
      int k1 = Convert.ToInt32(datas[0].Trim('"'));
      int k2 = Convert.ToInt32(datas[1].Trim('"'));
      int p = Convert.ToInt32(datas[2].Trim('"'));
      double lkp= Convert.ToDouble(datas[3].Trim('"'));
      double pagerank = Convert.ToDouble(datas[5].Trim('"'));
      double ukp = Convert.ToDouble(datas[6].Trim('"'));
      q = new Query2(k1,k2);
      q.dicResultsLKP.Add(p, lkp);
      q.dicResultsUKP.Add(p, ukp);
      q.dicResultsPagerank.Add(p, pagerank);
      }
      catch (Exception ex)
      {
        Console.WriteLine("Error {0} at line:{1}", ex.Message, line);
      }
      return q;
    }
    public bool Equals(Query2 q)
    {
      return keywords == q.keywords;
    }
    public override bool Equals(object obj)
    {
      return this.Equals(obj as Query2);
    }
    public override int GetHashCode()
    {
      return keywords.GetHashCode();
    }
    public override string ToString()
    {
      return string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}\t{11}\t{12}\t{13}\t{14}\t{15}\t{16}\t{17}\r\n", 
        keywords.Item1, keywords.Item2, dicResultsLKP.Count,
        kenDalUKP_Pagerank, kenDalUKP_LKP, kenDalpagerank_LKP,
        kenDalLKP_RCM, kenDalpagerank_RCM,
        kenDalUKP_Pagerank_concord, kenDalUKP_Pagerank_discord,
        kenDalUKP_LKP_concord, kenDalUKP_LKP_discord,
        kenDalpagerank_LKP_concord, kenDalpagerank_LKP_discord,
        kenDalLKP_RCM_concord, kenDalLKP_RCM_discord,
        kenDalpagerank_RCM_concord, kenDalpagerank_RCM_discord);
    }    
  }
  public class Query3 : IEquatable<Query3>
  {
    public double kenDalUKP_LKP { get; set; }
    public long kenDalUKP_LKP_concord { get; set; }
    public long kenDalUKP_LKP_discord { get; set; }
    public double kenDalUKP_Pagerank { get; set; }
    public long kenDalUKP_Pagerank_concord { get; set; }
    public long kenDalUKP_Pagerank_discord { get; set; }
    public double kenDalpagerank_LKP { get; set; }
    public long kenDalpagerank_LKP_concord { get; set; }
    public long kenDalpagerank_LKP_discord { get; set; }
    public double kenDalpagerank_RCM { get; set; }
    public long kenDalpagerank_RCM_concord { get; set; }
    public long kenDalpagerank_RCM_discord { get; set; }
    public double kenDalLKP_RCM { get; set; }
    public long kenDalLKP_RCM_concord { get; set; }
    public long kenDalLKP_RCM_discord { get; set; }
    public Tuple<int, int,int> keywords { get; set; }
    public Dictionary<int, double> dicResultsLKP { get; set; }
    public Dictionary<int, double> dicResultsUKP { get; set; }
    public Dictionary<int, double> dicResultsPagerank { get; set; }
    public Dictionary<int, double> dicResultsRCM { get; set; }
    public Query3(int k1, int k2, int k3)
    {
      keywords = Tuple.Create(k1, k2, k3);
      dicResultsLKP = new Dictionary<int, double>();
      dicResultsUKP = new Dictionary<int, double>();
      dicResultsPagerank = new Dictionary<int, double>();
      dicResultsRCM = new Dictionary<int, double>();
    }
    
    public bool Equals(Query3 q)
    {
      return keywords == q.keywords;
    }
    public override bool Equals(object obj)
    {
      return this.Equals(obj as Query3);
    }
    public override int GetHashCode()
    {
      return keywords.GetHashCode();
    }
    public override string ToString()
    {
      return string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}\t{11}\t{12}\t{13}\t{14}\t{15}\t{16}\t{17}\t{18}\r\n",
        keywords.Item1, keywords.Item2, keywords.Item3, dicResultsLKP.Count,
        kenDalUKP_Pagerank, kenDalUKP_LKP, kenDalpagerank_LKP,
        kenDalLKP_RCM, kenDalpagerank_RCM,
        kenDalUKP_Pagerank_concord, kenDalUKP_Pagerank_discord,
        kenDalUKP_LKP_concord, kenDalUKP_LKP_discord,
        kenDalpagerank_LKP_concord, kenDalpagerank_LKP_discord,
        kenDalLKP_RCM_concord, kenDalLKP_RCM_discord,
        kenDalpagerank_RCM_concord, kenDalpagerank_RCM_discord);
    }
  }

  public class Query4 : IEquatable<Query4>
  {
    public double kenDalUKP_LKP { get; set; }
    public long kenDalUKP_LKP_concord { get; set; }
    public long kenDalUKP_LKP_discord { get; set; }
    public double kenDalUKP_Pagerank { get; set; }
    public long kenDalUKP_Pagerank_concord { get; set; }
    public long kenDalUKP_Pagerank_discord { get; set; }
    public double kenDalpagerank_LKP { get; set; }
    public long kenDalpagerank_LKP_concord { get; set; }
    public long kenDalpagerank_LKP_discord { get; set; }
    public double kenDalpagerank_RCM { get; set; }
    public long kenDalpagerank_RCM_concord { get; set; }
    public long kenDalpagerank_RCM_discord { get; set; }
    public double kenDalLKP_RCM { get; set; }
    public long kenDalLKP_RCM_concord { get; set; }
    public long kenDalLKP_RCM_discord { get; set; }
    public Tuple<int, int, int,int> keywords { get; set; }
    public Dictionary<int, double> dicResultsLKP { get; set; }
    public Dictionary<int, double> dicResultsUKP { get; set; }
    public Dictionary<int, double> dicResultsPagerank { get; set; }
    public Dictionary<int, double> dicResultsRCM { get; set; }
    public Query4(int k1, int k2, int k3, int k4)
    {
      keywords = Tuple.Create(k1, k2, k3,k4);
      dicResultsLKP = new Dictionary<int, double>();
      dicResultsUKP = new Dictionary<int, double>();
      dicResultsPagerank = new Dictionary<int, double>();
      dicResultsRCM = new Dictionary<int, double>();
    }

    public bool Equals(Query4 q)
    {
      return keywords == q.keywords;
    }
    public override bool Equals(object obj)
    {
      return this.Equals(obj as Query4);
    }
    public override int GetHashCode()
    {
      return keywords.GetHashCode();
    }
    public override string ToString()
    {
      return string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}\t{11}\t{12}\t{13}\t{14}\t{15}\t{16}\t{17}\t{18}\t{19}\r\n",
        keywords.Item1, keywords.Item2, keywords.Item3, keywords.Item4, dicResultsLKP.Count,
        kenDalUKP_Pagerank, kenDalUKP_LKP, kenDalpagerank_LKP,
        kenDalLKP_RCM, kenDalpagerank_RCM,
        kenDalUKP_Pagerank_concord, kenDalUKP_Pagerank_discord,
        kenDalUKP_LKP_concord, kenDalUKP_LKP_discord,
        kenDalpagerank_LKP_concord, kenDalpagerank_LKP_discord,
        kenDalLKP_RCM_concord, kenDalLKP_RCM_discord,
        kenDalpagerank_RCM_concord, kenDalpagerank_RCM_discord);
    }
  }
}
