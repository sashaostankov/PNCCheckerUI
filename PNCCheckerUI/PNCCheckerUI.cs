//
// Ostankov Alexander
// Higher School of Economics
// sashaostankov@gmail.com
// (c) 2015
//

using System;
using System.IO;
using PNCChecker;
using System.Text;

namespace PNCCheckerUI
{
    class PNCCheckerUI
    {
        public static void Main()
        {
            Console.WriteLine("Программа расчета соответствия сети Петри и лога");
            Console.WriteLine("© Останков Александр Юрьевич, 2015, sashaostankov@gmail.com");
            Console.WriteLine("");

            string netFile = GetString("Введите название файла с сетью Петри:",
                "Файл не найден, попробуйте заново:",
                IsValidFile);

            string logFile = GetString("Введите название файла с логом:",
                "Файл не найден, попробуйте заново:",
                IsValidFile);


            try
            {
                var checker = new PNCChecker.PNCChecker();

                checker.OnChangeAlignmentsAmount += Started;
                checker.OnGenerateNewAlignment += PrintNewAlignement;

                var res = checker.Calculate(netFile, logFile);

                Console.WriteLine("Алгоритм закончил работу.");

                if (res >= 0.0)
                    Console.WriteLine("Соответствие между логом и сетью Петри равно {0}",
                        res);
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("В папке с программой не найден один из файлов PNCChecker.dll или PNTFReader.dll");
            }
            catch
            {
                Console.WriteLine("Программа экстренно завершила работу");
            }

            Console.WriteLine("Для выхода из программы нажмите любую клавишу");
            Console.ReadKey();
        }

        public static string GetString(
            string msg,
            string error,
            Func<string, bool> check
        )
        {
            if (msg.Length > 0)
                Console.WriteLine(msg);

            string res;

            while (!check(res = Console.ReadLine()))
            {
                if (error.Length > 0)
                    Console.WriteLine(error);
            }

            return res;
        }

        public static bool IsValidFile(string filename)
        {
            try
            {
                var File = new FileInfo(filename);
                return File.Exists;
            }
            catch
            {
                return false;
            }
        }

        public static void Started(int numbers)
        {
            Console.WriteLine("Начинаем обработку данных...");
            Console.WriteLine("Найдено записей в лог-файле: {0}", numbers);
        }

        public static void PrintNewAlignement(ComparableAlignment align)
        {
            var print = new StringBuilder();

            print.AppendFormat("Количество вхождений лог-трассы в файл = {0}\n",
                align.Amount);
            print.AppendFormat("Стоимость вставки события в лог = {0}\n",
                align.CostMoveInLog);
            print.AppendFormat("Стоимость вставки события в сеть = {0}\n",
                align.CostMoveInNet);

            print.AppendFormat("Оценка \"расстояния\" между логом и сетью = {0}\n",
                align.GetDistance());

            print.AppendFormat("Лог:  '{0}'\n",
                string.Join("'\t'",
                    align.Log));
            print.AppendFormat("Сеть: '{0}'\n",
                string.Join("'\t'",
                    align.Net));

            Console.WriteLine(print);
        }
    }
}
