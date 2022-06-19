#include <stdio.h>
#include <pthread.h>

#define MAXCNT 100000000
int count = 0;
pthread_mutex_t mutex;

void *MyThread1(void *arg)
{
	for (int i = 0; i < MAXCNT; i++) {
		pthread_mutex_lock(&mutex);
		count += 2;
		pthread_mutex_unlock(&mutex);
	}
	return 0;
}

void *MyThread2(void *arg)
{
	for (int i = 0; i < MAXCNT; i++) {
		pthread_mutex_lock(&mutex);
		count -= 2;
		pthread_mutex_unlock(&mutex);
	}
	return 0;
}

int main(int argc, char *argv[])
{
	// 뮤텍스 초기화
	pthread_mutex_init(&mutex, NULL);

	// 스레드 두 개 생성
	pthread_t tid[2];
	pthread_create(&tid[0], NULL, MyThread1, NULL); 
	pthread_create(&tid[1], NULL, MyThread2, NULL); 

	// 스레드 두 개 종료 대기
	pthread_join(tid[0], NULL);
	pthread_join(tid[1], NULL);

	// 뮤텍스 삭제
	pthread_mutex_destroy(&mutex);

	// 결과 출력
	printf("count = %d\n", count);
	return 0;
}
