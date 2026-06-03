#include <mysql/mysql.h>
#include <string.h>
#include <unistd.h>
#include <stdlib.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <stdio.h>
#include <pthread.h>
#include <time.h>
#include <signal.h>

#define MAX_PLAYERS 4
#define MAX_HAND 60
#define MAX_DECK 120

typedef struct {
    char color;
    char value[8];
} UnoCard;

// Global variables for thread synchronization
pthread_mutex_t mutex = PTHREAD_MUTEX_INITIALIZER;
int i = 0;
int sockets[100];

typedef struct {
    int socket;
    char name[20];
} ConnectedUser;

ConnectedUser listaUsuarios[100];
int numUsuarios = 0;

typedef struct {
    char inviter[20];
    char invitees[4][20]; // Assuming max 4 invited players
    int num_invitees;
    int responses_received;
    int all_accepted; // 1 if everyone said yes, 0 if anyone said no
} GameInvitation;

GameInvitation invitations[20];
int numInvitations = 0;

// --- GAME ROOM STRUCTURE ---
typedef struct {
    int id;
    int players[4]; // Store the sockets of the players in this game
    char playerNames[4][20];
    int num_players;
    UnoCard deck[MAX_DECK];
    int deck_count;
    UnoCard discard[MAX_DECK];
    int discard_count;
    UnoCard hands[MAX_PLAYERS][MAX_HAND];
    int hand_count[MAX_PLAYERS];
    int current_turn;
    int direction;
    char active_color;
    int started;
    int finished;
    int saved_to_db;
    time_t start_time;
} GameRoom;

GameRoom activeGames[20];
int numActiveGames = 0;

// Helper function to get socket by username
int GetSocketByName(char *username) {
    for (int j = 0; j < numUsuarios; j++) {
        if (strcmp(listaUsuarios[j].name, username) == 0) {
            return listaUsuarios[j].socket;
        }
    }
    return -1;
}

int FindUserByName(const char *username) {
    for (int j = 0; j < numUsuarios; j++) {
        if (strcmp(listaUsuarios[j].name, username) == 0) return j;
    }
    return -1;
}

int FindUserBySocket(int socket) {
    for (int j = 0; j < numUsuarios; j++) {
        if (listaUsuarios[j].socket == socket) return j;
    }
    return -1;
}

int GetPlayerIndex(GameRoom *game, char *username);

void SendMessage(int socket, const char *message) {
    if (socket < 0 || message == NULL) return;
    char framed[4096];
    snprintf(framed, sizeof(framed), "%s\n", message);
    write(socket, framed, strlen(framed));
}

void BroadcastConnectedUsers() {
    char listResponse[1024];
    strcpy(listResponse, "6");
    for (int j = 0; j < numUsuarios; j++) {
        if (strlen(listaUsuarios[j].name) == 0) continue;
        strcat(listResponse, "/");
        strcat(listResponse, listaUsuarios[j].name);
    }

    for (int j = 0; j < numUsuarios; j++) {
        SendMessage(listaUsuarios[j].socket, listResponse);
    }
}

void RemoveInvitationAt(int index) {
    if (index < 0 || index >= numInvitations) return;
    for (int j = index; j < numInvitations - 1; j++) {
        invitations[j] = invitations[j + 1];
    }
    numInvitations--;
}

void CleanupInvitationsForUser(const char *username) {
    for (int j = 0; j < numInvitations; ) {
        int involved = strcmp(invitations[j].inviter, username) == 0;
        for (int k = 0; k < invitations[j].num_invitees; k++) {
            if (strcmp(invitations[j].invitees[k], username) == 0) involved = 1;
        }

        if (involved) {
            if (strcmp(invitations[j].inviter, username) != 0) {
                int inviterSocket = GetSocketByName(invitations[j].inviter);
                SendMessage(inviterSocket, "10/0");
            }
            RemoveInvitationAt(j);
        } else {
            j++;
        }
    }
}

void NotifyGamesForDisconnectedUser(const char *username) {
    for (int g = 0; g < numActiveGames; g++) {
        int playerIndex = GetPlayerIndex(&activeGames[g], (char *)username);
        if (playerIndex == -1 || activeGames[g].finished) continue;

        activeGames[g].finished = 1;
        char msg[256];
        sprintf(msg, "18/%s disconnected. The game was cancelled.", username);
        for (int p = 0; p < activeGames[g].num_players; p++) {
            if (p != playerIndex) SendMessage(activeGames[g].players[p], msg);
        }
    }
}

