using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Misc;

namespace ElectricalPowerSystems.PowerModel.NewModel.Recloser
{
    public class Visitor : RecloserGrammarBaseVisitor<Node>
    {
        public override Node VisitArguments([NotNull] RecloserGrammarParser.ArgumentsContext context)
        {
            return base.VisitArguments(context);
        }
        //Done
        public override Node VisitArrayElementAccess([NotNull] RecloserGrammarParser.ArrayElementAccessContext context)
        {
            return new ArrayAccessNode
            {
                Array = Visit(context.array) as ExpressionNode,
                Index = Visit(context.index) as ExpressionNode
            };
        }

        public override Node VisitArrayInitializerList([NotNull] RecloserGrammarParser.ArrayInitializerListContext context)
        {
            return base.VisitArrayInitializerList(context);
        }

        public override Node VisitBinaryOperator([NotNull] RecloserGrammarParser.BinaryOperatorContext context)
        {
            return base.VisitBinaryOperator(context);
        }
        //Done
        public override Node VisitBooleanConstant([NotNull] RecloserGrammarParser.BooleanConstantContext context)
        {
            return new BoolNode
            {
                Value = (context.value.Type == RecloserGrammarLexer.TRUE ? true : false),
                Line = context.value.Line,
                Position = context.value.Column
            };
        }
        //Done
        public override Node VisitBracketExpression([NotNull] RecloserGrammarParser.BracketExpressionContext context)
        {
            return Visit(context.expression());
        }
        //Done
        public override Node VisitBreakStatement([NotNull] RecloserGrammarParser.BreakStatementContext context)
        {
            return new BreakNode {
                Line = context.Start.Line,
                Position = context.Start.Column
            };
        }
        //Done
        public override Node VisitConstant([NotNull] RecloserGrammarParser.ConstantContext context)
        {
            return base.VisitConstant(context);
        }
        //Done
        public override Node VisitConstantExpression([NotNull] RecloserGrammarParser.ConstantExpressionContext context)
        {
            return Visit(context.constant());
        }
        //Done
        public override Node VisitContinueStatement([NotNull] RecloserGrammarParser.ContinueStatementContext context)
        {
            return new ContinueNode
            {
                Line = context.Start.Line,
                Position = context.Start.Column
            };
        }

        public override Node VisitExpression([NotNull] RecloserGrammarParser.ExpressionContext context)
        {
            return base.VisitExpression(context);
        }
        //Done
        public override Node VisitExpressionStatement([NotNull] RecloserGrammarParser.ExpressionStatementContext context)
        {
            return Visit(context.expression());
        }
        //Done
        public override Node VisitFieldAccess([NotNull] RecloserGrammarParser.FieldAccessContext context)
        {
            return new FieldAccessNode
            {
                Parent = Visit(context.parent) as ExpressionNode,
                Field = context.field.Text
            };
        }
        //Done
        public override Node VisitFloatConstant([NotNull] RecloserGrammarParser.FloatConstantContext context)
        {
            return new FloatNode
            {
                Value = double.Parse(context.value.Text, CultureInfo.InvariantCulture),
                Line = context.value.Line,
                Position = context.value.Column
            };
        }
        //Done
        public override Node VisitFunctionBody([NotNull] RecloserGrammarParser.FunctionBodyContext context)
        {
            return base.VisitFunctionBody(context);
        }
        //Done
        public override Node VisitFunctionCall([NotNull] RecloserGrammarParser.FunctionCallContext context)
        {
            List<ExpressionNode> arguments = new List<ExpressionNode>();
            foreach (var argument in context.arguments().expression())
            {
                arguments.Add(Visit(argument) as ExpressionNode);
            }
            return new FunctionCallNode {
                Name = context.name.Text,
                Arguments = arguments
            };
        }
        //Done
        public override Node VisitFunctionDefinition([NotNull] RecloserGrammarParser.FunctionDefinitionContext context)
        {
            List<SignatureArgumentNode> signature = new List<SignatureArgumentNode>();
            List<StatementNode> body = new List<StatementNode>();
            foreach (var signatureArgument in context.functionSignature().signatureArgument())
            {
                signature.Add(Visit(signatureArgument) as SignatureArgumentNode);
            }
            foreach (var statement in context.functionBody().statement())
            {
                body.Add(Visit(statement) as StatementNode);
            }
            return new FunctionDefinitionNode
            {
                ReturnType = Visit(context.type) as TypeNode,
                Name = context.name.Text,
                Signature = signature,
                Body = body
            };
        }
        //Done
        public override Node VisitFunctionDeclaration([NotNull] RecloserGrammarParser.FunctionDeclarationContext context)
        {
            List<SignatureArgumentNode> signature = new List<SignatureArgumentNode>();
            foreach (var argument in context.functionSignature().signatureArgument())
            {
                signature.Add(Visit(argument) as SignatureArgumentNode);
            }
            return new FunctionDeclarationNode
            {
                ReturnType = Visit(context.type) as TypeNode,
                Name = context.name.Text,
                Signature = signature
            };
        }
        //Done
        public override Node VisitFunctionSignature([NotNull] RecloserGrammarParser.FunctionSignatureContext context)
        {
            return base.VisitFunctionSignature(context);
        }
        //Done
        public override Node VisitIdentifierExpression([NotNull] RecloserGrammarParser.IdentifierExpressionContext context)
        {
            return new IdentifierNode
            {
                Identifier = context.id.Text,
                Line = context.id.Line,
                Position = context.id.Column
            };
        }
        //Done
        public override Node VisitIfStatement([NotNull] RecloserGrammarParser.IfStatementContext context)
        {
            if (context.elsebody == null)
            {
                return new IfNode
                {
                    Condition = Visit(context.condition) as ExpressionNode,
                    IfBody = Visit(context.ifbody) as StatementNode
                };
            }
            return new IfElseNode
            {
                Condition = Visit(context.condition) as ExpressionNode,
                IfBody = Visit(context.ifbody) as StatementNode,
                ElseBody = Visit(context.elsebody) as StatementNode
            };
        }
        //Done
        public override Node VisitIntegerConstant([NotNull] RecloserGrammarParser.IntegerConstantContext context)
        {
            return new IntegerNode
            {
                Value = int.Parse(context.value.Text),
                Line = context.value.Line,
                Position = context.value.Column
            };
        }

