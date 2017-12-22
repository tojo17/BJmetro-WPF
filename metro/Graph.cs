using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Windows.Shapes;

namespace metro
{
    /*class PriorityQueue<T>
    {
        IComparer comparer;
        T[] heap;

        public int Count { get; private set; }

        public PriorityQueue() : this(null) { }
        public PriorityQueue(int capacity) : this(capacity, null) { }
        public PriorityQueue(IComparer comparer) : this(16, comparer) { }

        public PriorityQueue(int capacity, IComparer comparer)
        {
            this.comparer = (comparer == null) ? Comparer.Default : comparer;
            this.heap = new T[capacity];
        }

        public void Push(T v)
        {
            if (Count >= heap.Length) Array.Resize(ref heap, Count * 2);
            heap[Count] = v;
            SiftUp(Count++);
        }

        public T Pop()
        {
            var v = Top();
            heap[0] = heap[--Count];
            if (Count > 0) SiftDown(0);
            return v;
        }

        public T Top()
        {
            if (Count > 0) return heap[0];
            throw new InvalidOperationException("empty");
        }

        void SiftUp(int n)
        {
            var v = heap[n];
            for (var n2 = n / 2; n > 0 && comparer.Compare(v, heap[n2]) > 0; n = n2, n2 /= 2)

                heap[n] = heap[n2];
            heap[n] = v;
        }

        void SiftDown(int n)
        {
            var v = heap[n];
            for (var n2 = n * 2; n2 < Count; n = n2, n2 *= 2)
            {
                if (n2 + 1 < Count && comparer.Compare(heap[n2 + 1], heap[n2]) > 0) n2++;
                if (comparer.Compare(v, heap[n2]) >= 0) break;
                heap[n] = heap[n2];
            }
            heap[n] = v;
        }
    }


    struct Edge
    {
        public int v; //edge's node
        public int len;
    }

    class Node : IComparable<Node>
    {
        public int v;
        public int len;
        public int[] dist;
        public Node(int a = 0, int b = 0, int[] dist = null)
        {
            v = a;
            len = b;
            this.dist= dist;
        }
        public int CompareTo(Node obj)
        {
            return (len + dist[v] > obj.len + dist[obj.v]) ? 1 : 0;
        }

    }




    class Graph
    {
        const int INF = 999999999;
        int N;  // node count
        int M; // edge count
        int[] dist;
        int[] cnt;
        bool[] mark;
        int[,] adj; // graph
        // Dictionary<int, Edge> rev; // reverse graph
        public Graph()
        {
            N = 0;
            M = 0;
            dist = new int[500];
            cnt = new int[500];
            mark = new bool[500];
            adj = new int[500, 500];

        }


        public void addEdge(int u, int v, int l)
        {
            // from, to, length
            M++;
            adj[u, v] = l;
            adj[v, u] = l;

        }


        public void addNode()
        {
            N++;
        }

        // Dijkstra find least distance to T
        public void Dijkstra(int S, int T) // from S to T
        {
            for (int i = 1; i <= N; i++)
                mark[i] = false;
            for (int i = 1; i <= N; i++)
                dist[i] = INF;
            dist[T] = 0;
            int u, v, min;
            while (true)
            {
                u = -1;
                min = INF;
                for (int i = 1; i <= N; i++)
                    if (!mark[i] && dist[i] < min)
                    {
                        min = dist[i];
                        u = i;
                    }
                if (u == -1) break;
                mark[u] = true;
                for (int k = 0; k < M; k++)
                {
                    v = adj[u, k];
                    if (!mark[v] && dist[v] > dist[u] + adj[u, k])
                        dist[v] = dist[u] + adj[u, k];
                }
            }

        }

        public int Astar(int S, int T, int K) // from S to T, K short
        {
            if (dist[S] == INF) return -1;
            for (int i = 0; i < N; i++)
                cnt[i] = 0;
            PriorityQueue<Node> Q = new PriorityQueue<Node>();
            Q.Push(new Node(S, 0, dist));
            while (Q.Count!=0)
            {
                int len = Q.Top().len;
                int v = Q.Top().v;
                Q.Pop();
                cnt[v]++;
                if (cnt[T] == K)
                    return len;
                if (cnt[v] > K)
                    continue;
                for (int i = 0; i < N; i++)
                    Q.Push(new Node(adj[v, i], len + adj[v, i], dist));
            }
            return -1;
        }

    }*/

    struct NodePair
    {
        public int from;
        public int to;
        public NodePair(int from, int to)
        {
            this.from = from;
            this.to = to;
        }
    }

    //class Node{
    //    public int stationId;
    //    public Edge firstEdge;
    //    public int edgesCount;
    //    public Node()
    //    {
    //        firstEdge = null;
    //        edgesCount = 0;
    //    }
    //}
    class Edge
    {
        //public Node nextNode;
        public int routeId;
        public int length;
        //public Edge nextEdge;
        public int line;
        //public Edge()
        //{
        //    nextNode = null;
        //    nextEdge = null;

        //}

    }

    class Graph
    {
        public Dictionary<NodePair, Edge> edges;
        public List<int> nodes;
        public int[,] edgesM;
        //public Dictionary<int, Node> nodes;
        //public List<Edge> edges;
        public Graph()
        {
            edges = new Dictionary<NodePair, Edge>();
            edgesM = new int[100, 100];
            for (int i = 0; i < 100; i++)
            {
                for (int j = 0; j < 100; j++)
                {
                    if (i == j) edgesM[i, j] = 0;
                    else edgesM[i, j] = 99999999;
                }
            }
            nodes = new List<int>();
            //nodes = new Dictionary<int, Node>();
            //edges = new List<Edge>();
        }

