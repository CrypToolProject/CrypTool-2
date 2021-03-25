using System.Linq;
using System.Reflection;
using System.Threading;
using WorkspaceManager.Execution;
using WorkspaceManager.Model;

namespace Tests.TemplateAndPluginTests
{
    internal class TemplateTestScenario : TestScenario
    {
        private readonly WorkspaceModel _templateModel;

        public TemplateTestScenario(WorkspaceModel templateModel, string[] inputProperties, string[] outputProperties)
            : base(GetProperties(templateModel, inputProperties), GetObjectArray(templateModel, inputProperties),
                   GetProperties(templateModel, outputProperties), GetObjectArray(templateModel, outputProperties))
        {
            _templateModel = templateModel;
        }

        private static object[] GetObjectArray(WorkspaceModel model, string[] properties)
        {
            var res = new object[properties.Length];

            for (int i = 0; i < properties.Length; i++)
            {
                var el = properties[i].Split('>');
                var plugin = model.GetAllPluginModels().First(x => x.GetName() == el[0]).Plugin;
                
                if (el[1].StartsWith("."))
                {
                    res[i] = plugin.Settings;
                }
                else
                {
                    res[i] = plugin;
                }
            }
            return res;
        }

        private static PropertyInfo[] GetProperties(WorkspaceModel model, string[] properties)
        {
            var objects = GetObjectArray(model, properties);
            var res = new PropertyInfo[properties.Length];

            for (int i = 0; i < properties.Length; i++)
            {
                var prop = properties[i].Split('>')[1];
                if (prop.StartsWith("."))
                {
                    prop = prop.Substring(1);
                }
                res[i] = objects[i].GetType().GetProperty(prop);
            }
            return res;
        }

        protected override void Initialize()
        {
        }

        protected override void PreExecution()
        {
        }

        protected override void Execute()
        {
            var ee = new ExecutionEngine();
            ee.Execute(_templateModel, false);
            Thread.Sleep(10000);
        }
    }
}