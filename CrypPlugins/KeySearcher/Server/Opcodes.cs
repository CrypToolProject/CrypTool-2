using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

enum ClientOpcodes
{
    HELLO = 0,
    JOB_RESULT = 1,
    JOB_REQUEST = 2
}

enum ServerOpcodes
{
    NEW_JOB = 0,
    WRONG_PASSWORD = 1,
}

