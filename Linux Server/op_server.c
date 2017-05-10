#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <arpa/inet.h>
#include <sys/socket.h>
#include <netinet/in.h>
#define BUF_SIZE 1024
#define OP_SIZE 4
#define STACK_SIZE 100
#define EMPTY (-1)
int MAXSIZE = STACK_SIZE;
int opnd_stack[STACK_SIZE];
char operator_stack[STACK_SIZE];

int int_top = EMPTY;
int char_top = EMPTY;

void char_push(char operator)
{
	if(!char_isFull())
		operator_stack[++char_top] = operator;
	else
		printf("Stack is Full\n");
}
void int_push(int data)
{
	if(!int_isFull())
		opnd_stack[++int_top] = data;
	else
		printf("Stack is Full\n");
}
char char_pop()
{
	if(!char_isEmpty())
		return operator_stack[char_top--];
	else 
		printf("Stack is empty");
}
int int_pop()
{
	if(!int_isEmpty())
		return opnd_stack[int_top--];
	else
		printf("Stack is empty");
}
int char_isEmpty()
{
	return char_top == EMPTY;
}
int int_isEmpty()
{
	return int_top == EMPTY;
}
int char_isFull()
{
	return char_top == STACK_SIZE - 1;
}
int int_isFull()
{
	return int_top == STACK_SIZE - 1;
}
char char_peek()
{
	return operator_stack[char_top];
}
int int_peek()
{
	return opnd_stack[int_top];
}
void error_handling(char* message)
{
	fputs(message, stderr);
	fputc('\n', stderr);
	exit(1);
}
int calculate(int operator_count, int opnds[], char oprator[])
{
	int opnd_cnt = operator_count;
	int oprator_cnt = operator_count - 1;
	int result;
	int opnd_len = OP_SIZE * opnd_cnt;
	if(operator_count == 0)
		return 0;
	int_push(opnds[0]);//첫번째꺼 입력받음.
	for(int i = 1; i < opnd_cnt - 1; i++)
	{
		int_push(opnds[i]);
		char_push(oprator[opnd_len + i - 1]);
		if((char_peek() == '*') || (char_peek() == '/'))
		{
			int after = int_pop();
			int before = int_pop();
			char op = char_pop();
			switch(op)
			{
				case '*':
					before = before * after;
					int_push((int)before);
					break;
				case '/':
					before = before / after;
					int_push((int)before);
					break;
			}
		}
	}
	while(char_isEmpty())
	{
		int after = int_pop();
		int before = int_pop();
		char op = char_pop();
		switch(op)
		{
			case '+':
				before = before + after;
				int_push((int)before);
				break;
			case '-':
				before = before - after;
				int_push((int)before);
				break;
		}
	}
	result = int_pop();
	return result;
}
int main(int argc, char* argv[])
{
	int serv_sock, clnt_sock;
	char opinfo[BUF_SIZE];
	int result, opnd_cnt, i;
	int recv_cnt, recv_len;
	struct sockaddr_in serv_adr, clnt_adr;
	int clnt_adr_sz;

	if(argc != 2)
	{
		printf("Usage : %s <port>\n", argv[0]);
		exit(1);
	}
	serv_sock = socket(PF_INET, SOCK_STREAM, 0);
	if(serv_sock == -1)
		error_handling("socket error");

	memset(&serv_adr, 0, sizeof(serv_adr));
	serv_adr.sin_family = AF_INET;
	serv_adr.sin_addr.s_addr = htonl(INADDR_ANY);
	serv_adr.sin_port=htons(atoi(argv[1]));

	if(bind(serv_sock, (struct sockaddr*)&serv_adr, sizeof(serv_adr)) == -1)
		error_handling("bind error");
	else
		printf("bind okay\n");

	if(listen(serv_sock, 5) == -1)
		error_handling("listen error");
	else
		printf("listen okay\n");
	for(i = 0; i < 5; i++)
	{
		opnd_cnt = 0;
		clnt_adr_sz = sizeof(clnt_adr);
		printf("DEBUG \n");
		if((clnt_sock = accept(serv_sock, (struct sockaddr*)&clnt_adr, &clnt_adr_sz)) == -1)
			error_handling("accept error");
		read(clnt_sock, &opnd_cnt, 1);//갯수 받음
		recv_len = 0;
		while((1 + opnd_cnt * OP_SIZE + opnd_cnt - 1) > recv_len)
		{
			//1 + 4자리*피연산자갯수 + 연산자갯수(n-1)
			recv_cnt = read(clnt_sock, &opinfo[recv_len], BUF_SIZE - 1);
			recv_len += recv_cnt;
		}
		result = calculate(opnd_cnt, (int*)opinfo, opinfo);
		write(clnt_sock, (char*)&result, sizeof(result));
		close(clnt_sock);
	}
	close(serv_sock);
	return 0;
}

