using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arnetminer
{
  public class Publication : IEquatable<Publication>
  {
    static public Dictionary<int, Publication> dicAllPubs = new Dictionary<int, Publication>();
    public int idPaper { get; set; }
    public string title { get; set; }
    public string[] strAuthors { get; set; }
    public List<int> liAuthors { get; set; }
    public Dictionary<int,Author> dicAuthors { get; set; }

    public int year { get; set; }
    public string venue { get; set; }
    public int idVenue { get; set; }
    public int citationnumber { get; set; }
    public List<int> liCitationIds { get; set; }
    public Dictionary<int, Publication> dicCitationPubs { get; set; }
    public int index { get; set; }
    public int arnetid { get; set; }
    public string strAbstract { get; set; }
    public List<int> liReferenceIds { get; set; }
    public Dictionary<int,Publication> dicReferencePubs {  get; set; }

    public double oldValue { get; set; }
    public double curValue { get; set; }

    public bool Equals(Publication other)
    {
      return other.idPaper.Equals(this.idPaper);
    }

    public override bool Equals(object obj)
    {
      return this.Equals(obj as Publication);
    }

    public override int GetHashCode()
    {
      return idPaper.GetHashCode();
    }
    public Publication()
    {
      this.idPaper = -1;
      this.title = "";
      this.strAuthors = null;
      this.year = 0;
      this.venue = "";
      this.citationnumber = 0;
      this.index = -1;
      this.arnetid = -1;
      this.strAbstract = "";
      this.liReferenceIds = null;
      this.curValue = oldValue = 0;
      dicAuthors = new Dictionary<int, Author>();
      liAuthors = new List<int>();      
    }

    public Publication(int idPaper, double curvalue, string titles, string[] authors,
      int year, string conference, int citationnumber, int index,
      int arnetid, string strAbstract, List<int> referenceids)
    {
      this.idPaper = idPaper;
      this.title = titles;
      this.strAuthors = authors;
      this.year = year;
      this.venue = conference;
      this.citationnumber = citationnumber;
      this.index = index;
      this.arnetid = arnetid;
      this.strAbstract = strAbstract;
      this.liReferenceIds = referenceids;
      this.curValue = curvalue;
      this.oldValue = 0;
      dicAuthors = new Dictionary<int, Author>();
      liAuthors = new List<int>();

    }
    
    static public Publication ReadFromLine(string line, bool withtitle)
    {
      string[] datas = line.Split('\t');
      Publication pub = new Publication()
      {
        idPaper = Convert.ToInt32(datas[0].Trim('|')),
        curValue = Convert.ToDouble(datas[1].Trim('|')),
        title = (withtitle ? datas[2].Trim('|') : ""),
        strAuthors = datas[3].Trim('|').Split('|'),
        liAuthors = new List<int>(),
        liCitationIds = new List<int>(),
        year = Convert.ToInt32(datas[4].Trim('|')),
        venue = datas[5].Trim('|'),
        citationnumber = Convert.ToInt32(datas[6].Trim('|')),
        index = Convert.ToInt32(datas[7].Trim('|')),
        arnetid = Convert.ToInt32(datas[8].Trim('|')),
        //strAbstract = datas[9].Trim('|'),
        liReferenceIds = new List<int>()
      };
      pub.venue = (pub.venue == string.Empty ? "NoName" : pub.venue);
      Debug.Assert(pub.venue != string.Empty);
      string[] strRefIds = datas[10].Trim('|').Split('|');
      if (strRefIds.Length > 0)
      {
        foreach (string ids in strRefIds)
        {
          if (ids != "")
            pub.liReferenceIds.Add(Convert.ToInt32(ids));
        }
      }
      return pub;
    }
    static public Publication FromLine(string line)
    {
      string[] datas = line.Split('\t');
      Publication pub = null;
      try
      {
        pub = new Publication()
         {
           idPaper = Convert.ToInt32(datas[0].Trim('|')),
           curValue = Convert.ToDouble(datas[1].Trim('|')),
           title = "",//datas[2]
           strAuthors = datas[3].Trim('|').Split('|'),
           liAuthors = new List<int>(),  //datas[3]        
           year = Convert.ToInt32(datas[4].Trim('|')),
           venue = datas[5].Trim('|'),
           citationnumber = Convert.ToInt32(datas[6].Trim('|')),
           index = Convert.ToInt32(datas[7].Trim('|')),
           arnetid = Convert.ToInt32(datas[8].Trim('|')),
           //strAbstract = datas[9].Trim('|'),
           liReferenceIds = new List<int>(), //datas[10]
           liCitationIds = new List<int>(),//datas[11]
           idVenue = Convert.ToInt32(datas[12].Trim('|')),
         };
        pub.venue = (pub.venue == string.Empty ? "NoName" : pub.venue);
        Debug.Assert(pub.venue != string.Empty);
        string[] strRefIds = datas[10].Trim('|').Split('|');
        if (strRefIds.Length > 0)
        {
          foreach (string ids in strRefIds)
          {
            if (ids !=string.Empty)
            pub.liReferenceIds.Add(Convert.ToInt32(ids));
          }
        }
        if (pub.strAuthors.Length > 0)
        {
          foreach (string ida in pub.strAuthors)
          {
            if (ida != string.Empty)
              pub.liAuthors.Add(Convert.ToInt32(ida));
          }
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine("Error {0} at line:{1}", ex.Message, line);
      }
      return pub;
    }
    static public Publication FromHashCodeLine(string line, bool isDBLP)
    {
      
      Publication pub = null;
      try
      {
        if (isDBLP)
        {
          string[] datas = line.Split('\t');
          pub = new Publication()
          {
            idPaper = Convert.ToInt32(datas[0].Trim('|')),
            title = datas[1].Trim('|'),
            index = Convert.ToInt32(datas[2].Trim('|')),
            //liReferenceIds = new List<int>(),
            //liCitationIds = new List<int>()
          };
        }
        else
        {
          string[] datas = line.Split('\t');
          pub = new Publication()
          {
            idPaper = Convert.ToInt32(datas[0].Trim('|')),
            title = datas[1].Trim('|'),
            arnetid = Convert.ToInt32(datas[2].Trim('|')),
            year = Convert.ToInt32(datas[3].Trim('|')),
            //venue = datas[4],
            //liReferenceIds = new List<int>(),
            //liCitationIds = new List<int>()
          };
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine("Error {0} at line:{1}", ex.Message, line);
      }
      return pub;
    }
    static public Publication FromHashCodeLine(string line, bool isDBLP, bool notMapping)
    {

      Publication pub = null;
      try
      {
        if (isDBLP)
        {
          string[] datas = line.Split('~');
          pub = new Publication()
          {
            idPaper = Convert.ToInt32(datas[0]),
            title = datas[1],
            index = Convert.ToInt32(datas[2]),
            //liReferenceIds = new List<int>(),
            //liCitationIds = new List<int>()
          };
        }
        else if (notMapping)
        {
          string[] datas = line.Split('\t');
          int value= Convert.ToInt32(datas[4].Trim('|'));
          if (value == -100)
          {
            pub = new Publication()
            {
              idPaper = Convert.ToInt32(datas[0].Trim('|')),
              title = datas[1].Trim('|'),
              arnetid = Convert.ToInt32(datas[2].Trim('|')),
              year = Convert.ToInt32(datas[3].Trim('|')),
              //venue = datas[4],
              //liReferenceIds = new List<int>(),
              //liCitationIds = new List<int>()
            };
          }
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine("Error {0} at line:{1}", ex.Message, line);
      }
      return pub;
    }
    public override string ToString()
    {
      return string.Format("{0}\t|{1}\t|{2}\t|{3}\t|{4}\t|{5}\t|{6}\t|{7}\t|{8}\t|{9}\t|{10}\t|{11}\t|{12}\r\n",
        this.idPaper, this.curValue, this.title, string.Join("|", this.liAuthors),
        this.year, this.venue, this.citationnumber,
        this.index, this.arnetid, this.strAbstract, 
        string.Join("|", this.liReferenceIds),
        string.Join("|", this.liCitationIds), this.idVenue);
    }
    public string ToStringSimple()
    {
      return string.Format("{0}\t|{1}\t|{2}\t|{3}\t|{4}\t{5}",
        this.idPaper, this.title, this.title.GetHashCode().ToString(),
        this.year, (this.venue == "" ? "NoName" : this.venue),curValue);
      //return string.Format("{0}\t|{1}\t|{2}\t|{3}\t|{4}\t|{5}\t|{6}\t|{7}\t|{8}\r\n",
      //  this.idPaper, this.curValue,
      //  this.liAuthors.Count,string.Join("|", this.liAuthors),
      //  this.year, (this.venue==""?"NoName":this.venue), 
      //  this.citationnumber,
      //  this.liReferenceIds.Count(), string.Join("|", this.liReferenceIds));
    }
    public string ToStringHashcode()
    {
      return string.Format("{0}\t|{1}\t|{2}\t|{3}\t|{4}\t|{5}\r\n",
        this.idPaper, this.title, this.title.GetHashCode().ToString(),
        this.year, (this.venue == "" ? "NoName" : this.venue),curValue);
    }



    public double SetValueFromAuthorVenue(double a1, double a2, double a3)
    {
      oldValue = curValue;
      double authorsum = liAuthors.AsParallel().Sum(x => Author.dicAllAuthors[x].curValue / Author.dicAllAuthors[x].liPublications.Count);
      double conferencesum = Venue.dicAllVenues[idVenue].curValue / Venue.dicAllVenues[idVenue].liPublications.Count;
      double citesum = liCitationIds.Count > 0 ? liCitationIds.AsParallel().Sum(
        x => dicAllPubs[x].oldValue / dicAllPubs[x].liReferenceIds.Count) : 0;
      curValue = a1 * authorsum + a2 * conferencesum + a3 * citesum;// +(1 - a1 - a2 - a3) * oldValue; 
      return curValue;
    }
  }
}