void CardToString(UnoCard card, char *out) {
    sprintf(out, "%c-%s", card.color, card.value);
}

int FindGameById(int gameId) {
    for (int j = 0; j < numActiveGames; j++) {
        if (activeGames[j].id == gameId) return j;
    }
    return -1;
}

int GetPlayerIndex(GameRoom *game, char *username) {
    for (int j = 0; j < game->num_players; j++) {
        if (strcmp(game->playerNames[j], username) == 0) return j;
    }
    return -1;
}

int IsUserInActiveGame(const char *username) {
    for (int g = 0; g < numActiveGames; g++) {
        if (activeGames[g].finished) continue;
        for (int p = 0; p < activeGames[g].num_players; p++) {
            if (strcmp(activeGames[g].playerNames[p], username) == 0) return 1;
        }
    }
    return 0;
}

int GetPlayerIdByUsername(MYSQL *conn, const char *username) {
    char query[256];
    sprintf(query, "SELECT Id FROM Player WHERE Username='%s';", username);
    if (mysql_query(conn, query) != 0) {
        printf("GetPlayerIdByUsername failed for %s: %s\n", username, mysql_error(conn));
        return -1;
    }

    MYSQL_RES *res = mysql_store_result(conn);
    if (res == NULL) return -1;
    MYSQL_ROW row = mysql_fetch_row(res);
    int playerId = row ? atoi(row[0]) : -1;
    mysql_free_result(res);
    return playerId;
}

void SaveCompletedMatch(MYSQL *conn, GameRoom *game, const char *winner) {
    if (game->saved_to_db) return;
    game->saved_to_db = 1;

    int duration = (int)(difftime(time(NULL), game->start_time) / 60);
    if (duration < 1) duration = 1;

    char query[512];
    sprintf(query, "INSERT INTO Matchy (StartTime, DurationMinutes) VALUES (FROM_UNIXTIME(%ld), %d);", (long)game->start_time, duration);
    if (mysql_query(conn, query) != 0) {
        printf("Failed to save Matchy row. Winner=%s Error=%s\n", winner, mysql_error(conn));
        return;
    }

    unsigned long matchId = mysql_insert_id(conn);
    printf("Saved match. Winner=%s MatchId=%lu Duration=%d minutes\n", winner, matchId, duration);

    for (int p = 0; p < game->num_players; p++) {
        int playerId = GetPlayerIdByUsername(conn, game->playerNames[p]);
        if (playerId == -1) {
            printf("Could not save participation for %s: player not found\n", game->playerNames[p]);
            continue;
        }

        int score = strcmp(game->playerNames[p], winner) == 0 ? 1 : 0;
        sprintf(query, "INSERT INTO Participation (PlayerId, MatchId, Score) VALUES (%d, %lu, %d);", playerId, matchId, score);
        if (mysql_query(conn, query) != 0) {
            printf("Failed Participation insert for %s score=%d: %s\n", game->playerNames[p], score, mysql_error(conn));
        } else {
            printf("Participation saved: %s score=%d\n", game->playerNames[p], score);
        }
    }
}

void AddDeckCard(GameRoom *game, char color, const char *value) {
    game->deck[game->deck_count].color = color;
    strcpy(game->deck[game->deck_count].value, value);
    game->deck_count++;
}

void ShuffleDeck(GameRoom *game) {
    for (int j = game->deck_count - 1; j > 0; j--) {
        int k = rand() % (j + 1);
        UnoCard tmp = game->deck[j];
        game->deck[j] = game->deck[k];
        game->deck[k] = tmp;
    }
}

void BuildDeck(GameRoom *game) {
    char colors[] = {'R', 'Y', 'G', 'B'};
    char values[][8] = {"1", "2", "3", "4", "5", "6", "7", "8", "9", "SKIP", "REV", "DRAW2"};

    game->deck_count = 0;
    for (int c = 0; c < 4; c++) {
        AddDeckCard(game, colors[c], "0");
        for (int copy = 0; copy < 2; copy++) {
            for (int v = 0; v < 12; v++) AddDeckCard(game, colors[c], values[v]);
        }
    }
    for (int j = 0; j < 4; j++) {
        AddDeckCard(game, 'W', "WILD");
        AddDeckCard(game, 'W', "WILD4");
    }
    ShuffleDeck(game);
}

