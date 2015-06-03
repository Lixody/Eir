using PHPAnalysis.Configuration;
using QuickGraph.Graphviz;
using QuickGraph.Graphviz.Dot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PHPAnalysis.Data.CFG;
using QuickGraph;

namespace PHPAnalysis.Utils
{
    public static class GraphvizHelpers
    {
        private sealed class FileDotEngine : IDotEngine
        {
            private GraphConfiguration Settings { get; set; }

            public FileDotEngine(GraphConfiguration configuration)
            {
                this.Settings = configuration;
            }

            public string Run(GraphvizImageType imageType, string dot, string outputFileName)
            {
                string output = outputFileName;
                File.WriteAllText(output, dot);

                var args = string.Format(@"{0} {1}", output, Settings.GraphvizArguments);
                Process.Start(Settings.GraphvizPath, args);
                return output;
            }
        }

        public static void VisualizeGraph(this IEdgeListGraph<CFGBlock, TaggedEdge<CFGBlock, EdgeTag>> graph, string path, GraphConfiguration configuration) 
        {
            Preconditions.NotNull(graph, "graph");
            Preconditions.NotNull(path, "path");
            Preconditions.NotNull(configuration, "configuration");

            var graphviz = new GraphvizAlgorithm<CFGBlock, TaggedEdge<CFGBlock, EdgeTag>>(graph);
            int variable = 0;
            graphviz.FormatVertex += (o, e) =>
                                     {
                                         CFGBlock block = e.Vertex;
                                         if (block.AstEntryNode != null)
                                         {
                                             e.VertexFormatter.Label = variable + " " + block.AstEntryNode.LocalName;
                                         }
                                         else
                                         {
                                             e.VertexFormatter.Label = variable.ToString();

                                         }
                                         variable++;
                                     };
            graphviz.FormatEdge += (o, e) =>
                                   {
                                       TaggedEdge<CFGBlock, EdgeTag> edgeTag = e.Edge;
                                       switch (edgeTag.Tag.EdgeType)
                                       {
                                           case EdgeType.True:
                                               e.EdgeFormatter.Label.Value = "T";
                                               break;
                                           case EdgeType.False:
                                               e.EdgeFormatter.Label.Value = "F";
                                               break;
                                       }
                                   };

            string output = graphviz.Generate(new FileDotEngine(configuration), path);
        }

        public static IEnumerable<CFGBlock> ReachableBlocks(this IBidirectionalGraph<CFGBlock, TaggedEdge<CFGBlock, EdgeTag>> graph, CFGBlock root)
        {
            Preconditions.NotNull(graph, "graph");
            Preconditions.NotNull(root, "root");

            var reachableBlocks = new HashSet<CFGBlock> { root };
            var toVisitQueue = new Queue<CFGBlock>(reachableBlocks);

            while (toVisitQueue.Any())
            {
                var current = toVisitQueue.Dequeue();
                foreach (var block in graph.OutEdges(current).Select(e => e.Target))
                {
                    if (reachableBlocks.Add(block))
                    {
                        toVisitQueue.Enqueue(block);
                    }
                }
            }

            return reachableBlocks;
        } 
    }
}
