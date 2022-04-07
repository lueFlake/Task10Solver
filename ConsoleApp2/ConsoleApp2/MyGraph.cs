using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task10 {
    internal class MyGraph {
        internal class Edge {
            public Vertex First;
            public Vertex Second;
            public string Info;

            public Edge(Vertex first, Vertex second, string info) {
                First = first;
                Second = second;
                Info = info;
            }
        }
        internal class Vertex {
            public List<Edge> Adjacent = new();
            public object Tag;
            public int Id;

            public override int GetHashCode() {
                return Id;
            }

            public override bool Equals(object obj) {
                return Id.Equals(((Vertex)obj).Id);
            }
        }

        public readonly List<Vertex> Vertices;

        public MyGraph(int n) {
            Vertices = new();
            for (int i = 0; i < n; i++) {
                Vertices.Add(new Vertex());
            }
            int it = 0;
            Vertices.ForEach(v => v.Id = it++);
        }

        public void AddEdge(int u, int v, string info) {
            Vertices[u].Adjacent.Add(new(Vertices[u], Vertices[v], info));
            Vertices[v].Adjacent.Add(new(Vertices[v], Vertices[u], info));
        }
        internal class ComponentInfo {
            public List<Vertex> Vertices;
            public string[] UsedEdges;
            public string KeyInfo;
        }

        public List<ComponentInfo> GetAndColorComponents() {
            List<ComponentInfo> result = new();
            bool[] used = new bool[Vertices.Count];
            bool condition = true;
            while (condition) {
                int u = -1;
                for (int i = 0; i < Vertices.Count; i++) {
                    if (!used[i] && Vertices[i].Tag != null) {
                        u = i;
                        break;
                    }
                }
                if (u == -1) {
                    condition = false;
                }
                else {
                    HashSet<Vertex> visited = new();
                    List<string> usedEdges = new();
                    ColorComponent(Vertices[u], visited, usedEdges);
                    (int x, int y) = Program.IntToPair(Vertices[u].Id);
                    result.Add(new() {
                        Vertices = visited.ToList(),
                        UsedEdges = usedEdges.ToArray(),
                        KeyInfo = $"Известно, что: {(char)(x + 'a')}{(char)(y + 'a')} = {Vertices[u].Tag}"
                    });
                    object tag = Vertices[u].Tag;
                    visited.ToList().ForEach(x => {
                        x.Tag = tag;
                        used[x.Id] = true;
                    });
                }
            }
            return result;
        }

        private void ColorComponent(Vertex vertex, HashSet<Vertex> visited, List<string> usedEdges) {
            visited.Add(vertex);
            foreach (var item in vertex.Adjacent) {
                if (!visited.Contains(item.Second)) {
                    usedEdges.Add(item.Info);
                    ColorComponent(item.Second, visited, usedEdges);
                }
            }
        }
    }
}
