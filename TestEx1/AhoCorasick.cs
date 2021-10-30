using System.Collections;
using System.Collections.Generic;

namespace AhoCorasick
{
    /// <summary>
    /// Трие, которое найдет и вернет строки, найденные в тексте
    /// </summary>
    public class Trie : Trie<string>
    {
        /// <summary>
        /// Добавляем строчки
        /// </summary>
        /// <param name="s">Строка для добавления</param>
        public void Add(string s)
        {
            Add(s, s);
        }

        /// <summary>
        /// Добавить перечисление строк
        /// </summary>
        /// <param name="strings">Перечисление для добавления</param>
        public void Add(IEnumerable<string> strings)
        {
            foreach (string s in strings)
            {
                Add(s);
            }
        }
    }

    /// <summary>
    /// Трие, которое найдет строки в тексте и вернет значения типа <typeparamref name = "T" />
    /// для каждой найденной строки
    /// </summary>
    /// <typeparam name="TValue">Тип значения</typeparam>
    public class Trie<TValue> : Trie<char, TValue>
    {
    }

    /// <summary>
    /// Трие, которое найдет строки или фразы и вернет значения типа <typeparamref name = "T" />
    /// для каждой найденной строки или фразы
    /// </summary>
    /// <remarks>
    /// <typeparamref name = "T" /> обычно представляет собой символ для поиска строк
    /// или строку для поиска фраз или целых слов.
    /// </remarks>
    /// <typeparam name="T">Тип буквы в слове.</typeparam>
    /// <typeparam name="TValue">Тип значения, которое будет возвращено при нахождении слова</typeparam>
    public class Trie<T, TValue>
    {
        /// <summary>
        /// Корень дерева. У него нет ни значения, ни родителя.
        /// </summary>
        private readonly Node<T, TValue> root = new Node<T, TValue>();

        /// <summary>
        /// Добавляет слово к дереву
        /// </summary>
        /// <remarks>
        /// Слово состоит из букв. Для каждой буквы строится узел. 
        /// Если тип буквы - char, то слово будет строкой, поскольку состоит из букв.
        /// Но буква также может быть строкой, что означает, что узел будет добавлен 
        /// для каждого слова, и поэтому слово на самом деле является фразой
        /// </remarks>
        /// <param name="word">Слово, которое будем искать</param>
        /// <param name="value">Значение, которое будет возвращено, когда слово будет найдено</param>
        public void Add(IEnumerable<T> word, TValue value)
        {
            // Начинаем с корня
            var node = root;

            // Строим ответвление для слова, по букве за раз
            foreach (T c in word)
            {
                var child = node[c];

                // Если буквенного узла не существует, добавляем его
                if (child == null)
                    child = node[c] = new Node<T, TValue>(c, node);

                node = child;
            }

            // Обозначаем конец ветки
            // добавляя значение, которое будет возвращено, когда это слово будет найдено в тексте
            node.Values.Add(value);
        }


        /// <summary>
        /// Строим дерево
        /// </summary>
        public void Build()
        {
            // Строительство выполняется с использованием поиска в ширину
            var queue = new Queue<Node<T, TValue>>();
            queue.Enqueue(root);

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();

                // Посещаем детей
                foreach (var child in node)
                    queue.Enqueue(child);

                if (node == root)
                {
                    root.Fail = root;
                    continue;
                }

                var fail = node.Parent.Fail;

                while (fail[node.Word] == null && fail != root)
                    fail = fail.Fail;

                node.Fail = fail[node.Word] ?? root;
                if (node.Fail == node)
                    node.Fail = root;
            }
        }

        /// <summary>
        /// Находит все добавленные слова в тексте
        /// </summary>
        /// <param name="text">Текст для поиска</param>
        /// <returns>Все найденные значения</returns>
        public IEnumerable<TValue> Find(IEnumerable<T> text)
        {
            var node = root;

            foreach (T c in text)
            {
                while (node[c] == null && node != root)
                    node = node.Fail;

                node = node[c] ?? root;

                for (var t = node; t != root; t = t.Fail)
                {
                    foreach (TValue value in t.Values)
                        yield return value;
                }
            }
        }

        /// <summary>
        /// Узел в дереве
        /// </summary>
        /// <typeparam name="TNode">То же, что и родительский тип</typeparam>
        /// <typeparam name="TNodeValue">То же, что и родительский тип значения</typeparam>
        private class Node<TNode, TNodeValue> : IEnumerable<Node<TNode, TNodeValue>>
        {
            private readonly TNode word;
            private readonly Node<TNode, TNodeValue> parent;
            private readonly Dictionary<TNode, Node<TNode, TNodeValue>> children = new Dictionary<TNode, Node<TNode, TNodeValue>>();
            private readonly List<TNodeValue> values = new List<TNodeValue>();

            /// <summary>
            /// Конструктор корневого узла
            /// </summary>
            public Node()
            {
            }

            /// <summary>
            /// Конструктор узла со словом
            /// </summary>
            /// <param name="word"></param>
            /// <param name="parent"></param>
            public Node(TNode word, Node<TNode, TNodeValue> parent)
            {
                this.word = word;
                this.parent = parent;
            }

            /// <summary>
            /// Слово (или буква) для этого узла
            /// </summary>
            public TNode Word
            {
                get { return word; }
            }

            /// <summary>
            /// Родительский узел
            /// </summary>
            public Node<TNode, TNodeValue> Parent
            {
                get { return parent; }
            }

            /// <summary>
            /// Неудачный узел
            /// </summary>
            public Node<TNode, TNodeValue> Fail
            {
                get;
                set;
            }

            /// <summary>
            /// Потомки для этого узла
            /// </summary>
            /// <param name="c">Дочернее слово</param>
            /// <returns>Дочерний узел</returns>
            public Node<TNode, TNodeValue> this[TNode c]
            {
                get { return children.ContainsKey(c) ? children[c] : null; }
                set { children[c] = value; }
            }

            /// <summary>
            /// Значения слов, заканчивающихся на этом узле
            /// </summary>
            public List<TNodeValue> Values
            {
                get { return values; }
            }

            /// <inherit/>
            public IEnumerator<Node<TNode, TNodeValue>> GetEnumerator()
            {
                return children.Values.GetEnumerator();
            }

            /// <inherit/>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            /// <inherit/>
            public override string ToString()
            {
                return Word.ToString();
            }
        }
    }
}

