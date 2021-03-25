using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

public abstract class JobBase
{
    public String Guid {get;set;}
}

public class JobInput : JobBase
{
    public String Src {get;set;}
    public byte[] Key {get;set;}
    public bool LargerThen;
    public int Size {get;set;}
    public int ResultSize {get;set;}
}

public class JobResult : JobBase
{
    public List<KeyValuePair<float, int>> ResultList {get;set;}
}
