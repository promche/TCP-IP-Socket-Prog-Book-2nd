#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <pthread.h>
#include <errno.h>

void *MyThread(void *arg)
{
	// 우선순위를 높게 설정한다.
	errno = 0;
	int retval = nice(-20);
	if (retval < 0 && errno != 0) {
		perror("nice() in MyThread");
		exit(1);
	}

	while (1) {
		write(1, ".", 1);
	}
	return 0;
}

int main()
{
	// CPU 개수를 알아낸다.
	int numberOfProcessors = sysconf(_SC_NPROCESSORS_ONLN);

	// CPU 개수만큼 스레드를 생성한다.
	for (int i = 0; i < numberOfProcessors; i++) {
		pthread_t tid;
		pthread_create(&tid, NULL, MyThread, NULL);
	}

	// 우선순위를 낮게 설정한다.
	errno = 0;
	int retval = nice(19);
	if (retval < 0 && errno != 0) {
		perror("nice() in main");
		exit(1);
	}

	sleep(1);
	while (1) {
		write(1, "*", 1);
	}
	return 0;
}
