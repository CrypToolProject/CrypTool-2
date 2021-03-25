using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using WorkspaceManager.Model;

namespace WorkspaceManagerModel.Model.Tools
{
    public static class PartialCopyHelper
    {
        static public object CurrentSelection;

        public static bool Copy(List<PluginModel> copyComponents, WorkspaceModel model)
        {
            if (copyComponents == null) throw new ArgumentNullException("copyComponents");
            if (model == null) throw new ArgumentNullException("model");

            foreach (var copy in model.AllPluginModels)
            {
                copy.CopyID = string.Empty;
            }
            foreach (var copy in copyComponents)
            {
                copy.CopyID = Guid.NewGuid().ToString();
            }
            using (var cloneWS = Open(Save(model)))
            {
                try
                {
                    foreach (var comp in cloneWS.GetAllPluginModels())
                    {
                        if (comp.CopyID == string.Empty)
                        {
                            cloneWS.deletePluginModel(comp);
                        }
                    }

                    cloneWS.UndoRedoManager.ClearStacks();
                    CurrentSelection = cloneWS;
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }

        }

        private static StreamWriter Save(WorkspaceModel model)
        {
            var persistance = new ModelPersistance();
            var pmod= persistance.GetPersistantModel(model);

            MemoryStream ms = new MemoryStream();
            StreamWriter writer = null;
            try
            {
                writer = new StreamWriter(ms, Encoding.UTF8);
                XMLSerialization.XMLSerialization.Serialize(pmod, writer);
                return writer;
            }
            catch(Exception e)
            {
                throw new NullReferenceException();
            }
        }

        private static WorkspaceModel Open(StreamWriter writer)
        {
            try
            {
                var persistance = new ModelPersistance();
                var wp = persistance.loadModel(writer);
                return wp;
            }
            catch (Exception)
            {
                throw new NullReferenceException();
            }
            finally
            {
                if(writer !=null)
                    writer.Close();
            }
        }
    }
}
