using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchingRelatedContent
{
  public class Evaluation
  {
    public static double KendallTau(double[] a, double[] b, out long concord, out long discord)
    {
      if (a.Length != b.Length)
      {
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
    // Compute Matching between 1 query and 1 result list.
    public static double matchingQueryResult(String query, List<String> title, List<String> paperAbstract)
    {
      double sumPositionMatching = 0.0;
      double sumPosition = 0.0;
      double valuequerypaper = 0.0;
      for (int i = 0; i < title.Count; i++)
      {
        valuequerypaper = matchingQueryPaper(query, title.ElementAt(i), paperAbstract.ElementAt(i));
        sumPositionMatching += computeTraffic(i + 1, title.Count) * valuequerypaper;
        sumPosition += computeTraffic(i + 1, title.Count);
        Console.WriteLine("{0}\t{1}\t{2}", query, title.ElementAt(i), valuequerypaper);
      }

      return (double)sumPositionMatching / sumPosition;
    }

    // Traffic function
    public static double computeTraffic(int rank, int numResult)
    {
      if (rank == 1)
      {
        return 0.325;
      }
      else if (rank == 2)
      {
        return 0.176;
      }
      else if (rank == 3)
      {
        return 0.114;
      }
      else if (rank == 4)
      {
        return 0.081;
      }
      else if (rank == 5)
      {
        return 0.061;
      }
      else if (rank == 6)
      {
        return 0.044;
      }
      else if (rank == 7)
      {
        return 0.035;
      }
      else if (rank == 8)
      {
        return 0.031;
      }
      else if (rank == 9)
      {
        return 0.026;
      }
      else if (rank == 10)
      {
        return 0.024;
      }
      else
      {
        return 0.083 / (numResult - 10);
      }
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
    public static string directory = @"F:\vohoanghai2\Dropbox-vohoanghai2-gmail\Dropbox\LeAnhVu\Keyword-Web-Ranking\Experiment\Journal\Export data\Database Conference and Journal\Evaluation\Citation 0.5\";     
    public static int Main(string[] args)
    {
      string filestemkeyword = "X_J_Keyword_SplittedBy#_Removed.csv";
      string filestempaper = "X_J_Paper_WithAbstract_SplittedBy#_Removed.csv";
      string filepapernstar = "result-0.5-round-37-paper-X_J_Paper_Keyword.csv-ntier-0.0978390954551287.csv";
      
      string filequery1 = "queries-01-keywords.csv";
      string filequery2 = "queries-02-keywords.csv";
      string filequery3 = "queries-03-keywords.csv";
      string filequery4 = "queries-04-keywords.csv";
      KeywordSimilar.Program.ReadKeywordStemmingFromFile(directory + filestemkeyword);
      KeywordSimilar.Program.ReadPaperStemmingFromFile(directory + filestempaper);
      KeywordSimilar.Program.ReadPaperNStarFromFile(directory + filepapernstar);
      KeywordSimilar.Program.ReadQuery1Data(directory + filequery1);
      KeywordSimilar.Program.dicQuery1 = 
        KeywordSimilar.Program.dicQuery1.Where(x => x.Value.dicResultsLKP.Count >= 10).
        ToDictionary(x=> x.Key, x=>x.Value);
      Dictionary<int, int[]> distvalue = KeywordSimilar.Program.dicQuery1.Values.ToDictionary(x => x.keyword,
        x => new[]{x.dicResultsPagerank.Count(),
          x.dicResultsPagerank.Values.Distinct().Count(),
         x.dicResultsLKP.Values.Distinct().Count(),
         x.dicResultsRCM.Values.Distinct().Count()});
      WriteQuery1Different(filequery1, distvalue);
      /*
      Parallel.ForEach(KeywordSimilar.Program.dicQuery1.Values, q =>      
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

        q.kenDalLKP_RCM = KendallTau(
        q.dicResultsLKP.Values.ToArray(),
        q.dicResultsRCM.Values.ToArray(),
        out concord, out discord);
        q.kenDalLKP_RCM_concord = concord;
        q.kenDalLKP_RCM_discord = discord;

        q.kenDalpagerank_RCM = KendallTau(
        q.dicResultsPagerank.Values.ToArray(),
        q.dicResultsRCM.Values.ToArray(),
        out concord, out discord);
        q.kenDalpagerank_RCM_concord = concord;
        q.kenDalpagerank_RCM_discord = discord;
      }
      );
      KeywordSimilar.Program.WriteQuery1Data(filequery1);
      */


      //Calculate4QueriesRCM();
      
      //CalculateQuery();
      Console.ReadLine();
      return 0;
    }
    public static void WriteQuery1Different(string file, Dictionary<int, int[]> dict)
    {
      var results = new ConcurrentQueue<string>();
      results.Enqueue(string.Format("keyword1\tResults.Count\t" +
       "PR distinct\t" +
        "LKP distinct\t" +
        "RCM distinct\r\n"));
      Parallel.ForEach(dict, x =>
      {
        results.Enqueue(string.Format("{0},{1},{2},{3},{4}\r\n", x.Key, x.Value[0], x.Value[1], x.Value[2], x.Value[3]));
      });
      File.WriteAllText(Path.GetFullPath(directory + "\\result-distinct" +
          file), string.Concat(results.ToList()));
    }
    public static void WriteQuery2Different(string file, Dictionary<Tuple<int,int>, int[]> dict)
    {
      var results = new ConcurrentQueue<string>();
      results.Enqueue(string.Format("keyword1\tkeyword2\tResults.Count\t" +
       "PR distinct\t" +
        "LKP distinct\t" +
        "RCM distinct\r\n"));
      Parallel.ForEach(dict, x =>
      {
        results.Enqueue(string.Format("{0},{1},{2},{3},{4},{5}\r\n", x.Key.Item1, x.Key.Item2, x.Value[0], x.Value[1], x.Value[2], x.Value[3]));
      });
      File.WriteAllText(Path.GetFullPath(directory + "\\result-distinct" +
          file), string.Concat(results.ToList()));

    }
    public static void WriteQuery3Different(string file, Dictionary<Tuple<int, int,int>, int[]> dict)
    {
      var results = new ConcurrentQueue<string>();
      results.Enqueue(string.Format("keyword1\tkeyword2\tkeyword3\tResults.Count\t" +
       "PR distinct\t" +
        "LKP distinct\t" +
        "RCM distinct\r\n"));
      Parallel.ForEach(dict, x =>
      {
        results.Enqueue(string.Format("{0},{1},{2},{3},{4},{5},{6}\r\n", x.Key.Item1, x.Key.Item2, x.Key.Item3, x.Value[0], x.Value[1], x.Value[2], x.Value[3]));
      });
      File.WriteAllText(Path.GetFullPath(directory + "\\result-distinct" +
          file), string.Concat(results.ToList()));

    }
    public static void WriteQuery4Different(string file, Dictionary<Tuple<int, int, int,int>, int[]> dict)
    {
      var results = new ConcurrentQueue<string>();
      results.Enqueue(string.Format("keyword1\tkeyword2\tkeyword3\tkeyword4\tResults.Count\t" +
       "PR distinct\t" +
        "LKP distinct\t" +
        "RCM distinct\r\n"));
      Parallel.ForEach(dict, x =>
      {
        results.Enqueue(string.Format("{0},{1},{2},{3},{4},{5},{6},{7}\r\n", x.Key.Item1, x.Key.Item2, x.Key.Item3, x.Key.Item4, x.Value[0], x.Value[1], x.Value[2], x.Value[3]));
      });
      File.WriteAllText(Path.GetFullPath(directory + "\\result-distinct" +
          file), string.Concat(results.ToList()));

    }
    private static void Calculate4QueriesRCM()
    {
      //(PR/TP), (PR/RCM), (TP/RCM)
      double[] q1_tp = new double[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
      double[] q1_pr = new double[] { 7.5, 7.5, 7.5, 7.5, 2, 7.5, 1, 7.5, 3, 4 };
      double[] q1_rmc = new double[] { 1, 4.5, 2, 4.5, 7.5, 3, 9, 7.5, 10, 6 };
      double[] q1_up = new double[] { 10, 9, 7, 8, 4, 6, 1, 5, 3, 2, };

      double[] q2_tp = new double[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
      double[] q2_pr = new double[] { 7.5, 7.5, 7.5, 7.5, 1, 7.5, 4, 2, 3, 7.5 };
      double[] q2_rmc = new double[] { 1, 6, 10, 2, 4, 5, 3, 9, 7, 8 };
      double[] q2_up = new double[] { 9, 10, 7, 8, 1, 6, 5, 2, 4, 3, };

      double[] q3_tp = new double[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
      double[] q3_pr = new double[] { 8.5, 8.5, 6, 8.5, 4.5, 4.5, 3, 2, 1, 8.5 };
      double[] q3_rmc = new double[] { 3.5, 1, 3.5, 6, 5, 8, 2, 9, 7, 10 };
      double[] q3_up = new double[] { 10, 9, 7, 8, 3, 6, 5, 2, 4, 1 };

      double[] q4_tp = new double[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
      double[] q4_pr = new double[] { 1, 2, 7.5, 7.5, 7.5, 7.5, 7.5, 7.5, 3, 4 };
      double[] q4_rmc = new double[] { 3.5, 8, 1, 5.5, 7, 2, 5.5, 3.5, 9, 10 };
      double[] q4_up = new double[] { 1, 2, 8, 3, 9, 10, 6, 7, 4, 5 };

      long c, d;
      Console.WriteLine("Queries & UP/TP & UP/PR & TP/PR & TP/RMC & PR/RMC \\\\ \\hline");
      Console.WriteLine("Life Insurance & {0:0.00} & {1:0.00} & {2:0.00} & {3:0.00} & {4:0.00}  \\\\ \\hline",
        KendallTau(q1_up, q1_tp, out c, out d),
        KendallTau(q1_up, q1_pr, out c, out d),
        KendallTau(q1_tp, q1_pr, out c, out d),
        KendallTau(q1_tp, q1_rmc, out c, out d),
        KendallTau(q1_pr, q1_rmc, out c, out d));
      Console.WriteLine("Natural Language, Semantic Analysis & {0:0.00} & {1:0.00}  & {2:0.00} & {3:0.00} & {4:0.00}  \\\\ \\hline",
        KendallTau(q2_up, q2_tp, out c, out d),
        KendallTau(q2_up, q2_pr, out c, out d),
        KendallTau(q2_tp, q2_pr, out c, out d),
        KendallTau(q2_tp, q2_rmc, out c, out d),
        KendallTau(q2_pr, q2_rmc, out c, out d));
      Console.WriteLine("Decision Making, Decision Support System, Knowledge Management & {0:0.00} & {1:0.00}  & {2:0.00} & {3:0.00} & {4:0.00}  \\\\ \\hline",
        KendallTau(q3_up, q3_tp, out c, out d),
        KendallTau(q3_up, q3_pr, out c, out d),
        KendallTau(q3_tp, q3_pr, out c, out d),
        KendallTau(q3_tp, q3_rmc, out c, out d),
        KendallTau(q3_pr, q3_rmc, out c, out d));
      Console.WriteLine("Query Language, Database System, Query Processing, Data Model & {0:0.00} & {1:0.00}  & {3:0.00} & {3:0.00} & {4:0.00}  \\\\ \\hline",
        KendallTau(q4_up, q4_tp, out c, out d),
        KendallTau(q4_up, q4_pr, out c, out d),
        KendallTau(q4_tp, q4_pr, out c, out d),
        KendallTau(q4_tp, q4_rmc, out c, out d),
        KendallTau(q4_pr, q4_rmc, out c, out d));
    }

    public static void CalculateQuery()
    {
      String query4 = "qu langu,datab system,qu proc,dat model";
      List<String> title4 = new List<String>();
      title4.Add("order sign tool dat model qu proc optim");
      title4.Add("plug pl qu algebr secon gener dbm developm environm");
      title4.Add("evalu impact dat model qu langu qu perform");
      title4.Add("sem imag datab system");
      title4.Add("cost ba optim dec support quer tran view");
      title4.Add("qu proc inm datab system");
      title4.Add("qu proc larg xml datab");
      title4.Add("sign tool dat model qu proc optim");
      title4.Add("eff manag object orient quer lat bind");
      title4.Add("adv dat proc kr model concept implem techniqu client server issu");

      List<String> paperAbstract4 = new List<String>();
      paperAbstract4.Add("prop framework specif ext datab system particu lar goal impl softwar compon par rl ba optim wid var dat model qu langu repr qu pr ce system key id order sign algebr system coupl sort signatur top level sign offer kind typ construc bottom level sign polymorph oper typ defin term top level top level def dat repr model bottom level describ qu algebr qu proc algebr show appl framework exampl drawn rel model qu proc");
      paperAbstract4.Add("abstract pr secon gener environm support implem datab system wid rang dat model qu langu hand framework flec common ext object rel system offer fl ext order sign form ba dat qu langu definit secon hand compl structur datab system toolkit ext provid concept algebr modl defin implem typ typ construc fact oper support funct register system fram review order sign es paper pr system funct uniform se user command val dat model ext system architectur mon dbm featur implem system fram pur dat model dep funct cod algebr modl support var tool describ key strateg ext qu proc secon environm explain structur algebr mod l");
      paperAbstract4.Add("import understand user util datab system effect enh perform major research interest evalu compar user perform dat model qu langu experim test combin model langu interest theor pract quest perform differ caus dat model addit qu langu syntac cognit model qu proc suggest measur stag dat model impact stag model qu langu syntac impact stag experim compar object orient rel model qu langu stag fresh result");
      paperAbstract4.Add("sem imag datab system oisdb design structur manag imag dat sem imag dat model pr describ structur context imag diagram qu langu qb qu flavor prop disc found object rewrit dat model describ structur cont imag incorpor typ construc funct typ inherit support composit model ent con pict dat alphanumer attribut interact qu langu user direct manipl pict datab diagram qu forml repr user view datab schem aspect oisdb schem design st manag map sem schem rel schem disc");
      paperAbstract4.Add("gener dec support appl cap proc hug amount dat requir abil integr reason dat multipl heter dat sourc dat sourc differ var aspect dat model qu langu support network protocol typ spread wid geograph ar cost proc dec support quer se high proc quer involut redund repeat acc dat sourc multipl execut simil proc sequ minim redund signif reduc qu proc cost paper prop architectur proc complec dec support quer involut multipl heter dat sourc introduc not tran view mater view ec context execut qu minim redund involut execut quer develop cost ba algorithm tak qu plan input gener optim cover plan minim redund origin plan val approach mean implem algorithm detail perform stud ba tpc benchmark quer commerc datab system fin compar contrast approach work rel ar ar answer quer view optim common expr experim demonstr pract us tran view signif improv perform dec support quer");
      paperAbstract4.Add("real world object natur complec relationship relationship pl rol context dep proper natur direct model situ prop dat model cal inform network model inm design inform specif langu isl inform manipl langu iml inform qu langu iql implem datab manag system ba inm qu langu explor natur graph structur inm object extract mean inform simpl conc compact paper foc qu proc inm datab system kind search strateg system automat select strateg eff proc quer user interv");
      paperAbstract4.Add("ext markup langu xml model rec gain hug popl abil repr wid var structur sem structur dat qu langu prop xml dat model wid xqu tradit xml qu proc research concentr intr docum dat retrief structur support paper review algorithm structur show superior proc indep xml docum direct appl algorithm proc quer interlink xml docum shown gener incorrect result ext ver algorithm structur introduc shown perform correct proc indep inter link xml docum");
      paperAbstract4.Add("prop framework specif ext datab system particu lar goal impl softwar compon par rl ba optim wid var dat model qu langu repr qu pr ce system key id order sign algebr system coupl sort signatur top level sign offer kind typ construc bottom level sign polymorph oper typ defin term top level top level def dat repr model bottom level describ qu algebr qu proc algebr show appl framework exampl drawn rel model qu proc");
      paperAbstract4.Add("support appl ar datab system mechan engineer appl off autom appl power dat model requir support model complec dat object orient model object orient model support subtyp inherit oper overload overrid featur as programmer man complec dat model desir featur power dat model abil inver func tion qu langu arbitr funct cal fn retrief argu ment result optim datab quer import larg datab system qu optim reduc execut cost dramat optim consider cost ba glob optim oper assign cost prior estim number object result util ind optim fl acc oper qu impl ment object orient dat model featur lead requir lat bound funct quer requir spec qu proc strateg achief good perform lat bound funct obstruc glob optim implem lat bound funct acc optim ind remain hid funct bod th ar qu proc approach man ag lat bound funct pr optim inver lat bound funct ind util funct lat bound abil system support model complec rel eff execut quer complec rel");
      paperAbstract4.Add("incr power modern computer stead open appl domain adv dat proc engineer knowledg ba appl me requir concept adv dat manag investig decad field object or coupl year datab group univer kaiserslautern develop adv datab system kr prototyp articl report result exper obtain project prim object ver kr prov sem featur expr dat model se orient qu langu deduc act capabil kr prototyp compl oper evalu featur stabil funct start develop appl system exper mark start point redesign kr major goal tun kr qu proc facil suit client server environm prov elabor mechan con control compr sem integr constraint mult user failur recov es aspect result client server architectur embod client sid dat manag need effect support adv appl gain requir system perform interact work project stag kr proper reflect es developm plac research adv datab system year subsequ disc sion bring number import aspect adv dat proc signif gener import gener appl datab system");
      Console.WriteLine("4");
      Console.WriteLine(matchingQueryResult(query4, title4, paperAbstract4));

      String query3 = "dec mak,dec support system,knowledg manag";

      List<String> title3 = new List<String>();
      List<String> paperAbstract3 = new List<String>();
      title3.Add("dec knowledg");
      title3.Add("ds knowledg manag inquir organ upd");
      title3.Add("dec support system green dat center");
      title3.Add("aec method");
      title3.Add("architectur real tim dec support system challeng solut");
      title3.Add("lever organ knowledg forml manufactur strateg");
      title3.Add("mobil knowledg manag dec support system automat conduc electron bus");
      title3.Add("proc dat wareh trac reus engineer design proc");
      title3.Add("flec support spat dec mak");
      title3.Add("identif qu fact dat wareh");

      paperAbstract3.Add("chapter explor connect dec knowledg show dec mak knowledg int endeavor dec support system technolog knowledg dec maker tim cost system dec mak produc agil innov reput");
      paperAbstract3.Add("courtn suggest organ problem complec wick surfac paradigm examin dec mak ds knowledg manag organ not multipl perspectiv stakeholder churchman inquir system specif singer inquirer integr conceptu solut paper sum origin manuscript disc result cont an conduc evalu work cit dec support system articl publ ten year ag emerg research stream top identif disc");
      paperAbstract3.Add("paper prop dec support system green comput dat center campus green comput aim developm technolog greener sustain planet work foc green dat center hous server campus server consum hug amount power incur cost cool rel oper challeng me demand eff accur dat center promot greener environm cost cut work prop dec support system ba dec tr ca ba reason min ec dat order as dec mak manag dat center aspect dat environm manag perspect carbon footprint therm profil virtu issu eff accur account prop paper outl work challeng involut prop approach build system prelimin evalu futur plan work interest dat knowledg manag commun environm scient research rel ar");
      paperAbstract3.Add("aim elabor dec support system man concr exper artif intellig method ca ba reason targ organ wish captur exploit empl exper paper foc key point method obtain system memor pr aec exper feedback method develop instr risk manager shar exper support crit task interv elabor aec ba an model risk manager real act esp dec mak knowledg manag proc aec result computer tool ba corpor memor paper review aec method illustr disc scenar rel forest fir fight manag paper describ method instr foc fe perspectiv futur method pr");
      paperAbstract3.Add("larg enterpr hug vol dat gener consum subst fract dat chang rapid bus manager dat inform mak tim sound bus dec conv dec support system prov low lat need dec mak rapid chang environm paper introduc not real tim dec support system distil requir system real lif outsourc exampl drawn ext exper develop depl system argu real tim dec support system complec comb el typ technolog enterpr integr real tim system workflow system knowledg manag dat wareh dat min describ approach addr challeng approach ba me broker paradigm enterpr integr comb paradigm workflow manag knowledg manag dynam dat wareh an conclus lesson learnt build system ba architectur approach disc hard research problem ar");
      paperAbstract3.Add("paper describ web ba system integr knowledg manag dec mak featur en short term commun pract develop manufactur strateg system ba defin model problem domain build organ act resourc assoc main strateg objectiv manufactur funct cost flec dep speed qu manager involut strateg developm proc asser posit issu consider argu favor posit se peer stiml knowledg elicit system captur inform organ memor rat build structur knowledg graph addit exploit reason mechan trigger tim user inser piec knowledg graph system recom solut dialogu inst framework prop paper distribut asynchron collabor strateg forml allow user surp requir plac work tim");
      paperAbstract3.Add("paper pr mobil knowledg manag dec support system mult agent technolog automat provid eff solut dec mak man electron bus architectur con user interfac assignm agent knowledg reason agent search agent knowledg ba datab model ba knowledg reason agent select rl knowledg ba fact datab reason suit solut match effect foreword chain reason method prov inst manag control develop autom agent manu period monit manager problem knowledg rl addit system mak asp combin visu ba build funct system ea implem design user transpar approach modif vit rl rev program sourc cod user ea man knowledg ba user modif ad drop latest knowledg rl system knowledg introduc system automat evalu accur act altern conduc electron bus fin purp improv fin manag neur network predict futur fin measur");
      paperAbstract3.Add("design developm proc complec techn system cruc import enterpr proc character high creat strong determin dynam tradit inform sc method int determin work proc effect appl support cr act conceptu synth an dec mak method exper manag exploit paper pr integr approach design proc guid ba captur proc trac proc dat wareh pdw produc design proc step structur st ext method trac trac captur facilit proc subsequ reus inform proc integr developm environm concept pdw evalu engineer design ca stud foc ph conceptu design ba engineer design chem produc plant");
      paperAbstract3.Add("dec maker perc dec mak proc solut complec spat problem unsatisfact lack gener cur spat dec support system sd fulfil specif objectiv fail addr requir effect spat problem solut inflec complec domain specif technolog progr incr opportun sd number domain flec support spat dec mak solut complec sem structur unstructur spat problem offer adv individu organ research attempt overcom problem identif field spat dec mak sd synth id framework architectur geograph inform system gi dec support system ds sd concept spat model model scenar lif cycl manag knowledg manag mult criter dec mak mcdm methodolog explor leverag implem flec spat dec support system fsd object orient concept technolog part research prop gener spat dec mak proc develop domain indep fsd framework architectur support proc implem prototyp fsd act proof concept spat dec mak proc fsd framework architectur");
      paperAbstract3.Add("popl dat wareh dat an grown trem dat wareh system emerg cor manag inform system dat wareh part larger infrastructur inclus leg dat sourc extern dat sourc dat acquisit softwar reposit an tool user interfac difficult dat wareh wid cit liter research fact init ong dat wareh implem succ rar fragm compreh review liter fact found crit DW implem succ organ techn project environm infrastructur inform qu system qu serv qu relationship qu net benefit prop research model develop paper determin impact qu succ fact implem dat wareh adapt upd delon mclean inform system succ model dim prop model");

      Console.WriteLine("3");
      Console.WriteLine(matchingQueryResult(query3, title3, paperAbstract3));

      String query2 = "natur langu,sem an";
      List<String> title2 = new List<String>();
      List<String> paperAbstract2 = new List<String>();
      title2.Add("autom descript stat imag natur langu");
      title2.Add("inform extract text ba sem infer");
      title2.Add("syynx solut pract knowledg manag med environm");
      title2.Add("sem annot epc model engineer domain facilit autom identif common model pract");
      title2.Add("unknown list sem an id integr deduc approach natur langu interfac design");
      title2.Add("estim automatiqu de la distribut de sen de term");
      title2.Add("discover implicit intent level knowledg natur langu text");
      title2.Add("knowledg ba imag retrief system");
      title2.Add("requir an tool tool automat an softwar requir docum");
      title2.Add("csiec computer as engl learn chatbot ba textu knowledg reason");
      paperAbstract2.Add("paper descript imag natur langu car main id imag object mov obtain phr span describ posit object order put descript effect plac theor model cognit sem an lingu unit preposit sobr en entr verb tocar touch real an establ rl determin relationship posit object");
      paperAbstract2.Add("grow inform extract text system perform enrich infer order discover extract inform argu reason cur lim approach sem ba ontolog expr thing repr nam seek draw infer extract inform ba disregard lingu prac natur langu paper describ gener architectur system ba sem infer prop model seek expr infer power concept concept combin sent structur contribut infer power sent demonstr val approach evalu depl appl extract inform crim report lin newspaper");
      paperAbstract2.Add("paper describ knowledg manag approach biom scientif commun develop syynx solut gmbh");
      paperAbstract2.Add("autom identif common model pract epc event driv proc chain model requir perform sem an epc funct event sem an fac problem es part epc sem bound natur langu expr funct event undefin proc sem sem annot natur langu expr adequ approach tackl problem paper introduc approach enabl autom sem annot epc funct event empl sem pattern an textu structur natur langu expr rel inst refer ontolog sem annot epc model el input subsequ sem an identif common model pract");
      paperAbstract2.Add("framework research origin natur langu proc deduc datab technolog deduc datab po superior funct relev eff solut problem pract appl ec broad acquaint main obstacl identif ab user fr interfac natur langu interfac prop optim candid spit vast number ambit attempt build natur langu front en achief result disappoint opinion main reason mi integr respon insuff perform wrong interpr integr deduc approach id interfac constitut integr part datab system guar con map user qu sem appl model paper foc sem an introduc unknown list uvl an techniqu oper direct evalu datab valu deep form funct word syntact an appl disambigu prov fe id approach ca stud design implem produc plan control system");
      paperAbstract2.Add("résumé De façon favoriser util du lang naturel dan le systèm ind de recherch textuel il sembl intér de disposer de donné perm un désam biguï lec satisf La distribut de sen possibl pour le term du lexiqu constitu un inform import ma dél déterminer nous développon actuel ment systèm an de aspect thématiqu de text de lec basé sur le vecteur conceptuel qu repré ensembl idé associé tout seg ment textuel mot expr text ce approch perm par part inform mutuel effectuer de la lec toutef quand le context sémantiqu est fa il est nécessair de considérer la fréqu usag nous cherchon ic obtenir par tir du corpus du web la distribut de sen pour le item lexicaux aprè avoir présenté le modèl de vecteur conceptuel la méthod an sémantiqu montré intérêt de disp ser un distribut rel de accept de item lexicaux nous introduison la not de sign lec ce signatur perm de construir estimateur de ce distribut nous formalison ce estimateur indiquon quelqu prem résultat encour abstract order promot usag natur langu inform system interest obtain lec dat allow satisfact lec disambigu distribut word sen import typ dat un obtain develop system an themat aspect text undertak word sen disambigu ba conceptu vect aim handl se id textu segm word locut sent text wsd con duc mutu inform shar vect sem context weak usag frequ obtain web sen distribut lec item pr conceptu vect model sem an principl show motiv sen dister but introduc not lec sign signatur build estim distribut form encour result");
      paperAbstract2.Add("paper prop approach autom discov implicit rh inform text ba evolut comput method order guid search rh connect natur langu text model prev obtain train inform involut sem structur criter main featur model design oper evalu funct disc experim as robust accur approach experim result show prom evolut method rh rol discov");
      paperAbstract2.Add("retrief system concentr low level featur color textur shap posit pr system develop ba visu descript imag color textur shap descript high level sem an imag cont proc modl prop architectur simil measur prop perform evalu imag browser retrief imag imag support qu natur langu pr system work onl offl unsuperv kohon organ map som techniqu train imag ind schem refer system ba tr som prop approach fuzz color histogram color retrief li descript retrief shap test appraoch mpeg imag");
      paperAbstract2.Add("pr tool cal requir an tool perform wid rang pract an softwar requir docum novelt approach user defin gl extract structur cont support broad rang syntact sem an allow user writ requir styl natur langu advoc exper requir writer sem web technolog leverag deeper sem an extract structur cont find kind problem requir docum");
      paperAbstract2.Add("csiec computer siml educ commun system new develop multipl funct engl instruc foc suppl virtu chat partner chatbot chat engl engl learner anytim gener commun respon user input dialogu context user person knowledg common sen knowledg infer knowledg kind knowledg expr form nlml annot langu natur langu text nlml automat obtain par text ea author gu edit design csiec system suggest naïv approach log reason infer direct syntact sem an textu knowledg approach adv eliz keyword match mechan chat log sum fr internet usag month demonstr adv paper pr system architectur underl technolog educ appl result");

      Console.WriteLine("2");
      Console.WriteLine(matchingQueryResult(query2, title2, paperAbstract2));
      String query1 = "lif insur";
      List<String> title1 = new List<String>();
      List<String> paperAbstract1 = new List<String>();
      title1.Add("intellig bus electron appl autom underwrit solut western southern lif insur compan");
      title1.Add("bus orient specif serv");
      title1.Add("long term evolut conceptu schem lif insur compan");
      title1.Add("predict insolut lif insurer neur network");
      title1.Add("cooper support off work insur bus");
      title1.Add("bus intellig ca stud lif insur industr");
      title1.Add("eiaw bus fr dat wareh sem web technolog");
      title1.Add("min insur dat sw lif");
      title1.Add("rol disson knowledg exchang ca stud knowledg manag system implem");
      title1.Add("compreh coh manag cap ca stud north american lif insur industr");
      paperAbstract1.Add("intellig bus electron appl autom underwrit solut western southern lif insur compan");
      paperAbstract1.Add("standard register search serv ud great weak standard technolog driv serv inadequ major aspect relev bus point view stand sharp contrast main prem so incr flec reus serv bus alignm speak langu compreh approach serv bus compon specif framework aspect framework bus task framework def prec task task identif paper prop tak enterpr ontolog start point task demonstr approach lif insur compan ca");
      paperAbstract1.Add("long term evolut conceptu schem lif insur compan");
      paperAbstract1.Add("past research stud docum failur insur regl inform ystem ir rov adequ warn insurer fin dister insolut result schol xamin ltern parameter parameter model predict insurer insolut stud neur network parameter altern pa st echniqu show methodolog effect predict insurer insolut parameter model");
      paperAbstract1.Add("off task rel proc contract sur bus complec high dep le gal compan specif form regl hand incr competit strong influ eff ci qu requir mak ac tiv demand crit compan suc ce paper describ main id el system implem sw lif prov computer ba guid interact support typ off task deal priv lif insur system li knowledg repr langu cover dat proc aspect inclus encod rel vant regl context ba specif langu compiler enabl autom gener execut system sketch emb el gener adv softwar architectur wrt organ memor funct wor flow manag bus proc model");
      paperAbstract1.Add("bus intellig tod technolog integr solut bus bus requir key fact driv technolog innov ca stud lif insur industr introduc paper demonstr innov BI infrastructur appl effect addr major bus challeng chin lif insur industr achief oper excel bus impact");
      paperAbstract1.Add("dat wareh wid bus anal ys dec mak proc adapt rapid chang bus environm develop tool mak dat wareh bus fr sem web technolog main id mak bus sem explicit uniform repr bus me ad conceptu enterpr dat model model ext owl langu map bus metad schem dat wareh built an request ra is custom dat mart dat popl dat wareh automat gener built knowledg tool cal enterpr inform as workbench eiaw depl taikang lif insur compan top insur mpan chin user feedback show owl excel ba repr bus sem dat wareh ext need real appl user deem tool help flec speed dat mar deploym fac bus");
      paperAbstract1.Add("hug ma digit dat produc customer competit avail compan serv sect order exploit inh hid den knowledg improv bus pr ce appl dat min technol ogy reach good ef fic result op pur manu interact dat explor paper report step project init sw lif min dat resourc lif insur bus ba dat wareh ma collect relev dat oltp system proc priv lif insur contract dat min ing environm se integr pal tool autom dat an mach learn approach");
      paperAbstract1.Add("stud examin adopt knowledg manag system knowledg exchang distribut group lif insur exper user particip enthusiasm design proc provid funct dat specif system month introduc system an ca suggest system understood term disson domin them observ soc dynam implem disson observ dispar ment model system intent dispar ment model knowledg knowledg ownership rel power disson spirit knowledg shar imbu mi system challeng rel power posit affect stakeholder group understand impl issu inform requir engineer proc kind softwar appl");
      paperAbstract1.Add("prof research continu seek guid der sustain bus investm inform system identif specif govern mechan empl ensur posit result build resourc ba theor firm long term interpr field stud strateg plan proc conduc prop crit organ cap cal manag es effect plan prev stud view plan term discr investm dec stud examin compan achief bus int bus init tim ser adapt ong proc find ca stud suggest coh integr specif proc crit attain sustain bus investm prop coh manag cap firm int strateg init lead improv bus perform stud suggest import aspect manag proc");
      Console.WriteLine("1");
      Console.WriteLine(matchingQueryResult(query1, title1, paperAbstract1));

      Console.ReadKey();
     
    }
  }
}
