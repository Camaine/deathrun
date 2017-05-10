#include<stdio.h>
#include<stdlib.h>
#include<string.h>
#include<unistd.h>
#include<arpa/inet.h>
#include<sys/socket.h>
#include<netinet/in.h>

#define BUF_SIZE 1024

int main(int argc, char *argv[]){
	int serv_sock, clnt_sock, clnt_adr_sz, nbyte; // server socket, client socket, client address size, receiv from client
	struct sockaddr_in clnt_adr; // socket address info of client
	struct sockaddr_in serv_adr; // socket address info of server
	int mystat; // get status of game

	if((serv_sock = socket(PF_INET, SOCK_DGRAM, 0)) == -1){ // create server socket
		fprintf(stderr, "server socket initiate failed\n");
		exit(1);
	}

	memset(&serv_adr,0,sizeof(serv_adr)); // initialize sever address info
	serv_adr.sin_family = AF_INET; // set as IPv4
	serv_adr.sin_port = htons(6030); // set port as 6030
	serv_adr.sin_addr.s_addr = htonl(INADDR_ANY); // set address itself
	
	if(bind(serv_sock, (struct sockaddr*)&serv_adr, sizeof(serv_adr)) == -1){ // binding socket
		fprintf(stderr, "bind() run error\n");
		exit(1);
	}
	while(1){
		puts("Server : waiting request.");
		clnt_adr_sz = sizeof(clnt_adr); // setting size of client
		nbyte = recvfrom(serv_sock, &mystat, sizeof(mystat), 0, (struct sockaddr*)&clnt_adr, &clnt_adr_sz); // receive from game client
		if(nbyte < 0){ // if receiv failed, close server
			printf("recvfrom failed\n");
			exit(1);
		}
		printf("read : %d\n",ntohl(mystat)); // check info of status that receive from server
		
		if(sendto(serv_sock, &mystat, sizeof(mystat), 0, (struct sockaddr*)&clnt_adr, clnt_adr_sz) < 0){ // send to client
			printf("sendto failed\n");
			exit(1);
		}
	}
	close(serv_sock);
	exit(0);
	return 0;
}
