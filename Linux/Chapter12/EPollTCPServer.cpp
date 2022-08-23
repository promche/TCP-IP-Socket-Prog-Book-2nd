#include "../Common.h"

#define SERVERPORT 9000
#define BUFSIZE    512

// 소켓 정보 저장을 위한 구조체
struct SOCKETINFO
{
	SOCKET sock;
	char buf[BUFSIZE + 1];
	int recvbytes;
	int sendbytes;
};

// 소켓 이벤트 등록 함수
void RegisterEvent(int epollfd, SOCKET sock, uint32_t events)
{
	SOCKETINFO *ptr = new SOCKETINFO;
	ptr->sock = sock;
	ptr->recvbytes = 0;
	ptr->sendbytes = 0;

	struct epoll_event ev;
	ev.events = events;
	ev.data.ptr = ptr;
	if (epoll_ctl(epollfd, EPOLL_CTL_ADD, sock, &ev) < 0) {
		perror("epoll_ctl()");
		close(sock);
		exit(1);
	}
}

// 소켓 이벤트 수정 함수
void ModifyEvent(int epollfd, struct epoll_event ev, uint32_t events)
{
	ev.events = events;
	SOCKETINFO *ptr = (SOCKETINFO *)ev.data.ptr;
	if (epoll_ctl(epollfd, EPOLL_CTL_MOD, ptr->sock, &ev) < 0) {
		perror("epoll_ctl()");
		close(ptr->sock);
		exit(1);
	}
}

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

	// EPoll 인스턴스 생성
	int epollfd = epoll_create(1); /* 전달 인수(> 0)는 무시됨 */
	if (epollfd < 0) {
		perror("epoll_create()");
		exit(1);
	}

	// 소켓 이벤트 등록(1)
	RegisterEvent(epollfd, listen_sock, EPOLLIN);

	// 데이터 통신에 사용할 변수
	struct epoll_event events[FD_SETSIZE];
	int nready;
	SOCKET client_sock;
	struct sockaddr_in clientaddr;
	socklen_t addrlen;

	while (1) {
		// epoll_wait()
		nready = epoll_wait(epollfd, events, FD_SETSIZE, -1);
		if (nready < 0) err_quit("epoll_wait()");

		for (int i = 0; i < nready; i++) {
			SOCKETINFO *ptr = (SOCKETINFO *)events[i].data.ptr;
			// 소켓 이벤트 검사(1): 클라이언트 접속 수용
			if (ptr->sock == listen_sock) {
				addrlen = sizeof(clientaddr);
				client_sock = accept(listen_sock,
					(struct sockaddr *)&clientaddr, &addrlen);
				if (client_sock == INVALID_SOCKET) {
					err_display("accept()");
					goto MAIN_EXIT;
				}
				else {
					// 클라이언트 정보 출력
					char addr[INET_ADDRSTRLEN];
					inet_ntop(AF_INET, &clientaddr.sin_addr, addr, sizeof(addr));
					printf("\n[TCP 서버] 클라이언트 접속: IP 주소=%s, 포트 번호=%d\n",
						addr, ntohs(clientaddr.sin_port));
					// 넌블로킹 소켓으로 전환
					int flags = fcntl(client_sock, F_GETFL);
					flags |= O_NONBLOCK;
					fcntl(client_sock, F_SETFL, flags);
					// 소켓 이벤트 등록(2)
					RegisterEvent(epollfd, client_sock, EPOLLIN);
				}
				if (--nready <= 0)
					break;
			} /* end of if */
			// 소켓 이벤트 검사(2): 데이터 통신
			else {
				// 클라이언트 정보 얻기
				addrlen = sizeof(clientaddr);
				getpeername(ptr->sock, (struct sockaddr *)&clientaddr, &addrlen);
				char addr[INET_ADDRSTRLEN];
				inet_ntop(AF_INET, &clientaddr.sin_addr, addr, sizeof(addr));

				if (events[i].events & EPOLLIN) {
					// 데이터 받기
					retval = recv(ptr->sock, ptr->buf, BUFSIZE, 0);
					if (retval == SOCKET_ERROR) {
						err_display("recv()");
						// 소켓 이벤트 등록 해제(1)
						epoll_ctl(epollfd, EPOLL_CTL_DEL, ptr->sock, NULL);
						close(ptr->sock); delete ptr;
					}
					else if (retval == 0) {
						// 클라이언트 정보 출력
						printf("[TCP 서버] 클라이언트 종료: IP 주소=%s, "
								"포트 번호=%d\n", addr, ntohs(clientaddr.sin_port));
						// 소켓 이벤트 등록 해제(2)
						epoll_ctl(epollfd, EPOLL_CTL_DEL, ptr->sock, NULL);
						close(ptr->sock); delete ptr;
					}
					else {
						ptr->recvbytes = retval;
						// 받은 데이터 출력
						ptr->buf[ptr->recvbytes] = '\0';
						printf("[TCP/%s:%d] %s\n", addr,
							ntohs(clientaddr.sin_port), ptr->buf);
						// 소켓 이벤트 재등록(1)
						if (ptr->recvbytes > ptr->sendbytes) {
							ModifyEvent(epollfd, events[i], EPOLLOUT);
						}
					}
				}
				else if (events[i].events & EPOLLOUT) {
					// 데이터 보내기
					retval = send(ptr->sock, ptr->buf + ptr->sendbytes,
						ptr->recvbytes - ptr->sendbytes, 0);
					if (retval == SOCKET_ERROR) {
						err_display("send()");
						// 소켓 이벤트 등록 해제(3)
						epoll_ctl(epollfd, EPOLL_CTL_DEL, ptr->sock, NULL);
						close(ptr->sock); delete ptr;
					}
					else {
						ptr->sendbytes += retval;
						// 소켓 이벤트 재등록(2)
						if (ptr->recvbytes == ptr->sendbytes) {
							ptr->recvbytes = ptr->sendbytes = 0;
							ModifyEvent(epollfd, events[i], EPOLLIN);
						}
					}
				}
			} /* end of else */
		} /* end of for */
	} /* end of while (1) */

MAIN_EXIT:
	// EPoll 인스턴스 삭제
	close(epollfd);

	// 소켓 닫기
	close(listen_sock);
	return 0;
}
