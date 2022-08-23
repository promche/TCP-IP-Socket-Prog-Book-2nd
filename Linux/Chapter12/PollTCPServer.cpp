#include "../Common.h"

#define SERVERPORT 9000
#define BUFSIZE    512

// 소켓 정보 저장을 위한 구조체와 변수
struct SOCKETINFO
{
	char buf[BUFSIZE + 1];
	int recvbytes;
	int sendbytes;
};
int nTotalSockets = 0;
SOCKETINFO *SocketInfoArray[FD_SETSIZE];
struct pollfd Pollfds[FD_SETSIZE];

// 소켓 정보 관리 함수
bool AddSocketInfo(SOCKET sock);
void RemoveSocketInfo(int nIndex);

int main(int argc, char *argv[])
{
	int retval;

	// 소켓 생성
	SOCKET listen_sock = socket(AF_INET, SOCK_STREAM, 0);
	if (listen_sock == INVALID_SOCKET) err_quit("socket()");

	// bind()
	struct sockaddr_in serveraddr;
	memset(&serveraddr, 0, sizeof(serveraddr));
	serveraddr.sin_family = AF_INET;
	serveraddr.sin_addr.s_addr = htonl(INADDR_ANY);
	serveraddr.sin_port = htons(SERVERPORT);
	retval = bind(listen_sock, (struct sockaddr *)&serveraddr, sizeof(serveraddr));
	if (retval == SOCKET_ERROR) err_quit("bind()");

	// listen()
	retval = listen(listen_sock, SOMAXCONN);
	if (retval == SOCKET_ERROR) err_quit("listen()");

	// 넌블로킹 소켓으로 전환
	int flags = fcntl(listen_sock, F_GETFL);
	flags |= O_NONBLOCK;
	fcntl(listen_sock, F_SETFL, flags);

	// 소켓 이벤트 등록(1)
	Pollfds[0].fd = listen_sock;
	Pollfds[0].events = POLLIN; /* POLLRDNORM도 같음 */
	++nTotalSockets;

	// 데이터 통신에 사용할 변수
	int nready;
	SOCKET client_sock;
	struct sockaddr_in clientaddr;
	socklen_t addrlen;

	while (1) {
		// 소켓 이벤트 등록(2)
		for (int i = 1; i < nTotalSockets; i++) {
			if (SocketInfoArray[i]->recvbytes > SocketInfoArray[i]->sendbytes)
				Pollfds[i].events = POLLOUT; /* POLLWRNORM도 같음 */
			else
				Pollfds[i].events = POLLIN; /* POLLRDNORM도 같음 */
		}

		// poll()
		nready = poll(Pollfds, nTotalSockets, -1);
		if (nready == SOCKET_ERROR) err_quit("poll()");

		// 소켓 이벤트 검사(1): 클라이언트 접속 수용
		if (Pollfds[0].revents & POLLIN) {
			addrlen = sizeof(clientaddr);
			client_sock = accept(listen_sock,
				(struct sockaddr *)&clientaddr, &addrlen);
			if (client_sock == INVALID_SOCKET) {
				err_display("accept()");
				break;
			}
			else {
				// 넌블로킹 소켓으로 전환
				int flags = fcntl(client_sock, F_GETFL);
				flags |= O_NONBLOCK;
				fcntl(client_sock, F_SETFL, flags);
				// 클라이언트 정보 출력
				char addr[INET_ADDRSTRLEN];
				inet_ntop(AF_INET, &clientaddr.sin_addr, addr, sizeof(addr));
				printf("\n[TCP 서버] 클라이언트 접속: IP 주소=%s, 포트 번호=%d\n",
					addr, ntohs(clientaddr.sin_port));
				// 소켓 정보 추가: 실패 시 소켓 닫음
				if (!AddSocketInfo(client_sock))
					close(client_sock);
			}
			if (--nready <= 0)
				continue;
		}

		// 소켓 이벤트 검사(2): 데이터 통신
		for (int i = 1; i < nTotalSockets; i++) {
			SOCKETINFO *ptr = SocketInfoArray[i];
			SOCKET sock = Pollfds[i].fd;
			if (Pollfds[i].revents & POLLIN) {
				// 데이터 받기
				retval = recv(sock, ptr->buf, BUFSIZE, 0);
				if (retval == SOCKET_ERROR) {
					err_display("recv()");
					RemoveSocketInfo(i);
				}
				else if (retval == 0) {
					RemoveSocketInfo(i);
				}
				else {
					ptr->recvbytes = retval;
					// 클라이언트 정보 얻기
					addrlen = sizeof(clientaddr);
					getpeername(sock, (struct sockaddr *)&clientaddr, &addrlen);
					// 받은 데이터 출력
					ptr->buf[ptr->recvbytes] = '\0';
					char addr[INET_ADDRSTRLEN];
					inet_ntop(AF_INET, &clientaddr.sin_addr, addr, sizeof(addr));
					printf("[TCP/%s:%d] %s\n", addr,
						ntohs(clientaddr.sin_port), ptr->buf);
				}
			}
			else if (Pollfds[i].revents & POLLOUT) {
				// 데이터 보내기
				retval = send(sock, ptr->buf + ptr->sendbytes,
					ptr->recvbytes - ptr->sendbytes, 0);
				if (retval == SOCKET_ERROR) {
					err_display("send()");
					RemoveSocketInfo(i);
				}
				else {
					ptr->sendbytes += retval;
					if (ptr->recvbytes == ptr->sendbytes) {
						ptr->recvbytes = ptr->sendbytes = 0;
					}
				}
			}
		} /* end of for */
	} /* end of while (1) */

	// 소켓 닫기
	close(listen_sock);
	return 0;
}

// 소켓 정보 추가
bool AddSocketInfo(SOCKET sock)
{
	if (nTotalSockets >= FD_SETSIZE) {
		printf("[오류] 소켓 정보를 추가할 수 없습니다!\n");
		return false;
	}
	SOCKETINFO *ptr = new SOCKETINFO;
	if (ptr == NULL) {
		printf("[오류] 메모리가 부족합니다!\n");
		return false;
	}
	ptr->recvbytes = 0;
	ptr->sendbytes = 0;
	SocketInfoArray[nTotalSockets] = ptr;

	// 소켓 이벤트 등록(3)
	Pollfds[nTotalSockets].fd = sock;
	Pollfds[nTotalSockets].events = POLLIN; /* POLLRDNORM도 같음 */

	++nTotalSockets;
	return true;
}

// 소켓 정보 삭제
void RemoveSocketInfo(int nIndex)
{
	SOCKETINFO *ptr = SocketInfoArray[nIndex];
	SOCKET sock = Pollfds[nIndex].fd;

	// 클라이언트 정보 얻기
	struct sockaddr_in clientaddr;
	socklen_t addrlen = sizeof(clientaddr);
	getpeername(sock, (struct sockaddr *)&clientaddr, &addrlen);

	// 클라이언트 정보 출력
	char addr[INET_ADDRSTRLEN];
	inet_ntop(AF_INET, &clientaddr.sin_addr, addr, sizeof(addr));
	printf("[TCP 서버] 클라이언트 종료: IP 주소=%s, 포트 번호=%d\n",
		addr, ntohs(clientaddr.sin_port));

	// 소켓 닫기
	close(sock);
	delete ptr;

	if (nIndex != (nTotalSockets - 1)) {
		SocketInfoArray[nIndex] = SocketInfoArray[nTotalSockets - 1];
		Pollfds[nIndex] = Pollfds[nTotalSockets - 1];
	}
	--nTotalSockets;
}
