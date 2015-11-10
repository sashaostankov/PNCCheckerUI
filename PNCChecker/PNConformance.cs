//
// Ostankov Alexander
// Higher School of Economics
// sashaostankov@gmail.com
// (c) 2015
//

using System;
using System.Collections.Generic;

namespace PNCChecker
{
    public static class PNConformance
    {
        /// <summary>
        /// Получает Fitness (соответствие) между сетью Петри и логом по найденому выравниванию.
        /// </summary>
        /// <returns>Fitness (соответствие).</returns>
        /// <param name="alignments">Найденные выоавнивания.</param>
        public static double GetFitness(List<Alignment> alignments)
        {
            int sumSkipped = 0;
            int sumInserted = 0;
            int sumAll = 0;

            foreach (var item in alignments)
            {
                sumSkipped  += item.Amount * item.CostMoveInNet * item.AmountMovesInNet;
                sumInserted += item.Amount * item.CostMoveInLog * item.AmountMovesInLog;
                sumAll += item.Amount * item.Size * Math.Max(item.CostMoveInLog, item.CostMoveInNet);
                /*
                sumSkipped += item.Amount * item.CostMoveInNet * item.AmountMovesInNet;
                sumInserted += item.Amount * item.CostMoveInLog * item.AmountMovesInLog;
                sumAll += item.Amount * (item.CostMoveInNet * (item.Size - item.AmountMovesInNet) +
                item.CostMoveInLog * (item.Size - item.AmountMovesInLog));
                */
            }


            if (sumAll == 0)
                return 1;

            return (double)1 - (double)(sumSkipped + sumInserted) / sumAll;
        }
    }
}