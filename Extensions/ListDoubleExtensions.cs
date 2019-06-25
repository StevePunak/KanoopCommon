using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KanoopCommon.Extensions
{
	public static class ListDoubleExtensions
	{
		public static Double Median(this List<Double> values)
		{
			Double median = 0;
			if(values.Count == 0)
			{
				throw new InvalidOperationException("List contains no elements");
			}

			/** copy the list and make a new one, sorted */
			List<Double> list = new List<Double>(values);
			list.Sort();

			/** find the center item(s) */
			Double count = list.Count;
			Double centerItem = count / 2;
			if(centerItem / 2 == Math.Floor(centerItem / 2))
			{
				/** even numbered list, will need an average */
				median = (list[(int)centerItem - 1] + list[(int)centerItem]) / 2;
			}
			else
			{
				median = list[(int)centerItem];
			}

			return median;
		}

		public static Double LowerQuartile(this List<Double> values)
		{
			Double quartile = 0;
			if(values.Count < 4)
			{
				throw new InvalidOperationException("List contains must contain at least 4 elements");
			}

			/** copy the list and make a new one, sorted */
			List<Double> list = new List<Double>(values);
			list.Sort();

			/** find the center item(s) */
			Double count = list.Count;
			Double centerItem = count *.5;
			if(centerItem *.5 == Math.Floor(centerItem * .5))
			{
				/** even numbered list, will need an average */
				int index1 = (int)count / 4 - 1;
				int index2 = index1 + 1;
				/** two center items */
				quartile = (list[index1] + list[index2]) / 2;
			}
			else
			{
				int index1 = (int)count / 4;
				quartile = list[index1];
			}

			return quartile;
		}

		public static Double UpperQuartile(this List<Double> values)
		{
			Double quartile = 0;
			if(values.Count < 4)
			{
				throw new InvalidOperationException("List contains must contain at least 4 elements");
			}

			/** copy the list and make a new one, sorted */
			List<Double> list = new List<Double>(values);
			list.Sort();

			/** find the center item(s) */
			Double count = list.Count;
			Double centerItem = count *.5;
			if(centerItem *.5 == Math.Floor(centerItem * .5))
			{
				/** even numbered list, will need an average */
				int index1 = (int)(count *.75) - 1;
				int index2 = index1 + 1;
				/** two center items */
				quartile = (list[index1] + list[index2]) / 2;
			}
			else
			{
				int index1 = (int)(count *.75);
				quartile = list[index1];
			}

			return quartile;
		}
		/// <summary>
		/// Creates the population mean
		/// </summary>
		/// <param name="values">list of sample values</param>
		/// <returns>the mean</returns>
		public static double Mean(this List<double> values)
		{
			return values.Count == 0 ? 0 : values.Mean(0, values.Count);
		}

		/// <summary>
		/// Creates the population mean
		/// </summary>
		/// <param name="values">list of sample values</param>
		/// <param name="start">start index</param>
		/// <param name="end">end index</param>
		/// <returns>the mean</returns>
		public static double Mean(this List<double> values, int start, int end)
		{
			double s = 0;

			for (int i = start; i < end; i++)
			{
				s += values[i];
			}

			return s / (end - start);
		}

		/// <summary>
		/// Calculate the population variance
		/// </summary>
		/// <param name="values">list of sample values</param>
		/// <returns>the variance (sigma square) of the sample</returns>
		public static double Variance(this List<double> values)
		{
			return values.Variance(values.Mean(), 0, values.Count);
		}

		/// <summary>
		/// Calculate the population variance
		/// </summary>
		/// <param name="values">list of sample values</param>
		/// <param name="mean">the population mean</param>
		/// <returns>the variance (sigma square) of the sample</returns>
		public static double Variance(this List<double> values, double mean)
		{
			return values.Variance(mean, 0, values.Count);
		}

		/// <summary>
		/// Calculate the population variance
		/// </summary>
		/// <param name="values">list of sample values</param>
		/// <param name="mean">the population mean</param>
		/// <param name="start">value start index</param>
		/// <param name="end">value end index</param>
		/// <returns>the variance (sigma square) of the sample</returns>
		public static double Variance(this List<double> values, double mean, int start, int end)
		{
			double variance = 0;

			for (int i = start; i < end; i++)
			{
				variance += Math.Pow((values[i] - mean), 2);
			}

			int n = end - start;
			if (start > 0) n -= 1;

			return variance / (n);
		}

		/// <summary>
		/// Calculates the standard deviation of a sample set
		/// </summary>
		/// <param name="values">list of values in the sample</param>
		/// <returns>the calculated standard deviation</returns>
		public static double StandardDeviation(this List<double> values)
		{
			return values.Count == 0 ? 0 : values.StandardDeviation(0, values.Count);
		}

		/// <summary>
		/// Calculates the standard deviation of a sample set
		/// </summary>
		/// <param name="values">list of values in the sample</param>
		/// <param name="start">start index of sample</param>
		/// <param name="end">end index of population</param>
		/// <returns>the calculated standard deviation</returns>
		public static double StandardDeviation(this List<double> values, int start, int end)
		{
			double mean = values.Mean(start, end);
			double variance = values.Variance(mean, start, end);

			return Math.Sqrt(variance);
		}
	}
}
