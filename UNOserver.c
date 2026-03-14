#include <mysql/mysql.h>
#include <string.h>
#include <unistd.h>
#include <stdlib.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <stdio.h>
#include <pthread.h>

// Global variables for thread synchronization (from your original code)
pthread_mutex_t mutex = PTHREAD_MUTEX_INITIALIZER;
int i;
int sockets[100];

// ==============================================================================
// 1. OPERATION FUNCTIONS (Structured strictly as requested)
// ==============================================================================

// Code 1: Register a new user
void RegisterPlayer(MYSQL *conn, char *username, char *password, char *respuesta) {
    char query[512];
    // Create the SQL Insert string
    sprintf(query, "INSERT INTO Player (Username, Password) VALUES ('%s', '%s');", username, password);
    
    // Execute and format response for C# client
    if (mysql_query(conn, query) == 0) {
        sprintf(respuesta, "1/YES"); // Registration successful
    } else {
        sprintf(respuesta, "1/NO");  // Failed (likely username already exists)
    }
}

// Code 2: Login check
void LoginPlayer(MYSQL *conn, char *username, char *password, char *respuesta) {
    char query[512];
    sprintf(query, "SELECT Id FROM Player WHERE Username='%s' AND Password='%s';", username, password);
    
    mysql_query(conn, query);
    MYSQL_RES *res = mysql_store_result(conn);
    
    // If it returns at least 1 row, the credentials are correct
    if (mysql_num_rows(res) > 0) {
        sprintf(respuesta, "2/YES");
    } else {
        sprintf(respuesta, "2/NO");
    }
    mysql_free_result(res);
}

// Code 3: Query 1 - Match History
void GetMatchHistory(MYSQL *conn, char *username, char *respuesta) {
    char query[512];
    sprintf(query, 
            "SELECT Matchy.StartTime, Matchy.DurationMinutes, Participation.Score "
            "FROM Player "
            "INNER JOIN Participation ON Player.Id = Participation.PlayerId "
            "INNER JOIN Matchy ON Matchy.Id = Participation.MatchId "
            "WHERE Player.Username = '%s' ORDER BY Matchy.StartTime DESC;", username);
            
    mysql_query(conn, query);
    MYSQL_RES *res = mysql_store_result(conn);
    MYSQL_ROW row;
    
    strcpy(respuesta, "3"); // Start response with the protocol code
    
    if (mysql_num_rows(res) == 0) {
        strcat(respuesta, "/No history found");
    } else {
        // Append each row to the response string, separated by slashes
        while ((row = mysql_fetch_row(res)) != NULL) {
            char line[256];
            sprintf(line, "/Date: %s | Lobby: %s min | Score: %s", row[0], row[1], row[2]);
            strcat(respuesta, line);
        }
    }
    mysql_free_result(res);
}

// Code 4: Query 2 - Global Leaderboard
void GetLeaderboard(MYSQL *conn, char *respuesta) {
    char query[512] = 
            "SELECT Player.Username, SUM(Participation.Score) AS TotalWins "
            "FROM Player "
            "INNER JOIN Participation ON Player.Id = Participation.PlayerId "
            "GROUP BY Player.Username ORDER BY TotalWins DESC LIMIT 10;";
            
    mysql_query(conn, query);
    MYSQL_RES *res = mysql_store_result(conn);
    MYSQL_ROW row;
    
    strcpy(respuesta, "4");
    while ((row = mysql_fetch_row(res)) != NULL) {
        char line[256];
        sprintf(line, "/%s: %s pts", row[0], row[1]);
        strcat(respuesta, line);
    }
    mysql_free_result(res);
}

// Code 5: Query 3 - Match Results by ID
void GetMatchResults(MYSQL *conn, int matchId, char *respuesta) {
    char query[512];
    sprintf(query, 
            "SELECT Player.Username, Participation.Score "
            "FROM Player "
            "INNER JOIN Participation ON Player.Id = Participation.PlayerId "
            "WHERE Participation.MatchId = %d ORDER BY Participation.Score ASC;", matchId);
            
    mysql_query(conn, query);
    MYSQL_RES *res = mysql_store_result(conn);
    MYSQL_ROW row;
    
    strcpy(respuesta, "5");
    if (mysql_num_rows(res) == 0) {
        strcat(respuesta, "/No players found for this match");
    } else {
        while ((row = mysql_fetch_row(res)) != NULL) {
            char line[256];
            sprintf(line, "/%s scored %s", row[0], row[1]);
            strcat(respuesta, line);
        }
    }
    mysql_free_result(res);
}

