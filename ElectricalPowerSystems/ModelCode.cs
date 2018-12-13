using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricalPowerSystems
{
    enum VariableType
    {
        Element,
        String,
        Float
    }
    class ModelVariableType
    {

    }
    class ElementType : ModelVariableType
    {
        Element element;
    }
    class StringType : ModelVariableType
    {
        string value;
    }
    class FloatType : ModelVariableType
    {
        float value;
    }
    class ModelCode
    {
        Dictionary<string, ModelVariableType> variableTable;
        /*public void addElementVariable();
        public void addFloatVariable();
        public void addStringVariable();
        */

    }
}
