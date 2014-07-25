using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RankCorrelation
{
  public class Inversions
  {

    // merge and count
    private static long merge(IComparable[] a, IComparable[] aux, int lo, int mid, int hi)
    {
      long inversions = 0;

      // copy to aux[]
      for (int k = lo; k <= hi; k++)
      {
        aux[k] = a[k];
      }

      // merge back to a[]
      int i = lo, j = mid + 1;
      for (int k = lo; k <= hi; k++)
      {
        if (i > mid) a[k] = aux[j++];
        else if (j > hi) a[k] = aux[i++];
        else if (less(aux[j], aux[i])) { a[k] = aux[j++]; inversions += (mid - i + 1); }
        else a[k] = aux[i++];
      }
      return inversions;
    }

    // return the number of inversions in the subarray b[lo..hi]
    // side effect b[lo..hi] is rearranged in ascending order
    private static long count(IComparable[] a, IComparable[] b, IComparable[] aux, int lo, int hi) {
        long inversions = 0;
        if (hi <= lo) return 0;
        int mid = lo + (hi - lo) / 2;
        inversions += count(a, b, aux, lo, mid);  
        inversions += count(a, b, aux, mid+1, hi);
        inversions += merge(b, aux, lo, mid, hi);
        Debug.Assert( inversions == brute(a, lo, hi));
        return inversions;
    }


    // count number of inversions in the array a[] - do not overwrite a[]
    public static long count(IComparable[] a)
    {
      IComparable[] b = new IComparable[a.Length];
      IComparable[] aux = new IComparable[a.Length];
      for (int i = 0; i < a.Length; i++) b[i] = a[i];
      long inversions = count(a, b, aux, 0, a.Length - 1);
      return inversions;
    }


    // is v < w ?
    private static bool less(IComparable v, IComparable w)
    {
      return (v.CompareTo(w) < 0);
    }

    // count number of inversions in a[lo..hi] via brute force (for debugging only)
    private static long brute(IComparable[] a, int lo, int hi)
    {
      long inversions = 0;
      for (int i = lo; i <= hi; i++)
        for (int j = i + 1; j <= hi; j++)
          if (less(a[j], a[i])) inversions++;
      return inversions;
    }


    // count number of inversions via brute force
    public static long brute(IComparable[] a)
    {
      return brute(a, 0, a.Length - 1);
    }
    // return Kendall tau distance between two permutations
    public static long Kendalldistance(int[] a, int[] b)
    {
      if (a.Length != b.Length)
      {
        throw new Exception("Array dimensions disagree");
      }
      int N = a.Length;

      int[] ainv = new int[N];
      int i = 0;
      for (i = 0; i < N; i++) ainv[a[i]] = i;

      IComparable[] bnew = new IComparable[N];
    
      try
      {
        for (i = 0; i < N; i++) bnew[i] = ainv[b[i]];
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message + ex.StackTrace.ToString());
      }
      return count(bnew);
    }
  }
}
