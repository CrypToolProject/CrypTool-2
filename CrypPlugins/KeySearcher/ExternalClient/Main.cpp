#include <ctype.h>
#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netdb.h>
#include <string.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <signal.h>

#include "Network.h"

#define RECONNECT_TIME  5

void usage(char **argv)
{
    printf("Usage: %s $HOST $PORT [$PASSWORD]\n\n", argv[0]);
    printf("    HOST       - PC running cryptool\n");
    printf("    PORT       - CrypTool's listening port\n");
    printf("    [PASSWORD] - Password used to auth with server\n");
}

bool lookup(const char* host, sockaddr_in & serv_addr)
{
    hostent *server = gethostbyname(host);

    if(!server)
        return false;

    serv_addr.sin_family = AF_INET;
    bcopy((char *)server->h_addr, 
            (char *)&serv_addr.sin_addr.s_addr,
            server->h_length);

    return true;
}

bool receivedSigInt = false;

void terminate(int parm)
{
    if(receivedSigInt)
    {
        printf("Force quitting...\n");
        exit(1);
    }
    printf("Received SIGINT, will exit after current chunk\n");
    printf("Press again to force quit.\n");
    receivedSigInt = true;
}

int main (int argc, char **argv)
{
    if(argc != 3 && argc != 4)
    {
        usage(argv);
        return 1;
    }

    int pwlen = (argc==4?strlen(argv[3]):0) +1;
    char password[pwlen];
    bzero(password, pwlen);
    if(argc == 4)
    {
        strcpy(password, argv[3]);
        bzero(argv[3], pwlen);
    }

    int port = atoi(argv[2]);

    if(port == 0 || port <= 0 || port > 0xFFFF)
    {
        printf("Invalid port.\n");
        return 1;
    }

    // CTRL+C
    signal (SIGINT, terminate);

    sockaddr_in serv_addr;
    bzero((char *) &serv_addr, sizeof(serv_addr));
    serv_addr.sin_port = htons(port);

    CrypTool cryptool;

    while(!receivedSigInt)
    {
        if(!lookup(argv[1], serv_addr))
        {
            printf("failed to lookup %s..\n", argv[1]);
            continue;
        }
        networkThread(serv_addr, port, password, &cryptool, &receivedSigInt);
        printf("Reconnecting in %u seconds\n", RECONNECT_TIME);
        sleep(RECONNECT_TIME);
    }

}
