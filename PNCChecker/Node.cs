//
//  Ostankov Alexander
//  Higher School of Economics
//  sashaostankov@gmail.com
//  (c) 2015
//

using System;
using System.Collections.Generic;

namespace PNCChecker
{
    /// <summary>
    /// Класс описывает любую вершину.
    /// </summary>
    public class Node : ICloneable
    {
        /// <summary>
        /// Название вершины.
        /// </summary>
        public string Label;

        /// <summary>
        /// ID вершины. Уникальный номер.
        /// </summary>
        public int Id;

        /// <summary>
        /// Список входящих ребер.
        /// </summary>
        public List<Node> Incoming;

        /// <summary>
        /// Список исходящих ребер.
        /// </summary>
        public List<Node> Outgoing;

        public Node()
        {
            Incoming = new List<Node>();
            Outgoing = new List<Node>();

            Label = string.Empty;
        }

        public Node(string label, int id)
            : this()
        {
            if (string.IsNullOrEmpty(label))
                label = string.Empty;

            Label = string.Copy(label);
            Id = id;
        }

        /// <summary>
        /// Клонирует текущий объект.
        /// </summary>
        public virtual object Clone()
        {
            var res = new Node(Label, Id);

            res.Incoming = new List<Node>(Incoming);
            res.Outgoing = new List<Node>(Outgoing);

            return res;
        }
    }

    /// <summary>
    /// Вершиина типа "переход" или "событие".
    /// </summary>
    public class Event : Node
    {
        /// <summary>
        /// Получает или задает количество активных входящих позиций.
        /// Позиция считается активной, если она сожержит хотя бы один токен.
        /// </summary>
        /// <value>Количество активных входящих позиций .</value>
        public uint ActiveIncomingPlaces { get; set; }

        /// <summary>
        /// Определяет может ли срабутотать текущий переход.
        /// </summary>
        /// <returns><c>true</c> если текущий переход может сработать; иначе, <c>false</c>.</returns>
        public bool CanPlayEvent()
        {
            return (ActiveIncomingPlaces == Incoming.Count)
                && (Incoming.Count > 0)
                && (Outgoing.Count > 0);
        }

        /// <summary>
        /// Активирует переход.
        /// </summary>
        /// <returns><c>true</c>, если событие проиграно успешно, <c>false</c> иначе.</returns>
        public bool PlayEvent()
        {
            if (!CanPlayEvent())
                return false;

            for (int i = 0; i < Incoming.Count; i++)
                ((Place)Incoming[i]).Tokens--;

            for (int i = 0; i < Outgoing.Count; i++)
                ((Place)Outgoing[i]).Tokens++;

            return true;
        }

        public Event()
        {
        }

        public Event(string label, int id)
            : base(label, id)
        {
        }

        public override string ToString()
        {
            return string.Format("[Event: Id={0}, Label={1}, ActiveIncomingPlaces={2}]",
                Id,
                Label,
                ActiveIncomingPlaces);
        }

        /// <summary>
        /// Клонирует текущий объект.
        /// </summary>
        public override object Clone()
        {
            var res = (Event)base.Clone();

            res.ActiveIncomingPlaces = ActiveIncomingPlaces;

            return res;
        }
    }

    /// <summary>
    /// Вершина типа "позиция".
    /// </summary>
    public class Place : Node
    {
        /// <summary>
        /// Количество токенов в текущей позиции.
        /// </summary>
        protected uint _tokens;

        /// <summary>
        /// Получает или задает количество токенов в текущей позиции.
        /// </summary>
        /// <value>Количество токенов.</value>
        public uint Tokens
        {
            get { return _tokens; }
            set
            {
                if (value > 0 && _tokens == 0)
                {
                    for (int i = 0; i < Outgoing.Count; i++)
                        ((Event)Outgoing[i]).ActiveIncomingPlaces++;
                }
                else if (value == 0 && _tokens > 0)
                {
                    for (int i = 0; i < Outgoing.Count; i++)
                        ((Event)Outgoing[i]).ActiveIncomingPlaces--;
                }

                _tokens = value;
            }
        }

        public Place()
        {
            Tokens = 0;
        }

        public Place(string label, int id, uint tokens = 0)
            : base(label, id)
        {
            Tokens = tokens;
        }

        public override string ToString()
        {
            return string.Format("[Place: Label={0}, Tokens={1}]",
                Label,
                Tokens);
        }

        /// <summary>
        /// Клонирует текущий объект.
        /// </summary>
        public override object Clone()
        {
            var res = (Place)base.Clone();
            res.Tokens = Tokens;
            return res;
        }
    }
}