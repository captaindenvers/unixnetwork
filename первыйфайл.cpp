#include <iostream>
#define _XOPEN_SOURCE
#include <stdio.h>
#include <string.h>
#include <stdlib.h> 
#include <unistd.h>
#include <time.h>
#include <errno.h>   
#define SALT_LENGTH 16 

char* generate_salt(size_t length) {
    if (length <= 0) {
        fprintf(stderr, "Error\n");
        return NULL;
    }

    char* salt = (char*)malloc(length + 1);
    if (salt == NULL) {
        perror("Error");
        return NULL;
    }

    const char charset[] = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789./";
    size_t charset_len = strlen(charset);

    static int seeded = 0;
    if (!seeded) {
        srand(time(NULL));
        seeded = 1;
    }

    for (size_t i = 0; i < length; ++i) {
        salt[i] = charset[rand() % charset_len];
    }
    salt[length] = '\0';

    return salt;
}

char* hash_password(const char* password, const char* salt) {
    if (password == NULL || salt == NULL) {
        fprintf(stderr, "Error\n");
        return NULL;
    }

    char* hashed_password = crypt(password, salt);

    if (hashed_password == NULL) {
        if (errno == ENOSYS) {
            fprintf(stderr, "Error\n");
        } else {
            perror("Error");
        }
        return NULL;
    }

    char* result = strdup(hashed_password);
    if (result == NULL) {
        perror("Error");
        return NULL;
    }

    return result;
}

int main() 
{
    std::cout << "hello, world!";
    return 0;
}