void RefillDeckFromDiscard(GameRoom *game) {
    if (game->deck_count > 0 || game->discard_count <= 1) return;

    UnoCard top = game->discard[game->discard_count - 1];
    int refill_count = game->discard_count - 1;
    for (int j = 0; j < refill_count; j++) {
        game->deck[j] = game->discard[j];
    }
    game->deck_count = refill_count;
    game->discard[0] = top;
    game->discard_count = 1;
    ShuffleDeck(game);
}

int DrawOne(GameRoom *game, int playerIndex) {
    RefillDeckFromDiscard(game);
    if (game->deck_count <= 0 || game->hand_count[playerIndex] >= MAX_HAND) return 0;

    game->hands[playerIndex][game->hand_count[playerIndex]] = game->deck[game->deck_count - 1];
    game->hand_count[playerIndex]++;
    game->deck_count--;
    return 1;
}

void AdvanceTurn(GameRoom *game) {
    game->current_turn += game->direction;
    if (game->current_turn >= game->num_players) game->current_turn = 0;
    if (game->current_turn < 0) game->current_turn = game->num_players - 1;
}

int IsPlayable(GameRoom *game, UnoCard card) {
    UnoCard top = game->discard[game->discard_count - 1];
    if (card.color == 'W') return 1;
    if (card.color == game->active_color) return 1;
    if (strcmp(card.value, top.value) == 0) return 1;
    return 0;
}

void SendRuleError(int socket, const char *message) {
    char response[256];
    sprintf(response, "17/%s", message);
    SendMessage(socket, response);
}

void BroadcastGameState(GameRoom *game, const char *lastAction) {
    char topCard[16];
    CardToString(game->discard[game->discard_count - 1], topCard);

    for (int p = 0; p < game->num_players; p++) {
        char hand[1024] = "";
        char opponents[512] = "";

        for (int h = 0; h < game->hand_count[p]; h++) {
            char card[16];
            CardToString(game->hands[p][h], card);
            if (h > 0) strcat(hand, ",");
            strcat(hand, card);
        }

        for (int other = 0; other < game->num_players; other++) {
            char entry[64];
            if (other == p) continue;
            if (strlen(opponents) > 0) strcat(opponents, ",");
            sprintf(entry, "%s:%d", game->playerNames[other], game->hand_count[other]);
            strcat(opponents, entry);
        }

        char response[2048];
        sprintf(response, "12/%d/%s/%s/%d/%d/%c/%s/%s/%s",
                game->id,
                topCard,
                game->playerNames[game->current_turn],
                game->direction,
                game->deck_count,
                game->active_color,
                hand,
                opponents,
                lastAction);
        printf("BroadcastGameState player=%s hand_count=%d hand=\"%s\"\n", game->playerNames[p], game->hand_count[p], hand);
        printf("BroadcastGameState response=%s\n", response);
        SendMessage(game->players[p], response);
    }
}

void BroadcastGameOver(GameRoom *game, char *winner) {
    char response[256];
    sprintf(response, "16/%d/%s", game->id, winner);
    usleep(50000);
    for (int p = 0; p < game->num_players; p++) {
        SendMessage(game->players[p], response);
    }
}

void StartUnoGame(GameRoom *game) {
    game->discard_count = 0;
    game->current_turn = 0;
    game->direction = 1;
    game->started = 1;
    game->finished = 0;
    game->saved_to_db = 0;
    game->start_time = time(NULL);
    for (int p = 0; p < game->num_players; p++) game->hand_count[p] = 0;

    BuildDeck(game);
    for (int card = 0; card < 7; card++) {
        for (int p = 0; p < game->num_players; p++) DrawOne(game, p);
    }

    UnoCard first;
    do {
        first = game->deck[game->deck_count - 1];
        game->deck_count--;
        game->discard[game->discard_count++] = first;
    } while ((first.color == 'W' || strcmp(first.value, "DRAW2") == 0 || strcmp(first.value, "SKIP") == 0 || strcmp(first.value, "REV") == 0) && game->deck_count > 0);

    game->active_color = first.color == 'W' ? 'R' : first.color;
    for (int p = 0; p < game->num_players; p++) {
        printf("After dealing: %s has %d cards\n", game->playerNames[p], game->hand_count[p]);
    }
    BroadcastGameState(game, "Cards dealt. The first player can play.");
}

