//
//  Ostankov Alexander
//  Higher School of Economics
//  sashaostankov@gmail.com
//  (c) 2015
//

using System;
using System.Collections.Generic;
using PNTFReader;

namespace PNCChecker
{
    public class PNCChecker
    {
        /// <summary>
        /// Сообщает, когда обработано и получено очереное выравнивание.
        /// </summary>
        public event Action<ComparableAlignment> OnGenerateNewAlignment;

        /// <summary>
        /// Сообщает, когда изменилось количество выравниваний.
        /// </summary>
        public event Action<int> OnChangeAlignmentsAmount;

        /// <summary>
        /// Находит соответствие между сетью Петри и логом.
        /// </summary>
        /// <param name="netFileName">Имя файла с сетью Петри.</param>
        /// <param name="logFileName">Имя файла с логом.</param>
        public double Calculate(string netFileName, string logFileName)
        {
            try
            {
                // Читаем сеть
                var net = FileReader.PNReadNet(netFileName);

                // Читаем лог
                var log = FileReader.PNReadLog(logFileName);

                // Создаем сеть
                var petriNet = new PetriNet(net);

                // Создаем массив для alignments
                var perfectAlignments = new List<Alignment>();

                // Вызываем событие для передачи количества уникальных логов
                // Может потребоваться, например, для заполнения ProgressBar
                OnChangeAlignmentsAmount(log.Count);

                // пробегаемся по каждому кейсу лога
                foreach (var logCase in log)
                {
                    // Получаем alignment
                    var alignment = (ComparableAlignment)PNOptimalAlignment.CalculateOptimalAlignment(petriNet,
                        logCase.Key);

                    // Сколько раз встречается в логе
                    alignment.Amount = logCase.Value;

                    // Смотрим на полученный alignment
                    //Console.WriteLine (alignment.ToString (logCase.Key) + "\n");

                    // Вызываем события появления нового alignment и передаем его
                    OnGenerateNewAlignment(alignment);

                    // Добавляем в массив всех alignments
                    perfectAlignments.Add(alignment);
                }

                // возвращаем Fitness
                return PNConformance.GetFitness(perfectAlignments);
            }
            catch (Exception a)
            {
                Console.WriteLine("Исключение: " + a.Message);

                return -1.0;
            }
        }
    }
}