﻿CREATE TABLE IF NOT EXISTS `Username_Counter` ( `Id` integer PRIMARY KEY AUTO_INCREMENT, `Username` varchar(32), `Amount` int, `Created_At` DateTime, `Updated_At` DateTime);
CREATE TABLE IF NOT EXISTS `File_Type` (`Id` integer Primary Key AUTO_INCREMENT, `Type` varchar(32))
CREATE TABLE IF NOT EXISTS `File` ( `Id` integer PRIMARY KEY AUTO_INCREMENT, `File_Name` varchar(128), `File_location` varchar(256), `file_type_Id` integer, `Created_At` DateTime, `Deleted_At` DateTime, FOREIGN KEY (`file_type_Id`) REFERENCES `File_Type` (`Id`));
CREATE TABLE IF NOT EXISTS `Role`( `Id` integer PRIMARY KEY AUTO_INCREMENT, `Name` varchar(16), `Description` varchar(128));
CREATE TABLE IF NOT EXISTS `Users_Private` ( `Id` integer PRIMARY KEY AUTO_INCREMENT, `Role_Id` integer, `Email` varchar(64), `Password` varchar(128), `Salt` varchar(64), FOREIGN KEY (`Role_Id`) REFERENCES `Role` (`Id`));
CREATE TABLE IF NOT EXISTS `Users_Public` ( `Id` integer PRIMARY KEY AUTO_INCREMENT, `Person_Id` integer, `Username` varchar(32), `Profile_Picture` integer, `Created_At` DateTime, `Updated_At` DateTime, `Deleted_At` DateTime, AllowedUser tinyint(1), FOREIGN KEY (`Person_Id`) REFERENCES `Users_Private` (`Id`), FOREIGN KEY (`Profile_Picture`) REFERENCES `File` (`Id`));
CREATE TABLE IF NOT EXISTS `Following` ( `User_Id_Followed` integer,`User_Id_Follower` integer, `Followed_At` DateTime, FOREIGN KEY (`User_Id_Follower`) REFERENCES `Users_Public` (`Id`), FOREIGN KEY (`User_Id_Followed`) REFERENCES `Users_Public` (`Id`));
CREATE TABLE IF NOT EXISTS `Content` ( `Id` integer PRIMARY KEY AUTO_INCREMENT, `User_Id` integer, `File_Id` integer, `Title` varchar(32), `Type` varchar(3), `Description` varchar(128), `Thumbnail` integer, `Created_At` DateTime, `Updated_At` DateTime, `Deleted_At` DateTime, FOREIGN KEY (`File_Id`) REFERENCES `File` (`Id`), FOREIGN KEY (`User_Id`) REFERENCES `Users_Public` (`Id`), FOREIGN KEY (`Thumbnail`) REFERENCES `File` (`Id`));
CREATE TABLE IF NOT EXISTS `Reaction` ( `Id` integer PRIMARY KEY AUTO_INCREMENT, `User_Id` integer, `Content_Id` integer, `Content_Type` varchar(10), `Reaction` Text, `Created_At` DateTime, `Updated_At` DateTime, FOREIGN KEY (`User_Id`) REFERENCES `Users_Public` (`Id`));
CREATE TABLE IF NOT EXISTS `Report_User`( `Id` integer PRIMARY KEY AUTO_INCREMENT, `User_Reported_Id` integer, `User_Reporter_Id` integer, `Reason` varchar(512), `Reported_At` DateTime, FOREIGN KEY (`User_Reporter_Id`) REFERENCES `Users_Public` (`Id`),FOREIGN KEY (`User_Reported_Id`) REFERENCES `Users_Public` (`Id`));
INSERT IGNORE INTO `Role` (Id, Name, Description) VALUES (1, "User", "Someone who has an account on the site");
INSERT IGNORE INTO `Role` (Id, Name, Description) VALUES (2, "Creator", "Someone who Creates content");
INSERT IGNORE INTO `Role` (Id, Name, Description) VALUES (3, "Admin", "Someone who has unlimited power");
INSERT IGNORE INTO `File_Type` (Id, Type) VALUES (1, "Post image");
INSERT IGNORE INTO `File_Type` (Id, Type) VALUES (2, "Thumbnail");
INSERT IGNORE INTO `File_Type` (Id, Type) VALUES (3, "Video");
INSERT IGNORE INTO `File_Type` (Id, Type) VALUES (4, "Profile picture");
