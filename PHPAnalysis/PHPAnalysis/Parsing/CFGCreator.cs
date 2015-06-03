using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml;
using PHPAnalysis.Configuration;
using PHPAnalysis.Data;
using PHPAnalysis.Data.CFG;
using PHPAnalysis.Utils;
using PHPAnalysis.Utils.XmlHelpers;
using QuickGraph;
using System;
using System.Diagnostics;
using PHPAnalysis.Parsing.AstTraversing;

namespace PHPAnalysis.Parsing
{
    [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
    public sealed class CFGCreator : IXmlVisitor
    {
        public BidirectionalGraph<CFGBlock, TaggedEdge<CFGBlock, EdgeTag>> Graph { get; private set; }
        private CFGBlock _exitBlock;

        private CFGBlock CurrentBlock { get; set; }

        // HACK: This is not the prettiest solution, but it is quite simple and seems to work.
        // -||-: The basic purpose is to manage when to _not_ visit the children of a node.
        private readonly List<XmlNode> nodesNotToVisit = new List<XmlNode>(); 
        private XmlNode traverseBlockingNode;
        private bool IsCurrentlyBlocking { get { return traverseBlockingNode != null; } }

        private readonly ScopeHandler scopeHandler = new ScopeHandler();
        private bool isFirst = true;

        public void TraverseStart(object sender, XmlStartTraverseEventArgs e)
        {
            Graph = new BidirectionalGraph<CFGBlock, TaggedEdge<CFGBlock, EdgeTag>>();
            var root = new CFGBlock(isSpecial: true) { IsRoot = true };
            _exitBlock = new CFGBlock(isSpecial: true) { IsLeaf = true };
            Graph.AddVertex(root);
            Graph.AddVertex(_exitBlock);
            CurrentBlock = root;
        }

        public void EnteringNode(object sender, XmlTraverseEventArgs e)
        {
            var node = e.Node;
            if (!IsNodeOfInterest(node) || IsCurrentlyBlocking)
            {
                isFirst = false;
                return;
            }

            if (nodesNotToVisit.Contains(node))
            {
                DoNotVisitChildren(node);
                nodesNotToVisit.Remove(node);
                return;
            }

            switch (node.LocalName)
            {
                case AstConstants.Nodes.Stmt_Class:
                case AstConstants.Nodes.Stmt_Interface:
                case AstConstants.Nodes.Stmt_Trait:
                    DoNotVisitChildren(node);
                    break;
                case AstConstants.Nodes.Expr_Closure:
                case AstConstants.Nodes.Stmt_Function:
                case AstConstants.Nodes.Stmt_ClassMethod:
                    if(isFirst)
                        FunctionEnter(node);
                    else
                        DoNotVisitChildren(node);
                    break;
                case AstConstants.Nodes.Stmt_If:
                    IfStatementEnter(node);
                    break;
                case AstConstants.Nodes.Stmt_Else:
                    ElseStatementsEnter(node);
                    break;
                case AstConstants.Nodes.Stmt_ElseIf:
                    ElseIfStatementsEnter(node);
                    break;
                case AstConstants.Nodes.Stmt_Switch:
                    SwitchStatementEnter(node);
                    break;
                case AstConstants.Nodes.Stmt_Case:
                    SwitchCaseStatementEnter(node);
                    break;

                // Loops
                case AstConstants.Nodes.Stmt_For:
                    ForStatementEnter(node);
                    break;
                case AstConstants.Nodes.Stmt_While:
                    WhileStatementEnter(node);
                    break;
                case AstConstants.Nodes.Stmt_Foreach:
                    WhileOrForeachStatementEnter(node);
                    break;
                case AstConstants.Nodes.Stmt_Do:
                    DoStatementEnter(node);
                    break;
                case AstConstants.Nodes.Stmt_Continue:
                    ContinueStatementEnter(node);
                    break;
                case AstConstants.Nodes.Stmt_Break:
                    BreakStatementEnter(node);
                    break;
                case AstConstants.Nodes.Stmt_Return:
                    ReturnStatementEnter(node);     
                    break;
                default:
                    if (node.Prefix == AstConstants.Node)
                    {
                        NormalStatementEnter(node);
                    }
                    
                    break;
            }

            if (isFirst)
            {
                isFirst = false;
            }
        }

        public void LeavingNode(object sender, XmlTraverseEventArgs e)
        {
            var node = e.Node;
            if (traverseBlockingNode == node)
            {
                traverseBlockingNode = null;
                return;
            }

            if (!IsNodeOfInterest(node) || IsCurrentlyBlocking)
            {
                return;
            }
            if (node.Prefix.Equals(AstConstants.Node))
            {
                switch (node.LocalName)
                {
                    case AstConstants.Nodes.Expr_Closure:
                    case AstConstants.Nodes.Stmt_Function:
                        FunctionExit(node);
                        break;
                    case AstConstants.Nodes.Stmt_If:
                        IfStatementExit(node);
                        break;
                    case AstConstants.Nodes.Stmt_Else:
                        ElseStatementsExit(node);
                        break;
                    case AstConstants.Nodes.Stmt_ElseIf:
                        ElseIfStatementsExit(node);
                        break;
                    case AstConstants.Nodes.Stmt_Switch:
                        SwitchStatementExit(node);
                        break;
                    // Loops
                    case AstConstants.Nodes.Stmt_For:
                        ForStatementExit(node);
                        break;
                    case AstConstants.Nodes.Stmt_Do:
                    case AstConstants.Nodes.Stmt_While:
                    case AstConstants.Nodes.Stmt_Foreach:
                        DoAndWhileAndForeachStatementExit(node);
                        break;
                    case AstConstants.Nodes.Stmt_ClassMethod:
                        ClassMethodExit(node);
                        break;
                }
            }

            if (node.Prefix.Equals(AstConstants.Subnode))
            {
                switch (node.LocalName)
                {
                    // For loops:
                    case AstConstants.Subnodes.Init:
                        //InitExit(node);
                        break;
                    case AstConstants.Subnodes.Cond:
                        //CondExit(node);
                        break;
                    case AstConstants.Subnodes.Loop:
                        break;
                }
            }
        }

        private void ReturnStatementEnter(XmlNode node)
        {
            NormalStatementEnter(node);

            ConnectBlocks(CurrentBlock, _exitBlock, EdgeType.Normal);

            CurrentBlock.BreaksOutOfScope = true;
        }

        private void NormalStatementEnter(XmlNode node)
        {
            if (CurrentBlock.BreaksOutOfScope)
            {
                CurrentBlock = new CFGBlock();
                Graph.AddVertex(CurrentBlock);
            }
            else
            {
                CurrentBlock = ConnectNewBlockTo(CurrentBlock, EdgeType.Normal);
            }

            CurrentBlock.AstEntryNode = node;

            DoNotVisitChildren(node);
        }

        #region If statements
        private void IfStatementEnter(XmlNode node)
        {
            if (CurrentBlock.BreaksOutOfScope)
            {
                CurrentBlock = new CFGBlock();
                Graph.AddVertex(CurrentBlock);
            }
            CFGBlock conditionBlock = ConnectNewBlockTo(CurrentBlock, EdgeType.Normal);
            CurrentBlock = conditionBlock;

            CFGBlock trueBlock = ConnectNewBlockTo(CurrentBlock, EdgeType.True);
            CurrentBlock = trueBlock;

            conditionBlock.AstEntryNode = node;

            var ifScope = new IfScope(conditionBlock, trueBlock) { EndBlock = new CFGBlock() };
            Graph.AddVertex(ifScope.EndBlock);

            DoNotVisitChildren(Conditional.GetCondNode(node));
            scopeHandler.PushIfStmt(ifScope);
        }
        private void ElseStatementsEnter(XmlNode node)
        {
            var ifblock = (IfScope)scopeHandler.CurrentScope;
            if(ifblock.ElseifBlock == null)
                ifblock.TrueNode = CurrentBlock;
            var falseNode = new CFGBlock();
            TaggedEdge<CFGBlock, EdgeTag> newEdge;
            if (ifblock.ElseifBlock == null)
            {
                newEdge = new TaggedEdge<CFGBlock, EdgeTag>(ifblock.IfConditionNode, falseNode, new EdgeTag(EdgeType.False));
            }
            else
            {
                newEdge = new TaggedEdge<CFGBlock, EdgeTag>(ifblock.ElseifBlock, falseNode, new EdgeTag(EdgeType.False));
            }
            Graph.AddVerticesAndEdge(newEdge);
            CurrentBlock = falseNode;
        }
        private void ElseStatementsExit(XmlNode node)
        {
            scopeHandler.GetIfStmt().FalseNode = CurrentBlock;
        }
        private void IfStatementExit(XmlNode node)
        {
            var currentIfBlock = scopeHandler.GetIfStmt();
            var endIfNode = currentIfBlock.EndBlock;
            //Graph.AddVertex(endIfNode);

            if (currentIfBlock.IsFalseNodeSet())
            {
                if (!currentIfBlock.FalseNode.BreaksOutOfScope)
                {
                    var falseToEndEdge = new TaggedEdge<CFGBlock, EdgeTag>(currentIfBlock.FalseNode, endIfNode, new EdgeTag(EdgeType.Normal));
                    Graph.AddEdge(falseToEndEdge);
                }
            }
            else if (currentIfBlock.ElseifBlock != null)
            {
                if (!currentIfBlock.ElseifBlock.BreaksOutOfScope)
                {
                    var falseToEndEdge = new TaggedEdge<CFGBlock, EdgeTag>(currentIfBlock.ElseifBlock, endIfNode, new EdgeTag(EdgeType.False));
                    Graph.AddEdge(falseToEndEdge);
                }
            }
            else
            {
                var falseEdge = new TaggedEdge<CFGBlock, EdgeTag>(currentIfBlock.IfConditionNode, endIfNode, new EdgeTag(EdgeType.False));
                Graph.AddEdge(falseEdge);

                currentIfBlock.TrueNode = CurrentBlock;
            }

            if (!currentIfBlock.TrueNode.BreaksOutOfScope)
            {
                var trueToEndEdge = new TaggedEdge<CFGBlock, EdgeTag>(currentIfBlock.TrueNode, endIfNode, new EdgeTag(EdgeType.Normal));
                Graph.AddEdge(trueToEndEdge);
            }

            scopeHandler.PopIfStmt();
            CurrentBlock = endIfNode;
        }

        private void ElseIfStatementsEnter(XmlNode node)
        {
            var conditionNode = new CFGBlock { AstEntryNode = node };
            var trueNode = new CFGBlock();

            var currentScope = scopeHandler.GetIfStmt();
            TaggedEdge<CFGBlock, EdgeTag> toCurrentConditionNode;
            if (currentScope.ElseifBlock != null)
                toCurrentConditionNode = new TaggedEdge<CFGBlock, EdgeTag>(currentScope.ElseifBlock, conditionNode, new EdgeTag(EdgeType.False));
            else
                toCurrentConditionNode = new TaggedEdge<CFGBlock, EdgeTag>(currentScope.EntryBlock, conditionNode, new EdgeTag(EdgeType.False));

            var toTrueNodeEdge = new TaggedEdge<CFGBlock, EdgeTag>(conditionNode, trueNode, new EdgeTag(EdgeType.True));

            Graph.AddVerticesAndEdge(toCurrentConditionNode);
            currentScope.ElseifBlock = conditionNode;
            
            Graph.AddVerticesAndEdge(toTrueNodeEdge);

            DoNotVisitChildren(Conditional.GetCondNode(node));
            CurrentBlock = trueNode;
        }
        private void ElseIfStatementsExit(XmlNode node)
        {
            var currentScope = scopeHandler.GetIfStmt();
            var toEndEdge = new TaggedEdge<CFGBlock, EdgeTag>(CurrentBlock, currentScope.EndBlock, new EdgeTag(EdgeType.Normal));
            Graph.AddEdge(toEndEdge);
        }
        #endregion

        #region Switch statements
        private void SwitchStatementEnter(XmlNode node)
        {
            if (CurrentBlock.BreaksOutOfScope)
            {
                CurrentBlock = new CFGBlock();
                Graph.AddVertex(CurrentBlock);
            }
            var switchScope = new SwitchScope(new CFGBlock() { AstEntryNode = node }, new CFGBlock());
            var edgeToSwitch = new TaggedEdge<CFGBlock, EdgeTag>(CurrentBlock, switchScope.SwitchStartNode, new EdgeTag(EdgeType.Normal));
            Graph.AddVerticesAndEdge(edgeToSwitch);
            Graph.AddVertex(switchScope.EndBlock);
            CurrentBlock = switchScope.SwitchStartNode;
            scopeHandler.EnterLoop(switchScope);

            DoNotVisitChildren(Conditional.GetCondNode(node));
        }
        private void SwitchStatementExit(XmlNode node)
        {
            var currScope = (SwitchScope)scopeHandler.CurrentScope;
            
            // Get last inserted block and connect to end
            CFGBlock lastBlock;

            if (currScope.DefaultBlock != null && currScope.CurrentCondition == null)
            {
                // Special case: Switch with only default case
                var toDefault = new TaggedEdge<CFGBlock, EdgeTag>(currScope.SwitchStartNode, currScope.DefaultBlock, new EdgeTag(EdgeType.Normal));
                Graph.AddEdge(toDefault);
                lastBlock = currScope.DefaultTrueBlock;
            }
            else if (currScope.CurrentCondition == null)
            {
                // Special case: Empty switch
                lastBlock = CurrentBlock;
            }
            else if (currScope.DefaultBlock != null)
            {
                var toDefault = new TaggedEdge<CFGBlock, EdgeTag>(currScope.CurrentCondition, currScope.DefaultBlock, new EdgeTag(EdgeType.False));
                Graph.AddEdge(toDefault);
                lastBlock = currScope.DefaultTrueBlock;
            }
            else
            {
                var endCondEdge = new TaggedEdge<CFGBlock, EdgeTag>(currScope.CurrentCondition, currScope.EndBlock, new EdgeTag(EdgeType.False));
                lastBlock = CurrentBlock;
                Graph.AddEdge(endCondEdge);
            }
            
            // Only create edge to end if the break/continue has not already done so
            if (!CurrentBlock.BreaksOutOfScope)
            {
                var endTrueEdge = new TaggedEdge<CFGBlock, EdgeTag>(lastBlock, currScope.EndBlock, new EdgeTag(EdgeType.Normal));
                Graph.AddEdge(endTrueEdge);
            }
            CurrentBlock = currScope.EndBlock;
            scopeHandler.LeaveLoop();
        }
        private void SwitchCaseStatementEnter(XmlNode node)
        {
            var conditionNode = new CFGBlock();
            var trueNode = new CFGBlock();
            conditionNode.AstEntryNode = node;

            var currentScope = (SwitchScope)scopeHandler.GetInnermostLoop();
            TaggedEdge<CFGBlock, EdgeTag> toCurrentConditionNode;
            if (CurrentBlock == currentScope.EntryBlock)
            {
                toCurrentConditionNode = new TaggedEdge<CFGBlock, EdgeTag>(CurrentBlock, conditionNode, new EdgeTag(EdgeType.Normal));
            }
            else
            {
                if(currentScope.CurrentCondition != null)
                    toCurrentConditionNode = new TaggedEdge<CFGBlock, EdgeTag>(currentScope.CurrentCondition, conditionNode, new EdgeTag(EdgeType.False));
                else
                    toCurrentConditionNode = new TaggedEdge<CFGBlock, EdgeTag>(currentScope.EntryBlock, conditionNode, new EdgeTag(EdgeType.Normal));
            }

            var toTrueNodeEdge = new TaggedEdge<CFGBlock, EdgeTag>(conditionNode, trueNode, new EdgeTag(EdgeType.True));

            if (Case.IsDefaultCase(node))
            {
                currentScope.DefaultBlock = conditionNode;
                currentScope.DefaultTrueBlock = trueNode;
            }
            else
            {
                Graph.AddVerticesAndEdge(toCurrentConditionNode);
                currentScope.CurrentCondition = conditionNode;
            }

            Graph.AddVerticesAndEdge(toTrueNodeEdge);

            if (!CurrentBlock.BreaksOutOfScope && CurrentBlock != currentScope.EntryBlock)
            {
                var fallthrough = new TaggedEdge<CFGBlock, EdgeTag>(CurrentBlock, trueNode, new EdgeTag(EdgeType.Normal));
                Graph.AddEdge(fallthrough);
            }
            CurrentBlock = trueNode;

            DoNotVisitChildren(Conditional.GetCondNode(node));
        }
        #endregion

        #region Functions
        private void FunctionEnter(XmlNode node)
        {
            CurrentBlock.AstEntryNode = node;
        }
        private void FunctionExit(XmlNode node)
        {
            //_exitBlock.AstEntryNode = node;
        }
        #endregion

        private void ClassMethodEnter(XmlNode node)
        {
            CurrentBlock.AstEntryNode = node;
        }
        private void ClassMethodExit(XmlNode node)
        {
            //_exitBlock.AstEntryNode = node;
        }

        private void ForStatementEnter(XmlNode node)
        {
            CFGBlock forLoopInit = new CFGBlock();
            if (!CurrentBlock.BreaksOutOfScope)
            {
                forLoopInit = ConnectNewBlockTo(CurrentBlock, EdgeType.Normal);
            }
            forLoopInit.AstEntryNode = ForLoop.GetInitNode(node);
            DoNotVisitChildren(forLoopInit.AstEntryNode);

            CFGBlock conditionBlock = ConnectNewBlockTo(forLoopInit, EdgeType.Normal);
            conditionBlock.AstEntryNode = node;
            DoNotVisitChildren(Conditional.GetCondNode(node));

            CFGBlock loopUpdateBlock = new CFGBlock { AstEntryNode = ForLoop.GetLoopNode(node) };
            DoNotVisitChildren(loopUpdateBlock.AstEntryNode);

            var edge = new TaggedEdge<CFGBlock, EdgeTag>(loopUpdateBlock, conditionBlock, new EdgeTag(EdgeType.Normal));
            Graph.AddVerticesAndEdge(edge);

            CFGBlock loopBodyBlock = ConnectNewBlockTo(conditionBlock, EdgeType.True);
            CFGBlock loopDoneBlock = ConnectNewBlockTo(conditionBlock, EdgeType.False);

            var loopScope = new LoopScope(forLoopInit)
            {
                LoopConditionBlock = loopUpdateBlock,
                LoopBodyStartBlock = loopBodyBlock,
                LoopUpdateBlock = loopUpdateBlock,
                ContinueDestination = loopUpdateBlock,
                EndBlock = loopDoneBlock
            };
            scopeHandler.EnterLoop(loopScope);

            CurrentBlock = loopBodyBlock;
        }
        private void ForStatementExit(XmlNode node)
        {
            var loopScope = (LoopScope)scopeHandler.LeaveLoop();
            if (!CurrentBlock.BreaksOutOfScope)
            {
                ConnectBlocks(CurrentBlock, loopScope.LoopConditionBlock, EdgeType.Normal);
            }

            CurrentBlock = loopScope.EndBlock;
        }

        private void WhileStatementEnter(XmlNode node)
        {
            WhileOrForeachStatementEnter(node);
            DoNotVisitChildren(Conditional.GetCondNode(node));
        }
        private void WhileOrForeachStatementEnter(XmlNode node)
        {
            CFGBlock conditionBlock = new CFGBlock();
            if (!CurrentBlock.BreaksOutOfScope)
            {
                conditionBlock = ConnectNewBlockTo(CurrentBlock, EdgeType.Normal);
            }

            CFGBlock loopBodyBlock;
            CFGBlock loopExitBlock;
            if (node.LocalName == AstConstants.Nodes.Stmt_Foreach)
            {
                loopBodyBlock = ConnectNewBlockTo(conditionBlock, EdgeType.Normal);
                loopExitBlock = ConnectNewBlockTo(conditionBlock, EdgeType.Normal);
            }
            else
            {
                loopBodyBlock = ConnectNewBlockTo(conditionBlock, EdgeType.True);
                loopExitBlock = ConnectNewBlockTo(conditionBlock, EdgeType.False);
            }
            

            conditionBlock.AstEntryNode = node;

            var whileLoopScope = new LoopScope(conditionBlock)
            {
                LoopConditionBlock = conditionBlock,
                LoopBodyStartBlock = loopBodyBlock,
                ContinueDestination = conditionBlock,
                EndBlock = loopExitBlock
            };
            scopeHandler.EnterLoop(whileLoopScope);

            if (node.LocalName == AstConstants.Nodes.Stmt_Foreach)
            {
                DoNotVisitChildren(node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.Expr));
                DoNotVisitChildren(node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.KeyVar));
                DoNotVisitChildren(node.GetSubNode(AstConstants.Subnode + ":" + AstConstants.Subnodes.ValueVar));
            }
            else
            {
                DoNotVisitChildren(Conditional.GetCondNode(node));
            }
            

            CurrentBlock = loopBodyBlock;
        }
        private void DoStatementEnter(XmlNode node)
        {
            var loopEntryBlock = new CFGBlock();
            if (!CurrentBlock.BreaksOutOfScope)
            {
                loopEntryBlock = ConnectNewBlockTo(CurrentBlock, EdgeType.Normal);
            }
            Graph.AddVertex(loopEntryBlock);
            CurrentBlock = loopEntryBlock;

            CFGBlock conditionBlock = new CFGBlock { AstEntryNode = node };
            CFGBlock loopDoneBlock = ConnectNewBlockTo(conditionBlock, EdgeType.False);
            ConnectBlocks(conditionBlock, loopEntryBlock, EdgeType.True);

            var loopScope = new LoopScope(loopEntryBlock)
            {
                LoopBodyStartBlock = loopEntryBlock,
                LoopConditionBlock = conditionBlock,
                ContinueDestination = conditionBlock,
                EndBlock = loopDoneBlock,
            };
            scopeHandler.EnterLoop(loopScope);
            DoNotVisitChildren(Conditional.GetCondNode(node));
        }
        private void DoAndWhileAndForeachStatementExit(XmlNode node)
        {
            var loopScope = (LoopScope)scopeHandler.LeaveLoop();

            if (!CurrentBlock.BreaksOutOfScope)
            {
                ConnectBlocks(CurrentBlock, loopScope.LoopConditionBlock, EdgeType.Normal);
            }

            CurrentBlock = loopScope.EndBlock;
        }

