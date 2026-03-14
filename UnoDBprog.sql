DROP DATABASE IF EXISTS UnoGameDB;
CREATE DATABASE UnoGameDB;
USE UnoGameDB;

CREATE TABLE Player (
					Id INTEGER PRIMARY KEY AUTO_INCREMENT,
					Username VARCHAR(50) NOT NULL UNIQUE,
					Password VARCHAR(255) NOT NULL
					);

CREATE TABLE Matchy (
					Id INTEGER PRIMARY KEY AUTO_INCREMENT,
					StartTime DATETIME NOT NULL,
					DurationMinutes INTEGER NOT NULL
					);
	
CREATE TABLE Participation (
							PlayerId INTEGER NOT NULL,
							MatchId INTEGER NOT NULL,
							Score INTEGER NOT NULL,
							FOREIGN KEY (PlayerId) REFERENCES Player(Id),
							FOREIGN KEY (MatchId) REFERENCES Matchy(Id),
							PRIMARY KEY (PlayerId, MatchId)
							);

INSERT INTO Player (Username, Password) VALUES ('Scott', 'pass123');
INSERT INTO Player (Username, Password) VALUES ('Mattr', 'securepwd');
INSERT INTO Player (Username, Password) VALUES ('Zucca', 'qwerty');
INSERT INTO Player (Username, Password) VALUES ('Ayoub', 'password123');

-- Match 1 played on March 8th, lasted 25 minutes
INSERT INTO Matchy (StartTime, DurationMinutes) VALUES ('2026-03-08 14:30:00', 25);
-- Match 2 played on March 8th, lasted 40 minutes
INSERT INTO Matchy (StartTime, DurationMinutes) VALUES ('2026-03-08 16:00:00', 40);

-- Match 1
INSERT INTO Participation (PlayerId, MatchId, Score) VALUES (1, 1, 0);   
INSERT INTO Participation (PlayerId, MatchId, Score) VALUES (2, 1, 15);  
INSERT INTO Participation (PlayerId, MatchId, Score) VALUES (3, 1, 42);  
INSERT INTO Participation (PlayerId, MatchId, Score) VALUES (4, 1, 12);

-- Match 2
INSERT INTO Participation (PlayerId, MatchId, Score) VALUES (1, 2, 50); 
INSERT INTO Participation (PlayerId, MatchId, Score) VALUES (2, 2, 22);  
INSERT INTO Participation (PlayerId, MatchId, Score) VALUES (4, 2, 0);