void PlayUnoCard(MYSQL *conn, GameRoom *game, int playerIndex, int cardIndex, char chosenColor) {
    if (game->finished) return;
    if (playerIndex != game->current_turn) {
        SendRuleError(game->players[playerIndex], "It is not your turn.");
        return;
    }
    if (cardIndex < 0 || cardIndex >= game->hand_count[playerIndex]) {
        SendRuleError(game->players[playerIndex], "That card does not exist.");
        return;
    }

    UnoCard card = game->hands[playerIndex][cardIndex];
    if (!IsPlayable(game, card)) {
        SendRuleError(game->players[playerIndex], "You must match the color, value, or play a wild card.");
        return;
    }

    for (int h = cardIndex; h < game->hand_count[playerIndex] - 1; h++) {
        game->hands[playerIndex][h] = game->hands[playerIndex][h + 1];
    }
    game->hand_count[playerIndex]--;
    game->discard[game->discard_count++] = card;

    if (card.color == 'W') {
        if (chosenColor == 'R' || chosenColor == 'Y' || chosenColor == 'G' || chosenColor == 'B') game->active_color = chosenColor;
        else game->active_color = 'R';
    } else {
        game->active_color = card.color;
    }

    char cardText[16];
    char action[256];
    CardToString(card, cardText);
    sprintf(action, "%s played %s.", game->playerNames[playerIndex], cardText);

    if (game->hand_count[playerIndex] == 0) {
        game->finished = 1;
        SaveCompletedMatch(conn, game, game->playerNames[playerIndex]);
        BroadcastGameState(game, action);
        BroadcastGameOver(game, game->playerNames[playerIndex]);
        return;
    }

    AdvanceTurn(game);
    if (strcmp(card.value, "REV") == 0) {
        game->direction *= -1;
        if (game->num_players == 2) AdvanceTurn(game);
        sprintf(action, "%s played reverse.", game->playerNames[playerIndex]);
    } else if (strcmp(card.value, "SKIP") == 0) {
        sprintf(action, "%s skipped %s.", game->playerNames[playerIndex], game->playerNames[game->current_turn]);
        AdvanceTurn(game);
    } else if (strcmp(card.value, "DRAW2") == 0) {
        int punished = game->current_turn;
        DrawOne(game, punished);
        DrawOne(game, punished);
        sprintf(action, "%s made %s draw 2 cards.", game->playerNames[playerIndex], game->playerNames[punished]);
        AdvanceTurn(game);
    } else if (strcmp(card.value, "WILD4") == 0) {
        int punished = game->current_turn;
        for (int j = 0; j < 4; j++) DrawOne(game, punished);
        sprintf(action, "%s made %s draw 4 cards.", game->playerNames[playerIndex], game->playerNames[punished]);
        AdvanceTurn(game);
    }

    BroadcastGameState(game, action);
}

void DrawUnoCard(GameRoom *game, int playerIndex) {
    if (game->finished) return;
    if (playerIndex != game->current_turn) {
        SendRuleError(game->players[playerIndex], "It is not your turn.");
        return;
    }

    DrawOne(game, playerIndex);
    char action[256];
    sprintf(action, "%s drew a card.", game->playerNames[playerIndex]);
    AdvanceTurn(game);
    BroadcastGameState(game, action);
}

// ===========================================================================
// 1. OPERATION FUNCTIONS
// ===========================================================================

void RegisterPlayer(MYSQL *conn, char *username, char *password, char *respuesta) {
    char query[512];
    sprintf(query, "INSERT INTO Player (Username, Password) VALUES ('%s', '%s');", username, password);
    if (mysql_query(conn, query) == 0) sprintf(respuesta, "1/YES");
    else sprintf(respuesta, "1/NO");
}

void LoginPlayer(MYSQL *conn, char *username, char *password, char *respuesta) {
    char query[512];
    sprintf(query, "SELECT Id FROM Player WHERE Username='%s' AND Password='%s' AND Active=1;", username, password);
    mysql_query(conn, query);
    MYSQL_RES *res = mysql_store_result(conn);
    if (mysql_num_rows(res) > 0) sprintf(respuesta, "2/YES");
    else sprintf(respuesta, "2/NO");
    mysql_free_result(res);
}

