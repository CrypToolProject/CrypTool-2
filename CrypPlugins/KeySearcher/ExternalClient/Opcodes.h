#pragma once

namespace ClientOpcodes
{
    enum 
    {
        HELLO = 0,
        JOB_RESULT = 1,
        JOB_REQUEST = 2,
    };
};

namespace ServerOpcodes
{
    enum 
    {
        NEW_JOB = 0,
        WRONG_PASSWORD = 1,
    };
};

