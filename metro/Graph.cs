using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Windows.Shapes;

namespace metro
{

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
