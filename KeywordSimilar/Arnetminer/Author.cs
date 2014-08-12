using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arnetminer
{
  public class Author : IEquatable<Author>
  {
    public static Dictionary<int, Author> dicAllAuthors = new Dictionary<int, Author>();
    public int idAuthor { get; set; }
    public int idDBLPCCode { get; set; }
    public string authorName { get; set; }
    public List<int> liAlias { get; set; }
    public List<string> aliasNames { get; set; }
    public double curValue { get; set; }
    public double oldValue { get; set; }
    public List<int> liPublications { get; set; }
    public Dictionary<int,Publication> dicPublications { get; set; }
    
    public bool Equals(Author other)
    {
      return other.idAuthor.Equals(this.idAuthor) || 
        other.aliasNames.Contains(this.authorName) ||
        this.aliasNames.Contains(other.authorName);
    }

    public override bool Equals(object obj)
    {
      return this.Equals(obj as Author);
    }

    public override int GetHashCode()
    {
      return idAuthor.GetHashCode();
    }

    public Author()
    {
      idAuthor = -1;
      authorName = "-";
      aliasNames = new List<string>();
      oldValue = curValue = 0;
      dicPublications = new Dictionary<int, Publication>();
      liPublications = new List<int>();
    }

    public Author(int idauthor, double curvalue, string name, Dictionary<int, Publication> publications,
      Dictionary<int, Venue> venues)
    {
      idAuthor = idauthor;
      authorName = name;
      aliasNames = new List<string>();
      oldValue = 0;
      curValue = curvalue;
      dicPublications = publications;      
      if (dicPublications.Count() > 0)
        liPublications = dicPublications.Keys.ToList();
      else
        liPublications = new List<int>();
    }
    public override string ToString()
    {
      return string.Format("{0}\t|{1}\t|{2}\t|{3}\t|{4}\t|{5}",
        idAuthor, curValue, authorName, liPublications.Count,
        string.Join("|", liPublications.ToArray()), string.Join("|", liAlias.ToArray()));
    }
    public string ToStringForMapping()
    {
      return string.Format("{0}\t|{1}\t|{2}\t|{3}\t|{4}",
        idAuthor, authorName, authorName.GetHashCode(),curValue,idDBLPCCode);
    }
    public string ToStringWithAlias()
    {
      return string.Format("{0}\t|{1}\t|{2}\t|{3}\t|{4}\t|{5}",
        idAuthor, authorName, authorName.GetHashCode(), curValue, idDBLPCCode,
        string.Join("|", liAlias.ToArray()));
    }

    public static Author FromLine(string line)
    {
      string[] datas = line.Split('\t');
      Author author = null;
      try
      {
        int id = Convert.ToInt32(datas[0].Trim('"', '|'));
        string name = datas[2].Trim('"');
        double value = double.TryParse(datas[1].Trim('"', '|'), out value) ? value : -1;
        int pubCount = int.TryParse(datas[3].Trim('"','|'), out pubCount) ? pubCount : -1;
        string[] iPubs = datas[4].Trim('"','|').Split('|');
        
        author = new Author(id,value,name, new Dictionary<int, Publication>(), new Dictionary<int,Venue>());
        foreach(string ip in iPubs){
          if (ip!="")
            author.liPublications.Add(Convert.ToInt32(ip));          
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine("Error {0} at line:{1}", ex.Message, line);
      }
      return author;
    }



    public double SetValueFromPublication()
    {
      oldValue = curValue;
      curValue = liPublications.AsParallel().Sum(x => Publication.dicAllPubs[x].curValue / Publication.dicAllPubs[x].liAuthors.Count);
      return curValue;
    }

    internal static Author FromHashCodeLine(string line, bool isDBLP)
    {
      Author author = null;
      try
      {
        if (isDBLP)
        {
          string[] datas = line.Split('\t');
          author = new Author()
          {
            idAuthor = Convert.ToInt32(datas[0].Trim('|')),
            authorName = datas[1].Trim('|'),
            liAlias = new List<int>(), //datas[2]
            curValue = Convert.ToDouble(datas[3].Trim('|'))
          };
          string[] aliasHashcode = datas[2].Trim('|').Split('|');
          if (aliasHashcode.Length > 0)
          {
            foreach (string ah in aliasHashcode)
            {
              if (ah != "")
              {
                author.liAlias.Add(Convert.ToInt32(ah));
              }
            }
          }
        }
        else
        {
          string[] datas = line.Split('\t');
          author = new Author()
          {
            idAuthor = Convert.ToInt32(datas[0].Trim('|')),
            authorName = datas[1].Trim('|'),
            //data[2] = hashcode of authorName
            curValue = Convert.ToDouble(datas[3].Trim('|')),
            idDBLPCCode = Convert.ToInt32(datas[4].Trim('|')) ,
            liAlias = new List<int>()
          };
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine("Error {0} at line:{1}", ex.Message, line);
      }
      return author;
    }
  }
}
