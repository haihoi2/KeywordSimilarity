using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTierRankingKeyword
{
  public class Author : IEquatable<Author>
  {
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
   
    public int NConfs
    {
      get;
      set;
    }

    public int idAuthor
    {
      get;
      set;
    }
    public string authorName
    {
      get;
      set;
    }
    public int idOrg
    {
      get;
      set;
    }
    public double hIndex
    {
      get;
      set;
    }
    public int gIndex
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
    public bool Equals(Author other)
    {
      return other.idAuthor.Equals(this.idAuthor);
    }

    public override bool Equals(object obj)
    {
      return this.Equals(obj as Author);
    }

    public override int GetHashCode()
    {
      return idAuthor.GetHashCode();
    }
    public Author(int id, string name, int idO, int hI, int gI, string urls)
    {
      idAuthor = id;
      authorName = name;
      idOrg = idO;
      hIndex = hI;
      gIndex = gI;
      url = urls;
      curValue = 100;
      oldValue = 100;
      dicPapers = new Dictionary<int, Paper>();
    }

    public Author(int id, string name, int NPC, double value, int NConfs)
    {
      // TODO: Complete member initialization
      this.idAuthor = id;
      this.authorName = name;
      this.NPC = NPC;
      this.curValue = value;
      this.NConfs = NConfs;
    }
    public Author(int id, string name, double NPC, double NCC, double Hindex, double value)
    {
      // TODO: Complete member initialization
      this.idAuthor = id;
      this.authorName = name;
      this.NPC = NPC;
      this.NCC = NCC;
      this.curValue = value;
      this.hIndex = Hindex;
;
    }
    public double SetValueFromPaper()
    {
      oldValue= curValue;
      curValue= dicPapers.AsParallel().Sum(x => x.Value.curValue / x.Value.dicAuthors.Count);
      return curValue;
    }
    public static Author FromLine(string line)
    {
      string[] datas = line.Split(',');      
      Author author = null;
      try
      {
        int id = int.TryParse(datas[0].Trim('"'),out id) ? id : -1;
        string name = datas[1].Trim('"');
        int idO = int.TryParse(datas[5].Trim('"'), out idO) ? idO : -1 ;
        int hI = int.TryParse(datas[6].Trim('"'), out hI) ? hI : -1;
        int gI = int.TryParse(datas[7].Trim('"'), out gI) ? gI : -1;
        string url = datas[8].Trim('"');
        author = new Author(id, name,idO, hI, gI, url);
      }
      catch (Exception ex)
      {
        Console.WriteLine("Error {0} at line:{1}", ex.Message,line);
      }
      return author;
    }

    public static Author FromResultLine(string line)
    {
      string[] datas = line.Split('\t');
      Author author = null;
      try
      {
        int id = int.TryParse(datas[0].Trim('"'), out id) ? id : -1;
        string name = datas[1].Trim('|');
        int NPC = int.TryParse(datas[2].Trim('"'), out NPC) ? NPC : -1;
        double value = double.TryParse(datas[3].Trim('"'), out value) ? value : -1;
        int NConfs = int.TryParse(datas[6].Trim('"'), out NConfs) ? NConfs : -1;        
        author = new Author(id, name, NPC, value, NConfs);
      }
      catch (Exception ex)
      {
        Console.WriteLine("Error {0} at line:{1}", ex.Message, line);
      }
      return author;
    }
    public static Author FromResultLineNew(string line)
    {
      string[] datas = line.Split('\t');
      Author author = null;
      try
      {
        int id = int.TryParse(datas[0].Trim('"'), out id) ? id : -1;
        string name = datas[1].Trim('|');
        double NPC = double.TryParse(datas[2].Trim('"'), out NPC) ? NPC : -1;
        double NCC = double.TryParse(datas[3].Trim('"'), out NCC) ? NPC : -1;
        double value = double.TryParse(datas[5].Trim('"'), out value) ? value : -1;
        double hindex = double.TryParse(datas[4].Trim('"'), out hindex) ? hindex : -1;
        author = new Author(id, name, NPC, NCC,hindex,value);
      }
      catch (Exception ex)
      {
        Console.WriteLine("Error {0} at line:{1}", ex.Message, line);
      }
      return author;
    }
  }
}
