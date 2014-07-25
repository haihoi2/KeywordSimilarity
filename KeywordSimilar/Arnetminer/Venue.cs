using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arnetminer
{
  public class Venue :IEquatable<Venue>
  {
    public static Dictionary<int, Venue> dicAllVenues = new Dictionary<int, Venue>();
   
    public int idVenue { get; set; }
    public string venueName { get; set; }
    public double curValue { get; set; }
    public double oldValue { get; set; }
    
    public Dictionary<int, Publication> dicPublications { get; set; }
    public List<int> liPublications { get; set; }
    
    public bool Equals(Venue other)
    {
      return other.idVenue.Equals(this.idVenue);
    }
    public override bool Equals(object other)
    {
      return this.Equals(other as Venue);
    }
    public override int GetHashCode()
    {
      return idVenue.GetHashCode();
    }
    public Venue()
    {
      this.idVenue = -1;
      this.venueName = "-";
      this.oldValue = curValue = 0;
      this.dicPublications = new Dictionary<int, Publication>();
      liPublications = new List<int>();
    }
    public Venue(int id, double value, string name, Dictionary<int, Publication> pubs)
    {
      idVenue = id;
      venueName = name;
      curValue = value;
      oldValue = 0;
      dicPublications = pubs;
      if (dicPublications.Count() > 0)
        liPublications = dicPublications.Keys.ToList();
      else
        liPublications = new List<int>();
    }
    public override string ToString()
    {
      return string.Format("{0}\t|{1}\t|{2}\t|{3}\t|{4}\r\n",
        idVenue, curValue, venueName, liPublications.Count,
        string.Join("|",liPublications.ToArray()));
    }
    public static Venue FromLine(string line)
    {
      string[] datas = line.Split('\t');
      Venue venue = null;
      try
      {
        int idvenue = Convert.ToInt32(datas[0].Trim('"', '|'));
        double value = double.TryParse(datas[1].Trim('"', '|'), out value) ? value : -1;
        string name = datas[2].Trim('"', '|');
        int pubCount = int.TryParse(datas[3].Trim('"', '|'), out pubCount) ? pubCount : -1;
        string[] iPubs = datas[4].Trim('"', '|').Split('|');
        venue = new Venue(idvenue, value, name, new Dictionary<int, Publication>());
        foreach (string ip in iPubs)
        {
          if (ip != "")
            venue.liPublications.Add(Convert.ToInt32(ip));
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine("Error {0} at line:{1}", ex.Message, line);
      }
      return venue;
    }

    public double SetValueFromPublication()
    {
      oldValue = curValue;
      curValue = liPublications.AsParallel().Sum(x => Publication.dicAllPubs[x].curValue);
      return curValue;
    }
  }
}
