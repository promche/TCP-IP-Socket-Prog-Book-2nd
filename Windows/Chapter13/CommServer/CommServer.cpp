#include "..\..\Common.h"

#define PORTNAME _T("\\\\.\\COM10") /* 실습 환경에 따라 변경 필요! */
#define BUFSIZE  512

int main(int argc, char *argv[])
{
	int retval;

	// 직렬 포트 열기
	HANDLE hComm = CreateFile(PORTNAME, GENERIC_READ | GENERIC_WRITE,
		0, NULL, OPEN_EXISTING, 0, NULL);
	if (hComm == INVALID_HANDLE_VALUE)
		err_quit("CreateFile()");

	// 직렬 포트 설정값 얻기
	DCB dcb;
	if (!GetCommState(hComm, &dcb))
		err_quit("GetCommState()");

	// 직렬 포트 설정값 수정
	dcb.BaudRate = CBR_57600;
	dcb.ByteSize = 8;
	dcb.fParity = FALSE;
	dcb.StopBits = ONESTOPBIT;
	if (!SetCommState(hComm, &dcb))
		err_quit("SetCommState()");

	// 읽기와 쓰기 타임아웃 설정
	COMMTIMEOUTS timeouts;
	timeouts.ReadIntervalTimeout = 0;
	timeouts.ReadTotalTimeoutMultiplier = 0;
	timeouts.ReadTotalTimeoutConstant = 0;
	timeouts.WriteTotalTimeoutMultiplier = 0;
	timeouts.WriteTotalTimeoutConstant = 0;
	if (!SetCommTimeouts(hComm, &timeouts))
		err_quit("SetCommTimeouts()");

	// 데이터 통신에 사용할 변수
	char buf[BUFSIZE + 1];
	DWORD BytesRead, BytesWritten;

	// 클라이언트와 데이터 통신
	while (1) {
		// 데이터 받기
		retval = ReadFile(hComm, buf, BUFSIZE, &BytesRead, NULL);
		if (retval == 0) err_quit("ReadFile()");

		// 받은 데이터 출력
		buf[BytesRead] = '\0';
		printf("[받은 데이터] %s\n", buf);

		// 데이터 보내기
		retval = WriteFile(hComm, buf, BUFSIZE, &BytesWritten, NULL);
		if (retval == 0) err_quit("WriteFile()");
	}

	// 직렬 포트 닫기
	CloseHandle(hComm);
	return 0;
}
