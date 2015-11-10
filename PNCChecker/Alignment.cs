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
    /// <summary>
    /// Выравнивание.
    /// Соответствие между сетью Петри и логом работы.
    /// </summary>
    public class Alignment : ICloneable
    {
        /// <summary>
        /// Стандартная цена вставки события в лог.
        /// </summary>
        protected const int DefaultCostMoveInLog = 1;

        /// <summary>
        /// Стандартная цена вставки события в лог сети.
        /// </summary>
        protected const int DefaultCostMoveInNet = 1;

        /// <summary>
        /// Нужный лог работы.
        /// </summary>
        public List<string> Log;

        /// <summary>
        /// Лог работы сети.
        /// </summary>
        public List<string> Net;

        /// <summary>
        /// Получает размер (длину) выравнивания.
        /// </summary>
        /// <value>Размер текущего выравнивания.</value>
        public int Size { get { return Log.Count; } }

        /// <summary>
        /// Получает или задает цену вставки события в лог.
        /// </summary>
        /// <value>Цена вставки события в лог.</value>
        public int CostMoveInLog { get; set; }

        /// <summary>
        /// Получает или задает цену вставки события в лог сети.
        /// </summary>
        /// <value>Цена вставки события в сеть.</value>
        public int CostMoveInNet { get; set; }

        /// <summary>
        /// Получает или задает количество повторений этого выравнивания в логе событий.
        /// </summary>
        /// <value>Количество повторений в логе.</value>
        public int Amount { get; set; }

        /// <summary>
        /// Получает или задает количетсво вставок событий в лог в выравнивании.
        /// </summary>
        /// <value>Количество вставок событий в лог.</value>
        public int AmountMovesInLog { protected set; get; }

        /// <summary>
        /// Получает или задает количество вставок событий в лог работы сети 
        /// </summary>
        /// <value>Количество вставок событий в лог сети.</value>
        public int AmountMovesInNet { protected set; get; }

        /// <summary>
        /// Получает количество вставок событий в лог.
        /// Также обновляет значение свойства AmountMovesInLog.
        /// Использовать (!)ТОЛЬКО когда нужно обновить значение свойства AmountMovesInLog.
        /// </summary>
        /// <returns>Количество вставок событий в лог.</returns>
        protected int GetAmountMovesInLog()
        {
            int res = 0;

            for (int i = 0; i < Log.Count; i++)
            {
                if (Log[i].Length > 0 &&
                    Net[i].Length == 0)
                    res++;
            }

            return AmountMovesInLog = res;
        }

        /// <summary>
        /// Получает количество вставок событий в лог сети.
        /// Также обновляет значение свойства AmountMovesInNet.
        /// Использовать (!)ТОЛЬКО когда нужно обновить значение свойства AmountMovesInNet.
        /// </summary>
        /// <returns>Количество вставок событий в лог сети.</returns>
        protected int GetAmountMovesInNet()
        {
            int res = 0;

            for (int i = 0; i < Log.Count; i++)
            {
                if (Net[i].Length > 0 &&
                    Log[i].Length == 0)
                    res++;
            }

            return AmountMovesInNet = res;
        }

        /// <summary>
        /// Проверяет на корректность текущее выравнивание.
        /// </summary>
        /// <returns><c>true</c> если выравниевание корректно; иначе <c>false</c>.</returns>
        protected bool IsValidAlignment()
        {
            if (Log.Count != Net.Count)
                return false;

            for (int i = 0; i < Log.Count; i++)
            {
                if (!string.IsNullOrEmpty(Log[i]) &&
                    !string.IsNullOrEmpty(Net[i]) &&
                    Log[i] != Net[i])
                    return false;
            }

            return true;
        }

        public Alignment()
        {
            // Default values
            CostMoveInLog = DefaultCostMoveInLog;
            CostMoveInNet = DefaultCostMoveInNet;

            Amount = 1;

            Log = new List<string>();
            Net = new List<string>();
        }

        public Alignment(List<string> log, List<string> net)
            : this()
        {
            if (net == null) throw new ArgumentNullException("net");
            if (log == null) throw new ArgumentNullException("log");

            Log = new List<string>(log);
            Net = new List<string>(net);

            AmountMovesInLog = GetAmountMovesInLog();
            AmountMovesInNet = GetAmountMovesInNet();

            if (!IsValidAlignment())
                throw new Exception("Задан неправильный Alignment!\n" + this);
        }

        /// <summary>
        /// Клонирует текущий объект.
        /// </summary>
        public virtual object Clone()
        {
            var res = new Alignment(Log, Net);

            res.Amount = Amount;
            res.CostMoveInLog = CostMoveInLog;
            res.CostMoveInNet = CostMoveInNet;

            return res;
        }

        /// <summary>
        /// Возвращает строку <see cref="System.String"/>, которая описывает текущий <see cref="PNCChecker.Alignment"/>.
        /// </summary>
        /// <returns>A <see cref="System.String"/> описывает текущий <see cref="PNCChecker.Alignment"/>.</returns>
        public override string ToString()
        {
            string res = string.Format("Amount = {0}, CostMoveLog = {1}, CostMoveNet = {2}\n",
                Amount, CostMoveInLog, CostMoveInNet);

            res += "'" + string.Join("'\t'", Log) + "'\n";
            res += "'" + string.Join("'\t'", Net) + "'";

            return res;
        }
    }

    /// <summary>
    /// Comparable alignment.
    /// </summary>
    public class ComparableAlignment : Alignment, ICloneable
    {
        /// <summary>
        /// Возвращает индекс последнего схожего элемента из двух логов.
        /// </summary>
        /// <returns>Последний совпадающий индекс.</returns>
        /// <param name="needLog">Необходимый лог.</param>
        public int GetLastCommonIndex(List<string> needLog)
        {
            int a = 0;
            int b = 0;

            for (; a < Log.Count && b < needLog.Count; a++)
                if (Log[a] == needLog[b])
                    b++;

            return b;
        }

        /// <summary>
        /// Эвристическая функция
        /// </summary>
        /// <returns>Возвращает оставшееся расстояние.</returns>
        /// <param name="needLog">Необходимый лог.</param>
        public virtual int GetHeuristic(List<string> needLog)
        {
            return needLog.Count - GetLastCommonIndex(needLog);
        }

        /// <summary>
        /// Кратчайшее расстояние от начальной разметки до текущей.
        /// </summary>
        /// <returns>Расстояние.</returns>
        public virtual int GetDistance()
        {
            return AmountMovesInLog * CostMoveInLog
                + AmountMovesInNet * CostMoveInNet;
        }

        /// <summary>
        /// Оценочное расстояние для текущего выравнивания.
        /// F(v) = Distance(v) + Heuristic(v);
        /// </summary>
        /// <param name="needLog">Необходимый лог.</param>
        public virtual int Evaluation(List<string> needLog)
        {
            return GetDistance() + GetHeuristic(needLog);
        }

        /// <summary>
        /// Добавляет элементы в лог и лог сети.
        /// </summary>
        /// <param name="inLog">Элемент для вставки в лог.</param>
        /// <param name="inNet">Элемент для вставки в сеть..</param>
        public void AddElement(string inLog, string inNet)
        {
            Log.Add(inLog);
            Net.Add(inNet);

            bool log = !string.IsNullOrEmpty(inLog);
            bool net = !string.IsNullOrEmpty(inNet);

            if (log && !net)
                AmountMovesInLog++;
            if (net && !log)
                AmountMovesInNet++;
        }

        public ComparableAlignment()
        {
        }

        public ComparableAlignment(List<string> log, List<string> net)
            : base(log, net)
        {
        }

        /// <summary>
        /// Клонирует текущий объект.
        /// </summary>
        public override object Clone()
        {
            var res = new ComparableAlignment(Log, Net);

            res.Amount = Amount;
            res.CostMoveInLog = CostMoveInLog;
            res.CostMoveInNet = CostMoveInNet;

            return res;
        }

        public string ToString(List<string> needLog)
        {
            if (needLog == null)
                needLog = Log;

            var intermid = new List<String>(base.ToString().Split(new []{ '\n' }));

            intermid.Insert(1, string.Format("Distance = {0}, Heuristic = {1}, Evaluation = {2}",
                GetDistance(), GetHeuristic(needLog), Evaluation(needLog)));

            return string.Join("\n", intermid);
        }

        public override string ToString()
        {
            return ToString(null);
        }
    }
}