// ==============================================================================
// 2. THREAD FUNCTION (Handles individual client parsing and routing)
// ==============================================================================

void *AtenderCliente (void *socket) {
    int sock_conn = * (int *) socket;
    char peticion[512];
    char respuesta[2048]; // Increased size because queries return lots of text
    int terminar = 0;

    // Connect to the DB specifically for this client/thread
    MYSQL *conn = mysql_init(NULL);
    if (!mysql_real_connect(conn, "localhost", "root", "mysql", "UnoGameDB", 0, NULL, 0)) {
        printf("Error connecting to database in thread\\n");
    }

    // Main interaction loop
    while (terminar == 0) {
        int ret = read(sock_conn, peticion, sizeof(peticion));
        peticion[ret] = '\0';
        printf("Peticion: %s\\n", peticion);
        
        char *p = strtok(peticion, "/");
        int codigo = atoi(p);
        
        char username[50];
        char password[50];

        // Route the command to the correct discrete function
        if (codigo == 0) { 
            terminar = 1; // Disconnect
        } 
        else if (codigo == 1) { // 1/Username/Password (REGISTER)
            p = strtok(NULL, "/");
            strcpy(username, p);
            p = strtok(NULL, "/");
            strcpy(password, p);
            RegisterPlayer(conn, username, password, respuesta);
            write(sock_conn, respuesta, strlen(respuesta));
        }
        else if (codigo == 2) { // 2/Username/Password (LOGIN)
            p = strtok(NULL, "/");
            strcpy(username, p);
            p = strtok(NULL, "/");
            strcpy(password, p);
            LoginPlayer(conn, username, password, respuesta);
            write(sock_conn, respuesta, strlen(respuesta));
        }
        else if (codigo == 3) { // 3/Username (MATCH HISTORY)
            p = strtok(NULL, "/");
            strcpy(username, p);
            GetMatchHistory(conn, username, respuesta);
            write(sock_conn, respuesta, strlen(respuesta));
        }
        else if (codigo == 4) { // 4 (LEADERBOARD - no extra params needed)
            GetLeaderboard(conn, respuesta);
            write(sock_conn, respuesta, strlen(respuesta));
        }
        else if (codigo == 5) { // 5/MatchId (MATCH RESULTS)
            p = strtok(NULL, "/");
            int matchId = atoi(p);
            GetMatchResults(conn, matchId, respuesta);
            write(sock_conn, respuesta, strlen(respuesta));
        }
    }
    
    // Cleanup when client disconnects
    mysql_close(conn);
    close(sock_conn); 
    pthread_exit(NULL);
}

// ==============================================================================
// 3. MAIN SERVER ENGINE (Accepts clients and spawns threads)
// ==============================================================================

int main(int argc, char *argv[]) {
    int sock_conn, sock_listen;
    struct sockaddr_in serv_adr;
    
    if ((sock_listen = socket(AF_INET, SOCK_STREAM, 0)) < 0)
        printf("Error creating socket\\n");
    
    memset(&serv_adr, 0, sizeof(serv_adr));
    serv_adr.sin_family = AF_INET;
    serv_adr.sin_addr.s_addr = htonl(INADDR_ANY); // Listens on ALL VM IPs
    serv_adr.sin_port = htons(9050); // Matches your C# client port
    
    if (bind(sock_listen, (struct sockaddr *) &serv_adr, sizeof(serv_adr)) < 0)
        printf ("Error binding socket\\n");
    
    if (listen(sock_listen, 3) < 0)
        printf("Error listening\\n");
    
    pthread_t thread;
    i = 0;
    
    for (;;) {
        printf("Listening for clients...\n");
        sock_conn = accept(sock_listen, NULL, NULL);
        printf("Client connected!\n");
        
        sockets[i] = sock_conn;
        
        // Spawn a new thread for the client
        pthread_create(&thread, NULL, AtenderCliente, &sockets[i]);
        i++;
    }
}