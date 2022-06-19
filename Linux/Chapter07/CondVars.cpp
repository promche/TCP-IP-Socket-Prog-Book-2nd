#include <stdio.h>
#include <string.h>
#include <pthread.h>

#define BUFSIZE 10

pthread_cond_t writeCond;
pthread_cond_t readCond;
pthread_mutex_t writeMutex;
pthread_mutex_t readMutex;
int writeDone = 0;
int readDone = 0;
int buf[BUFSIZE];

void *WriteThread(void *arg)
{
	for (int k = 1; k <= 500; k++) {
		// 읽기 완료 대기
		pthread_mutex_lock(&readMutex);
		while (readDone == 0)
			pthread_cond_wait(&readCond, &readMutex);
		readDone = 0;
		pthread_mutex_unlock(&readMutex);

		// 공유 버퍼에 데이터 저장
		for (int i = 0; i < BUFSIZE; i++)
			buf[i] = k;

		// 쓰기 완료 알림
		pthread_mutex_lock(&writeMutex);
		writeDone = 1;
		pthread_mutex_unlock(&writeMutex);
		pthread_cond_signal(&writeCond);
	}
	return 0;
}

void *ReadThread(void *arg)
{
	while (1) {
		// 쓰기 완료 대기
		pthread_mutex_lock(&writeMutex);
		while (writeDone == 0)
			pthread_cond_wait(&writeCond, &writeMutex);
		writeDone = 0;
		pthread_mutex_unlock(&writeMutex);

		// 읽은 데이터 출력 후 버퍼를 0으로 초기화
		printf("Thread %4d:\t", (int)pthread_self());
		for (int i = 0; i < BUFSIZE; i++)
			printf("%3d ", buf[i]);
		printf("\n");
		memset(buf, 0, sizeof(buf));

		// 읽기 완료 알림
		pthread_mutex_lock(&readMutex);
		readDone = 1;
		pthread_mutex_unlock(&readMutex);
		pthread_cond_signal(&readCond);
	}
	return 0;
}

int main(int argc, char *argv[])
{
	// 조건 변수와 뮤텍스 생성
	pthread_cond_init(&writeCond, NULL);
	pthread_cond_init(&readCond, NULL);
	pthread_mutex_init(&writeMutex, NULL);
	pthread_mutex_init(&readMutex, NULL);

	// 스레드 세 개 생성
	pthread_t tid[3];
	pthread_create(&tid[0], NULL, WriteThread, NULL);
	pthread_create(&tid[1], NULL, ReadThread, NULL);
	pthread_create(&tid[2], NULL, ReadThread, NULL);

	// 읽기 완료 알림
	pthread_mutex_lock(&readMutex);
	readDone = 1;
	pthread_mutex_unlock(&readMutex);
	pthread_cond_signal(&readCond);

	// 스레드 세 개 종료 대기
	pthread_join(tid[0], NULL);
	pthread_join(tid[1], NULL);
	pthread_join(tid[2], NULL);

	// 조건 변수와 뮤텍스 제거
	pthread_cond_destroy(&writeCond);
	pthread_cond_destroy(&readCond);
	pthread_mutex_destroy(&writeMutex);
	pthread_mutex_destroy(&readMutex);
	return 0;
}