        private void ContinueStatementEnter(XmlNode node)
        {
            int continueScopeLevel = GetBreakOrContinueScopeLevel(node);
            AbstractScope scope = scopeHandler.GetLoopScope(continueScopeLevel - 1);
            if (scope is SwitchScope)
            {
                ConnectBlocks(CurrentBlock, scope.EndBlock, EdgeType.Normal);
            }
            else
            {
                var loopScope = (LoopScope) scope;
                ConnectBlocks(CurrentBlock, loopScope.ContinueDestination, EdgeType.Normal);
            }
            CurrentBlock.BreaksOutOfScope = true;
        }
        private void BreakStatementEnter(XmlNode node)
        {
            var breakArgument = GetBreakOrContinueScopeLevel(node);
            //Sometimes code contains "invalid" break statements.
            if (scopeHandler.IsInLoop)
            {
                AbstractScope loopScope = scopeHandler.GetLoopScope(breakArgument - 1);
                ConnectBlocks(CurrentBlock, loopScope.EndBlock, EdgeType.Normal);
                CurrentBlock.BreaksOutOfScope = true;
            }
        }

        private static int GetBreakOrContinueScopeLevel(XmlNode node)
        {
            int breakArgument;
            if (!BreakContinue.TryGetScopeNumber(node, out breakArgument) || breakArgument < BreakContinue.DefaultScopeNumber)
            {
                // POSSIBLY INVALID: If we can't parse the number, we assume it is 1 (the default). 
                // POSSIBLY INVALID: This is not necessarily correct. Older versions of PHP (< 5.4) allowed the
                // POSSIBLY INVALID: continue/break argument to be dynamic. We currently do not support dynamic values. 
                // POSSIBLY INVALID: We do support constants below 1 (since they default to 1)
                breakArgument = 1;
            }
            return breakArgument; 
        }

