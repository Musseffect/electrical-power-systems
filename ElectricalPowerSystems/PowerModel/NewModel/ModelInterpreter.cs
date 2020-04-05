
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems.PowerModel.NewModel
{
    public partial class ModelInterpreter
    {
        static ModelInterpreter instance;
        public class ValueException : Exception
        {
        }
        public class MissingValueException : Exception
        {
            string key;
            public MissingValueException(string key) : base()
            {
                this.key = key;
            }
            public string Key { get { return key; } }
        }
        public class TypeConversionError : Exception
        {
            string src;
            string dst;
            public TypeConversionError(string src, string dst):base()
            {
                this.src = src;
                this.dst = dst;
            }
            public string Src { get { return src; } }
            public string Dst { get { return dst; } }
        }
        public struct Connection
        {
            public Pin pin1;
            public Pin pin2;
            public Connection(Pin pin1,Pin pin2)
            {
                this.pin1 = pin1;
                this.pin2 = pin2;
            }
            public ISteadyStateElement CreateSteadyStateElement()
            {
                if (pin1 is Pin1Phase)
                    return new Elements.Connection1P(pin1 as Pin1Phase, pin2 as Pin1Phase);
                else
                    return new Elements.Connection3P(pin1 as Pin3Phase, pin2 as Pin3Phase);
            }
            public ITransientElement CreateTransientElement()
            {
                if (pin1 is Pin1Phase)
                    return new Elements.Connection1P(pin1 as Pin1Phase, pin2 as Pin1Phase);
                else
                    return new Elements.Connection3P(pin1 as Pin3Phase, pin2 as Pin3Phase);
            }
        }
        public class ElementDescription
        {
            public enum NodeType
            {
                OnePhase,
                ThreePhase
            }
            readonly Dictionary<string, NodeType> nodes;
            readonly Dictionary<string, IType> parameterTypes;
            ITransientElementModel transientModel;
            ISteadyStateElementModel steadyStateModel;
            public ElementDescription(Dictionary<string, NodeType> nodes, Dictionary<string, IType> parameterTypes,
                ITransientElementModel transientModel, ISteadyStateElementModel steadyStateModel)
            {
                this.nodes = nodes;
                this.parameterTypes = parameterTypes;
                this.transientModel = transientModel;
                this.steadyStateModel = steadyStateModel;
            }
            public Pin CreatePin(string name,int index)
            {
                var type = GetNodeType(name);
                switch (type)
                {
                    case ElementDescription.NodeType.OnePhase:
                        return new Pin1Phase(index);
                    case ElementDescription.NodeType.ThreePhase:
                        return new Pin3Phase(index);
                }
                throw new Exception("Unkown type");
            }
            public ITransientElement CreateTransientElement(Object elementObject,
                Dictionary<string, Pin> elementNodes)
            {
                return transientModel?.CreateElement(elementObject, elementNodes);
            }
            public ISteadyStateElement CreateSteadyStateElement(Object elementObject,
                Dictionary<string, Pin> elementNodes)
            {
                return steadyStateModel?.CreateElement(elementObject, elementNodes);
            }
            public bool ContainsNode(string name)
            {
                return nodes.ContainsKey(name);
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
            //Done
            public bool Validate(ref Object elementObject, ref List<ErrorMessage> errors)
            {
                bool result = true;
                foreach (var parameter in parameterTypes)
                {
                    if (elementObject.ContainsKey(parameter.Key))
                    {
                        try
                        {
                            elementObject.SetValue(parameter.Key, parameter.Value.Validate(elementObject.GetValue(parameter.Key)));
                        }
                        catch (TypeConversionError exc)
                        {
                            result = false;
                            errors.Add(new ErrorMessage($"Cannot convert {exc.Src} to {exc.Dst} for parameter {parameter.Key} in element {elementObject.Name}"));
                        }
                        catch (Exception exc)
                        {
                            result = false;
                            errors.Add(new ErrorMessage(exc.Message));
                        }
                    }
                    else {
                        result = false;
                        errors.Add(new ErrorMessage($"Missing parameter {parameter.Key} in element {elementObject.Name}"));
                    }
                }
                return result;
            }
        }
        Dictionary<string, ElementDescription> elementsMap;
        Dictionary<string, Constant> variableTable;

        class ElementEntry
        {
            ElementDescription description;
            Object obj;
            Dictionary<string, Pin> elementPins;
            public ElementEntry(ElementDescription description, Object obj, Dictionary<string, Pin> elementPins)
            {
                this.description = description;
                this.obj = obj;
                this.elementPins = elementPins;
            }
            public ElementDescription GetDescription()
            {
                return description;
            }
            public Pin GetPin(string name)
            {
                return elementPins[name];
            }
        }
        Dictionary<string, Constant> GetParameters(List<KeyValueNode> parameters)
        {
            Dictionary<string, Constant> result = new Dictionary<string, Constant>() ;
            foreach (var parameter in parameters)
            {
                result.Add(parameter.Key,Eval(parameter.Value).GetRValue());
            }
            return result;
        }
        private TransientModel GetTransientModel(List<ElementNode> elements, List<ConnectionNode> rootConnections, Object modelParameters, ref List<ErrorMessage> errorList, ref List<string> output)
        {
            Dictionary<string, ElementEntry> elementEntries = new Dictionary<string, ElementEntry>();
            int nodeIndicies = 0;
            TransientModel model = new TransientModel();
            foreach (var element in elements)
            {
                //get Object from element.Definition
                Object obj = BuildObject(element.Definition);

                //get Element description( nodes and parameters)
                ElementDescription description = elementsMap[obj.Name];

                Dictionary<string, Pin> elementPins = new Dictionary<string, Pin>();
                var keys = description.GetNodes();
                for (int i = 0; i < keys.Count; i++)
                {
                    string key = keys.ElementAt(i);
                    Pin pin = description.CreatePin(key, nodeIndicies);
                    elementPins.Add(key, pin);
                    model.AddPin(pin);
                    nodeIndicies++;
                }
                //create element entry
                elementEntries.Add(element.Id, new ElementEntry(description, obj, elementPins));
                bool valid = description.Validate(ref obj, ref errors);
                if (valid)
                {
                    try
                    {
                        if (description.Validate(ref obj, ref errors))
                        {
                            ITransientElement modelElement = description.CreateTransientElement(obj, elementPins);
                            if (modelElement is null)
                            {
                                errors.Add(new ErrorMessage($"Element {obj.Name} cannot be used in steady state model", element.Line, element.Position));
                            }
                            else
                            {
                                model.AddElement(modelElement);
                            }
                        }
                    }
                    catch (MissingValueException exc)
                    {
                        errors.Add(new ErrorMessage($"Missing parameter {exc.Key} in element {obj.Name}", element.Line, element.Position));
                    }
                    catch (Exception)
                    {
                        errors.Add(new ErrorMessage($"Error during element {obj.Name} creation", element.Line, element.Position));
                    }
                }
            }
            //resolve connections between elements
            List<Connection> connections = new List<Connection>();
            foreach (var connection in rootConnections)
            {
                try
                {
                    if (!elementEntries.ContainsKey(connection.Element1))
                    {
                        errors.Add(new ErrorMessage($"Non existing element {connection.Element1} in connection", connection.Line, connection.Position));
                        continue;
                    }
                    if (!elementEntries.ContainsKey(connection.Element2))
                    {
                        errors.Add(new ErrorMessage($"Non existing element {connection.Element2} in connection", connection.Line, connection.Position));
                        continue;
                    }
                    ElementEntry entry1 = elementEntries[connection.Element1];
                    ElementEntry entry2 = elementEntries[connection.Element2];
                    ElementDescription element1 = entry1.GetDescription();
                    ElementDescription element2 = entry2.GetDescription();
                    if (!element1.ContainsNode(connection.Node1))
                    {
                        errors.Add(new ErrorMessage($"Non existing pin {connection.Node1} in element {connection.Element1}", connection.Line, connection.Position));
                        continue;
                    }
                    if (!element2.ContainsNode(connection.Node2))
                    {
                        errors.Add(new ErrorMessage($"Non existing element {connection.Node2} in element {connection.Element2}", connection.Line, connection.Position));
                        continue;
                    }
                    ElementDescription.NodeType node1 = element1.GetNodeType(connection.Node1);
                    ElementDescription.NodeType node2 = element2.GetNodeType(connection.Node2);
                    if (node1 != node2)
                    {
                        errors.Add(new ErrorMessage($"Pins in connection ({connection.Element1}.{connection.Node1},{connection.Element2}.{connection.Node2}) have different types", connection.Line, connection.Position));
                        continue;
                    }
                    connections.Add(new Connection(entry1.GetPin(connection.Node1), entry2.GetPin(connection.Node2)));
                }
                catch (Exception exc)
                {
                    throw exc;
                }
            }
            foreach (Connection connection in connections)
            {
                model.AddElement(connection.CreateTransientElement());
            }
            try
            {
                Object solver = (Object)Convert(modelParameters.GetValue("solver"), Constant.Type.Object);
                switch (solver.Name)
                {
                    case "radauIIA5":
                        FloatValue fAbsTol = Convert(solver.GetValue("fAbsTol"), Constant.Type.Float) as FloatValue;
                        IntValue iterations = Convert(solver.GetValue("iterations"), Constant.Type.Int) as IntValue;
                        FloatValue alpha = Convert(solver.GetValue("alpha"), Constant.Type.Float) as FloatValue;
                        FloatValue step = Convert(solver.GetValue("step"), Constant.Type.Float) as FloatValue;
                        //model.SetSolver(new Radau(fAbsTol.Value, iterations.Value, alpha.Value));
                        break;
                    default:
                        errors.Add(new ErrorMessage("Unknown solver in transient model"));
                        return null;
                }
                FloatValue t0 = Convert(modelParameters.GetValue("t0"), Constant.Type.Float) as FloatValue;
                FloatValue t1 = Convert(modelParameters.GetValue("t1"), Constant.Type.Float) as FloatValue;
                model.SetT0(t0.Value);
                model.SetT1(t1.Value);
            }
            catch (MissingValueException exc)
            {
                errors.Add(new ErrorMessage($"Отсутствует аргумент {exc.Key} в определении модели."));
                return null;
            }
            catch (Exception exc)
            {
                errors.Add(new ErrorMessage($"Exception: {exc.Message}"));
                return null;
            }
            return model;
        }
        private SteadyStateModel GetSteadyStateModel(List<ElementNode> elements,List<ConnectionNode> rootConnections, Object modelParameters, ref List<ErrorMessage> errorList, ref List<string> output)
        {
            Dictionary<string, ElementEntry> elementEntries = new Dictionary<string, ElementEntry>();
            int nodeIndicies = 0;
            SteadyStateModel model = new SteadyStateModel();
            foreach (var element in elements)
            {
                //get Object from element.Definition
                Object obj = BuildObject(element.Definition);

                //get Element description( nodes and parameters)
                if (!elementsMap.ContainsKey(obj.Name))
                {
                    errors.Add(new ErrorMessage($"Не существует элемента {obj.Name}",element.Line,element.Position));
                    continue;
                }
                ElementDescription description = elementsMap[obj.Name];
                
                Dictionary<string, Pin> elementPins = new Dictionary<string, Pin>();
                var keys = description.GetNodes();
                for (int i = 0; i < keys.Count; i++)
                {
                    string key = keys.ElementAt(i);
                    Pin pin = description.CreatePin(key, nodeIndicies);
                    elementPins.Add(key, pin);
                    model.AddPin(pin);
                    nodeIndicies++;
                }
                //create element entry
                elementEntries.Add(element.Id, new ElementEntry(description, obj, elementPins));
                bool valid = description.Validate(ref obj, ref errors);
                if (valid)
                {
                    try
                    {
                        if (description.Validate(ref obj,ref errors))
                        {
                            ISteadyStateElement modelElement = description.CreateSteadyStateElement(obj, elementPins);
                            if (modelElement is null)
                            {
                                errors.Add(new ErrorMessage($"Element {obj.Name} cannot be used in steady state model", element.Line, element.Position));
                            }
                            else
                            {
                                model.AddElement(modelElement);
                            }
                        }
                    }
                    catch (MissingValueException exc)
                    {
                        errors.Add(new ErrorMessage($"Missing parameter {exc.Key} in element {obj.Name}", element.Line, element.Position));
                    }
                    catch (Exception exc)
                    {
                        errors.Add(new ErrorMessage($"Error during element {obj.Name} creation: {exc.Message}", element.Line, element.Position));
                    }
                }
            }
            //resolve connections between elements
            List<Connection> connections = new List<Connection>();
            foreach (var connection in rootConnections)
            {
                try
                {
                    if (!elementEntries.ContainsKey(connection.Element1))
                    {
                        errors.Add(new ErrorMessage($"Non existing element {connection.Element1} in connection",connection.Line,connection.Position));
                        continue;
                    }
                    if (!elementEntries.ContainsKey(connection.Element2))
                    {
                        errors.Add(new ErrorMessage($"Non existing element {connection.Element2} in connection", connection.Line, connection.Position));
                        continue;
                    }
                    ElementEntry entry1 = elementEntries[connection.Element1];
                    ElementEntry entry2 = elementEntries[connection.Element2];
                    ElementDescription element1 = entry1.GetDescription();
                    ElementDescription element2 = entry2.GetDescription();
                    if (!element1.ContainsNode(connection.Node1))
                    {
                        errors.Add(new ErrorMessage($"Non existing pin {connection.Node1} in element {connection.Element1}", connection.Line, connection.Position));
                        continue;
                    }
                    if (!element2.ContainsNode(connection.Node2))
                    {
                        errors.Add(new ErrorMessage($"Non existing element {connection.Node2} in element {connection.Element2}", connection.Line, connection.Position));
                        continue;
                    }
                    ElementDescription.NodeType node1 = element1.GetNodeType(connection.Node1);
                    ElementDescription.NodeType node2 = element2.GetNodeType(connection.Node2);
                    if (node1 != node2)
                    {
                        errors.Add(new ErrorMessage($"Pins in connection ({connection.Element1}.{connection.Node1},{connection.Element2}.{connection.Node2}) have different types", connection.Line, connection.Position));
                        continue;
                    }
                    connections.Add(new Connection(entry1.GetPin(connection.Node1), entry2.GetPin(connection.Node2)));
                }
                catch (Exception exc)
                {
                    throw exc;
                }
            }
            foreach (Connection connection in connections)
            {
                model.AddElement(connection.CreateSteadyStateElement());
            }
            try
            {
                Object solver = (Object)Convert(modelParameters.GetValue("solver"), Constant.Type.Object);
                switch (solver.Name)
                {
                    case "newton":
                        FloatValue fAbsTol = Convert(solver.GetValue("fAbsTol"), Constant.Type.Float) as FloatValue;
                        IntValue iterations = Convert(solver.GetValue("iterations"), Constant.Type.Int) as IntValue;
                        FloatValue alpha = Convert(solver.GetValue("alpha"), Constant.Type.Float) as FloatValue;
                        model.SetSolver(new SteadyStateNewtonSolver(fAbsTol.Value, iterations.Value, alpha.Value));
                        break;
                    default:
                        errors.Add(new ErrorMessage("Unknown solver in steadystate model"));
                        return null;
                }
                FloatValue baseFrequency = (FloatValue)Convert(modelParameters.GetValue("baseFrequency"), Constant.Type.Float);
                model.SetBaseFrequency(baseFrequency.Value);
            }
            catch (MissingValueException exc)
            {
                errors.Add(new ErrorMessage($"Отсутствует аргумент {exc.Key} в определении модели."));
                return null;
            }
            catch (Exception exc)
            {
                errors.Add(new ErrorMessage($"Exception: {exc.Message}"));
                return null;
            }
            return model;
        }
        List<ErrorMessage> errors;
        private void InitElements()
        {
            variableTable = new Dictionary<string, Constant>();
            variableTable.Add("Dd0",new IntValue(0));
            variableTable.Add("Dd2", new IntValue(1));
            variableTable.Add("Dd4", new IntValue(2));
            variableTable.Add("Dd6", new IntValue(3));
            variableTable.Add("Dd8", new IntValue(4));
            variableTable.Add("Dd10", new IntValue(5));

            variableTable.Add("Yy0", new IntValue(0));
            variableTable.Add("Yy2", new IntValue(1));
            variableTable.Add("Yy4", new IntValue(2));
            variableTable.Add("Yy6", new IntValue(3));
            variableTable.Add("Yy8", new IntValue(4));
            variableTable.Add("Yy10", new IntValue(5));

            variableTable.Add("Yd1", new IntValue(0));
            variableTable.Add("Yd3", new IntValue(1));
            variableTable.Add("Yd5", new IntValue(2));
            variableTable.Add("Yd7", new IntValue(3));
            variableTable.Add("Yd9", new IntValue(4));
            variableTable.Add("Yd11", new IntValue(5));

            variableTable.Add("Dy1", new IntValue(0));
            variableTable.Add("Dy3", new IntValue(1));
            variableTable.Add("Dy5", new IntValue(2));
            variableTable.Add("Dy7", new IntValue(3));
            variableTable.Add("Dy9", new IntValue(4));
            variableTable.Add("Dy11", new IntValue(5));

            variableTable.Add("PI",new FloatValue(Math.PI));

            elementsMap = new Dictionary<string, ElementDescription>();
            elementsMap.Add("Scope1p", new ElementDescription(
                new Dictionary<string, ElementDescription.NodeType>
                {
                    { "in", ElementDescription.NodeType.OnePhase},
                    { "out", ElementDescription.NodeType.OnePhase}
                },
                new Dictionary<string, IType>()
                {
                    { "Label",new StringType() }
                }, null, new Elements.SteadyStateScope1PModel())
                );
            elementsMap.Add("Resistor", new ElementDescription(
                new Dictionary<string, ElementDescription.NodeType>{
                    { "in", ElementDescription.NodeType.OnePhase},
                    { "out", ElementDescription.NodeType.OnePhase}
                },
                new Dictionary<string, IType>{
                        { "R",new FloatType()}
                }, null, new Elements.SteadyStateResistorModel())
            );
            elementsMap.Add("Ground", new ElementDescription(
                new Dictionary<string, ElementDescription.NodeType>{
                    { "in", ElementDescription.NodeType.OnePhase}
                },
                new Dictionary<string, IType>(), null, new Elements.SteadyStateGroundModel())
            );
            elementsMap.Add("Capacitor", new ElementDescription(
                new Dictionary<string, ElementDescription.NodeType>{
                    { "in", ElementDescription.NodeType.OnePhase },
                    { "out", ElementDescription.NodeType.OnePhase}
                },
                new Dictionary<string, IType>{
                        { "C", new FloatType()}
                }, null, new Elements.SteadyStateCapacitorModel())
            );
            elementsMap.Add("Inductor", new ElementDescription(
                new Dictionary<string, ElementDescription.NodeType>{
                    { "in", ElementDescription.NodeType.OnePhase },
                    { "out", ElementDescription.NodeType.OnePhase}
                },
                new Dictionary<string, IType>{
                        { "L", new FloatType()}
                }, null, new Elements.SteadyStateInductorModel())
            );
            elementsMap.Add("Transformer", new ElementDescription(
                new Dictionary<string, ElementDescription.NodeType>
                {
                    { "in_p", ElementDescription.NodeType.OnePhase},
                    { "out_p", ElementDescription.NodeType.OnePhase},
                    { "in_s", ElementDescription.NodeType.OnePhase},
                    { "out_s", ElementDescription.NodeType.OnePhase}
                },
                new Dictionary<string, IType>()
                {
                    { "Zp",new ComplexType() },
                    { "Zs",new ComplexType() },
                    { "Xm",new FloatType() },
                    { "Rc",new FloatType() },
                    { "K",new FloatType() },
                    { "Group",new IntType() }
                }, null, new Elements.SteadyStateTransformerDdModel())
            );
            elementsMap.Add("VoltageSource", new ElementDescription(
                new Dictionary<string, ElementDescription.NodeType>{
                    { "in", ElementDescription.NodeType.OnePhase },
                    { "out", ElementDescription.NodeType.OnePhase}
                },
                new Dictionary<string, IType>{
                    { "Peak", new FloatType()},
                    { "Phase", new FloatType()},
                    { "Frequency", new FloatType()}
                }, null, new Elements.SteadyStateVoltageSourceModel())
            );
            elementsMap.Add("Scope3p", new ElementDescription(
                new Dictionary<string, ElementDescription.NodeType>
                {
                    { "in", ElementDescription.NodeType.ThreePhase},
                    { "out", ElementDescription.NodeType.ThreePhase}
                },
                new Dictionary<string, IType>()
                {
                    { "Label",new StringType() }
                }, null, new Elements.SteadyStateScope3PModel())
            );
            elementsMap.Add("TransformerDd", new ElementDescription(
                new Dictionary<string, ElementDescription.NodeType>
                {
                    { "in", ElementDescription.NodeType.ThreePhase},
                    { "out", ElementDescription.NodeType.ThreePhase}
                },
                new Dictionary<string, IType>()
                {
                    { "Zp",new ComplexType() },
                    { "Zs",new ComplexType() },
                    { "Xm",new FloatType() },
                    { "Rc",new FloatType() },
                    { "K",new FloatType() },
                    { "Group",new IntType() }
                }, null, new Elements.SteadyStateTransformerDdModel())
            );
            elementsMap.Add("TransformerYd", new ElementDescription(
                new Dictionary<string, ElementDescription.NodeType>
                {
                    { "in", ElementDescription.NodeType.ThreePhase},
                    { "in_n", ElementDescription.NodeType.OnePhase},
                    { "out", ElementDescription.NodeType.ThreePhase}
                },
                new Dictionary<string, IType>()
                {
                    { "Zp",new ComplexType() },
                    { "Zs",new ComplexType() },
                    { "Xm",new FloatType() },
                    { "Rc",new FloatType() },
                    { "K",new FloatType() },
                    { "Group",new IntType() }
                }, null, new Elements.SteadyStateTransformerDdModel())
            );
            elementsMap.Add("TransformerDy", new ElementDescription(
                new Dictionary<string, ElementDescription.NodeType>
                {
                    { "in", ElementDescription.NodeType.ThreePhase},
                    { "out", ElementDescription.NodeType.ThreePhase},
                    { "out_n", ElementDescription.NodeType.OnePhase}
                },
                new Dictionary<string, IType>()
                {
                    { "Zp",new ComplexType() },
                    { "Zs",new ComplexType() },
                    { "Xm",new FloatType() },
                    { "Rc",new FloatType() },
                    { "K",new FloatType() },
                    { "Group",new IntType() }
                }, null, new Elements.SteadyStateTransformerDdModel())
            );
            elementsMap.Add("TransformerYy", new ElementDescription(
                new Dictionary<string, ElementDescription.NodeType>
                {
                    { "in", ElementDescription.NodeType.ThreePhase},
                    { "in_n", ElementDescription.NodeType.OnePhase},
                    { "out", ElementDescription.NodeType.ThreePhase},
                    { "out_n", ElementDescription.NodeType.OnePhase}
                },
                new Dictionary<string, IType>()
                {
                    { "Zp",new ComplexType() },
                    { "Zs",new ComplexType() },
                    { "Xm",new FloatType() },
                    { "Rc",new FloatType() },
                    { "K",new FloatType() },
                    { "Group",new IntType() }
                }, null, new Elements.SteadyStateTransformerDdModel())
            );
            elementsMap.Add("LoadY", new ElementDescription(
                new Dictionary<string, ElementDescription.NodeType>
                {
                    { "in", ElementDescription.NodeType.ThreePhase},
                    { "n", ElementDescription.NodeType.OnePhase}
                },
                new Dictionary<string, IType>()
                {
                    { "ZA",new ComplexType() },
                    { "ZB",new ComplexType() },
                    { "ZC",new ComplexType() }
                }, null, new Elements.SteadyStateLoadYModel())
            );
            elementsMap.Add("LoadD", new ElementDescription(
                new Dictionary<string, ElementDescription.NodeType>
                {
                    { "in", ElementDescription.NodeType.ThreePhase}
                },
                new Dictionary<string, IType>()
                {
                    { "ZAB",new ComplexType() },
                    { "ZBC",new ComplexType() },
                    { "ZCA",new ComplexType() }
                }, null, new Elements.SteadyStateLoadDModel())
            );
            elementsMap.Add("GeneratorY", new ElementDescription(
                new Dictionary<string, ElementDescription.NodeType>
                {
                    { "out", ElementDescription.NodeType.ThreePhase},
                    { "n", ElementDescription.NodeType.OnePhase}
                },
                new Dictionary<string, IType>()
                {
                    { "Peak",new FloatType() },
                    { "Phase",new FloatType() },
                    { "Frequency",new FloatType() },
                    { "Z",new ComplexType() }
                }, null, new Elements.SteadyStateGeneratorYModel())
            );
            elementsMap.Add("Switch3P", new ElementDescription(
                new Dictionary<string, ElementDescription.NodeType>
                {
                    { "in", ElementDescription.NodeType.ThreePhase},
                    { "out", ElementDescription.NodeType.ThreePhase}
                },
                new Dictionary<string, IType>()
                {
                    { "State",new BoolType() }
                }, null, new Elements.SteadyStateSwitch3PModel())
            );
            elementsMap.Add("Connection3P1P", new ElementDescription(
                new Dictionary<string, ElementDescription.NodeType>
                {
                    { "abc", ElementDescription.NodeType.ThreePhase},
                    { "a", ElementDescription.NodeType.OnePhase},
                    { "b", ElementDescription.NodeType.OnePhase},
                    { "c", ElementDescription.NodeType.OnePhase}
                },
                new Dictionary<string, IType>()
                {
                }, null, new Elements.SteadyStateSwitch3PModel())
            );
            elementsMap.Add("LinePiSection", new ElementDescription(
                new Dictionary<string, ElementDescription.NodeType>
                {
                    { "in", ElementDescription.NodeType.ThreePhase},
                    { "out", ElementDescription.NodeType.ThreePhase}
                },
                new Dictionary<string, IType>()
                {
                    { "R",new FloatType() },
                    { "L",new FloatType() },
                    { "B",new FloatType() },
                    { "G",new FloatType() },
                    { "Bp",new FloatType() }
                }, null, new Elements.SteadyStateLinePiModel())
            );
        }
        //Done
        private void ResolveStatements(List<ExpressionNode> statements)
        {
            foreach (var expression in statements)
            {
                Eval(expression);
            }
        }
        //Done
        public IModel Generate(ModelNode root, ref List<ErrorMessage> errorList, ref List<string> output)
        {
            this.errors = errorList;
            ResolveStatements(root.Statements);
            Object modelParameters = BuildObject(root.ModelParameters);
            switch (modelParameters.Name)
            {
                case "steadystate":
                    return GetSteadyStateModel(root.Elements, root.Connections, modelParameters, ref errorList, ref output);
                case "transient":
                    return GetTransientModel(root.Elements, root.Connections, modelParameters, ref errorList, ref output);
                default:
                    errorList.Add(new ErrorMessage("Incorrect model type", root.Line, root.Position));
                    return null;
            }
        }
        //Done
        private ModelInterpreter()
        {
            InitElements();
            InitFunctionDictionary();
        }
        //Done
        static public ModelInterpreter GetInstanse()
        {
            if (instance == null)
                instance = new ModelInterpreter();
            return instance;
        }
    }
    public interface IModel
    {
        string Solve();
        string GetEquations();
    }
    public interface ITransientElementModel
    {
        ITransientElement CreateElement(ModelInterpreter.Object elementObject, Dictionary<string, Pin> elementNodes);
    }
    public interface ISteadyStateElementModel
    {
        ISteadyStateElement CreateElement(ModelInterpreter.Object elementObject, Dictionary<string, Pin> elementNodes);
    }
}
