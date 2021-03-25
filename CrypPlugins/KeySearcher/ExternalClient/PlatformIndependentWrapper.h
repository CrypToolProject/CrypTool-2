#pragma once

#include <string>

class SocketException
{
};

class PlatformIndependentWrapper
{
    private:
        int sockfd;

    public:
        PlatformIndependentWrapper(int sockfd):
            sockfd(sockfd)
        {
        }
        std::string ReadString();
        void WriteString(std::string);

        int32_t ReadInt();
        void WriteInt(int32_t);
        void WriteFloat(float);

        void ReadArray(char* buf, int len);
        void WriteArray(const char* buf, int num);
};
