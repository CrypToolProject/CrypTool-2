#include <sys/types.h>
#include <arpa/inet.h>
#include <stdio.h>

#include "PlatformIndependentWrapper.h"

std::string PlatformIndependentWrapper::ReadString()
{
    int32_t strlen = ReadInt();
    char str[strlen];
    ReadArray(str, strlen);
    std::string ret = std::string(str, strlen);
    return ret;
}

void PlatformIndependentWrapper::WriteString(std::string str)
{
    int strlen = str.length();
    WriteInt(strlen);
    WriteArray(str.c_str(), strlen);
}

int32_t PlatformIndependentWrapper::ReadInt()
{
    char buf[4];
    ReadArray(buf, 4);
    return ntohl(*((int*)buf));
}

void PlatformIndependentWrapper::WriteInt(int32_t i)
{
    int networked = htonl(i);
    WriteArray((char*)(&networked), 4);
}

void PlatformIndependentWrapper::WriteFloat(float f)
{
    WriteArray((char*)(&f), 4);
}

void PlatformIndependentWrapper::ReadArray(char* buf, int num)
{
    int rec = 0;
    int offset = 0;
    do{
        rec = read(this->sockfd, buf+offset, num-offset);
        offset += rec;
    } while(rec > 0 && offset< num);

    if(rec <= 0)
    {
        throw SocketException();
    }
}

void PlatformIndependentWrapper::WriteArray(const char* buf, int num)
{
    if(write(this->sockfd, buf, num)!=num)
    {
        printf("failed to write :(\n");
        throw SocketException();
    }
}
