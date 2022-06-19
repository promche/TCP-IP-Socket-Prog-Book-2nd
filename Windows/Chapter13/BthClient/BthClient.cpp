#include "..\..\Common.h"
#include <ws2bth.h>
#include <initguid.h>
#include <locale.h>

#define SERVERADDR _T("E0:06:E6:AF:78:9F") /* 실습 환경에 따라 변경 필요! */
#define BUFSIZE    512

DEFINE_GUID(BthServer_Service, 0x4672de25, 0x588d, 0x48af,
	0x80, 0x73, 0x5f, 0x2b, 0x7b, 0x0, 0x60, 0x1f);

int main(int argc, char *argv[])
{
	int retval;

	// Unicode 한국어 출력
	setlocale(LC_ALL, "");

	// 윈속 초기화
	WSADATA wsa;
	if (WSAStartup(MAKEWORD(2, 2), &wsa) != 0)
		return 1;

	// 블루투스 장치 검색 (생략)
	// - 편의상 SERVERADDR 상수로 대신한다.

	// 블루투스 서비스 검색 준비
	DWORD qslen = sizeof(WSAQUERYSET);
	WSAQUERYSET *qs = (WSAQUERYSET *)malloc(qslen);
	memset(qs, 0, qslen);
	qs->dwSize = qslen;
	qs->dwNameSpace = NS_BTH;
	qs->lpServiceClassId = (GUID *)&BthServer_Service;
	qs->lpszContext = (LPTSTR)SERVERADDR;
	DWORD flags = LUP_FLUSHCACHE | LUP_RETURN_ADDR;

	// 블루투스 서비스 검색 시작
	HANDLE hLookup;
	retval = WSALookupServiceBegin(qs, flags, &hLookup);
	if (retval == SOCKET_ERROR) {
		printf("[오류] 발견된 블루투스 장치 없음!\n");
		exit(1);
	}

	// 검색된 블루투스 서비스 정보 확인
	SOCKADDR_BTH *sa = NULL;
	int serverport = 0;
	bool done = false;
	while (!done) {
		retval = WSALookupServiceNext(hLookup, flags, &qslen, qs);
		if (retval == NO_ERROR) {
			// 장치 정보를 담은 소켓 주소 구조체 접근
			sa = (SOCKADDR_BTH *)qs->lpcsaBuffer->RemoteAddr.lpSockaddr;
			// 서버 포트 번호 저장 후 탈출
			serverport = sa->port;
			break;
		}
		else {
			if (WSAGetLastError() == WSAEFAULT) {
				free(qs);
				qs = (WSAQUERYSET *)malloc(qslen);
			}
			else {
				done = true;
			}
		}
	}
	if (sa == NULL) {
		_tprintf(_T("[오류] %s에서 실행 중인 서버 없음!\n"), SERVERADDR);
		exit(1);
	}

	// 블루투스 서비스 검색 종료
	WSALookupServiceEnd(hLookup);
	free(qs);

	// 소켓 생성
	SOCKET sock = socket(AF_BTH, SOCK_STREAM, BTHPROTO_RFCOMM);
	if (sock == INVALID_SOCKET) err_quit("socket()");

	// connect()
	SOCKADDR_BTH serveraddr;
	memset(&serveraddr, 0, sizeof(serveraddr));
	int addrlen = sizeof(serveraddr);
	WSAStringToAddress((LPTSTR)SERVERADDR, AF_BTH, NULL,
		(struct sockaddr *)&serveraddr, &addrlen);
	serveraddr.port = serverport;
	retval = connect(sock, (struct sockaddr *)&serveraddr, sizeof(serveraddr));
	if (retval == SOCKET_ERROR) err_quit("connect()");

	// 데이터 통신에 사용할 변수
	char buf[BUFSIZE + 1];
	int len;

	// 서버와 데이터 통신
	while (1) {
		// 데이터 입력
		printf("\n[보낼 데이터] ");
		if (fgets(buf, BUFSIZE + 1, stdin) == NULL)
			break;

		// '\n' 문자 제거
		len = (int)strlen(buf);
		if (buf[len - 1] == '\n') buf[len - 1] = '\0';
		if (strlen(buf) == 0) break;

		// 데이터 보내기
		retval = send(sock, buf, (int)strlen(buf), 0);
		if (retval == SOCKET_ERROR) {
			err_display("send()");
			break;
		}
		printf("[블루투스 클라이언트] %d바이트를 보냈습니다.\n", retval);
	}

	// 소켓 닫기
	closesocket(sock);

	// 윈속 종료
	WSACleanup();
	return 0;
}
