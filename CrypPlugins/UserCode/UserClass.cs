using System.Numerics;
using System;

namespace CrypTool.Plugins.UserCode
{

    public class UserClass
    {
        public UserClass()
        {
            Input1 = AppDomain.CurrentDomain.GetData("input1");
            Input2 = AppDomain.CurrentDomain.GetData("input2");
            Input3 = AppDomain.CurrentDomain.GetData("input3");
            Input4 = AppDomain.CurrentDomain.GetData("input4");
            Input5 = AppDomain.CurrentDomain.GetData("input5");

            Execute();

            AppDomain.CurrentDomain.SetData("output1", Output1);
            AppDomain.CurrentDomain.SetData("output2", Output2);
            AppDomain.CurrentDomain.SetData("output3", Output3);
            AppDomain.CurrentDomain.SetData("output4", Output4);
            AppDomain.CurrentDomain.SetData("output5", Output5);
        }

        //USERCODE//

        public object Input1
        {
            get; 
            set;
        }

        public object Input2
        {
            get;
            set;
        }

        public object Input3
        {
            get;
            set;
        }

        public object Input4
        {
            get;
            set;
        }

        public object Input5
        {
            get;
            set;
        }

        public object Output1
        {
            get;
            set;
        }

        public object Output2
        {
            get;
            set;
        }

        public object Output3
        {
            get;
            set;
        }

        public object Output4
        {
            get;
            set;
        }

        public object Output5
        {
            get;
            set;
        }
    }
}