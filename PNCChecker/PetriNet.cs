//
//  Ostankov Alexander
//  Higher School of Economics
//  sashaostankov@gmail.com
//  (c) 2015
//

using System;
using System.Collections.Generic;

using Property = System.Collections.Generic.SortedDictionary<string, string>;
using PetriNetData = System.Collections.Generic.SortedDictionary<int,
System.Collections.Generic.List<
System.Collections.Generic.SortedDictionary<string, string>>>;

namespace PNCChecker
{
    /// <summary>
    /// Сеть Петри.
    /// </summary>
    public class PetriNet
    {
        /// <summary>
        /// Список всех вершин (события + позиции).
        /// </summary>
        public List<Node> Nodes = new List<Node>();

        /// <summary>
        /// По ID вершины возвращает индекс в массиве
        /// </summary>
        public SortedDictionary<int, int> HashNodes = new SortedDictionary<int, int>();

        /// <summary>
        /// По ID вершины возвращает количество токенов в текущей разметке.
        /// </summary>
        public SortedDictionary<int, uint> Marking = new SortedDictionary<int, uint>();

        /// <summary>
        /// По ID вершины возвращает количество токенов в стартовой разметке.
        /// </summary>
        public SortedDictionary<int, uint> MarkingS = new SortedDictionary<int, uint>();

        /// <summary>
        /// По ID вершины возвращает количество токенов в конечной разметке.
        /// </summary>
        public SortedDictionary<int, uint> MarkingE = new SortedDictionary<int, uint>();

        /// <summary>
        /// Добавляет вершину в сеть.
        /// </summary>
        /// <returns>Возвращает индекс вершины в массиве.</returns>
        /// <param name="node">Вершина</param>
        public int AddNode(Node node)
        {
            if (NodeExists(node))
                throw new Exception("Узел #" + node.Id + " уже содержится в сети.");

            HashNodes.Add(node.Id, Nodes.Count);
            Nodes.Add(node);

            return Nodes.Count - 1;
        }

        /// <summary>
        /// Добавляет направленное ребро между двумя вершинами.
        /// </summary>
        /// <param name="from">ID начальной вершины.</param>
        /// <param name="to">ID конечной вершины.</param>
        public void AddArc(int from, int to)
        {
            var indexA = GetIndexByNodeId(from);
            var indexB = GetIndexByNodeId(to);

            bool typeA = Nodes[indexA] is Event;
            bool typeB = Nodes[indexB] is Place;

            if (typeA != typeB)
                throw new Exception(string.Format("Попытка соединить два узла с одинаковыми типа (дуга из #{0} в #{1})",
                    from,
                    to));

            Node objA = Nodes[indexA];
            Node objB = Nodes[indexB];

            objA.Outgoing.Add(objB);
            objB.Incoming.Add(objA);
        }

        /// <summary>
        /// Получает индекс вершины по ее ID
        /// </summary>
        /// <returns>Возвращает индекс вершины.</returns>
        /// <param name="id">ID вершины.</param>
        protected int GetIndexByNodeId(int id)
        {
            int res;

            if (!TryGetIndexByNodeId(id, out res))
                throw new Exception("Сеть не содержит узел #" + id);

            return res;
        }

        /// <summary>
        /// Безопасное получение индекса вершины по ее ID
        /// </summary>
        /// <returns><c>true</c>, если такая вершины имеется в сети, <c>false</c> иначе.</returns>
        /// <param name="id">ID вершины.</param>
        /// <param name="res">Индекс вершины.</param>
        protected bool TryGetIndexByNodeId(int id, out int res)
        {
            return HashNodes.TryGetValue(id, out res);
        }

        /// <summary>
        /// Проверяет, существует ли такая вершины в сети.
        /// </summary>
        /// <returns><c>true</c>, если вершина имеется в сети, <c>false</c> иначе.</returns>
        /// <param name="node">Вершина.</param>
        protected bool NodeExists(Node node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            return HashNodes.ContainsKey(node.Id);
        }

