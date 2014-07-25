using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTierRankingKeyword
{
  public class Conference : IEquatable<Conference>
  {
    public int idConference
    {
      get;
      set;
    }
    public string conferenceName
    {
      get;
      set;
    }
    public double NCC
    {
      get;
      set;
    }
    public double NPC
    {
      get;
      set;
    }
    public int NAuthors
    {
      get;
      set;
    }
    public int yearStart
    {
      get;
      set;
    }
    public int yearEnd
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
    public double curRank
    {
      get;
      set;
    }
    public double oldRank
    {
      get;
      set;
    }
    public bool Equals(Conference other)
    {
      return other.idConference.Equals(this.idConference);
    }

    public override bool Equals(object obj)
    {
      return this.Equals(obj as Author);
    }

    public override int GetHashCode()
    {
      return idConference.GetHashCode();
    }
    public Conference(int id, string name, int yS, int yE, string urls)
    {
      idConference = id;
      conferenceName= name;
      yearStart= yS;
      yearEnd= yE;
      url = urls;
      curValue = 100;
      oldValue = 100;
      dicPapers = new Dictionary<int, Paper>();
    }
    public Conference(int id, string name, int count, double val, int nauthors)
    {
      idConference = id;
      conferenceName = name;
      NPC = count;
      NAuthors = nauthors;      
      curValue = val;
      oldValue = 100;
      dicPapers = new Dictionary<int, Paper>();
    }

    public Conference(int id, string name, double value, double NPC1, double NCC1)
    {
      // TODO: Complete member initialization
      this.idConference = id;
      this.conferenceName = name;
      this.curValue= value;
      this.NPC = NPC1;
      this.NCC = NCC1;
    }

    public static Conference FromLine(string line)
    {
      string[] datas = line.Split(',');
      Conference conference = null;
      try
      {
        int id = int.TryParse(datas[0].Trim('"'), out id) ? id : -1;
        string name = datas[1].Trim('"');
        int yS = int.TryParse(datas[6].Trim('"'), out yS) ? yS : -1;
        int yE = int.TryParse(datas[7].Trim('"'), out yE) ? yE : -1;
        string url = datas[8].Trim('"');
        conference = new Conference(id, name, yS, yE, url);
      }
      catch (Exception ex)
      {
        Console.WriteLine("Error {0} at line:{1}", ex.Message,line);
      }
      return conference;
    }
    public double SetValueFromPaper()
    {
      oldValue = curValue;
      curValue = 0;
      curValue = dicPapers.AsParallel().Sum(x => x.Value.curValue);      
      return curValue;
    }

    public static Conference FromResultLine(string line)
    {
      string[] datas = line.Split('\t');
      Conference conference = null;
      try
      {
        int id = int.TryParse(datas[0].Trim('"'), out id) ? id : -1;
        string name = datas[1].Trim('"');
        int count = int.TryParse(datas[2].Trim('"'), out count) ? count : -1;
        double value = double.TryParse(datas[3].Trim('"'), out value) ? value : -1;
        int authors = int.TryParse(datas[5].Trim('"'), out authors) ? authors : -1;
        conference = new Conference(id, name, count, value, authors);
      }
      catch (Exception ex)
      {
        Console.WriteLine("Error {0} at line:{1}", ex.Message, line);
      }
      return conference;
    }

    public static Conference FromResultLineNew(string line)
    {
      string[] datas = line.Split('\t');
      Conference conference = null;
      try
      {
        int id = int.TryParse(datas[0].Trim('"'), out id) ? id : -1;
        string name = datas[1].Trim('"');
        double value = double.TryParse(datas[2].Trim('"'), out value) ? value : -1;
        double NPC = double.TryParse(datas[3].Trim('"'), out NPC) ? NPC : -1;
        double NCC = double.TryParse(datas[4].Trim('"'), out NCC) ? NCC : -1;
        conference = new Conference(id, name, value, NPC, NCC);
      }
      catch (Exception ex)
      {
        Console.WriteLine("Error {0} at line:{1}", ex.Message, line);
      }
      return conference;
    }
  }
}