        public override Node VisitPostIncrementDecrement([NotNull] RecloserGrammarParser.PostIncrementDecrementContext context)
        {
            return base.VisitPostIncrementDecrement(context);
        }

        public override Node VisitPreIncrementDecrement([NotNull] RecloserGrammarParser.PreIncrementDecrementContext context)
        {
            return base.VisitPreIncrementDecrement(context);
        }

        //Done
        public override Node VisitProgram([NotNull] RecloserGrammarParser.ProgramContext context)
        {
            List<StatementNode> statements = new List<StatementNode>();
            foreach (var statement in context.statement())
            {
                statements.Add(Visit(statement) as StatementNode);
            }
            return new ProgramNode(statements);
        }
        //Done
        public override Node VisitReturnStatement([NotNull] RecloserGrammarParser.ReturnStatementContext context)
        {
            return new ReturnNode
            {
                Expression = context.expression()==null?null:Visit(context.expression()) as ExpressionNode
            };
        }
        //Done
        public override Node VisitScopeStatement([NotNull] RecloserGrammarParser.ScopeStatementContext context)
        {
            List<StatementNode> body = new List<StatementNode>();
            foreach (var statement in context.statement())
            {
                body.Add(Visit(statement) as StatementNode);
            }
            return new ScopeNode
            {
                Body = body
            };
        }
        //Done
        public override Node VisitSignatureArgument([NotNull] RecloserGrammarParser.SignatureArgumentContext context)
        {
            SignatureArgumentNode.WayType way = SignatureArgumentNode.WayType.In;
            if (context.way.Type == RecloserGrammarLexer.INOUT)
            {
                way = SignatureArgumentNode.WayType.InOut;
            }
            return new SignatureArgumentNode
            {
                Name = context.name.Text,
                Way = way,
                ArgumentType = Visit(context.type) as TypeNode
            };
        }

        //Done
        public override Node VisitStatement([NotNull] RecloserGrammarParser.StatementContext context)
        {
            return base.VisitStatement(context);
        }
        //Done
        public override Node VisitStructField([NotNull] RecloserGrammarParser.StructFieldContext context)
        {
            return new StructFieldNode
            {
                FieldType = Visit(context.type) as TypeNode,
                Name = context.name.Text
            };
        }
        //Done
        public override Node VisitStructDefinition([NotNull] RecloserGrammarParser.StructDefinitionContext context)
        {
            List<StructFieldNode> fields = new List<StructFieldNode>();
            foreach (var field in context.structField())
            {
                fields.Add(Visit(field) as StructFieldNode);
            }
            return new StructDefinitionNode
            {
                Name = context.structname.Text,
                Fields = fields,
            };
        }

        public override Node VisitStructInitializerList([NotNull] RecloserGrammarParser.StructInitializerListContext context)
        {
            return base.VisitStructInitializerList(context);
        }
        //Done
        public override Node VisitSwitchStatement([NotNull] RecloserGrammarParser.SwitchStatementContext context)
        {
            List<CaseNode> cases = new List<CaseNode>();
            foreach (var @case in context.@case())
            {
                cases.Add(Visit(@case) as CaseNode);
            }
            return new SwitchStatementNode
            {
                Expression = Visit(context.expression()) as ExpressionNode,
                Cases = cases
            };
        }
        //Done
        public override Node VisitTernaryOperator([NotNull] RecloserGrammarParser.TernaryOperatorContext context)
        {
            return new TernaryOperatorNode
            {
                Condition = Visit(context.condition) as ExpressionNode,
                First = Visit(context.first) as ExpressionNode,
                Second = Visit(context.second) as ExpressionNode
            };
        }
        //Done
        public override Node VisitTypeRule([NotNull] RecloserGrammarParser.TypeRuleContext context)
        {
            return new TypeNode
            {
                Typename = context.typename.Text,
                Size = context.size == null ? null : new IntegerNode
                {
                    Value = int.Parse(context.size.Text)
                }
            };
        }

        public override Node VisitUnaryOperator([NotNull] RecloserGrammarParser.UnaryOperatorContext context)
        {
            return base.VisitUnaryOperator(context);
        }
        public override Node VisitVariableDeclaration([NotNull] RecloserGrammarParser.VariableDeclarationContext context)
        {
            return base.VisitVariableDeclaration(context);
        }

        public override Node VisitVariableDeclarationStatement([NotNull] RecloserGrammarParser.VariableDeclarationStatementContext context)
        {
            return base.VisitVariableDeclarationStatement(context);
        }
        public override Node VisitVariable([NotNull] RecloserGrammarParser.VariableContext context)
        {
            return base.VisitVariable(context);
        }

        //Done
        public override Node VisitWhileStatement([NotNull] RecloserGrammarParser.WhileStatementContext context)
        {
            return new WhileNode
            {
                Condition = Visit(context.condition) as ExpressionNode,
                Body = Visit(context.body) as StatementNode,
                Line = context.Start.Line,
                Position = context.Start.Column
            };
        }
    }
}
