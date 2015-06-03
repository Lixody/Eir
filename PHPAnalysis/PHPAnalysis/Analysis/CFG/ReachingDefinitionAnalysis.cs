using System;
using System.Collections.Immutable;
using System.Linq;
using PHPAnalysis.Data;
using PHPAnalysis.Data.CFG;
using PHPAnalysis.Parsing.AstTraversing;
using PHPAnalysis.Utils;
using PHPAnalysis.Utils.XmlHelpers;
using QuickGraph;

namespace PHPAnalysis.Analysis.CFG
{
    public class ReachingSet : IEquatable<ReachingSet>
    {
        public System.Collections.Immutable.IImmutableDictionary<string, Variable> DefinedOutVars { get; private set; }
        public System.Collections.Immutable.IImmutableDictionary<string, Variable> DefinedInVars { get; private set; }

        public ReachingSet()
        {
            DefinedOutVars = ImmutableDictionary<string, Variable>.Empty;
            DefinedInVars = ImmutableDictionary<string, Variable>.Empty;
        }

        public ReachingSet AddInVar(string var, Variable gs)
        {
            return new ReachingSet()
            {
                DefinedInVars = this.DefinedInVars.SetItem(var, gs),
                DefinedOutVars = this.DefinedOutVars
            };
        }
        public ReachingSet AddInVarRange(System.Collections.Immutable.IImmutableDictionary<string, Variable> vars, bool mergeSets = false)
        {
            if (!mergeSets)
            {
                return new ReachingSet()
                {
                    DefinedInVars = this.DefinedInVars.SetItems(vars),
                    DefinedOutVars = this.DefinedOutVars
                    
                };
            }
            else
            {
                return new ReachingSet
                {
                    DefinedInVars = this.DefinedInVars.Merge(vars),
                    DefinedOutVars = DefinedOutVars
                };
            }
        }

        public ReachingSet AddOutVar(string var, Variable gs)
        {
            return new ReachingSet()
            {
                DefinedOutVars = this.DefinedOutVars.SetItem(var, gs),
                DefinedInVars = this.DefinedInVars
            };
        }

        public ReachingSet AddOutVarRange(System.Collections.Immutable.IImmutableDictionary<string, Variable> reachSet, bool mergeSets = false)
        {
            if (!mergeSets)
            {
                return new ReachingSet()
                {
                    DefinedOutVars = this.DefinedOutVars.SetItems(reachSet),
                    DefinedInVars = this.DefinedInVars
                };
            }
            else
            {
                return new ReachingSet {
                                           DefinedInVars = this.DefinedInVars, DefinedOutVars = DefinedOutVars.Merge(reachSet)
                                       };
            }
        }

        public bool Equals(ReachingSet other)
        {
            return other != null && (DefinedOutVars.SequenceEqual(other.DefinedOutVars)
                                 && DefinedInVars.SequenceEqual(other.DefinedInVars));
        }
    }
    class ReachingDefinitionAnalysis : ICFGAnalysis
    {
        public ImmutableDictionary<CFGBlock, ReachingSet> ReachingSetDictionary { get; private set; }

        public ReachingDefinitionAnalysis()
        {
            ReachingSetDictionary = ImmutableDictionary<CFGBlock, ReachingSet>.Empty;
        }

        public void Initialize(CFGBlock cfgBlock)
        {
            ReachingSetDictionary = ReachingSetDictionary.Add(cfgBlock, null);
        }

        public bool Analyze(TaggedEdge<CFGBlock, EdgeTag> edge)
        {
            var oldRes = ReachingSetDictionary[edge.Target];

            var newRes = AnalyzeNode(edge);

            return MonotonicChange(oldRes, newRes);
        }

        public bool Analyze2(CFGBlock block, IBidirectionalGraph<CFGBlock, TaggedEdge<CFGBlock, EdgeTag>> graph)
        {
            throw new NotImplementedException();
        }

        private ReachingSet AnalyzeNode(TaggedEdge<CFGBlock, EdgeTag> edge)
        {
            if (ReachingSetDictionary[edge.Source] == null)
            {
                ReachingSetDictionary = ReachingSetDictionary.SetItem(edge.Source, new ReachingSet());
            }
            if (ReachingSetDictionary[edge.Target] == null)
            {
                ReachingSetDictionary = ReachingSetDictionary.SetItem(edge.Target, new ReachingSet());
            }

            var node = edge.Target;
            // IN:
            //    U   RD_OUT(l')
            // (l'~>l)
            ReachingSetDictionary = ReachingSetDictionary.SetItem(node,
                ReachingSetDictionary[node].AddInVarRange(ReachingSetDictionary[edge.Source].DefinedOutVars, true));

            // OUT:
            // (RD_IN(l) \ kill(l)) U gen(l)
            var Out = ReachingSetDictionary[node];
            Out = Out.AddOutVarRange(ReachingSetDictionary[node].DefinedInVars);
            XmlTraverser xmlTraverser = new XmlTraverser();
            CFGASTNodeVisitor nodeVisitor = new CFGASTNodeVisitor();
            xmlTraverser.AddVisitor(nodeVisitor);

            if (node.AstEntryNode != null)
            {
                var nodeToTraverse = Conditional.HasConditionNode(node.AstEntryNode) ? Conditional.GetCondNode(node.AstEntryNode)
                                                                                     : node.AstEntryNode;
                xmlTraverser.Traverse(nodeToTraverse);

                foreach (var currNode in nodeVisitor.NodesOfInterest)
                {
                    var varNode = AstNodeInfo.GetVarNameXmlNode(currNode);

                    ValueInfo varInfo = new ValueInfo() { Block = node };
                    varInfo = VariableInfoComposer.AnalyzeBlock(varInfo);
                    var gs = new Variable(AstNodeInfo.GetVarNameXmlNode(currNode).Name, VariableScope.Unknown);
                    gs = gs.AddVarInfo(varInfo);
                    
                    Out = Out.AddOutVar(varNode.InnerText, gs);
                }
            }
            ReachingSetDictionary = ReachingSetDictionary.SetItem(node, ReachingSetDictionary[node].AddOutVarRange(Out.DefinedOutVars));

            return ReachingSetDictionary[node];
        }

        private bool MonotonicChange(ReachingSet oldResult, ReachingSet newResult)
        {
            if (oldResult == null)
                return true;
            return !oldResult.Equals(newResult);
        }
    }
}