        public void TraverseEnd(object sender, XmlEndTraverseEventArgs e)
        {
            ConnectBlocks(CurrentBlock, _exitBlock, EdgeType.Normal);
        }

        private CFGBlock ConnectNewBlockTo(CFGBlock block, EdgeType edgeType)
        {
            var newBlock = new CFGBlock();
            var edge = new TaggedEdge<CFGBlock, EdgeTag>(block, newBlock, new EdgeTag(edgeType));
            Graph.AddVerticesAndEdge(edge);

            return newBlock;
        }
        private Edge<CFGBlock> ConnectBlocks(CFGBlock source, CFGBlock target, EdgeType edgeType)
        {
            var edge = new TaggedEdge<CFGBlock, EdgeTag>(source, target, new EdgeTag(edgeType));
            Graph.AddEdge(edge);

            return edge;
        }

        private static bool IsNodeOfInterest(XmlNode node)
        {
            return node.Prefix.Equals(AstConstants.Node) ||
                   node.Prefix.Equals(AstConstants.Subnode);
        }

        private void DoNotVisitChildren(XmlNode node)
        {
            if (traverseBlockingNode == null)
            {
                traverseBlockingNode = node;
            }
            else
            {
                nodesNotToVisit.Add(node);
            }
        }
    }
}