        // add node
        public void addNode(int stationId)
        {
            //Node n = new Node();
            //n.stationId = stationId;
            //nodes.Add(n.stationId, n);
            nodes.Add(stationId);
        }
        public void addEdge(int routeId, int from, int to, int line, int length)
        {

            Edge e = new Edge();
            e.routeId = routeId;
            //e.nextNode = nodes[to];
            e.length = length;
            e.line = line;
            edges.Add(new NodePair(from, to), e);
            edges.Add(new NodePair(to, from), e);
            edgesM[from, to] = length;
            edgesM[to, from] = length;


            //edges.Add(e);
            //nodes[from].edgesCount++;
            //e.nextEdge = nodes[from].firstEdge;
            //nodes[from].firstEdge = e;

            //Edge r = new Edge();
            //r.routeId = routeId;
            //r.nextNode = nodes[from];
            //r.length = length;
            //r.line = line;
            //edges.Add(r);
            //nodes[to].edgesCount++;
            //r.nextEdge = nodes[to].firstEdge;
            //nodes[to].firstEdge = r;

        }

        public int[] dijM(int from)
        {
            int[] dis = new int[100];
            int[] book = new int[100];
            int[] path = new int[100];
            for (int i = 1; i <= nodes.Count; i++)
                dis[i] = edgesM[from, i];
            for (int i = 1; i <= nodes.Count; i++)
                book[i] = 0;
            book[from] = 1;
            int min;
            int u = 0, v;

            //Dijkstra
            for (int i = 1; i <= nodes.Count - 1; i++)
            {
                // nearest point
                min = 9999999;
                for (int j = 1; j <= nodes.Count; j++)
                {
                    if (book[j] == 0 && dis[j] < min)
                    {
                        min = dis[j];
                        u = j;
                    }
                }
                book[u] = 1;
                for (v = 1; v <= nodes.Count; v++)
                {
                    if (edgesM[u, v] < 9999999)
                    {
                        if (dis[v] > dis[u] + edgesM[u, v])
                        {
                            path[v] = u;
                            dis[v] = dis[u] + edgesM[u, v];
                            //if (v == nodeId[idMax]) break;
                        }
                    }
                }
                //if (v == nodeId[idMax]) break;
            }
            return path;
        }


        public Dictionary<int, int> dijkstra(int from)
        {
            Dictionary<int, int> distance = new Dictionary<int, int>();
            List<int> mark = new List<int>();
            Dictionary<int, int> path = new Dictionary<int, int>();
            int u = 0;
            mark.Add(from);
            foreach (int i in nodes)
            {
                if (i == from) distance.Add(i, 0);
                if (edges.ContainsKey(new NodePair(from, i)))
                    distance.Add(i, edges[new NodePair(from, i)].length);
            }
            foreach (int i in nodes)
            {
                int min = 9999999;
                foreach (int j in nodes)
                {
                    //if (!distance.ContainsKey(j)) distance[j] = 0;
                    if (!mark.Contains(j) && (!distance.ContainsKey(j) || distance[j] < min))
                    {
                        min = distance.ContainsKey(j) ? distance[j] : 9999999;
                        u = j;
                    }
                }
                mark.Add(u);
                foreach (int v in nodes)
                {
                    if (edges.ContainsKey(new NodePair(u, v)))
                    {
                        int curL = edges.ContainsKey(new NodePair(u, v)) ?
                            edges[new NodePair(u, v)].length : 9999999;
                        int dv = distance.ContainsKey(v) ? distance[v] : 9999999;
                        int du = distance.ContainsKey(u) ? distance[u] : 9999999;
                        if (du != int.MaxValue && curL != int.MaxValue && dv > du + curL)
                        {
                            if (path.ContainsKey(v)) path[v] = u;
                            else path.Add(v, u);
                            distance[v] = du + curL;
                        }
                    }
                }
            }
            return distance;
            //return path;

        }
        public int Astar(int from, int to, int k)
        {


            Dictionary<int, int> distance = dijkstra(to);
            if (!distance.ContainsKey(from)) return -1;
            Dictionary<int, int> cnt = new Dictionary<int, int>();
            SortedList q = new SortedList();
            q.Add(0, from);
            while (q.Count > 0)
            {
                int len = (int)q.GetKey(0);
                int v = (int)q.GetByIndex(0);
                q.RemoveAt(0);
                if (cnt.ContainsKey(v)) cnt[v]++;
                else cnt.Add(v, 1);
                int ct = cnt.ContainsKey(to) ? cnt[to] : 0;
                if (ct == k) return v;
                if (cnt[v] > k) continue;
                foreach (int i in nodes)
                {
                    if (edges.ContainsKey(new NodePair(v, i)))
                        q.Add(edges[new NodePair(v, i)].length, v);
                }
            }
            return -1;



            //if (dist[S] == INF) return -1;
            //memset(cnt, 0, sizeof(cnt));
            //priority_queue<Node> Q;
            //Q.push(Node(S, 0));
            //while (!Q.empty())
            //{
            //    int len = Q.top().len;
            //    int v = Q.top().v;
            //    Q.pop();
            //    cnt[v]++;
            //    if (cnt[T] == K)
            //        return len;
            //    if (cnt[v] > K)
            //        continue;
            //    for (int i = 0; i < Adj[v].size(); i++)
            //        Q.push(Node(Adj[v][i].v, len + Adj[v][i].len));
            //}
            //return -1;
        }
    }
}