void DeleteAccount(MYSQL *conn, char *username, char *password, char *respuesta) {
    char query[512];
    sprintf(query, "SELECT Id FROM Player WHERE Username='%s' AND Password='%s' AND Active=1;", username, password);
    if (mysql_query(conn, query) != 0) {
        sprintf(respuesta, "19/0/Database error");
        printf("DeleteAccount credential query failed: %s\n", mysql_error(conn));
        return;
    }

    MYSQL_RES *res = mysql_store_result(conn);
    int found = res != NULL && mysql_num_rows(res) > 0;
    if (res != NULL) mysql_free_result(res);

    if (!found) {
        sprintf(respuesta, "19/0/Wrong username or password");
        return;
    }

    if (IsUserInActiveGame(username)) {
        sprintf(respuesta, "19/0/Cannot delete account while in an active game");
        return;
    }

    sprintf(query, "UPDATE Player SET Active=0 WHERE Username='%s' AND Password='%s';", username, password);
    if (mysql_query(conn, query) != 0) {
        sprintf(respuesta, "19/0/Database error");
        printf("DeleteAccount update failed for %s: %s\n", username, mysql_error(conn));
        return;
    }

    sprintf(respuesta, "19/1/Account deleted successfully");
    printf("Account soft-deleted: %s\n", username);
}

void GetMatchHistory(MYSQL *conn, char *username, char *respuesta) {
    char query[512];
    sprintf(query, "SELECT Matchy.StartTime, Matchy.DurationMinutes, Participation.Score FROM Player INNER JOIN Participation ON Player.Id = Participation.PlayerId INNER JOIN Matchy ON Matchy.Id = Participation.MatchId WHERE Player.Username = '%s' ORDER BY Matchy.StartTime DESC;", username);
    mysql_query(conn, query);
    MYSQL_RES *res = mysql_store_result(conn);
    MYSQL_ROW row;
    strcpy(respuesta, "3");
    if (mysql_num_rows(res) == 0) strcat(respuesta, "/No history found");
    else {
        while ((row = mysql_fetch_row(res)) != NULL) {
            char line[256];
            sprintf(line, "/Date: %s | Lobby: %s min | Score: %s", row[0], row[1], row[2]);
            strcat(respuesta, line);
        }
    }
    mysql_free_result(res);
}

void GetLeaderboard(MYSQL *conn, char *respuesta) {
    char query[512] = "SELECT Player.Username, SUM(Participation.Score) AS TotalWins FROM Player INNER JOIN Participation ON Player.Id = Participation.PlayerId WHERE Player.Active=1 GROUP BY Player.Username ORDER BY TotalWins DESC LIMIT 10;";
    mysql_query(conn, query);
    MYSQL_RES *res = mysql_store_result(conn);
    MYSQL_ROW row;
    strcpy(respuesta, "4");
    while ((row = mysql_fetch_row(res)) != NULL) {
        char line[256];
        sprintf(line, "/%s: %s wins", row[0], row[1]);
        strcat(respuesta, line);
    }
    mysql_free_result(res);
}

void GetMatchResults(MYSQL *conn, int matchId, char *respuesta) {
    char query[512];
    sprintf(query, "SELECT Player.Username, Participation.Score FROM Player INNER JOIN Participation ON Player.Id = Participation.PlayerId WHERE Participation.MatchId = %d ORDER BY Participation.Score ASC;", matchId);
    mysql_query(conn, query);
    MYSQL_RES *res = mysql_store_result(conn);
    MYSQL_ROW row;
    strcpy(respuesta, "5");
    if (mysql_num_rows(res) == 0) strcat(respuesta, "/No players found");
    else {
        while ((row = mysql_fetch_row(res)) != NULL) {
            char line[256];
            sprintf(line, "/%s scored %s", row[0], row[1]);
            strcat(respuesta, line);
        }
    }
    mysql_free_result(res);
}

// ===========================================================================
// 2. THREAD FUNCTION
// ===========================================================================

