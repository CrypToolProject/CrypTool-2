/*
   Copyright (C) CrypTool 2 Team

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using System;
using System.Reflection;

namespace UnitTests
{
    internal abstract class TestScenario
    {
        private readonly PropertyInfo[] _inputProperties;
        private readonly object[] _inputObjects;
        private readonly PropertyInfo[] _outputProperties;
        private readonly object[] _outputObjects;

        protected TestScenario(PropertyInfo[] inputProperties, object[] inputObjects, PropertyInfo[] outputProperties, object[] outputObjects)
        {
            _inputProperties = inputProperties;
            _inputObjects = inputObjects;
            _outputProperties = outputProperties;
            _outputObjects = outputObjects;
        }

        public bool Test(object[] inputValues, object[] expectedOutputValues)
        {
            if (expectedOutputValues.Length != _outputProperties.Length)
            {
                throw new ArgumentException("output vector doesn't match scenario.");
            }

            object[] outputValues = GetOutputs(inputValues);

            for (int i = 0; i < _outputProperties.Length; i++)
            {
                if (!outputValues[i].Equals(expectedOutputValues[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public object[] GetOutputs(object[] inputValues, bool preexecfirst = true)
        {
            Initialize();
            if (preexecfirst)
            {
                PreExecution();
            }

            if (inputValues.Length != _inputProperties.Length)
            {
                throw new ArgumentException("input vector doesn't match scenario.");
            }

            for (int i = 0; i < _inputProperties.Length; i++)
            {
                _inputProperties[i].SetValue(_inputObjects[i], inputValues[i], null);
            }

            if (!preexecfirst)
            {
                PreExecution();
            }

            Execute();

            object[] outputValues = new object[_outputProperties.Length];

            for (int i = 0; i < _outputProperties.Length; i++)
            {
                outputValues[i] = _outputProperties[i].GetValue(_outputObjects[i], null);
            }

            return outputValues;
        }

        protected abstract void Initialize();
        protected abstract void PreExecution();
        protected abstract void Execute();
    }
}