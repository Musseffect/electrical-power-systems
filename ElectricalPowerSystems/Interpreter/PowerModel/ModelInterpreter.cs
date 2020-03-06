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
        static ModelInterpreter instance;
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
        public interface ITransientElementModel
        {
            ITransientElement CreateElement(Object elementObject);
        }
        public interface ISteadyStateElementModel
        {
            ISteadyStateElement CreateElement(Object elementObject);
        }
        public abstract class Node
        {
            private int index;
            public int Index { get; }
            public Node(int index)
            {
                this.index = index;
            }
            public abstract string[] GetNodeVariables();
        }
        public class Node1Phase : Node
        {
            public Node1Phase(int index) : base(index)
            {
            }
            public override string[] GetNodeVariables()
            {
                return new string[] { $"v_{Index}" };
            }
        }
        public class Node3Phase : Node
        {
            public Node3Phase(int index) : base(index)
            {
            }
            public override string[] GetNodeVariables()
            {
                return new string[] { $"v_{Index}a", $"v_{Index}b", $"v_{Index}c" };
            }
        }
        public struct Connection
        {
            public int node1;
            public int node2;
        }
        public class ElementDescription
        {
            public class ParameterDescription
            {
                Constant.Type type;
                Constant defaultValue;
                Constant minValue;
                Constant maxValue;
                public ParameterDescription(Constant.Type type, Constant defaultValue = null, Constant minValue = null, Constant maxValue = null)
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
            readonly Dictionary<string, NodeType> nodes;
            Dictionary<string, ParameterDescription> parameters;
            ITransientElementModel transientModel;
            ISteadyStateElementModel steadyStateModel;
            public ElementDescription(Dictionary<string, NodeType> nodes, Dictionary<string, ParameterDescription> parameters,
                ITransientElementModel transientModel, ISteadyStateElementModel steadyStateModel)
            {
                this.nodes = nodes;
                this.parameters = parameters;
                this.transientModel = transientModel;
                this.steadyStateModel = steadyStateModel;
            }
            public ITransientElement CreateTransientElement(Object elementObject)
            {
                return transientModel!=null?transientModel.CreateElement(elementObject):null;
            }
            public ISteadyStateElement CreateSteadyStateElement(Object elementObject,
                Dictionary<string, int> elementNodes)
            {
                return steadyStateModel != null ? steadyStateModel.CreateElement(elementObject, elementNodes) : null;
            }
            public NodeType GetNodeType(string name)
            {
                return nodes[name];
            }
            public Dictionary<string,NodeType>.KeyCollection GetNodes()
            {
                return nodes.Keys;
            }
            public int GetNodeCount()
            {
                return nodes.Count;
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
                            Constant.Type.Float,
                            new FloatValue{Value=1.0f },
                            new FloatValue{Value=0.0f },
                            null
                        )
                    }
                },null, new SteadyStateResistorModel())
            );
            elementsMap.Add("voltageSource", new ElementDescription(
                new Dictionary<string, ElementDescription.NodeType>{
                    { "in",ElementDescription.NodeType.OnePhase },
                    { "out",ElementDescription.NodeType.OnePhase}
                },
                new Dictionary<string, ElementDescription.ParameterDescription>{
                        { "V",new ElementDescription.ParameterDescription(
                            Constant.Type.Float,
                            new FloatValue{Value=1.0f },
                            null,
                            null
                        )
                        },
                        { "phase",new ElementDescription.ParameterDescription(
                            Constant.Type.Float,
                            new FloatValue{Value=0.0f },
                            null,
                            null
                        )
                    }
                },null,new SteadyStateVoltageSourceModel())
            );
            elementsMap.Add("loadWye",new ElementDescription(
                new Dictionary<string, ElementDescription.NodeType>{
                    { "in",ElementDescription.NodeType.ThreePhase },
                    { "n",ElementDescription.NodeType.OnePhase}
                },
                new Dictionary<string, ElementDescription.ParameterDescription>{
                        { "RA",new ElementDescription.ParameterDescription(
                            Constant.Type.Float,
                            new FloatValue{Value=1.0f },
                            new FloatValue{Value=0.0f },
                            null
                        )
                        },
                        { "RB",new ElementDescription.ParameterDescription(
                            Constant.Type.Float,
                            new FloatValue{Value=1.0f },
                            new FloatValue{Value=0.0f },
                            null
                        )
                        },
                        { "RC",new ElementDescription.ParameterDescription(
                            Constant.Type.Float,
                            new FloatValue{Value=1.0f },
                            new FloatValue{Value=0.0f },
                            null
                        )
                        }
                },null,null)
            );
        }
        public interface IModel
        {
            string Solve();
        }
        private TransientModel GetTransientModel(ModelNode root, ref List<ErrorMessage> errorList, ref List<string> output)
        {
            throw new NotImplementedException();
        }
        struct ElementEntry
        {
            ElementDescription description;
            Object obj;
            Dictionary<string, int> elementNodes;

            public int GetNodeIndex(string name)
            {
                return elementNodes[name];
            }
        }
        private SteadyStateModel GetSteadyStateModel(ModelNode root, ref List<ErrorMessage> errorList, ref List<string> output)
        {
            //SteadyStateModel model = new SteadyStateModel();
            //Dictionary<string, ElementDescription> elementIdentifiers = new Dictionary<string, ElementDescription>();
            //Dictionary<string, Object> elementDefinitions = new Dictionary<string, Object>();
            Dictionary<string, ElementEntry> elementEntries = new Dictionary<string, ElementEntry>();
            int nodeIndicies = 0;
            foreach (var element in root.Elements)
            {
                //get Object from element.Definition
                Object obj = BuildObject(elementNode.Definition);
                ElementDescription description = elementsMap[obj.Name];
                //elementDefinitions.Add(element.Id, obj);
                //elementIdentifiers.Add(element.Id,elementsMap[obj.Name]);
                Dictionary<string,int> elementNodes = new Dictionary<string, int>();
                var keys = description.GetNodes();
                for (int i = 0; i < keys.Count; i++)
                {
                    elementNodes.Add(keys.ElementAt(i), nodeIndicies);
                    nodeIndicies++;
                }
                elementEntries.Add(new ElementEntry(description,obj, elementNodes));
                ISteadyStateElement modelElement = description.CreateSteadyStateElement(obj,elementNodes);
                if (element is null)
                {
                    errors.Add(new ErrorMessage($"Element {obj.Name} cannot be used in steady state model",element.Line,element.Position));
                }
            }
            List<Connection> connections = new List<Connection>();
            foreach (var connection in root.Connections)
            {
                try
                {
                    ElementEntry entry1 = elementEntries[connection.Element1];
                    ElementEntry entry2 = elementEntries[connection.Element1];
                    ElementDescription element1 = entry1.GetDescription();
                    ElementDescription element2 = entry2.GetDescription();
                    ElementDescription.NodeType node1 = element1.GetNodeType(connection.Node1);
                    ElementDescription.NodeType node2 = element2.GetNodeType(connection.Node2);
                    if (node1 != node2)
                    {
                        //add error

                        continue;
                    }
                    connections.Add(new Connection(entry1.GetNodeIndex(connection.Node1), entry2.GetNodeIndex(connection.Node2)));
                    /*switch (node1)
                    {
                        case ElementDescription.NodeType.OnePhase:
                            break;
                        case ElementDescription.NodeType.ThreePhase:
                            break;
                    }*/
                } catch (Exception exc)
                {
                    throw new NotImplementedException();
                }
            }
            //parameters:
            //solver
            //create SteadyStateElement
            SteadyStateModel model = new SteadyStateModel();
        }
        List<ErrorMessage> errors;
        private void ResolveStatements(List<ExpressionNode> statements)
        {
            foreach (var expression in statements)
            {
                Eval(expression);
            }
        }
        public IModel Generate(ModelNode root, ref List<ErrorMessage> errorList, ref List<string> output)
        {
            this.errors = errorList;
            ResolveStatements(root.Statements);
            switch (root.ModelType)
            {
                case "steadystate":
                    return GetSteadyStateModel(root, ref errorList, ref output);
                case "transient":
                    return GetTransientModel(root, ref errorList, ref output);
                default:
                    errorList.Add(new ErrorMessage("Incorrect model type", root.Line, root.Position));
                    return null;
            }
        }

        private ModelInterpreter()
        {
            InitElements();
            InitFunctionDictionary();
        }
        static public ModelInterpreter GetInstanse()
        {
            if (instance == null)
                instance = new ModelInterpreter();
            return instance;
        }
    }
#endif
}