void *AtenderCliente(void *socket) {
    int sock_conn = *(int *)socket;
    char peticion[1024];
    char requestBuffer[4096] = "";
    char respuesta[2048];
    int terminar = 0;
    
    MYSQL *conn = mysql_init(NULL);
    if (!mysql_real_connect(conn, "localhost", "root", "mysql", "UnoGameDB", 0, NULL, 0)) {
        printf("Database connection failed for socket %d\n", sock_conn);
        pthread_exit(NULL);
    }
    
    while (terminar == 0) {
        int ret = read(sock_conn, peticion, sizeof(peticion) - 1);
        if (ret <= 0) {
            terminar = 1;
            continue;
        }
        peticion[ret] = '\0';

        if (strlen(requestBuffer) + ret >= sizeof(requestBuffer) - 1) {
            requestBuffer[0] = '\0';
        }
        strncat(requestBuffer, peticion, ret);

        char *lineEnd;
        while ((lineEnd = strchr(requestBuffer, '\n')) != NULL) {
            char requestLine[1024];
            int lineLength = lineEnd - requestBuffer;
            if (lineLength >= sizeof(requestLine)) lineLength = sizeof(requestLine) - 1;
            strncpy(requestLine, requestBuffer, lineLength);
            requestLine[lineLength] = '\0';
            memmove(requestBuffer, lineEnd + 1, strlen(lineEnd + 1) + 1);
            requestLine[strcspn(requestLine, "\r\n")] = '\0';
            if (strlen(requestLine) == 0) continue;
        
        char *p = strtok(requestLine, "/");
        if (p == NULL) continue;
        int codigo = atoi(p);
        
        if (codigo == 0) terminar = 1;
        else if (codigo == 1) { // REGISTER
            char user[50], pass[50];
            strcpy(user, strtok(NULL, "/"));
            strcpy(pass, strtok(NULL, "/"));
            RegisterPlayer(conn, user, pass, respuesta);
            SendMessage(sock_conn, respuesta);
            printf("Register attempt: %s -> %s\n", user, respuesta);
        }
        else if (codigo == 2) { // LOGIN
            char user[50], pass[50];
            strcpy(user, strtok(NULL, "/"));
            strcpy(pass, strtok(NULL, "/"));
            LoginPlayer(conn, user, pass, respuesta);
            
            if (strstr(respuesta, "2/YES") != NULL) {
                pthread_mutex_lock(&mutex);
                if (FindUserByName(user) != -1 || FindUserBySocket(sock_conn) != -1) {
                    pthread_mutex_unlock(&mutex);
                    SendMessage(sock_conn, "2/ALREADY");
                    printf("Rejected duplicate login for '%s' on socket %d\n", user, sock_conn);
                    continue;
                }

                listaUsuarios[numUsuarios].socket = sock_conn;
                strcpy(listaUsuarios[numUsuarios].name, user);
                numUsuarios++;
                SendMessage(sock_conn, respuesta);
                printf("User '%s' logged in on socket %d\n", user, sock_conn);
                BroadcastConnectedUsers();
                pthread_mutex_unlock(&mutex);
            } else {
                SendMessage(sock_conn, respuesta);
            }
        }
        else if (codigo == 3) { // HISTORY
            char user[50];
            strcpy(user, strtok(NULL, "/"));
            GetMatchHistory(conn, user, respuesta);
            SendMessage(sock_conn, respuesta);
        }
        else if (codigo == 4) { // LEADERBOARD
            GetLeaderboard(conn, respuesta);
            SendMessage(sock_conn, respuesta);
        }
        else if (codigo == 5) { // MATCH RESULTS
            int mId = atoi(strtok(NULL, "/"));
            GetMatchResults(conn, mId, respuesta);
            SendMessage(sock_conn, respuesta);
        }
        else if (codigo == 7) { // SEND INVITATION
            char inviter[50];
            strcpy(inviter, strtok(NULL, "/"));
            
            pthread_mutex_lock(&mutex);
            if (FindUserByName(inviter) == -1 || GetSocketByName(inviter) != sock_conn || numInvitations >= 20) {
                pthread_mutex_unlock(&mutex);
                SendMessage(sock_conn, "10/0");
                continue;
            }

            int invIndex = numInvitations;
            strcpy(invitations[invIndex].inviter, inviter);
            invitations[invIndex].num_invitees = 0;
            invitations[invIndex].responses_received = 0;
            invitations[invIndex].all_accepted = 1;
            
            char *invitee;
            while ((invitee = strtok(NULL, "/")) != NULL) {
                invitee[strcspn(invitee, "\r\n")] = 0;
                if (strlen(invitee) == 0 || strcmp(invitee, inviter) == 0) continue;
                if (invitations[invIndex].num_invitees >= 4) continue;

                int alreadyAdded = 0;
                for (int k = 0; k < invitations[invIndex].num_invitees; k++) {
                    if (strcmp(invitations[invIndex].invitees[k], invitee) == 0) alreadyAdded = 1;
                }
                if (alreadyAdded) continue;

                int inviteeSocket = GetSocketByName(invitee);
                if (inviteeSocket != -1) {
                    strcpy(invitations[invIndex].invitees[invitations[invIndex].num_invitees], invitee);
                    invitations[invIndex].num_invitees++;

                    char inviteMsg[512];
                    sprintf(inviteMsg, "8/%s", inviter);
                    SendMessage(inviteeSocket, inviteMsg);
                }
            }

            if (invitations[invIndex].num_invitees > 0) {
                numInvitations++;
            } else {
                SendMessage(sock_conn, "10/0");
            }
            pthread_mutex_unlock(&mutex);
            printf("User '%s' sent game invites.\n", inviter);
        }
        else if (codigo == 9) { // RECEIVE INVITATION RESPONSE
            char inviter[50], responder[50];
            int answer;
            strcpy(inviter, strtok(NULL, "/"));
            strcpy(responder, strtok(NULL, "/"));
            answer = atoi(strtok(NULL, "/")); // 1 for Yes, 0 for No
            
            pthread_mutex_lock(&mutex);
            for (int j = 0; j < numInvitations; j++) {
                int responderFound = 0;
                for (int r = 0; r < invitations[j].num_invitees; r++) {
                    if (strcmp(invitations[j].invitees[r], responder) == 0) responderFound = 1;
                }

                if (strcmp(invitations[j].inviter, inviter) == 0 && responderFound && GetSocketByName(responder) == sock_conn) {
                    invitations[j].responses_received++;
                    if (answer == 0) {
                        invitations[j].all_accepted = 0;
                    }
                    
                    if (invitations[j].responses_received == invitations[j].num_invitees) {
                        if (invitations[j].all_accepted == 1) {
                            int gameId = numActiveGames;
                            activeGames[gameId].id = gameId;
                            activeGames[gameId].num_players = 0;
                            
                            int inviterSocket = GetSocketByName(inviter);
                            activeGames[gameId].players[activeGames[gameId].num_players++] = inviterSocket;
                            strcpy(activeGames[gameId].playerNames[activeGames[gameId].num_players - 1], inviter);
                            
                            for (int k = 0; k < invitations[j].num_invitees; k++) {
                                int inviteeSocket = GetSocketByName(invitations[j].invitees[k]);
                                activeGames[gameId].players[activeGames[gameId].num_players++] = inviteeSocket;
                                strcpy(activeGames[gameId].playerNames[activeGames[gameId].num_players - 1], invitations[j].invitees[k]);
                            }
                            numActiveGames++;
                            
                            char startMsg[512];
                            sprintf(startMsg, "10/1/%d", gameId); 
                            
                            for (int k = 0; k < activeGames[gameId].num_players; k++) {
                                SendMessage(activeGames[gameId].players[k], startMsg);
                            }
                            usleep(50000);
                            StartUnoGame(&activeGames[gameId]);
                            printf("Game Room %d Created!\n", gameId);
                        } else {
                            char failMsg[512];
                            sprintf(failMsg, "10/0");
                            
                            SendMessage(GetSocketByName(inviter), failMsg);
                            for (int k = 0; k < invitations[j].num_invitees; k++) {
                                SendMessage(GetSocketByName(invitations[j].invitees[k]), failMsg);
                            }
                            printf("Game invitation by '%s' was declined.\n", inviter);
                        }
                        RemoveInvitationAt(j);
                    }
                    break;
                }
            }
            pthread_mutex_unlock(&mutex);
        }
        else if (codigo == 11) { // CHAT MESSAGE
            int gameId = atoi(strtok(NULL, "/"));
            char *user = strtok(NULL, "/");
            char *chatMsg = strtok(NULL, "/");
            
            char broadcast[512];
            sprintf(broadcast, "11/%d/%s/%s", gameId, user, chatMsg);
            
            // Broadcast only to players in this specific Game Room
            for (int j = 0; j < activeGames[gameId].num_players; j++) {
                SendMessage(activeGames[gameId].players[j], broadcast);
            }
            printf("Chat in Room %d: %s -> %s\n", gameId, user, chatMsg);
        }
        else if (codigo == 13) { // PLAY UNO CARD
            int gameId = atoi(strtok(NULL, "/"));
            char *user = strtok(NULL, "/");
            int cardIndex = atoi(strtok(NULL, "/"));
            char *chosen = strtok(NULL, "/");
            char chosenColor = (chosen != NULL && strlen(chosen) > 0) ? chosen[0] : ' ';

            pthread_mutex_lock(&mutex);
            int gameIndex = FindGameById(gameId);
            if (gameIndex == -1) {
                SendRuleError(sock_conn, "Game room not found.");
            } else {
                int playerIndex = GetPlayerIndex(&activeGames[gameIndex], user);
                if (playerIndex == -1) SendRuleError(sock_conn, "Player not found in this game.");
                else PlayUnoCard(conn, &activeGames[gameIndex], playerIndex, cardIndex, chosenColor);
            }
            pthread_mutex_unlock(&mutex);
        }
        else if (codigo == 14) { // DRAW UNO CARD
            int gameId = atoi(strtok(NULL, "/"));
            char *user = strtok(NULL, "/");

            pthread_mutex_lock(&mutex);
            int gameIndex = FindGameById(gameId);
            if (gameIndex == -1) {
                SendRuleError(sock_conn, "Game room not found.");
            } else {
                int playerIndex = GetPlayerIndex(&activeGames[gameIndex], user);
                if (playerIndex == -1) SendRuleError(sock_conn, "Player not found in this game.");
                else DrawUnoCard(&activeGames[gameIndex], playerIndex);
            }
            pthread_mutex_unlock(&mutex);
        }
        else if (codigo == 19) { // DELETE ACCOUNT
            char user[50], pass[50];
            char *userTok = strtok(NULL, "/");
            char *passTok = strtok(NULL, "/");
            if (userTok == NULL || passTok == NULL) {
                SendMessage(sock_conn, "19/0/Missing username or password");
                continue;
            }

            strcpy(user, userTok);
            strcpy(pass, passTok);

            pthread_mutex_lock(&mutex);
            if (GetSocketByName(user) != sock_conn) {
                pthread_mutex_unlock(&mutex);
                SendMessage(sock_conn, "19/0/You can only delete the logged-in account");
                continue;
            }

            DeleteAccount(conn, user, pass, respuesta);
            int deleted = strstr(respuesta, "19/1/") == respuesta;
            if (deleted) {
                CleanupInvitationsForUser(user);
                int removeIndex = FindUserBySocket(sock_conn);
                if (removeIndex != -1) {
                    for (int j = removeIndex; j < numUsuarios - 1; j++) {
                        listaUsuarios[j] = listaUsuarios[j + 1];
                    }
                    numUsuarios--;
                }
            }
            pthread_mutex_unlock(&mutex);

            SendMessage(sock_conn, respuesta);
            if (deleted) {
                pthread_mutex_lock(&mutex);
                BroadcastConnectedUsers();
                pthread_mutex_unlock(&mutex);
                terminar = 1;
            }
        }

        if (terminar) break;
        }
    }

    // --- DISCONNECTION MANAGEMENT ---
    pthread_mutex_lock(&mutex); 
    int index_to_remove = FindUserBySocket(sock_conn);
    if (index_to_remove != -1) {
        char disconnectedUser[20];
        strcpy(disconnectedUser, listaUsuarios[index_to_remove].name);
        printf("User '%s' disconnected.\n", disconnectedUser);

        CleanupInvitationsForUser(disconnectedUser);
        NotifyGamesForDisconnectedUser(disconnectedUser);

        for (int j = index_to_remove; j < numUsuarios - 1; j++) {
            listaUsuarios[j] = listaUsuarios[j + 1];
        }
        numUsuarios--;
        BroadcastConnectedUsers();
    }
    pthread_mutex_unlock(&mutex);

    mysql_close(conn);
    close(sock_conn);
    pthread_exit(NULL);
}

int main(int argc, char *argv[]) {
    int sock_conn, sock_listen;
    struct sockaddr_in serv_adr;
    signal(SIGPIPE, SIG_IGN);
    srand(time(NULL));
    sock_listen = socket(AF_INET, SOCK_STREAM, 0);
    memset(&serv_adr, 0, sizeof(serv_adr));
    serv_adr.sin_family = AF_INET;
    serv_adr.sin_addr.s_addr = htonl(INADDR_ANY);
    serv_adr.sin_port = htons(9050); // <-- SET BACK TO 9050 FOR YOUR ENVIRONMENT!
    
    bind(sock_listen, (struct sockaddr *)&serv_adr, sizeof(serv_adr));
    listen(sock_listen, 5);
    
    printf("UNO Server Initialized! Listening on port 9050...\n"); // Updated log to match
    
    for (;;) {
        sock_conn = accept(sock_listen, NULL, NULL);
        printf("New Client Connected! Socket: %d\n", sock_conn);
        
        pthread_t thread;
        pthread_mutex_lock(&mutex);
        sockets[i] = sock_conn;
        pthread_create(&thread, NULL, AtenderCliente, &sockets[i]);
        i++;
        pthread_mutex_unlock(&mutex);
    }
}   