        /// <summary>
        /// Получает текущую разметку.
        /// </summary>
        /// <returns>Возвращает разметку.</returns>
        public SortedDictionary<int, uint> GetMarking()
        {
            var res = new SortedDictionary<int, uint>();

            foreach (var item in Nodes)
                if (item is Place &&
                    (item as Place).Tokens > 0)
                    res.Add(item.Id, (item as Place).Tokens);

            return res;
        }

        /// <summary>
        /// Задает разметку в сети.
        /// </summary>
        /// <param name="marking">Разметка.</param>
        public void LoadMarking(SortedDictionary<int, uint> marking = null)
        {
            if (marking != null)
                Marking = new SortedDictionary<int, uint>(marking);

            foreach (Node item in Nodes)
                if (item as Place != null)
                    (item as Place).Tokens = 0;

            foreach (var item in Marking)
                ((Place)Nodes[GetIndexByNodeId(item.Key)]).Tokens = item.Value;
        }

        /// <summary>
        /// Проверяет, является ли заданная разметка конечной или нет.
        /// </summary>
        /// <returns><c>true</c> если разметка является конечно, иначе <c>false</c>.</returns>
        /// <param name="marking">Разметка.</param>
        public bool IsEndMarking(SortedDictionary<int, uint> marking)
        {
            return IsEqualDict(marking, MarkingE);
        }

        /// <summary>
        /// Проверяет равны ли два упорядоченный словаря.
        /// </summary>
        /// <returns><c>true</c>, если равны, <c>false</c> иначе.</returns>
        /// <param name="a">Первый словарь.</param>
        /// <param name="b">Второй словарь.</param>
        public virtual bool IsEqualDict(SortedDictionary<int, uint> a,
            SortedDictionary<int, uint> b)
        {
            if (a.Count != b.Count)
                return false;

            if (a.Count == 0)
                return true;

            var iterA = a.GetEnumerator();
            var iterB = b.GetEnumerator();

            do
            {
                if (iterA.Current.Key != iterB.Current.Key ||
                    iterA.Current.Value != iterB.Current.Value)
                    return false;
            }
            while (iterA.MoveNext() && iterB.MoveNext());

            return true;
        }

        /// <summary>
        /// Конвертирует данные формата PetriNetData в формат класса PetriNet.
        /// </summary>
        /// <param name="net">Данные в формате PetriNetData.</param>
        public void LoadPetriNetFromData(PetriNetData net)
        {
            if (net == null)
                throw new ArgumentNullException("net");

            foreach (var ITEMS in net)
            {
                if (ITEMS.Key == 0) // Nodes
                {
                    foreach (var props in ITEMS.Value)
                    {
                        string label, id, type, tokens;

                        props.TryGetValue("label", out label);
                        props.TryGetValue("type", out type);
                        props.TryGetValue("id", out id);

                        if (type == "place")
                            AddNode(new Place(label, int.Parse(id)));
                        else if (type == "transition")
                            AddNode(new Event(label, int.Parse(id)));
                        else
                            throw new Exception("Invalid or no type");

                        if (props.TryGetValue("tokens", out tokens))
                            MarkingS.Add(int.Parse(id), uint.Parse(tokens));

                        if (props.TryGetValue("end_tokens", out tokens))
                            MarkingE.Add(int.Parse(id), uint.Parse(tokens));
                    }
                }
                else if (ITEMS.Key == 1) // Arcs
                {
                    foreach (var props in ITEMS.Value)
                    {
                        string from, to;

                        props.TryGetValue("from", out from);
                        props.TryGetValue("to", out to);

                        AddArc(int.Parse(from), int.Parse(to));
                    }
                }
            }

            if (MarkingS.Count == 0)
                throw new Exception("Не задана начальная разметка.");

            if (MarkingE.Count == 0)
                throw new Exception("Не задана конечная разметка.");

            LoadMarking(MarkingS);
        }

        public PetriNet()
        {
        }

        public PetriNet(PetriNetData net)
        {
            LoadPetriNetFromData(net);
        }
    }
}