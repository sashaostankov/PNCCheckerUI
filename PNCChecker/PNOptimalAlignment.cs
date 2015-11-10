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
    public static class PNOptimalAlignment
    {
        /// <summary>
        /// Находит оптимальное выравнивание между сетью Петри и данным логом работы.
        /// </summary>
        /// <returns>Оптимальное выравнивание.</returns>
        /// <param name="net">Сеть Петри.</param>
        /// <param name="needLog">Необходимый лог.</param>
        public static Alignment CalculateOptimalAlignment(
            PetriNet net,
            List<string> needLog
        )
        {
            var align = new ComparableAlignment();
            var mark = new SortedDictionary<int, uint>(net.MarkingS);
            var set = new SortedSet<PNState>();

            set.Add(new PNState(align, mark, needLog));

            while (set.Count > 0)
            {
                var current = set.Min;
                var newState = (PNState)current.Clone();
                var nextNeed = string.Empty;
                var index = current.Alignment.GetLastCommonIndex(needLog);

                set.Remove(set.Min);

                if (net.IsEndMarking(current.Marking))
                {
                    for (int i = index; i < needLog.Count; i++)
                        current.Alignment.AddElement(needLog[i], "");

                    return current.Alignment;
                }

                if (index < needLog.Count)
                {
                    nextNeed = needLog[index];
                    newState.Alignment.AddElement(nextNeed, "");
                    set.Add(newState);
                }

                foreach (var item in net.Nodes)
                {
                    if (item is Place)
                        continue;

                    var transition = item as Event;

                    net.LoadMarking(current.Marking);

                    if (!transition.CanPlayEvent())
                        continue;

                    newState = (PNState)current.Clone();
                    transition.PlayEvent();
                    newState.Marking = new SortedDictionary<int, uint>(net.GetMarking());

                    if (transition.Label.Length == 0)
                    {
                    } // Возможно, здесь что-то должно быть
                    else if (transition.Label == nextNeed)
                        newState.Alignment.AddElement(nextNeed, nextNeed);
                    else if (transition.Label.Length > 0)
                        newState.Alignment.AddElement("", transition.Label);

                    set.Add((PNState)newState.Clone());
                }
            }

            return new Alignment();
        }
    }

    /// <summary>
    /// Класс описывает состояние сети Петри.
    /// </summary>
    public class PNState : IComparable<PNState>, ICloneable
    {
        /// <summary>
        /// Необходимый лог.
        /// </summary>
        public List<string> NeedLog = new List<string>();

        /// <summary>
        /// Выравнивание.
        /// </summary>
        public ComparableAlignment Alignment = new ComparableAlignment();

        /// <summary>
        /// Маркировка сети.
        /// </summary>
        public SortedDictionary<int, uint> Marking = new SortedDictionary<int, uint>();

        /// <summary>
        /// Клонирует текущий объект.
        /// </summary>
        public virtual object Clone()
        {
            return new PNState(Alignment, Marking, NeedLog);
        }

        /// <Docs>To be added.</Docs>
        /// <para>Returns the sort order of the current instance compared to the specified object.</para>
        /// <summary>
        /// Сравнивает два сосотяние.
        /// </summary>
        /// <returns>Порядок сортировки.</returns>
        /// <param name="anotherState">Второе состояние.</param>
        public virtual int CompareTo(PNState anotherState)
        {
            if (anotherState == null)
                return -1;

            if (GetHashCode() == anotherState.GetHashCode())
                return 0;

            if (Alignment.Evaluation(NeedLog) < anotherState.Alignment.Evaluation(NeedLog))
                return -1;

            if (Alignment.Evaluation(NeedLog) > anotherState.Alignment.Evaluation(NeedLog))
                return 1;

            if (Marking.Count < anotherState.Marking.Count)
                return -1;

            if (Marking.Count > anotherState.Marking.Count)
                return 1;

            if (Alignment.Size < anotherState.Alignment.Size)
                return -1;

            if (GetHashCode() > anotherState.GetHashCode())
                return 1;

            return -1;
        }

        /// <summary>
        /// Инициализирует новый объект класса <see cref="PNCChecker.PNState"/>.
        /// </summary>
        /// <param name="align">Выравнивание.</param>
        /// <param name="mark">Маркировка.</param>
        /// <param name="need">Необходимый лог.</param>
        public PNState(ComparableAlignment align,
            SortedDictionary<int, uint> mark,
            List<string> need)
        {
            if (align == null) throw new ArgumentNullException("align");
            if (mark == null)  throw new ArgumentNullException("mark");
            if (need == null)  throw new ArgumentNullException("need");

            Alignment = (ComparableAlignment)align.Clone();
            Marking = new SortedDictionary<int, uint>(mark);
            NeedLog = new List<string>(need);
        }
    }
}