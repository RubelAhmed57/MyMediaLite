// Copyright (C) 2010 Steffen Rendle, Zeno Gantner
// Copyright (C) 2011 Zeno Gantner
//
// This file is part of MyMediaLite.
//
// MyMediaLite is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// MyMediaLite is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with MyMediaLite.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using MyMediaLite.DataType;
using MyMediaLite.Util;

namespace MyMediaLite.Correlation
{
	/// <summary>Class for storing cosine similarities</summary>
	public class Cosine : CorrelationMatrix
	{
		/// <summary>Creates an object of type Cosine</summary>
		/// <param name="num_entities">the number of entities</param>
		public Cosine(int num_entities) : base(num_entities) { }

		/// <summary>Copy constructor. Creates an object of type Cosine from an existing one.</summary>
		/// <param name ="correlation_matrix">the correlation matrix to copy</param>
		public Cosine(CorrelationMatrix correlation_matrix) : base(correlation_matrix.NumberOfRows)
		{
			this.data = correlation_matrix.data;
		}

		/// <summary>Creates a Cosine similarity matrix from given data</summary>
		/// <param name="vectors">the boolean data</param>
		/// <returns>the similarity matrix based on the data</returns>
		static public CorrelationMatrix Create(SparseBooleanMatrix vectors)
		{
			CorrelationMatrix cm;
			int num_entities = vectors.NumberOfRows;
			try
			{
				cm = new Cosine(num_entities);
			}
			catch (OverflowException)
			{
				Console.Error.WriteLine("Too many entities: " + num_entities);
				throw;
			}
			cm.ComputeCorrelations(vectors);
			return cm;
		}

		/// <inheritdoc/>
		public override void ComputeCorrelations(SparseBooleanMatrix entity_data)
		{
			var transpose = entity_data.Transpose(); // TODO save memory by having a fixed relation here ...
			
			var overlap = new SparseMatrix<int>(entity_data.NumberOfRows, entity_data.NumberOfRows);
			
			// go over all (other) entities
			foreach (var row_id in transpose.NonEmptyRowIDs)
			{
				var row = transpose[row_id].ToList();
				
				for (int i = 0; i < row.Count; i++)
				{
					int x = row[i];
					
					for (int j = i + 1; j < row.Count; j++)
					{
						int y = row[j];
						
						// ensure x < y
						if (x > y)
						{
							int tmp = x;
							x = y;
							y = tmp;
						}

						overlap[x, y]++;
					}
				}
			}
			
			// the diagonal of the correlation matrix
			for (int i = 0; i < num_entities; i++)
				this[i, i] = 1;

			// compute cosine
			foreach (var index_pair in overlap.NonEmptyEntryIDs)
			{
				int x = index_pair.First;
				int y = index_pair.Second;
				
				this[x, y] = (float) (overlap[x, y] / Math.Sqrt(entity_data[x].Count * entity_data[y].Count));
			}
						
		}

		/// <summary>Computes the cosine similarity of two binary vectors</summary>
		/// <param name="vector_i">the first vector</param>
		/// <param name="vector_j">the second vector</param>
		/// <returns>the cosine similarity between the two vectors</returns>
		public static float ComputeCorrelation(HashSet<int> vector_i, HashSet<int> vector_j)
		{
			int cntr = 0;
            foreach (int k in vector_j)
            	if (vector_i.Contains(k))
	            	cntr++;
            return (float) cntr / (float) Math.Sqrt(vector_i.Count * vector_j.Count);
		}
	}
}