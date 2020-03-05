//#define MODELINTERPRETER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.Interpreter.PowerModel
{
#if MODELINTERPRETER
    public partial class ModelInterpreter
    {
        public class ValueException : Exception
        {
        }
        /*
         Generator
            LoadWye
            {
                Nodes:
                {
                    {name:"in",type:ThreePhase},
                    {name:"n",type:OnePhase}
                }
                Parameters:
                {
                    {name:"RA",type:Float,DefaultValue,Min,Max},
                    {name:"RB",type:Float},
                    {name:"RC",type:Float},
                }
            }
            Resistor
            {
                Nodes:
                {
                    {name:"in",type:OnePhase}
                    {name:"out",type:OnePhase}
                }
                Parameters:
                {
                    {name:"R",type:Float}
                }
            }

            Ground
            {
                Nodes:
                {name:"g",type: OnePhase}
                Parameters:
                {
                }
            }
             
             
             
         */
        private class Value
        {
            public enum Type
            {
                Float,
                Int,
                Complex,
                Bool,
                Object,
                Array,
                Void
            };
            Type type;
            public Value(Type type)
            {
                this.type = type;
            }
        }
        private class VoidValue:Value
        {
            public VoidValue():base(Type.Void)
            {
            }
        }
        private class FloatValue:Value
        {
            public float Value { get; set; }
            public FloatValue():base(Type.Float)
            {
            }
        }
        private class IntValue : Value
        {
            public int Value { get; set; }
            public IntValue() : base(Type.Int)
            {
            }
        }
        private class BoolValue : Value
        {
            public bool Value { get; set; }
            public BoolValue() : base(Type.Bool)
            {
            }
        }

        private class Object : Value
        {
            public string Name;
            public Dictionary<string, Value> Values;
            public Object():base(Type.Object)
            {
            }
        }
        private class Array : Value
        {
            public Value[] values;
            public Array() : base(Type.Array)
            {

            }
        }
        private class ElementDescription
        {
            public abstract class TransientElementModel
            {
                public abstract ITransientElement CreateElement(Object elementObject);
            }
            public abstract class SteadyStateElementModel
            {
                public abstract ISteadyStateElement CreateElement(Object elementObject);
            }
            public class ParameterDescription
            {
                Value.Type type;
                Value defaultValue;
                Value minValue;
                Value maxValue;
                public ParameterDescription(Value.Type type,Value defaultValue = null,Value minValue = null,Value maxValue = null)
                {
                    this.type = type;
                    this.defaultValue = defaultValue;
                    this.minValue = minValue;
                    this.maxValue = maxValue;
                }
            }
            public enum NodeType
            {
                OnePhase,
                ThreePhase
            }
            Dictionary<string, NodeType> nodes;
            Dictionary<string, ParameterDescription> parameters;
            TransientElementModel transientModel;
            SteadyStateElementModel steadyStateModel;
            public ElementDescription(Dictionary<string, NodeType> nodes, Dictionary<string, ParameterDescription> parameters,
                TransientElementModel transientModel, SteadyStateElementModel steadyStateModel)
            {
                this.nodes = nodes;
                this.parameters = parameters;
                this.transientModel = transientModel;
                this.steadyStateModel = steadyStateModel;
            }
            public TransientElement CreateTransientElement(Object elementObject)
            {
                return transientModel!=null?transientModel.CreateElement(elementObject):null;
            }
            public SteadyStateElement CreateSteadyStateElement(Object elementObject)
            {
                return steadyStateModel != null ? steadyStateModel.CreateElement(elementObject) : null;
            }
        }
        Dictionary<string, ElementDescription> elementsMap;
        private void InitElements()
        {
            elementsMap = new Dictionary<string, ElementDescription>();
            elementsMap.Add("resistor", new ElementDescription (
                new Dictionary<string, ElementDescription.NodeType>{
                    { "in",ElementDescription.NodeType.OnePhase },
                    { "out",ElementDescription.NodeType.OnePhase}
                },
                new Dictionary<string, ElementDescription.ParameterDescription>{
                        { "R",new ElementDescription.ParameterDescription(
                            Value.Type.Float,
                            new FloatValue{Value=1.0f },
                            new FloatValue{Value=0.0f },
                            null
                        )
                    }
                })
            );
            elementsMap.Add("loadWye",new ElementDescription(
                new Dictionary<string, ElementDescription.NodeType>{
                    { "in",ElementDescription.NodeType.ThreePhase },
                    { "n",ElementDescription.NodeType.OnePhase}
                },
                new Dictionary<string, ElementDescription.ParameterDescription>{
                        { "RA",new ElementDescription.ParameterDescription(
                            Value.Type.Float,
                            new FloatValue{Value=1.0f },
                            new FloatValue{Value=0.0f },
                            null
                        )
                        },
                        { "RB",new ElementDescription.ParameterDescription(
                            Value.Type.Float,
                            new FloatValue{Value=1.0f },
                            new FloatValue{Value=0.0f },
                            null
                        )
                        },
                        { "RC",new ElementDescription.ParameterDescription(
                            Value.Type.Float,
                            new FloatValue{Value=1.0f },
                            new FloatValue{Value=0.0f },
                            null
                        )
                        }
                })
            );
        }
        /*private class ObjectDescription:Type
        {
            public class ObjectField
            {
                Type type;
                Value defaultValue;
                Value minValue;
                Value maxValue;
            }
            Dictionary<string, ObjectField> keyValue;
        }*/
        public interface IModel
        {
            string Solve();
        }
        private Object BuildObject(ObjectNode node)
        {
            Object result = new Object();
            result.Name = node.Name;
            result.Values = new Dictionary<string, Value>();
            foreach (var argument in node.Arguments)
            {
                try
                {
                    ExpressionNode value = ComputeExpression(argument.Value);
                    result.Values.Add(argument.Key, ToValue(value));
                }
                catch (ArgumentException exc)
                {
                    errors.Add(new ErrorMessage($"Нельзя использовать два параметра с одинаковыми именами:{argument.Key} в {node.Name}", argument.Line, argument.Position));
                }
                catch (ValueException exc)
                {
                    errors.Add(new ErrorMessage($"Не возможно преобразовать выражение в константное значение", argument.Value.Line, argument.Value.Position));
                }
            }
            return result;
        }

        public class TransientModel: IModel
        {
            List<TransientElement> elements;
            TransientSolver solver;
            private string GenerateEquations()
            {

            }
            public string Solve()
            {

            }
        }
        public class SteadyStateSolver
        {
        }
        public class SteadyStateModel: IModel
        {
            List<SteadyStateElement> elements;
            SteadyStateSolver solver;
            double frequency;
            private string GenerateEquations()
            {

            }
            public string Solve()
            {
                
            }
        }
        /*public class OptimizationModel : IModel
        {
            List<SteadyStateElement> elements;
            List<OptimizationVariable> variables;
            int iterations;
            private string GenerateEquations()
            {

            }
            public string Solve()
            {
                throw new NotImplementedException();
            }
        }*/
        private void ResolveStatements(List<ExpressionNode> statements)
        {

        }
        private ExpressionNode ComputeExpression(ExpressionNode expression)
        {

        }
        private Value ToValue(ExpressionNode node)
        {

        }
        private TransientModel GetTransientModel(ModelNode root, ref List<ErrorMessage> errorList, ref List<string> output);
        private SteadyStateModel GetSteadyStateModel(ModelNode root, ref List<ErrorMessage> errorList, ref List<string> output)
        {
            //parameters:
            //solver
            foreach connection
                assign int index to each node
            //for each element in element definitions
            foreach (var elementDefinition in elementDefinitions)
            {
                ISteadyStateElement element = CreateSteadyStateElement(elementDefinition);
            }
            //create SteadyStateElement
            SteadyStateModel model = new SteadyStateModel();
        }
        private void ResolveConnection()
        {
            //for each connection
            //find object in element defintions
            //get element nodes description
            //create connection
        }
        Dictionary<string, Object> elementDefinitions;
        List<ErrorMessage> errors;
        public IModel Generate(ModelNode root, ref List<ErrorMessage> errorList, ref List<string> output)
        {
            this.errors = errorList;
            ResolveStatements(root.Statements);
            //set model type
            List<Object> objects;
            List<string> ids;
            foreach (var element in root.Elements)
            {
                //get Object from element.Definition
                Object obj = BuildObject(element.Definition);
                elementDefinitions.Add(element.Id,obj);
            }
            ResolveConnection(root.Connections);
            switch (root.ModelType)
            {
                case "steadystate":
                    GetSteadyStateModel(root, ref errorList, ref output);
                    break;
                case "transient":
                    GetTransientModel(root, ref errorList, ref output);
                    break;
                default:
                    errorList.Add(new ErrorMessage("Incorrect model type", root.Line, root.Position));
                    return null;
            }
            //resolve connections
        }
    }
#endif
}
