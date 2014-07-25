using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeywordSimilar
{
  public static class Program
  {
    static object lock_state = new object();
    static object lock_keyword = new object();
    static object lock_paper = new object();
    static int parallelLoop = 300;
    //static string directory = @"F:\vohoanghai2\Dropbox-vohoanghai2-gmail\Dropbox\Python\";
    static string directory = @"G:\dblp\2014\mas\";
    public static Dictionary<int, Paper> dicAllPapers = new Dictionary<int,Paper>();
    public static Dictionary<int, Keyword> dicAllKeywords = new Dictionary<int, Keyword>();
    public static Dictionary<KeyValuePair<int, int>, double> dicSpaceStateValues = new Dictionary<KeyValuePair<int, int>, double>();
    public static Dictionary<KeyValuePair<int, int>, double> dicSpaceStateOldValues = new Dictionary<KeyValuePair<int, int>, double>();
    public static Dictionary<KeyValuePair<int, int>, Dictionary<int, double>> dicSpaceStateResults = new Dictionary<KeyValuePair<int, int>, Dictionary<int, double>>();
    public static Dictionary<KeyValuePair<int, int>, State> dicAllStates = new Dictionary<KeyValuePair<int, int>, State>();
    public static Dictionary<KeyValuePair<int, int>, Link> dicAllLinks = new Dictionary<KeyValuePair<int, int>, Link>();
    public static Dictionary<Tuple<int, int>, Query2> dicQuery2 = new Dictionary<Tuple<int, int>, Query2>();
    public static Dictionary<Tuple<int, int, int>, Query3> dicQuery3 = new Dictionary<Tuple<int, int, int>, Query3>();
    public static Dictionary<Tuple<int, int, int, int>, Query4> dicQuery4 = new Dictionary<Tuple<int, int, int, int>, Query4>();
    public static Dictionary<int, Query1> dicQuery1 = new Dictionary<int, Query1>();

    public static double[] alpha = { 0.05, 0.05, 0.85, 0.05 };
    public static Random random = new Random();
    public static double alpha1 = 0.2;
    public static double alpha2 = 0.2;
    public static double alpha3 = 0.5;
    public static double alpha4 = 0.1;

    public static void ReadKeywordStemmingFromFile(string str)
    {
      IEnumerable<string> lines = File.ReadLines(Path.GetFullPath(str));
      var line = lines.Where(x => (x != ""));
      var results = new ConcurrentQueue<Keyword>();
      Parallel.ForEach(line, l =>
      {
        Keyword keyword = Keyword.FromStemmingLine(l,'#');
        if (keyword != null)
        {
          results.Enqueue(keyword);
        }
      });
      dicAllKeywords = results.ToDictionary(x => x.idKeyword, x => x);
    }
    public static void ReadKeywordFromFile(string str)
    {
      IEnumerable<string> lines = File.ReadLines(Path.GetFullPath(str));
      var line = lines.Where(x => (x != ""));
      var results = new ConcurrentQueue<Keyword>();
      Parallel.ForEach(line, l =>
      {
        Keyword keyword = Keyword.FromLine(l);
        if (keyword != null)
        {
            results.Enqueue(keyword);
        }
      });
      dicAllKeywords = results.ToDictionary(x => x.idKeyword, x => x);
    }

    public static void ReadPaperStemmingFromFile(string str)
    {
      IEnumerable<string> lines = File.ReadLines(Path.GetFullPath(str));
      var line = lines.Where(x => (x != ""));
      var results = new ConcurrentQueue<Paper>();
      Parallel.ForEach(line, l =>
      {
        Paper paper = Paper.FromStemmingLine(l,'#');
        if (paper != null)
        {
          results.Enqueue(paper);
        }
      });
      dicAllPapers = results.ToDictionary(x => x.idPaper, x => x);
    }
    public static void ReadPaperFromFile(string str)
    {
      IEnumerable<string> lines = File.ReadLines(Path.GetFullPath(str));
      var line = lines.Where(x => (x != ""));
      var results = new ConcurrentQueue<Paper>();
      Parallel.ForEach(line, l =>
      {
        Paper paper = Paper.FromLine(l);
        if (paper != null)
        {
          results.Enqueue(paper);
        }
      });
      dicAllPapers = results.ToDictionary(x => x.idPaper, x => x);
    }
    static void ReadKeywordPaperRelationFromFile(string str)
    {
      IEnumerable<string> lines = File.ReadLines(Path.GetFullPath(str));
      var line = lines.Where(x => (x != ""));
      var results = new ConcurrentQueue<KeyValuePair<int,int>>();
      Parallel.ForEach(line, l =>
      {
        string[] datas = l.Split(',','\t');
        try
        {
          int idPaper = Convert.ToInt32(datas[0].Trim('"'));
          int idKeyword = Convert.ToInt32(datas[1].Trim('"'));
          if (dicAllPapers.Keys.Contains(idPaper) && dicAllKeywords.Keys.Contains(idKeyword))
          {
            lock (lock_paper)
            {
              dicAllPapers[idPaper].dicKeywords.Add(idKeyword, dicAllKeywords[idKeyword]);
            }
            lock (lock_keyword)
            {
              dicAllKeywords[idKeyword].dicPapers.Add(idPaper, dicAllPapers[idPaper]);
            }
            lock (lock_state)
            {
              State state = new State(idKeyword, idPaper, 100);
              dicAllStates.Add(new KeyValuePair<int, int>(idKeyword,idPaper),state);
              dicAllPapers[idPaper].dicState.Add(idKeyword, state);
              dicAllKeywords[idKeyword].dicState.Add(idPaper, state);
            }
            results.Enqueue(new KeyValuePair<int, int>(idKeyword, idPaper));
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine("Error {0} at line:{1}", ex.Message, l);
        }
      });
      //dicAllStates = results.ToDictionary(x => x, x => new State(x.Key, x.Value, 100.0));
      dicSpaceStateValues = results.ToDictionary(x => x, x=> 0.0);
      dicSpaceStateOldValues = results.ToDictionary(x => x, x => 0.0);
      dicSpaceStateResults = results.ToDictionary(x => x, x=> new Dictionary<int,double>());
    }
    static void ReadPaperCiteFromFile(string str)
    {
      IEnumerable<string> lines = File.ReadLines(Path.GetFullPath(str));
      var line = lines.Where(x => (x != ""));
      var results = new ConcurrentQueue<KeyValuePair<int, int>>();
      Parallel.ForEach(line, l =>
      {
        string[] datas = l.Split(',');
        try
        {
          int idPaper = Convert.ToInt32(datas[0].Trim('"'));
          int idCitePaper = Convert.ToInt32(datas[1].Trim('"'));

          if (dicAllPapers.Keys.Contains(idCitePaper) && dicAllPapers.Keys.Contains(idPaper))
          {
            Link link = new Link(idPaper, idCitePaper, 0);
            lock (lock_paper)
            {
              dicAllPapers[idPaper].dicCitePaper.Add(idCitePaper, dicAllPapers[idCitePaper]);
              dicAllPapers[idCitePaper].dicRefPaper.Add(idPaper, dicAllPapers[idPaper]);
              dicAllLinks.Add(new KeyValuePair<int, int>(idPaper, idCitePaper), link);
            }
            lock (lock_state)
            {
              Parallel.ForEach(dicAllPapers[idCitePaper].dicState, s =>
              {
                s.Value.listLinks.Add(link);
              });
            }
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine("Error {0} at line:{1}", ex.Message, l);
        }
      });
    }
    static void ResetAllStates()
    {
      var keys = dicSpaceStateValues.Keys.ToList();
      Parallel.ForEach(keys, key =>
      {
        dicSpaceStateOldValues[key] = dicSpaceStateValues[key];
        dicSpaceStateValues[key] = 0;
      });
    }
    static void InitStateResults()
    {
      var keys = dicSpaceStateValues.Keys.ToList();
      Parallel.ForEach(keys, key =>
      {
        dicSpaceStateResults[key].Add(0, 1);
        dicSpaceStateOldValues[key] = 0;
        dicSpaceStateValues[key] = 1000;
      });
      
    }
    static void MoveToNextState(int step, bool readvaluefromfile)
    {
      object lock_loop=new object();
      double max_loop=0;     
      var results = new ConcurrentQueue<KeyValuePair<KeyValuePair<int,int>,double>>();
      Parallel.ForEach(dicSpaceStateValues.Keys, key =>
      {
        var statevalue = dicSpaceStateResults[key];
        lock (lock_loop)
        {
          if (step > 0)
          {

            max_loop = max_loop < (statevalue[step] - statevalue[step - 1]) ? (statevalue[step] - statevalue[step - 1]) : max_loop;
          }
          else
          {
            max_loop = 10000;
          }
        }
        results.Enqueue(new KeyValuePair<KeyValuePair<int, int>, double>(key, statevalue[step]));
      });
      if (max_loop < 10) return; //stop 
      dicSpaceStateValues.Clear();
      dicSpaceStateValues = results.ToDictionary(x => x.Key, x => x.Value);
      if (readvaluefromfile)
      {
        ReadFromFileResult(step);
      }
      List<KeyValuePair<KeyValuePair< int, int>,double>> myList = dicSpaceStateValues.ToList();
      myList.Sort(
        delegate(KeyValuePair<KeyValuePair<int, int>, double> firstPair,
          KeyValuePair<KeyValuePair<int, int>, double> nextPair)
        {
          return nextPair.Value.CompareTo(firstPair.Value);
        });
      Dictionary<double, KeyValuePair<int, int>> choiceDict = new Dictionary<double, KeyValuePair<int,int>>();
      choiceDict.Add(myList[0].Value,myList[0].Key);
      double aggregateValue = myList[0].Value;
      double sumValue = myList.Sum(x => x.Value);
      myList.Where(x => x.Value == 0).ToList();
      for (int i = 1; i < myList.Count; i++)
      {
         choiceDict.Add(aggregateValue += myList[i].Value, new KeyValuePair<int, int>(
           myList[i].Key.Key,myList[i].Key.Value));
      }
      //Get current value of all state, for first time this equal 1
      //Todo: set the value of diceChooseNextPaper
      int diceChooseFirstState = random.Next(1, (int)aggregateValue);
      List<double> choiceKeys = choiceDict.Keys.ToList();
      int index=0;
      for (index=0; index<choiceKeys.Count;index++){
        if (diceChooseFirstState > choiceKeys[index])
        {
          diceChooseFirstState = (int)choiceKeys[index];
          break;
        }
      }
      if (index == choiceKeys.Count)
      {
        diceChooseFirstState = (int)choiceKeys[0];
      }
      KeyValuePair<int, int> chooseState = choiceDict[diceChooseFirstState];
      //diceChooseState to begin a next move
      //int diceChooseState = random.Next(dicSpaceStateValues.Count);
      //KeyValuePair<int, int> chooseState = dicSpaceStateValues.Keys.ToList()[diceChooseState];
      List<KeyValuePair<int, int>> listStates = dicSpaceStateValues.Keys.ToList();
      int loop = dicSpaceStateValues.Count * 50000;
      //then reset all states to next move
      double oldval = dicSpaceStateValues.Sum(x => x.Value);
      ResetAllStates();
      Stopwatch sw = new Stopwatch();
      sw.Start();
      
      double s =Math.Pow( loop,1.0/3);
      for (int k = 0; k < s+1; k++)
      {
        Parallel.For(0, (int)s+1, j =>
        {
          Parallel.For(0, (int)s + 1, i =>
          {
            ProcessMoveNextState(chooseState, listStates);
          });
        });
        //sw.Stop();
        //Console.WriteLine("loop {0}:-{1}, time:{2}",k, dicSpaceStateValues.Sum(x => x.Value),sw.Elapsed);
        //sw.Restart();
      }
      sw.Stop();
      Console.WriteLine("loop {0}, time:{1}", dicSpaceStateValues.Sum(x => x.Value), sw.Elapsed);
      Parallel.ForEach(dicSpaceStateValues, dp => {
        dicSpaceStateResults[dp.Key][step + 1] = dp.Value;
      });
      WriteResultToFile( step,"-");
    }

    static void WriteResultToFile(int step, string file)
    {
      StringBuilder sb = new StringBuilder();
      var results = new ConcurrentQueue<string>();
      Parallel.ForEach(dicSpaceStateValues, x =>
      {
        results.Enqueue(string.Format("\"{0}\",\"{1}\",\"{2}\"\n", x.Key.Key, x.Key.Value, x.Value));
      });
      File.WriteAllText(Path.GetFullPath(directory + "\\result-round-" +
          step.ToString() +"-"+file + ".csv"), string.Concat(results.ToList()));
    }

    static int CalculateIntersectIGAB(List<List<int>> lines, int keyword1, int keyword2, 
      out int k1,out int k2)
    {
      int sum =  k1 = k2 = 0;
      int t1 = 0;
      int t2 = 0;
      Parallel.ForEach(lines, l =>
      {
        try
        {
          if (l.Contains(keyword1))
          {
            lock (lock_keyword)
            { t1++; }
          }
          if (l.Contains(keyword2))
          {
            lock (lock_keyword)
            { t2++; }
          }
          if (l.Contains(keyword1) && l.Contains(keyword2))
          {
            lock (lock_keyword)
            {
              sum++;
            }
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine("Error {0} at line:{1}", ex.Message, l);
        }
      });
      k1 = t1;
      k2 = t2;
      return sum;
    }
    static void WriteIGABToFile(List<int> idKeywordList, string file)
    {
      IEnumerable<string> lines = File.ReadLines(Path.GetFullPath(directory + file));
      var line = lines.Where(x => (x != ""));
      var result = new ConcurrentQueue<List<int>>();
      Parallel.ForEach(line, l =>
      {
        List<int> ilist = new List<int>();
        string[] datas = l.Trim(';').Split(';');
        try
        {
          for (int i = 0; i < datas.Length; i++)
          {
            ilist.Add(Convert.ToInt32(datas[i]));
          }
          result.Enqueue(ilist);
        }
        catch (Exception ex)
        {
          Console.WriteLine("Error {0} at line:{1}", ex.Message, l);
        }
      });
      List<List<int>> listData = result.ToList();

      int k1,k2,kt;
      StringBuilder sb = new StringBuilder("\"keyword1\",\"keyword2\",\"common\",\"k1\",\"k2\" \n");
      for (int i = 0; i < idKeywordList.Count; i++)
      {
        for (int j = i + 1; j < idKeywordList.Count; j++)
        {
          kt = CalculateIntersectIGAB(listData, idKeywordList[i], idKeywordList[j], out k1, out k2);
          sb.AppendLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\"\n", idKeywordList[i],
            idKeywordList[j], kt,k1,k2 ));
        }
      }
      File.WriteAllText(Path.GetFullPath(directory + "\\IGAB-" +
           idKeywordList.Count.ToString() + "-" + file + ".csv"), sb.ToString());

    }
    static void WriteIABToFile(List<int> idKeywordList, string file)
    {
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < idKeywordList.Count; i++)
      {
        for (int j = i + 1; j < idKeywordList.Count; j++)
        {
          sb.AppendLine(string.Format("\"{0}\",\"{1}\",\"{2}\"\n", idKeywordList[i],
            idKeywordList[j], CalculateIntersectAB(idKeywordList[i], idKeywordList[j])));
        }
      }
      File.WriteAllText(Path.GetFullPath(directory + "\\result-" +
           idKeywordList.Count.ToString() + "-" + file + ".csv"), sb.ToString());
     
    }
    static void ReadFromFileResult(int step)
    {
      IEnumerable<string> lines = File.ReadLines(Path.GetFullPath(directory+
        "\\result-round-" + step.ToString() + ".csv"));
      var line = lines.Where(x => (x != ""));
      var results = new ConcurrentQueue<KeyValuePair<KeyValuePair<int, int>,double>>();
      Parallel.ForEach(line, l =>
      {
        string[] datas = l.Split(',');
        try
        {
          int idKeyword = Convert.ToInt32(datas[0].Trim('"'));
          int idPaper = Convert.ToInt32(datas[1].Trim('"'));
          int idValue = Convert.ToInt32(datas[2].Trim('"'));
          results.Enqueue(new KeyValuePair<KeyValuePair<int, int>,double>(
            new KeyValuePair<int,int>(idKeyword, idPaper),idValue));
        }
        catch (Exception ex)
        {
          Console.WriteLine("Error {0} at line:{1}", ex.Message, l);
        }
      });
      dicSpaceStateValues.Clear(); 
      dicSpaceStateValues = results.ToDictionary(x => x.Key, x => x.Value);
    }

    static void ProcessMoveNextState(KeyValuePair<int, int> chooseState, List<KeyValuePair<int, int>> listStates)
    {
      double diceChooseAlpha = random.NextDouble();
      int diceChooseNextPaper, diceChooseNextKeyword;
      int idKeyword = chooseState.Key;
      int idPaper = chooseState.Value;
      KeyValuePair<int, int> nextState;
      int nextidPaper, nextidKeyword;
      Keyword curKeyword;
      Paper curPaper, citePaper;
      if (diceChooseAlpha < alpha1)
      { //follow keyword
        curKeyword = dicAllKeywords[idKeyword];
        if (curKeyword.dicPapers.Count > 1)
        {
          diceChooseNextPaper = random.Next(curKeyword.dicPapers.Count);
          nextidPaper = curKeyword.dicPapers.Keys.ToList()[diceChooseNextPaper];
        }
        else
        {
          nextidPaper = curKeyword.dicPapers.Keys.ToList()[0];
        }
        nextState = listStates.Where(
          x => (x.Key == idKeyword) && (x.Value == nextidPaper)).ToList()[0];
      }
      else if (diceChooseAlpha < alpha2)
      { //follow paper
        curPaper = dicAllPapers[idPaper];
        if (curPaper.dicKeywords.Count > 1)
        {
          diceChooseNextKeyword = random.Next(curPaper.dicKeywords.Count);
          nextidKeyword = curPaper.dicKeywords.Keys.ToList()[diceChooseNextKeyword];
        }
        else
        {
          nextidKeyword = curPaper.dicKeywords.Keys.ToList()[0];
        }
        nextState = listStates.Where(
          x => (x.Key == nextidKeyword) && (x.Value == idPaper)).ToList()[0];
      }
      else if (diceChooseAlpha < alpha3)
      {  //follow citation
        curPaper = dicAllPapers[idPaper];
        if (curPaper.dicCitePaper.Count > 1)
        {
          diceChooseNextPaper = random.Next(curPaper.dicCitePaper.Count);
          nextidPaper = curPaper.dicCitePaper.Keys.ToList()[diceChooseNextPaper];
        }
        else if (curPaper.dicCitePaper.Count > 0)
        {
          nextidPaper = curPaper.dicCitePaper.Keys.ToList()[0];
        }
        else
        {
          if (dicAllPapers.Count > 1)
          {
            diceChooseNextPaper = random.Next(dicAllPapers.Count);
            nextidPaper = dicAllPapers.Keys.ToList()[diceChooseNextPaper];
          }
          else
          {
            nextidPaper = dicAllPapers.Keys.ToList()[0];
          }
        }
        citePaper = dicAllPapers[nextidPaper];
        if (citePaper.dicKeywords.Count > 1)
        {
          diceChooseNextKeyword = random.Next(citePaper.dicKeywords.Count);
          nextidKeyword = citePaper.dicKeywords.Keys.ToList()[diceChooseNextKeyword];
        }
        else
        {
          nextidKeyword = citePaper.dicKeywords.Keys.ToList()[0];
        }
        nextState = listStates.Where(
          x => (x.Key == nextidKeyword) && (x.Value == nextidPaper)).ToList()[0];
      }
      else //if (diceChooseAlpha < alpha4)
      {
        if (dicAllPapers.Count > 1)
        {
          diceChooseNextPaper = random.Next(dicAllPapers.Count);
          nextidPaper = dicAllPapers.Keys.ToList()[diceChooseNextPaper];
        }
        else
        {
          nextidPaper = dicAllPapers.Keys.ToList()[0];
        }
        citePaper = dicAllPapers[nextidPaper];
        if (citePaper.dicKeywords.Count > 1)
        {
          diceChooseNextKeyword = random.Next(citePaper.dicKeywords.Count);
          nextidKeyword = citePaper.dicKeywords.Keys.ToList()[diceChooseNextKeyword];
        }
        else
        {
          nextidKeyword = citePaper.dicKeywords.Keys.ToList()[0];
        }
        nextState = listStates.Where(
          x => (x.Key == nextidKeyword) && (x.Value == nextidPaper)).ToList()[0];
      }
      lock (lock_state)
      {
        dicSpaceStateValues[nextState]++;
      }
    }

    static void MarkovMove(int step, out double max, bool invert)
    {
      ResetAllStates();
     
      Stopwatch sw = new Stopwatch();
      ParallelOptions po = new ParallelOptions
      {
        MaxDegreeOfParallelism = Environment.ProcessorCount
      };
      var keys = dicSpaceStateValues.Keys.ToList();
      var listkeys = Split(keys, parallelLoop);
      double curmax = 0, cursum = 0, oldsum = 0;
      oldsum = dicSpaceStateOldValues.Values.Sum(x => x);
      int count = 0;
      //Parallel.ForEach(listkeys, po, liststate =>
      
      foreach (var liststate in listkeys)        
      {
        sw.Start();
        Parallel.ForEach(liststate, po, curstate =>
        //foreach (var curstate in liststate)
        {
          if (!invert)
            ComputeMarkovValue(curstate);
          else
            ComputeMarkovInvertValue(curstate,oldsum);
        }
        );
        sw.Stop();
        count++;
        Console.WriteLine("Elapse time:{0} -Count={1}", sw.Elapsed, count * liststate.Count);
      }
      //);
      Parallel.ForEach(keys, state =>
      {
        dicSpaceStateValues[state] += oldsum * alpha4 / dicAllPapers.Count /
          dicAllPapers[state.Value].dicKeywords.Count;
      });

      cursum = dicSpaceStateValues.Values.Sum(x => x);
      
      if ((oldsum - cursum) > 0)
      {
        Parallel.ForEach(keys, state =>
        {
          dicSpaceStateValues[state] += (oldsum - cursum) /
            dicAllPapers.Count / dicAllPapers[state.Value].dicKeywords.Count;
        });
      }
      
      cursum = dicSpaceStateValues.Values.Sum(x => x);
      
      Parallel.ForEach(keys, state =>
      {
        var diff = Math.Abs(dicSpaceStateValues[state] - dicSpaceStateOldValues[state]);
        lock (lock_state)
        {
          if (curmax < diff)
          {
            curmax = diff;
          }
        }
      });
      max = curmax;
    }

    static void ComputeMarkovValue(KeyValuePair<int, int> curstate)
    {
      //ParallelOptions po = new ParallelOptions
      //{
      //  MaxDegreeOfParallelism = Environment.ProcessorCount
      //};
      List<KeyValuePair<int, int>> listStates = dicSpaceStateValues.Keys.ToList();
      Paper curPaper = dicAllPapers[curstate.Value];
      Keyword curKeyword = dicAllKeywords[curstate.Key];
      //Transfer to same papers
      Parallel.ForEach(curPaper.dicKeywords, keyword => 
      //foreach (var keyword in curPaper.dicKeywords)
      {
        var nextstate = listStates.Where(x => (x.Key == keyword.Key) && (x.Value == curPaper.idPaper)).ToList()[0];
        lock (lock_state)
        {
          dicSpaceStateValues[nextstate] += dicSpaceStateOldValues[curstate] * alpha1 / curPaper.dicKeywords.Count;
        }
      }
      );
      //Transfer to same keywords
      Parallel.ForEach(curKeyword.dicPapers, paper =>
      //foreach (var paper in curKeyword.dicPapers)
      {
        var nextstate = listStates.Where(x => (x.Key == curKeyword.idKeyword) && (x.Value == paper.Key)).ToList()[0];
        lock (lock_state)
        {
          dicSpaceStateValues[nextstate] += dicSpaceStateOldValues[curstate] * alpha2 / curKeyword.dicPapers.Count;
        }
      }
      );
       //Transfer to citations
      if (curPaper.dicCitePaper.Count > 0)
      {
        Parallel.ForEach(curPaper.dicCitePaper, paper =>
        //foreach (var paper in curPaper.dicCitePaper)
        {
          //Parallel.ForEach(paper.Value.dicKeywords, po, keyword =>
          foreach (var keyword in paper.Value.dicKeywords)
          {
            var nextstate = listStates.Where(x => (x.Key == keyword.Key) && (x.Value == paper.Key)).ToList()[0];
            lock (lock_state)
            {
              dicSpaceStateValues[nextstate] += dicSpaceStateOldValues[curstate] * alpha3 / (curPaper.dicCitePaper.Count * paper.Value.dicKeywords.Count);
            }
          }
          //);
        }
        );
      }
      else
      {
        //var keys = dicSpaceStateValues.Keys.ToList();
        //Parallel.ForEach(keys, state =>
        ////foreach (var state in keys)
        //{
        //  lock (lock_state)
        //  {
        //    dicSpaceStateValues[state] += dicSpaceStateOldValues[curstate] * alpha3 /
        //      dicAllPapers.Count / dicAllPapers[state.Value].dicKeywords.Count;
        //  }
        //}
        //);
      }
    }
    static void ComputeMarkovInvertValue(KeyValuePair<int, int> curstate, double oldsum)
    {
      List<KeyValuePair<int, int>> listStates = dicSpaceStateValues.Keys.ToList();
      Paper curPaper = dicAllPapers[curstate.Value], nextPage;
      Keyword curKeyword = dicAllKeywords[curstate.Key], nextKeyword;
      //Transfer to same papers
      Parallel.ForEach(curPaper.dicKeywords, keyword =>
      //foreach (var keyword in curPaper.dicKeywords)
      {
        var nextstate = listStates.Where(x => (x.Key == keyword.Key) && (x.Value == curPaper.idPaper)).ToList()[0];
        nextPage = dicAllPapers[nextstate.Value];
        nextKeyword = dicAllKeywords[nextstate.Key];
        lock (lock_state)
        {
          dicSpaceStateValues[curstate] += dicSpaceStateOldValues[nextstate] * alpha1 / curPaper.dicKeywords.Count;
        }
      }
      );
      //Transfer to same keywords
      Parallel.ForEach(curKeyword.dicPapers, paper =>
      //foreach (var paper in curKeyword.dicPapers)
      {
        var nextstate = listStates.Where(x => (x.Key == curKeyword.idKeyword) && (x.Value == paper.Key)).ToList()[0];
        nextPage = dicAllPapers[nextstate.Value];
        nextKeyword = dicAllKeywords[nextstate.Key];
        lock (lock_state)
        {
          dicSpaceStateValues[curstate] += dicSpaceStateOldValues[nextstate] * alpha2 / nextKeyword.dicPapers.Count;
        }
      }
      );
      //Transfer to citations
      if (curPaper.dicCitePaper.Count > 0)
      {
        Parallel.ForEach(curPaper.dicCitePaper, paper =>
        //foreach (var paper in curPaper.dicCitePaper)
        {
          Parallel.ForEach(paper.Value.dicKeywords, keyword =>
          //foreach (var keyword in paper.Value.dicKeywords)
          {
            var nextstate = listStates.Where(x => (x.Key == keyword.Key) && (x.Value == paper.Key)).ToList()[0];
            nextPage = dicAllPapers[nextstate.Value];
            nextKeyword = dicAllKeywords[nextstate.Key];
            lock (lock_state)
            {
              if (paper.Value.dicRefPaper.Count == 0)
                Console.WriteLine("Error:---{0}-(1)-ref:{2}", paper.Key, paper.Value.title, paper.Value.dicRefPaper.Count);
              dicSpaceStateValues[curstate] += dicSpaceStateOldValues[nextstate] * alpha3 / (paper.Value.dicRefPaper.Count * paper.Value.dicKeywords.Count);
            }
          }
          );
        }
        );
      }
      else
      {
        //dicSpaceStateValues[curstate] += oldsum * alpha3 / dicSpaceStateValues.Count;
      }
    }
    
    public static List<List<KeyValuePair<int, int>>> Split(List<KeyValuePair<int, int>>
      source, int part)
    {
      return source
          .Select((x, i) => new { Index = i, Value = x })
          .GroupBy(x => x.Index / part)
          .Select(x => x.Select(v => v.Value).ToList())
          .ToList();
    }
    
    static void Main(string[] args)
    {
      directory = @"G:\dblp\2014\dbcj\";
      int stoploop = 1000000;
      double stopval = 0.1;
      //int nsequence = 3;
      parallelLoop = 50;
      bool invertmarkov = false;
      int computetype = 4; //0= pagerank; 1=n-tier; 2: markov; 3: kendall; 4: query
      //string filekeyword = "Test_Keyword.csv";
      //string filepaper = "Test_Paper.csv" ;
      //string filekeywordpaper = "Test_Paper_Keyword.csv";
      //string filepaperpaper = "Test_Paper_Paper.csv";

      string filekeyword =  "X_J_Keyword.csv"; //"Test_Keyword.csv";
      string filepaper = "X_J_Paper_WithAbstract.csv";//"Test_Paper.csv" ;
      string filekeywordpaper = "X_J_Paper_Keyword.csv";//"Test_Paper_Keyword.csv";
      string filepaperpaper = "X_J_Paper_Paper.csv";//"Test_Paper_Paper.csv";
      string filequery1 = "queries-01-keywords.csv";
      string filequery2 = "queries-02-keywords.csv";
      string filequery3 = "queries-03-keywords.csv";
      string filequery4 = "queries-04-keywords.csv";
      //string filestatevalue = "-";
      //string filepapernstar = "-";
      //string filepaperpagerank = "-";
      //string filestatevalue = "-";
      string filestatevalue = "result-0.3-round-34-state-X_J_Paper_Keyword.csv-ntier-0.3-0.3-0.3-0.1-0.0809256176889903.csv";
      string filepapernstar = "result-0.3-round-34-paper-X_J_Paper_Keyword.csv-ntier-0.0809256176889903.csv";
      string filepaperpagerank = "result-0.3-round-10-X_J_Paper_WithAbstract.csv-pagerank-0.0432496803307458.csv";
      alpha = new double[] { 0.3, 0.3, 0.3, 0.1 };
      try
      {
        if (args.Length > 0)
        {
          directory = args[0];
          stoploop = Convert.ToInt32(args[1]);
          stopval = Convert.ToDouble(args[2]);
          filekeyword = args[3];
          filepaper = args[4];
          filekeywordpaper = args[5];
          filepaperpaper = args[6];
          invertmarkov = Convert.ToBoolean(args[7]);
          filestatevalue = args[8];
          parallelLoop = Convert.ToInt32(args[9]);
          computetype = Convert.ToInt32(args[10]);
          alpha[0]=alpha1 = Convert.ToDouble(args[11]);
          alpha[1]=alpha2 = Convert.ToDouble(args[12]);
          alpha[2]=alpha3 = Convert.ToDouble(args[13]);
          alpha[3]=alpha4 = Convert.ToDouble(args[14]); 
          filepapernstar = args[15];
          filepaperpagerank = args[16];
          filestatevalue = args[17];
        }
        

        if (computetype == 3)
        {
          Console.WriteLine("Calculate Query Kendall");

          ReadQuery1Data(directory + filequery1);
          //ReadQuery2Data(directory + filequery2);
          //ReadQuery3Data(directory + filequery3);
          //ReadQuery4Data(directory + filequery4);
          Parallel.ForEach(dicQuery1.Values, q =>
          //foreach (Query1  q in dicQuery1.Values)
          {
            long concord = 0, discord = 0;
            q.kenDalUKP_Pagerank = KendallTau(
              q.dicResultsUKP.Values.ToArray(),
              q.dicResultsPagerank.Values.ToArray(),
              out concord, out discord);
            q.kenDalUKP_Pagerank_concord = concord;
            q.kenDalUKP_Pagerank_discord = discord;

            q.kenDalUKP_LKP = KendallTau(
             q.dicResultsUKP.Values.ToArray(),
             q.dicResultsLKP.Values.ToArray(),
             out concord, out discord);
            q.kenDalUKP_LKP_concord = concord;
            q.kenDalUKP_LKP_discord = discord;

            q.kenDalpagerank_LKP = KendallTau(
             q.dicResultsLKP.Values.ToArray(),
             q.dicResultsPagerank.Values.ToArray(),
             out concord, out discord);
            q.kenDalpagerank_LKP_concord = concord;
            q.kenDalpagerank_LKP_discord = discord;
          }
          );
          WriteQuery1Data(filequery1);
          //WriteQuery2Data(filequery2);
          //WriteQuery4Data(filequery4);
          //ReadQuery3Data(directory + filequery2);
          //ReadQuery4Data(directory + filequery2);
          return;
        }
        ReadKeywordFromFile(directory + filekeyword);
        ReadPaperFromFile(directory + filepaper);
        ReadKeywordPaperRelationFromFile(directory + filekeywordpaper);
        Console.WriteLine("Paper before remove:{0}", dicAllPapers.Count);
        List<KeyValuePair<int, Paper>> lremove = dicAllPapers.Where(x => x.Value.dicKeywords.Count == 0).ToList();
        Parallel.ForEach(lremove, item =>
        {
          lock (lock_paper)
          {
            dicAllPapers.Remove(item.Key);
          }
        });
        Console.WriteLine("Paper after remove:{0}", dicAllPapers.Count);

        Console.WriteLine("Keyword before remove:{0}", dicAllKeywords.Count);
        List<KeyValuePair<int, Keyword>> kremove = dicAllKeywords.Where(x => x.Value.dicPapers.Count == 0).ToList();
        Parallel.ForEach(kremove, item =>
        {
          lock (lock_keyword)
          {
            dicAllKeywords.Remove(item.Key);
          }
        });
        Console.WriteLine("Keyword after remove:{0}", dicAllKeywords.Count);

        ReadPaperCiteFromFile(directory + filepaperpaper);

        InitStateResults();
        if (filestatevalue != "-")
        {
          ReadValueResultFromFile(directory + filestatevalue);
        }
        if (filepapernstar != "-")
        {
          ReadPaperNStarFromFile(directory+filepapernstar);
          ReadPaperPageRankFromFile(directory+filepaperpagerank);
          KendallTauPaper();
        }
        Stopwatch sw = new Stopwatch();
        //int count=0;
        //Parallel.ForEach(dicAllStates, s =>
        //foreach (KeyValuePair<KeyValuePair<int,int>, State> s in dicAllStates)
        //{
        //  sw.Start();
        //  IEnumerable<KeyValuePair<KeyValuePair<int, int>, State>> samekeys = dicAllStates.AsParallel().Where(x => x.Key.Key == s.Key.Key);
        //  foreach (KeyValuePair<KeyValuePair<int, int>, State> sk in samekeys)
        //    s.Value.dicStateSameKeywords.Add(sk.Key, sk.Value);
        //  IEnumerable<KeyValuePair<KeyValuePair<int, int>, State>> samepapers = dicAllStates.AsParallel().Where(x => x.Key.Value == s.Key.Value);
        //  foreach (KeyValuePair<KeyValuePair<int, int>, State> sp in samepapers)
        //    s.Value.dicStateSamePapers.Add(sp.Key, sp.Value);
        //  if ((count++) % 1000 == 0)
        //  {
        //    sw.Stop();
        //    Console.WriteLine("Count={0}, Time={1}", count, sw.Elapsed);
        //  }
        //}
        //);
        int step = 0;
        double max = 100;
        //BuildQuerySet();

        if (computetype == 1) //
        {

          Console.WriteLine("Start N-tier loop({0}) -stopval: {1}", stoploop, stopval);
          Console.WriteLine("State ({0}) -Keyword {1} -Paper {2} - Links{3} ",
            dicAllStates.Count, dicAllKeywords.Count, dicAllPapers.Count, dicAllLinks.Count);
          double sumvalue = dicAllStates.Values.Sum(x => x.curValue);
          while (step < stoploop && max > stopval)
          {
            sw.Start();
            NtierCalculate(step, out max);
            sw.Stop();
            if (step % 10 == 0)
            {
              Console.WriteLine("Loop: {0} - Elapse:{1}", step, sw.Elapsed);
              WriteNtierToFile(step, filekeywordpaper + "-ntier-" + max.ToString());
            }
            step++;
          }
          WriteNtierToFile(step, filekeywordpaper + "-ntier-" + max.ToString());
          Console.WriteLine("Total Step:{0} - Done", step);
          //Console.ReadLine();          
          return;
        }
       
        if (computetype == 4)
        {
          BuildQuerySet();
          Console.WriteLine("Total Step:{0} - Done Query", step);
          return;
        }
        if (computetype==0)
        {
          Console.WriteLine("Start PAGERANK loop({0}) -stopval: {1}", stoploop, stopval);
          Console.WriteLine("State ({0}) -Keyword {1} -Paper {2} ",
            dicSpaceStateValues.Count, dicAllKeywords.Count, dicAllPapers.Count);
          double sumvalue = dicAllPapers.Values.Sum(x => x.curValue);
          while (step < stoploop && max > stopval)
          {
            sw.Start();
            PageRankMove(step, out max);
            sw.Stop();
            if (step % 50 == 0)
            {
              Console.WriteLine("Loop: {0} - Elapse:{1}", step, sw.Elapsed);
              WritePageRankToFile(step, filepaper + "-pagerank-" + max.ToString());
            } 
            step++;
          }
          WritePageRankToFile(step, filepaper + "-pagerank-" + max.ToString());
          Console.WriteLine("Total Step:{0} - Done", step);
          return;
        }
        //Generate markov chain state
        //GenerateMarkov(stoploop, stopval, filekeyword); 
       /* //Canculate IG(A,B)
        List<int> idKeywordList = new List<int>(){
391,
3644,
2815,
1885,
551,
2416,
3832,
1066,
3894,
2396,
3814,
965,
172,
216,
560,
2654,
5856,
2290,
382,
3014,
996,
555,
484,
3646,
3689,
1655,
140,
1420,
2225,
2789,
5327,
9549,
11175,
487,
1969,
5015,
465,
1171,
3069,
506
};
        //WriteIABToFile(idKeywordList, "IAB-x-db-4");
        WriteIGABToFile(idKeywordList, "generate-1000000-X_DB_4_Keyword.csv.csv");
        /*Cal IGAB dataset x-all-2
        {
          List<int> idKeywordList = new List<int>(){
            3744,3804,10193,2375,2163,1437,379,
          7879,          988,          6830,          9581,          545,
          555,          11041,          2376,          74,          4812,
          14906,          16372,          9489,          2758,          9605,
          7572,          106,          8595,          8,          81,
          1289,          2660,          4036};

          WriteIGABToFile(idKeywordList, "generate-100000-X_All_2_Keyword.csv.csv");
        }
        /*
        List<int> idKeywordList = new List<int>(){
        1387,555,8,313,1781,
        4432,11988,208,11054,1995,
        10534,6037,2971,1780,4642,
        2100,604,3844,520,3804};

       WriteIGABToFile(idKeywordList, "generate-100000-X_All_3_Keyword.csv.csv");
       */
        /*//Canculate I(A,B)
        List<int> idKeywordList = new List<int>(){
          936,264,4695,190,9260,
          2875,555,4696,4748,14488,
          6047,1498,8887,627,1171,
          965,1794,8120,172,3316,
          5952,3647,84,2292,749,
          6603,1485,1495,5748,2413};

        WriteIABToFile(idKeywordList, "IAB-x-all-4");
        */
        //Run Markov chain
        Console.WriteLine("Start MARKOV loop({0}) -stopval: {1}", stoploop, stopval);
        Console.WriteLine("State ({0}) -Keyword {1} -Paper {2} ", 
          dicSpaceStateValues.Count, dicAllKeywords.Count, dicAllPapers.Count);
        //Stopwatch sw= new Stopwatch();
        while (step < stoploop && max > stopval)
        { 
          sw.Start();
          MarkovMove(step, out max, invertmarkov);
          sw.Stop();
          Console.WriteLine("Loop - 1: {0} - Elapse:{1}", step,sw.Elapsed);
          WriteResultToFile(step, filekeyword +"-"+ max.ToString());
          step++;
        }
        Console.WriteLine("Done");
      }
      catch (Exception ex)
      {
        Console.WriteLine("Error:{0} - {1}", ex.Message, ex.StackTrace);
        Console.WriteLine("Usage: Keywordsimilarity.exe \"filepath\" \"stoploop\" \"stopval\""+
          " \"keywordfile\" \"paperfile\" \"keyword_paper\" \"paper_paper\" \"invertmethod\"" +
          "\"valuefile\" \"parallelLoop\" \"pagerankonly\" \"alpha1\" \"alpha2\" \"alpha3\" \"alpha4\"");
      }
      
    }

    public static void WriteQuery1Data(string file)
    {
      var results = new ConcurrentQueue<string>();
      results.Enqueue(string.Format("keyword1\tResults.Count\t" +
        "UKP_Pagerank\tUKP_LKP\tpagerank_LKP\t" +
         "kenDalLKP_RCM\tkenDalpagerank_RCM\t" +
        "UKP_Pagerank_concord\tUKP_Pagerank_discord\t" +
        "UKP_LKP_concord\tUKP_LKP_discord\t" +
        "pagerank_LKP_concord\tpagerank_LKP_discord" +
        "LKP_RCM_concord\tLKP_RCM_discord"+
        "pagerank_RCM_concord\tpagerank_RCM_discord\r\n"));
      Parallel.ForEach(dicQuery1.Values, x =>
      {
        results.Enqueue(x.ToString());
      });
      File.WriteAllText(Path.GetFullPath(directory + "\\result-" +
          file), string.Concat(results.ToList()));

    }


    public static void WriteQuery2Data(string file)
    {
      var results = new ConcurrentQueue<string>();
      results.Enqueue(string.Format("keyword1\tkeyword2\tResults.Count\t"+
        "UKP_Pagerank\tUKP_LKP\tpagerank_LKP\t" +
         "kenDalLKP_RCM\tkenDalpagerank_RCM\t" +
        "UKP_Pagerank_concord\tUKP_Pagerank_discord\t" +
        "UKP_LKP_concord\tUKP_LKP_discord\t" +
        "pagerank_LKP_concord\tpagerank_LKP_discord" +
        "LKP_RCM_concord\tLKP_RCM_discord" +
        "pagerank_RCM_concord\tpagerank_RCM_discord\r\n"));
      Parallel.ForEach(dicQuery2.Values, x =>
      {
        results.Enqueue(x.ToString());
      });
      File.WriteAllText(Path.GetFullPath(directory + "\\result-" +
          file), string.Concat(results.ToList()));
      
    }

    public static void WriteQuery3Data(string file)
    {
      var results = new ConcurrentQueue<string>();
      results.Enqueue(string.Format("keyword1\tkeyword2\tkeyword3\tResults.Count\t" +
        "UKP_Pagerank\tUKP_LKP\tpagerank_LKP\t" +
         "kenDalLKP_RCM\tkenDalpagerank_RCM\t" +
        "UKP_Pagerank_concord\tUKP_Pagerank_discord\t" +
        "UKP_LKP_concord\tUKP_LKP_discord\t" +
        "pagerank_LKP_concord\tpagerank_LKP_discord" +
        "LKP_RCM_concord\tLKP_RCM_discord" +
        "pagerank_RCM_concord\tpagerank_RCM_discord\r\n"));
      Parallel.ForEach(dicQuery3.Values, x =>
      {
        results.Enqueue(x.ToString());
      });
      File.WriteAllText(Path.GetFullPath(directory + "\\result-" +
          file), string.Concat(results.ToList()));

    }
    public static void WriteQuery4Data(string file)
    {
      var results = new ConcurrentQueue<string>();
      results.Enqueue(string.Format("keyword1\tkeyword2\tkeyword3\tkeyword4\tResults.Count\t" +
       "UKP_Pagerank\tUKP_LKP\tpagerank_LKP\t" +
         "kenDalLKP_RCM\tkenDalpagerank_RCM\t" +
        "UKP_Pagerank_concord\tUKP_Pagerank_discord\t" +
        "UKP_LKP_concord\tUKP_LKP_discord\t" +
        "pagerank_LKP_concord\tpagerank_LKP_discord" +
        "LKP_RCM_concord\tLKP_RCM_discord" +
        "pagerank_RCM_concord\tpagerank_RCM_discord\r\n"));
      Parallel.ForEach(dicQuery4.Values, x =>
      {
        results.Enqueue(x.ToString());
      });
      File.WriteAllText(Path.GetFullPath(directory + "\\result-" +
          file), string.Concat(results.ToList()));

    }

    public static void ReadQuery1Data(string str)
    {
      IEnumerable<string> lines = File.ReadLines(Path.GetFullPath(str));
      var line = lines.Where(x => (x != ""));
      object lock_query = new object();
      Parallel.ForEach(line, l =>
      {
        try
        {
          string[] datas = l.Split('\t', ',');
          int k1 = Convert.ToInt32(datas[0].Trim('"'));         
          int p = Convert.ToInt32(datas[1].Trim('"'));
          double lkp = Convert.ToDouble(datas[2].Trim('"'));
          double pagerank = Convert.ToDouble(datas[3].Trim('"'));
          double ukp = Convert.ToDouble(datas[4].Trim('"'));
          double rcm = CalculateRCM(k1, p);
          lock (lock_query)
          {
            if (dicQuery1.Keys.Contains(k1))
            {
              dicQuery1[k1].dicResultsLKP.Add(p, lkp);
              dicQuery1[k1].dicResultsUKP.Add(p, ukp);
              dicQuery1[k1].dicResultsPagerank.Add(p, pagerank);
              dicQuery1[k1].dicResultsRCM.Add(p, rcm);
            }
            else
            {
              Query1 q = new Query1(k1);
              q.dicResultsLKP.Add(p, lkp);
              q.dicResultsUKP.Add(p, ukp);
              q.dicResultsPagerank.Add(p, pagerank);
              q.dicResultsRCM.Add(p, rcm);
              dicQuery1.Add(k1, q);
            }
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine("Error {0} at line:{1}", ex.Message, line);
        }
      });
    }

    public static double CalculateRCM(int k1, int p)
    {
      return matchingQueryPaper(dicAllKeywords[k1].keyword, dicAllPapers[p].title,
        dicAllPapers[p].strabstract) / dicAllPapers[p].keywordCount;
    }
    public static double CalculateRCM(int k1, int k2, int p)
    {
      return matchingQueryPaper(dicAllKeywords[k1].keyword + "," + dicAllKeywords[k2].keyword, 
        dicAllPapers[p].title,
        dicAllPapers[p].strabstract) / dicAllPapers[p].keywordCount;
    }
    public static double CalculateRCM(int k1, int k2, int k3, int p)
    {
      return matchingQueryPaper(dicAllKeywords[k1].keyword + "," + dicAllKeywords[k2].keyword +
        "," + dicAllKeywords[k3].keyword,
        dicAllPapers[p].title,
        dicAllPapers[p].strabstract) / dicAllPapers[p].keywordCount;
    }
    public static double CalculateRCM(int k1, int k2, int k3, int k4, int p)
    {
      return matchingQueryPaper(dicAllKeywords[k1].keyword + "," + dicAllKeywords[k2].keyword +
        "," + dicAllKeywords[k3].keyword + "," + dicAllKeywords[k4].keyword,
        dicAllPapers[p].title,
        dicAllPapers[p].strabstract) / dicAllPapers[p].keywordCount;
    }
    // Compute Matching between 1 query and 1 paper.
    public static double matchingQueryPaper(String query, String title, String paperAbstract)
    {
      double matching = 0.0;
      string[] keys = query.Split(',');
      foreach (String keyword in keys)
      {
        matching += matchingKeywordPaper(keyword, title, paperAbstract);
      }

      return matching;
    }

    // Compute Matching between 1 keyword and 1 paper.
    public static double matchingKeywordPaper(string keyword, string title, string paperAbstract)
    {
      if (title.Contains(keyword) && paperAbstract.Contains(keyword))
      {
        return 3;
      }
      else if (title.Contains(keyword))
      {
        return 2;
      }
      else if (paperAbstract.Contains(keyword))
      {
        return 1;
      }
      else
      {
        return 0;
      }
    }
    
    public static void ReadQuery2Data(string str)
    {
      IEnumerable<string> lines = File.ReadLines(Path.GetFullPath(str));
      var line = lines.Where(x => (x != ""));
      object lock_query = new object();
      Parallel.ForEach(line, l =>
      {
        try
        {
          string[] datas = l.Split('\t', ',');
          int k1 = Convert.ToInt32(datas[0].Trim('"'));
          int k2 = Convert.ToInt32(datas[1].Trim('"'));
          int p = Convert.ToInt32(datas[2].Trim('"'));
          double lkp = Convert.ToDouble(datas[3].Trim('"'));
          double pagerank = Convert.ToDouble(datas[5].Trim('"'));
          double ukp = Convert.ToDouble(datas[6].Trim('"'));
          double rcm = CalculateRCM(k1, k2, p);
          Tuple<int, int> keywords = Tuple.Create(k1, k2);
          lock (lock_query)
          {
            if (dicQuery2.Keys.Contains(keywords))
            {
              dicQuery2[keywords].dicResultsLKP.Add(p, lkp);
              dicQuery2[keywords].dicResultsUKP.Add(p, ukp);
              dicQuery2[keywords].dicResultsPagerank.Add(p, pagerank);
              dicQuery2[keywords].dicResultsRCM.Add(p, rcm);
            }
            else
            {
              Query2 q = new Query2(k1, k2);
              q.dicResultsLKP.Add(p, lkp);
              q.dicResultsUKP.Add(p, ukp);
              q.dicResultsPagerank.Add(p, pagerank);
              q.dicResultsRCM.Add(p, rcm);
              dicQuery2.Add(keywords, q);
            }
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine("Error {0} at line:{1}", ex.Message, line);
        }
      });
    }
   
    public static void ReadQuery3Data(string str)
    {
      IEnumerable<string> lines = File.ReadLines(Path.GetFullPath(str));
      var line = lines.Where(x => (x != ""));
      object lock_query = new object();
      Parallel.ForEach(line, l =>
      {
        try
        {
          string[] datas = l.Split('\t', ',');
          int k1 = Convert.ToInt32(datas[0].Trim('"'));
          int k2 = Convert.ToInt32(datas[1].Trim('"'));
          int k3 = Convert.ToInt32(datas[2].Trim('"'));
          int p = Convert.ToInt32(datas[3].Trim('"'));
          double lkp = Convert.ToDouble(datas[4].Trim('"'));
          double pagerank = Convert.ToDouble(datas[6].Trim('"'));
          double ukp = Convert.ToDouble(datas[7].Trim('"'));
          double rcm = CalculateRCM(k1, k2, k3, p);
          Tuple<int, int,int> keywords = Tuple.Create(k1, k2,k3);
          lock (lock_query)
          {
            if (dicQuery3.Keys.Contains(keywords))
            {
              dicQuery3[keywords].dicResultsLKP.Add(p, lkp);
              dicQuery3[keywords].dicResultsUKP.Add(p, ukp);
              dicQuery3[keywords].dicResultsPagerank.Add(p, pagerank);
              dicQuery3[keywords].dicResultsRCM.Add(p, rcm);
            }
            else
            {
              Query3 q = new Query3(k1, k2,k3);
              q.dicResultsLKP.Add(p, lkp);
              q.dicResultsUKP.Add(p, ukp);
              q.dicResultsPagerank.Add(p, pagerank);
              q.dicResultsRCM.Add(p, rcm);
              dicQuery3.Add(keywords, q);
            }
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine("Error {0} at line:{1}", ex.Message, line);
        }
      });
    }

    public static void ReadQuery4Data(string str)
    {
      IEnumerable<string> lines = File.ReadLines(Path.GetFullPath(str));
      var line = lines.Where(x => (x != ""));
      object lock_query = new object();
      Parallel.ForEach(line, l =>
      {
        try
        {
          string[] datas = l.Split('\t', ',');
          int k1 = Convert.ToInt32(datas[0].Trim('"'));
          int k2 = Convert.ToInt32(datas[1].Trim('"'));
          int k3 = Convert.ToInt32(datas[2].Trim('"'));
          int k4 = Convert.ToInt32(datas[3].Trim('"'));
          int p = Convert.ToInt32(datas[4].Trim('"'));
          double lkp = Convert.ToDouble(datas[5].Trim('"'));
          double pagerank = Convert.ToDouble(datas[7].Trim('"'));
          double ukp = Convert.ToDouble(datas[8].Trim('"'));
          double rcm = CalculateRCM(k1, k2,k3,k4, p);
          Tuple<int, int, int,int> keywords = Tuple.Create(k1, k2, k3,k4);
          lock (lock_query)
          {
            if (dicQuery4.Keys.Contains(keywords))
            {
              dicQuery4[keywords].dicResultsLKP.Add(p, lkp);
              dicQuery4[keywords].dicResultsUKP.Add(p, ukp);
              dicQuery4[keywords].dicResultsPagerank.Add(p, pagerank);
              dicQuery4[keywords].dicResultsRCM.Add(p, rcm);
            }
            else
            {
              Query4 q = new Query4(k1, k2, k3,k4);
              q.dicResultsLKP.Add(p, lkp);
              q.dicResultsUKP.Add(p, ukp);
              q.dicResultsPagerank.Add(p, pagerank);
              q.dicResultsRCM.Add(p, rcm);
              dicQuery4.Add(keywords, q);
            }
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine("Error {0} at line:{1}", ex.Message, l);
        }
      });
    }
   
    public static double KendallTau(double[] a, double[] b, out long concord, out long discord)
    {
      if (a.Length!=b.Length){
        Console.WriteLine("not same count!out!");
        concord = discord = 0;
        return -2.0;
      }
      long count = a.Length;
      if (count == 1)
      {
        concord = discord = 0;
        return 1;
      }
      object lock_cord = new object(), lock_discord = new object();
      concord = discord = 0;
      ParallelOptions po = new ParallelOptions()
      {
        MaxDegreeOfParallelism = Environment.ProcessorCount
      };
      for (int i = 0; i < count - 1; i++)
      {
        for (int j = i + 1; j < count; j++)
        {
          if ((a[i] == a[j] && b[i] == b[j]) ||
            ((a[i] - a[j]) * (b[i] - b[j]) > 0))
          {
            lock (lock_cord)
            {
              concord++;
            }
          }
          if ((a[i] - a[j]) * (b[i] - b[j]) < 0)
          {
            lock (lock_discord)
            {
              discord++;
            }
          }
        }
      }
      return 2.0 * (concord - discord) / count / (count - 1);
    }
    private static void KendallTauPaper()
    {
      Paper[] aPaper = dicAllPapers.Values.ToArray();
      long count = aPaper.Length;
      ParallelOptions po = new ParallelOptions()
      {
        MaxDegreeOfParallelism = Environment.ProcessorCount
      };
      long ccUkpPk = 0, ccUkpNCC = 0, ccPkNCC = 0; 
      long disccUkpPk = 0, disccUkpNCC = 0, disccPkNCC = 0;
      object lock_1 = new object(), lock_2 = new object(), lock_3 = new object();
      object dlock_1 = new object(), dlock_2 = new object(), dlock_3 = new object();
      
      //Parallel.For(0, count - 1, po, i =>
      for (int i = 0; i < count - 1; i++)
      {
        Paper p1 = aPaper[i];
        Parallel.For(i + 1, count, po, j =>
        //for (long j = i + 1; j < count; j++)
        {
          Paper p2 = aPaper[j];

          if (((p1.curValue == p2.curValue) && (p1.PageRank == p2.PageRank)) ||
            ((p1.curValue - p2.curValue) * (p1.PageRank - p2.PageRank) > 0))
            lock (lock_1)
            {
              ccUkpPk++;
            }
            
          if ((p1.curValue - p2.curValue) * (p1.PageRank - p2.PageRank) < 0)
            lock (dlock_1)
            {
              disccUkpPk++;
            }
            

          if (((p1.curValue == p2.curValue) && (p1.refCount == p2.refCount)) ||
            ((p1.curValue - p2.curValue) * (p1.refCount - p2.refCount) > 0))
            lock (lock_2)
            {
              ccUkpNCC++;
            }
           
          if ((p1.curValue - p2.curValue) * (p1.refCount - p2.refCount) < 0)
            lock (dlock_2)
            {
              disccUkpNCC++;
            }          

          if (((p1.PageRank == p2.PageRank) && (p1.refCount == p2.refCount)) ||
           ((p1.PageRank - p2.PageRank) * (p1.refCount - p2.refCount) > 0))
            lock (lock_3)
            {
              ccPkNCC++;
            }
           
          if ((p1.PageRank - p2.PageRank) * (p1.refCount - p2.refCount) < 0)
            lock (dlock_3)
            {
              disccPkNCC++;              
            }            
        }
        );
      }
      //);
      File.WriteAllText(directory + "kendall-paper.csv",
        "Pair, concordant, disconcordant, kendallTau\r\n" +
        string.Format("Ukp/PK,{0},{1},{2}\r\n",ccUkpPk,disccUkpPk, 2.0*(ccUkpPk-disccUkpPk)/
          count/(count-1)) +
        string.Format("Ukp/NCC,{0},{1},{2}\r\n",ccUkpNCC,disccUkpNCC, 2.0*(ccUkpNCC-disccUkpNCC)/
          count/(count-1)) +
        string.Format("PK/NCC,{0},{1},{2}\r\n",ccPkNCC,disccPkNCC, 2.0*(ccPkNCC-disccPkNCC)/
          count/(count-1)));
    }

    //private static void BuildQuerySet2()
    //{
    //  var results = new ConcurrentQueue<string>();
    //  results.Enqueue(string.Format("idKeyword1,idKeyword2,idPaper,value\r\n"));
    //  Keyword[] aKey = dicAllKeywords.Values.Where(x => x.dicPapers.Count >= 2).ToArray();
     
    //  int count = aKey.Length;
    //  ParallelOptions po = new ParallelOptions()
    //  {
    //    MaxDegreeOfParallelism = Environment.ProcessorCount
    //  };
    //  int querycount = 0;
    //  Parallel.For(0,count - 1,po,i=>
    //  //for (int i = 0; i < count - 1; i++)
    //  {
    //    Keyword k1 = aKey[i];
    //    Parallel.For(i+1,count,po,j=>
    //    //for (int j = i + 1; j < count; j++)
    //    {
    //      Keyword k2 = aKey[j];
    //      List<int> k1p = k1.dicState.Keys.ToList();
    //      List<int> k2p = k2.dicState.Keys.ToList();
    //      List<int> r = k1p.Intersect(k2p).ToList();
    //      if (r.Count >= 10)
    //      {
    //        querycount++;
    //        foreach (int idpaper in r)
    //        {
    //          results.Enqueue(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\r\n",
    //            k1.idKeyword, k2.idKeyword, idpaper,
    //            k1.dicState[idpaper].curValue + k2.dicState[idpaper].curValue));
    //        }
    //      }
    //    }
    //    );
    //  }
    //  );
    //  File.WriteAllText(Path.GetFullPath(directory + "\\result-02-keywords.csv"), 
    //    "total query:"+ querycount.ToString() + ",total line:"+results.Count.ToString()+ "\r\n"+ string.Concat(results.ToList()));
    //  GC.Collect();
    //}

    private static void BuildQuerySet()
    {
      var results = new ConcurrentQueue<string>();
      var results3 = new ConcurrentQueue<string>();
      var results4 = new ConcurrentQueue<string>();
      results.Enqueue(string.Format("idKeyword1,idKeyword2,idKeyword2,idPaper,value\r\n"));
      results3.Enqueue(string.Format("idKeyword1,idKeyword2,idKeyword2,idKeyword3,idPaper,value\r\n"));
      results4.Enqueue(string.Format("idKeyword1,idKeyword2,idKeyword2,idKeyword3,idKeyword4,idPaper,value\r\n"));
      Keyword[] aKey = dicAllKeywords.Values.Where(x => x.dicPapers.Count >= 2).ToArray();

      int count = aKey.Length;
      ParallelOptions po = new ParallelOptions()
      {
        MaxDegreeOfParallelism = Environment.ProcessorCount
      };
      int q2count = 0, q3count = 0, q4count = 0;
      object lock_r = new object();
      object lock_r3 = new object();
      Parallel.For(0, count - 1, po, i =>
      //for (int i = 0; i < count - 1; i++)
      {
        Keyword k1 = aKey[i];
        Parallel.For(i + 1, count, po, j =>
        //for (int j = i + 1; j < count; j++)
        {
          Keyword k2 = aKey[j];
          List<int> k1p = k1.dicState.Keys.ToList();
          List<int> k2p = k2.dicState.Keys.ToList();
          List<int> r = k1p.Intersect(k2p).ToList();
          if (r.Count >= 10)
          {            
            q2count++;
            foreach (int idpaper in r)
            {
              results.Enqueue(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\r\n",
                k1.idKeyword, k2.idKeyword, idpaper,
                k1.dicState[idpaper].curValue + k2.dicState[idpaper].curValue));
            }
            lock (lock_r)
            {
              Parallel.For(j + 1, count, po, k =>
              //for (int k = j + 1; k < count; k++)
              {
                Keyword k3 = aKey[k];
                List<int> k3p = k3.dicState.Keys.ToList();
                List<int> r3 = r.Intersect(k3p).ToList();
                if (r3.Count >= 10)
                {
                  q3count++;
                  foreach (int idpaper3 in r3)
                  {
                    results3.Enqueue(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\r\n",
                      k1.idKeyword, k2.idKeyword, k3.idKeyword, idpaper3,
                      k1.dicState[idpaper3].curValue + k2.dicState[idpaper3].curValue + k3.dicState[idpaper3].curValue));
                  }
                  lock (lock_r3)
                  {
                    Parallel.For(k + 1, count, po, t =>
                    //for (int t = k + 1; t < count; t++)
                    {
                      Keyword k4 = aKey[t];
                      List<int> k4p = k4.dicState.Keys.ToList();
                      List<int> r4 = r3.Intersect(k4p).ToList();
                      if (r4.Count >= 10)
                      {
                        q4count++;
                        foreach (int idpaper4 in r4)
                        {
                          results4.Enqueue(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\r\n",
                            k1.idKeyword, k2.idKeyword, k3.idKeyword, k4.idKeyword, idpaper4,
                            k1.dicState[idpaper4].curValue + k2.dicState[idpaper4].curValue
                            + k3.dicState[idpaper4].curValue + k4.dicState[idpaper4].curValue));
                        }
                      }
                    }
                    );
                  }
                }
              }
              );
            }
          }
        }
        );
      }
      );
      File.WriteAllText(Path.GetFullPath(directory + "\\result-02-keywords.csv"),
        "total query:" + q2count.ToString() + ",total line:" + results.Count.ToString() + "\r\n" + string.Concat(results.ToList()));
      File.WriteAllText(Path.GetFullPath(directory + "\\result-03-keywords.csv"),
            "total query:" + q3count.ToString() + ",total line:" + results3.Count.ToString() + 
            "\r\n" + string.Concat(results3.ToList()));
      File.WriteAllText(Path.GetFullPath(directory + "\\result-04-keywords.csv"),
           "total query:" + q4count.ToString() + ",total line:" + results4.Count.ToString() +
           "\r\n" + string.Concat(results4.ToList()));

    }
    private static void WriteNtierToFile(int step, string file)
    {
     
      var results = new ConcurrentQueue<string>();
      results.Enqueue(string.Format("idKeyword,idPaper,value\r\n"));
      Parallel.ForEach(dicAllStates, x =>
      {
        results.Enqueue(string.Format("\"{0}\",\"{1}\",\"{2}\"\r\n", x.Key.Key,x.Key.Value, x.Value.curValue));
      });
      File.WriteAllText(Path.GetFullPath(directory + "\\result-round-" +
          step.ToString() + "-state-" + file + ".csv"), string.Concat(results.ToList()));
      
      var results2 = new ConcurrentQueue<string>();
      results2.Enqueue("idPaper, curValue, Keywords.Count, CitePaper.Count, RefPaper.Count\r\n");
      Parallel.ForEach(dicAllPapers, x =>
      {
        results2.Enqueue(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\"\r\n", x.Key, x.Value.curValue, 
          x.Value.dicKeywords.Count,x.Value.dicCitePaper.Count,x.Value.dicRefPaper.Count));
      });
      File.WriteAllText(Path.GetFullPath(directory + "\\result-round-" +
          step.ToString() + "-paper-" + file + ".csv"), string.Concat(results2.ToList()));
     
      var results3 = new ConcurrentQueue<string>();
      results3.Enqueue("idKeyword, curValue, Paper.Count\r\n");
      Parallel.ForEach(dicAllKeywords, x =>
      {
        results3.Enqueue(string.Format("\"{0}\",\"{1}\",\"{2}\"\r\n", x.Key, x.Value.curValue,
          x.Value.dicPapers.Count));
      });
      File.WriteAllText(Path.GetFullPath(directory + "\\result-round-" +
          step.ToString() + "-keyword-" + file + ".csv"), string.Concat(results3.ToList()));

      var results4 = new ConcurrentQueue<string>();
      results4.Enqueue("idKeyword,idKeywordRef, curValue\r\n");
      Parallel.ForEach(dicAllLinks, x =>
      {
        results4.Enqueue(string.Format("\"{0}\",\"{1}\",\"{2}\"\r\n", x.Value.idPaper, 
          x.Value.idPaperRef, x.Value.curValue));
      });
      File.WriteAllText(Path.GetFullPath(directory + "\\result-round-" +
          step.ToString() + "-citations-" + file + ".csv"), string.Concat(results4.ToList()));
      
  
    }

    private static void NtierCalculate(int step, out double max)
    {
      double sumpp = dicAllPapers.AsParallel().Sum(x => x.Value.curValue);
      double sumkw = dicAllKeywords.AsParallel().Sum(x => x.Value.curValue);
      double sumst1 = dicAllStates.AsParallel().Sum(x => x.Value.curValue);
      double sumln = dicAllLinks.AsParallel().Sum(x => x.Value.curValue);
      double curmax = 0;
      max = 10000;
      ParallelOptions po = new ParallelOptions()
      {
         MaxDegreeOfParallelism = Environment.ProcessorCount
      };
      Parallel.ForEach(dicAllPapers, po, dp =>
      //foreach (KeyValuePair<int, Paper> dp in dicAllPapers)
      {
        //dp.Value.SetValueFromStates(dicAllStates);
        dp.Value.SetValueFromStates();
      }
      );
      sumpp = dicAllPapers.AsParallel().Sum(x => x.Value.curValue);

      Parallel.ForEach(dicAllKeywords, po, dk =>
      //foreach (KeyValuePair<int, Keyword> dk in dicAllKeywords)
      {
        //dk.Value.SetValueFromStates(dicAllStates);
        dk.Value.SetValueFromStates();
      }
      );
      sumkw = dicAllKeywords.AsParallel().Sum(x => x.Value.curValue);

      Parallel.ForEach(dicAllLinks, po, dl =>
      //foreach (KeyValuePair<KeyValuePair<int,int>, Link> dl in dicAllLinks)
      {
        dl.Value.SetValueFromPapers(dicAllPapers);
      }
      );
      sumln = dicAllLinks.AsParallel().Sum(x => x.Value.curValue);

      Parallel.ForEach(dicAllStates, po, ds =>
      //foreach (KeyValuePair<KeyValuePair<int, int>, State> ds in dicAllStates)
      {
        //ds.Value.SetValue(dicAllKeywords,dicAllPapers,dicAllLinks,alpha);
        ds.Value.SetValue(dicAllKeywords, dicAllPapers, alpha);
      
      }
      );

      double sumst2 = dicAllStates.Sum(x => x.Value.curValue);
     
      double delta = sumst1 - sumst2;
      if (delta > 0)
      {
        Parallel.ForEach(dicAllStates, po, ds =>
        //foreach (KeyValuePair<KeyValuePair<int,int>, State> ds in dicAllStates)
        {
          //ds.Value.curValue += delta / dicAllStates.Count;
          ds.Value.curValue += delta /
            (dicAllPapers.Count * dicAllPapers[ds.Key.Value].dicKeywords.Count);

        }
        );
      }
      var keys = dicAllStates.Keys.ToList();
      sumst2 = dicAllStates.AsParallel().Sum(x => x.Value.curValue);
      curmax = 0;

      Parallel.ForEach(keys, po, state =>
      {
        var diff = Math.Abs(dicAllStates[state].curValue - dicAllStates[state].oldValue);
        lock (lock_paper)
        {
          if (curmax < diff)
          {
            curmax = diff;
          }
        }
      });
      max = curmax;
      //curmax = 0;
      //keys = dicAllLinks.Keys.ToList();
      //Parallel.ForEach(keys, po, link =>
      //{
      //  var diff = Math.Abs(dicAllLinks[link].curValue - dicAllLinks[link].oldValue);
      //  lock (lock_paper)
      //  {
      //    if (curmax < diff)
      //    {
      //      curmax = diff;
      //    }
      //  }
      //});
      //if (max < curmax)
      //  max = curmax;     
    }

    private static void WritePageRankToFile(int step, string file)
    {
      StringBuilder sb = new StringBuilder();
      var results = new ConcurrentQueue<string>();
      Parallel.ForEach(dicAllPapers, x =>
      {
        results.Enqueue(string.Format("\"{0}\",\"{1}\",\"{2}\"\n", x.Key, x.Value.curValue, x.Value.dicKeywords.Count));
      });
      File.WriteAllText(Path.GetFullPath(directory + "\\result-round-" +
          step.ToString() + "-" + file + ".csv"), string.Concat(results.ToList()));
  
    }

    private static void PageRankMove(int step, out double max)
    {
      ParallelOptions po = new ParallelOptions
      {
        MaxDegreeOfParallelism = Environment.ProcessorCount
      };
      var keys = dicAllPapers.Keys.ToList();
      Parallel.ForEach(keys,po, key =>
      {
        dicAllPapers[key].oldValue = dicAllPapers[key].curValue;
        dicAllPapers[key].curValue = 0;
      });
      double oldSum = dicAllPapers.AsParallel().Sum(x => x.Value.oldValue);

      
      Parallel.ForEach(keys, po, key =>
      {
        lock (lock_paper)
        {
          //sum_nocite += dicAllPapers[key].dicRefPaper.Count == 0 ? dicAllPapers[key].oldValue : 0;
          double temp = 0;
          foreach (KeyValuePair<int, Paper> refpaper in dicAllPapers[key].dicRefPaper)
          {
            temp += refpaper.Value.oldValue / refpaper.Value.dicCitePaper.Count;
          }
          dicAllPapers[key].curValue = (1 - alpha4) * temp; // +alpha4 * dicAllPapers[key].oldValue;
        }
      }
      );
      double curSum = dicAllPapers.AsParallel().Sum(x => x.Value.curValue);
      double delta = oldSum - curSum;
      if (delta>0)
      {
        Parallel.ForEach(keys, po, key =>
        {
          lock (lock_paper)
          {
            dicAllPapers[key].curValue += delta / dicAllPapers.Count;
          }
        }
        );
      }
     
      double curmax = 0;
      Parallel.ForEach(keys, p =>
      {
        var diff = Math.Abs(dicAllPapers[p].curValue - dicAllPapers[p].oldValue);
        lock (lock_state)
        {
          if (curmax < diff)
          {
            curmax = diff;
          }
        }
      });
      max = curmax;
    }

    private static void GenerateMarkov(int stoploop, int nsequence, string file)
    {
      Console.WriteLine("start loop generate {0}, nsequence:{1}", stoploop, nsequence);

      var results = new ConcurrentQueue<string>();
      List<KeyValuePair<KeyValuePair<int, int>, double>> listStates = dicSpaceStateValues.ToList();
      listStates.Sort(
        delegate(KeyValuePair<KeyValuePair<int, int>, double> firstPair,
          KeyValuePair<KeyValuePair<int, int>, double> nextPair)
        {
          return nextPair.Value.CompareTo(firstPair.Value);
        });
      Dictionary<double, KeyValuePair<int, int>> choiceDict = new Dictionary<double, KeyValuePair<int, int>>();
      choiceDict.Add(listStates[0].Value, listStates[0].Key);
      double aggregateValue = listStates[0].Value;
      double sumValue = listStates.Sum(x => x.Value);
      List<KeyValuePair<KeyValuePair<int, int>, double>> removelist = listStates.Where(x => x.Value == 0).ToList();
      if (removelist.Count > 0) { }
      for (int i = 1; i < listStates.Count; i++)
      {
        choiceDict.Add(aggregateValue += listStates[i].Value, new KeyValuePair<int, int>(
          listStates[i].Key.Key, listStates[i].Key.Value));
      }
      //Get current value of all state, for first time this equal 1
      //Todo: set the value of diceChooseNextPaper
      Stopwatch sw0 = new Stopwatch();
      sw0.Start();
      Parallel.For(0, (int)(stoploop / 300), ii =>
      //for (int ii = 0; ii < stoploop / 300; ii++)
      {
        string strStates = "";
        //Parallel.For(0, 300, step =>
        for (int step = 0; step < 300; step++)
        {
          //choose first state
          double diceChooseFirstState = random.NextDouble() * aggregateValue;
          List<double> choiceKeys = choiceDict.Keys.ToList();
          int index = 0;
          for (index = 0; index < choiceKeys.Count; index++)
          {
            if (diceChooseFirstState < choiceKeys[index])
            {
              diceChooseFirstState = choiceKeys[index];
              break;
            }
          }
          KeyValuePair<int, int> chooseState = choiceDict[diceChooseFirstState];
          KeyValuePair<int, int> nextState = chooseState;
          strStates += chooseState.Key + ";" + chooseState.Value + ";";

          for (int i = 0; i < nsequence; i++)
          {
            nextState = GenerateNextState(nextState);
            strStates += nextState.Key + ";" + nextState.Value + ";";
          }
          strStates += "\n";
        }
        results.Enqueue(string.Format("{0}", strStates));
        if (results.Count % 10 == 0)
        {
          sw0.Stop();
          Console.WriteLine("line {0}, time:{1}", results.Count, sw0.Elapsed);
          sw0.Start();
        }
        //);
      }
      );
      File.WriteAllText(Path.GetFullPath(directory + "\\generate-" +
          stoploop.ToString() + "-" + file + ".csv"), string.Concat(results.ToList()));
      sw0.Stop();
      Console.WriteLine("loop generate {0}, nsequence:{1}, time:{2}", stoploop, nsequence,sw0.Elapsed);

    }

    private static KeyValuePair<int,int> GenerateNextState(KeyValuePair<int, int> chooseState)
    {
      double diceChooseAlpha = random.NextDouble();
      int diceChooseNextPaper, diceChooseNextKeyword;
      int idKeyword = chooseState.Key;
      int idPaper = chooseState.Value;
      KeyValuePair<int, int> nextState;
      int nextidPaper, nextidKeyword;
      Keyword curKeyword;
      Paper curPaper, citePaper;
      List<KeyValuePair<int, int>> listStates = dicSpaceStateValues.Keys.ToList();
      if (diceChooseAlpha < alpha1)
      { //follow keyword
        curKeyword = dicAllKeywords[idKeyword];
        if (curKeyword.dicPapers.Count > 1)
        {
          diceChooseNextPaper = random.Next(curKeyword.dicPapers.Count);
          nextidPaper = curKeyword.dicPapers.Keys.ToList()[diceChooseNextPaper];
        }
        else
        {
          nextidPaper = curKeyword.dicPapers.Keys.ToList()[0];
        }
        nextState = listStates.Where(
          x => (x.Key == idKeyword) && (x.Value == nextidPaper)).ToList()[0];
      }
      else if (diceChooseAlpha < (alpha1+alpha2))
      { //follow paper
        curPaper = dicAllPapers[idPaper];
        if (curPaper.dicKeywords.Count > 1)
        {
          diceChooseNextKeyword = random.Next(curPaper.dicKeywords.Count);
          nextidKeyword = curPaper.dicKeywords.Keys.ToList()[diceChooseNextKeyword];
        }
        else
        {
          nextidKeyword = curPaper.dicKeywords.Keys.ToList()[0];
        }
        nextState = listStates.Where(
          x => (x.Key == nextidKeyword) && (x.Value == idPaper)).ToList()[0];
      }
      else if (diceChooseAlpha < (alpha1+alpha2+alpha3))
      {  //follow citation
        curPaper = dicAllPapers[idPaper];
        if (curPaper.dicCitePaper.Count > 1)
        {
          diceChooseNextPaper = random.Next(curPaper.dicCitePaper.Count);
          nextidPaper = curPaper.dicCitePaper.Keys.ToList()[diceChooseNextPaper];
        }
        else if (curPaper.dicCitePaper.Count > 0)
        {
          nextidPaper = curPaper.dicCitePaper.Keys.ToList()[0];
        }
        else
        {
          if (dicAllPapers.Count > 1)
          {
            diceChooseNextPaper = random.Next(dicAllPapers.Count);
            nextidPaper = dicAllPapers.Keys.ToList()[diceChooseNextPaper];
          }
          else
          {
            nextidPaper = dicAllPapers.Keys.ToList()[0];
          }
        }
        citePaper = dicAllPapers[nextidPaper];
        if (citePaper.dicKeywords.Count > 1)
        {
          diceChooseNextKeyword = random.Next(citePaper.dicKeywords.Count);
          nextidKeyword = citePaper.dicKeywords.Keys.ToList()[diceChooseNextKeyword];
        }
        else
        {
          nextidKeyword = citePaper.dicKeywords.Keys.ToList()[0];
        }
        nextState = listStates.Where(
          x => (x.Key == nextidKeyword) && (x.Value == nextidPaper)).ToList()[0];
      }
      else //if (diceChooseAlpha < alpha4)
      {
        if (dicAllPapers.Count > 1)
        {
          diceChooseNextPaper = random.Next(dicAllPapers.Count);
          nextidPaper = dicAllPapers.Keys.ToList()[diceChooseNextPaper];
        }
        else
        {
          nextidPaper = dicAllPapers.Keys.ToList()[0];
        }
        citePaper = dicAllPapers[nextidPaper];
        if (citePaper.dicKeywords.Count > 1)
        {
          diceChooseNextKeyword = random.Next(citePaper.dicKeywords.Count);
          nextidKeyword = citePaper.dicKeywords.Keys.ToList()[diceChooseNextKeyword];
        }
        else
        {
          nextidKeyword = citePaper.dicKeywords.Keys.ToList()[0];
        }
        nextState = listStates.Where(
          x => (x.Key == nextidKeyword) && (x.Value == nextidPaper)).ToList()[0];
      }
      return nextState;
    }

    private static int CalculateIntersectAB(int a, int b)
    {
      int sum = 0;
      Parallel.ForEach(dicAllKeywords[a].dicPapers, paper =>
      {
        if (paper.Value.dicKeywords.Keys.Contains(b))
        {        
          lock (lock_keyword)
          {
            sum += 1;
          }
        }
      });
      return sum;
    }
    private static void ReadPaperPageRankFromFile(string str)
    {
      IEnumerable<string> lines = File.ReadLines(Path.GetFullPath(str));
      var line = lines.Where(x => (x != ""));

      List<KeyValuePair<int, int>> listStates = dicSpaceStateValues.Keys.ToList();
      ParallelOptions po = new ParallelOptions()
      {
        MaxDegreeOfParallelism = Environment.ProcessorCount
      };
      Parallel.ForEach(line, po, l =>
      //foreach (string l in line)
      {
        string[] datas = l.Split(',');
        try
        {
          int idPaper = Convert.ToInt32(datas[0].Trim('"'));          
          double value = Convert.ToDouble(datas[1].Trim('"'));
          dicAllPapers[idPaper].PageRank = value;
        }
        catch (Exception ex)
        {
          Console.WriteLine("Error {0} at line:{1}", ex.Message, l);
        }
      }
      );
    }
    public static void ReadPaperNStarFromFile(string str)
    {
      IEnumerable<string> lines = File.ReadLines(Path.GetFullPath(str));
      var line = lines.Where(x => (x != ""));

      List<KeyValuePair<int, int>> listStates = dicSpaceStateValues.Keys.ToList();
      ParallelOptions po = new ParallelOptions()
      {
        MaxDegreeOfParallelism = Environment.ProcessorCount
      };
      Parallel.ForEach(line, po, l =>
      //foreach (string l in line)
      {
        string[] datas = l.Split(',');
        try
        {
          int idPaper = Convert.ToInt32(datas[0].Trim('"'));
          double value = Convert.ToDouble(datas[1].Trim('"'));
          double keycount = Convert.ToInt32(datas[2].Trim('"'));
          double citecount = Convert.ToInt32(datas[3].Trim('"'));
          double refcount = Convert.ToInt32(datas[4].Trim('"'));         
          dicAllPapers[idPaper].curValue = value;
          dicAllPapers[idPaper].keywordCount = keycount;
          dicAllPapers[idPaper].refCount = refcount;
        }
        catch (Exception ex)
        {
          Console.WriteLine("Error {0} at line:{1}", ex.Message, l);
        }
      }
      );
    }
    private static void ReadValueResultFromFile(string str)
    {
      IEnumerable<string> lines = File.ReadLines(Path.GetFullPath(str));
      var line = lines.Where(x => (x != ""));

      List<KeyValuePair<int, int>> listStates = dicSpaceStateValues.Keys.ToList();
      ParallelOptions po = new ParallelOptions()
      {
        MaxDegreeOfParallelism = Environment.ProcessorCount
      };
      Parallel.ForEach(line,po, l =>
      //foreach (string l in line)
      {
        string[] datas = l.Split(',');
        try
        {
          int idKeyword = Convert.ToInt32(datas[0].Trim('"'));
          int idPaper = Convert.ToInt32(datas[1].Trim('"'));
          double value = Convert.ToDouble(datas[2].Trim('"'));
          //var state = listStates.Where(x => (x.Key == idKeyword) && (x.Value == idPaper)).ToList()[0];
          dicSpaceStateValues[new KeyValuePair<int, int>(idKeyword, idPaper)] = value;
          dicAllStates[new KeyValuePair<int, int>(idKeyword, idPaper)].curValue = value;
        }
        catch (Exception ex)
        {
          Console.WriteLine("Error {0} at line:{1}", ex.Message, l);
        }
      }
      );
    }
  }
}
