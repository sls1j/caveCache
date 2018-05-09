-- phpMyAdmin SQL Dump
-- version 4.2.12deb2+deb8u2
-- http://www.phpmyadmin.net
--
-- Host: localhost
-- Generation Time: May 08, 2018 at 06:43 PM
-- Server version: 5.5.58-0+deb8u1
-- PHP Version: 5.6.30-0+deb8u1

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;

--
-- Database: `caveCache`
--
CREATE DATABASE IF NOT EXISTS `caveCache` DEFAULT CHARACTER SET utf8 COLLATE utf8_general_ci;
USE `caveCache`;

-- --------------------------------------------------------

--
-- Table structure for table `Cave`
--

DROP TABLE IF EXISTS `Cave`;
CREATE TABLE IF NOT EXISTS `Cave` (
`CaveId` int(11) NOT NULL,
  `Name` varchar(128) NOT NULL,
  `Description` mediumtext NOT NULL,
  `LocationId` int(11) DEFAULT NULL,
  `IsDeleted` tinyint(1) NOT NULL DEFAULT '0',
  `DateDeleted` datetime DEFAULT NULL,
  `CreatedDate` date NOT NULL,
  `Saved` tinyint(1) NOT NULL DEFAULT '0'
) ENGINE=InnoDB AUTO_INCREMENT=62 DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Table structure for table `CaveData`
--

DROP TABLE IF EXISTS `CaveData`;
CREATE TABLE IF NOT EXISTS `CaveData` (
  `CaveId` int(11) NOT NULL,
  `Name` varchar(128) NOT NULL,
  `Type` varchar(128) NOT NULL,
  `MetaData` text NOT NULL,
  `Value` text NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Table structure for table `CaveLocation`
--

DROP TABLE IF EXISTS `CaveLocation`;
CREATE TABLE IF NOT EXISTS `CaveLocation` (
  `LocationId` int(11) NOT NULL,
  `CaveId` int(11) NOT NULL,
  `CaptureDate` datetime DEFAULT NULL,
  `Latitude` decimal(11,8) NOT NULL,
  `Longitude` decimal(11,8) NOT NULL,
  `Altitude` decimal(11,2) DEFAULT NULL,
  `Accuracy` decimal(6,0) DEFAULT NULL,
  `AltitudeAccuracy` decimal(6,0) DEFAULT NULL,
  `Unit` enum('Emperial','Metric') NOT NULL,
  `Source` text NOT NULL,
  `Notes` mediumtext NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Table structure for table `CaveMedia`
--

DROP TABLE IF EXISTS `CaveMedia`;
CREATE TABLE IF NOT EXISTS `CaveMedia` (
  `CaveId` int(11) NOT NULL,
  `MediaId` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Table structure for table `CaveUser`
--

DROP TABLE IF EXISTS `CaveUser`;
CREATE TABLE IF NOT EXISTS `CaveUser` (
  `CaveId` int(11) NOT NULL,
  `UserId` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Table structure for table `Global`
--

DROP TABLE IF EXISTS `Global`;
CREATE TABLE IF NOT EXISTS `Global` (
  `Key` varchar(128) NOT NULL,
  `Value` text NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Table structure for table `History`
--

DROP TABLE IF EXISTS `History`;
CREATE TABLE IF NOT EXISTS `History` (
`HistoryId` int(11) NOT NULL,
  `UserId` int(11) DEFAULT NULL,
  `CaveId` int(11) DEFAULT NULL,
  `SurveyId` int(11) DEFAULT NULL,
  `MediaId` int(11) DEFAULT NULL,
  `EventDateTime` datetime NOT NULL,
  `Description` text NOT NULL,
  `Data` mediumtext NOT NULL
) ENGINE=InnoDB AUTO_INCREMENT=63 DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Table structure for table `Media`
--

DROP TABLE IF EXISTS `Media`;
CREATE TABLE IF NOT EXISTS `Media` (
`MediaId` int(11) NOT NULL,
  `Name` varchar(128) NOT NULL,
  `Description` mediumtext NOT NULL,
  `FileName` varchar(1024) NOT NULL,
  `MimeType` varchar(256) NOT NULL,
  `FileSize` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Table structure for table `MediaBody`
--

DROP TABLE IF EXISTS `MediaBody`;
CREATE TABLE IF NOT EXISTS `MediaBody` (
  `MediaId` int(11) NOT NULL,
  `Body` mediumblob NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Table structure for table `MediaSetSessions`
--

DROP TABLE IF EXISTS `MediaSetSessions`;
CREATE TABLE IF NOT EXISTS `MediaSetSessions` (
  `MediaId` int(11) NOT NULL,
  `SessionId` varchar(24) NOT NULL,
  `ExpireTime` datetime NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Table structure for table `User`
--

DROP TABLE IF EXISTS `User`;
CREATE TABLE IF NOT EXISTS `User` (
`UserId` int(11) NOT NULL,
  `Name` varchar(128) NOT NULL,
  `Email` varchar(256) NOT NULL,
  `Created` datetime NOT NULL,
  `LastLoggedIn` datetime DEFAULT NULL,
  `Expire` datetime DEFAULT NULL,
  `Profile` text NOT NULL,
  `Permissions` tinytext NOT NULL,
  `PasswordSalt` text NOT NULL,
  `PasswordHash` text NOT NULL
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Table structure for table `UserData`
--

DROP TABLE IF EXISTS `UserData`;
CREATE TABLE IF NOT EXISTS `UserData` (
`DataId` int(11) NOT NULL,
  `UserId` int(11) NOT NULL,
  `Name` varchar(64) NOT NULL,
  `Value` tinytext NOT NULL,
  `Type` tinytext NOT NULL,
  `MetaData` text NOT NULL
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Table structure for table `UserMedia`
--

DROP TABLE IF EXISTS `UserMedia`;
CREATE TABLE IF NOT EXISTS `UserMedia` (
  `UserId` int(11) NOT NULL,
  `MediaId` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Table structure for table `UserSession`
--

DROP TABLE IF EXISTS `UserSession`;
CREATE TABLE IF NOT EXISTS `UserSession` (
  `UserId` int(11) NOT NULL,
  `SessionId` varchar(24) NOT NULL,
  `Timeout` datetime NOT NULL,
  `IsCommandLine` tinyint(1) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Indexes for dumped tables
--

--
-- Indexes for table `Cave`
--
ALTER TABLE `Cave`
 ADD PRIMARY KEY (`CaveId`), ADD KEY `LocationId` (`LocationId`);

--
-- Indexes for table `CaveData`
--
ALTER TABLE `CaveData`
 ADD PRIMARY KEY (`CaveId`,`Name`), ADD UNIQUE KEY `CaveId_2` (`CaveId`,`Name`), ADD KEY `CaveId` (`CaveId`);

--
-- Indexes for table `CaveLocation`
--
ALTER TABLE `CaveLocation`
 ADD PRIMARY KEY (`LocationId`,`CaveId`), ADD KEY `CaveId` (`CaveId`);

--
-- Indexes for table `CaveMedia`
--
ALTER TABLE `CaveMedia`
 ADD PRIMARY KEY (`CaveId`,`MediaId`), ADD KEY `MediaId` (`MediaId`);

--
-- Indexes for table `CaveUser`
--
ALTER TABLE `CaveUser`
 ADD KEY `CaveId` (`CaveId`,`UserId`), ADD KEY `UserId` (`UserId`);

--
-- Indexes for table `Global`
--
ALTER TABLE `Global`
 ADD PRIMARY KEY (`Key`);

--
-- Indexes for table `History`
--
ALTER TABLE `History`
 ADD PRIMARY KEY (`HistoryId`), ADD KEY `UserId` (`UserId`,`CaveId`,`SurveyId`,`MediaId`);

--
-- Indexes for table `Media`
--
ALTER TABLE `Media`
 ADD PRIMARY KEY (`MediaId`);

--
-- Indexes for table `MediaBody`
--
ALTER TABLE `MediaBody`
 ADD PRIMARY KEY (`MediaId`);

--
-- Indexes for table `MediaSetSessions`
--
ALTER TABLE `MediaSetSessions`
 ADD PRIMARY KEY (`MediaId`);

--
-- Indexes for table `User`
--
ALTER TABLE `User`
 ADD PRIMARY KEY (`UserId`);

--
-- Indexes for table `UserData`
--
ALTER TABLE `UserData`
 ADD PRIMARY KEY (`DataId`), ADD UNIQUE KEY `UserId_2` (`UserId`,`Name`), ADD KEY `UserId` (`UserId`);

--
-- Indexes for table `UserMedia`
--
ALTER TABLE `UserMedia`
 ADD PRIMARY KEY (`UserId`,`MediaId`), ADD KEY `MediaId` (`MediaId`);

--
-- Indexes for table `UserSession`
--
ALTER TABLE `UserSession`
 ADD PRIMARY KEY (`SessionId`), ADD KEY `UserId` (`UserId`);

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table `Cave`
--
ALTER TABLE `Cave`
MODIFY `CaveId` int(11) NOT NULL AUTO_INCREMENT,AUTO_INCREMENT=62;
--
-- AUTO_INCREMENT for table `History`
--
ALTER TABLE `History`
MODIFY `HistoryId` int(11) NOT NULL AUTO_INCREMENT,AUTO_INCREMENT=63;
--
-- AUTO_INCREMENT for table `Media`
--
ALTER TABLE `Media`
MODIFY `MediaId` int(11) NOT NULL AUTO_INCREMENT;
--
-- AUTO_INCREMENT for table `User`
--
ALTER TABLE `User`
MODIFY `UserId` int(11) NOT NULL AUTO_INCREMENT,AUTO_INCREMENT=5;
--
-- AUTO_INCREMENT for table `UserData`
--
ALTER TABLE `UserData`
MODIFY `DataId` int(11) NOT NULL AUTO_INCREMENT,AUTO_INCREMENT=6;
--
-- Constraints for dumped tables
--

--
-- Constraints for table `CaveData`
--
ALTER TABLE `CaveData`
ADD CONSTRAINT `CaveData_ibfk_1` FOREIGN KEY (`CaveId`) REFERENCES `Cave` (`CaveId`);

--
-- Constraints for table `CaveLocation`
--
ALTER TABLE `CaveLocation`
ADD CONSTRAINT `CaveLocation_ibfk_1` FOREIGN KEY (`CaveId`) REFERENCES `Cave` (`CaveId`);

--
-- Constraints for table `CaveMedia`
--
ALTER TABLE `CaveMedia`
ADD CONSTRAINT `CaveMedia_ibfk_2` FOREIGN KEY (`MediaId`) REFERENCES `Media` (`MediaId`),
ADD CONSTRAINT `CaveMedia_ibfk_1` FOREIGN KEY (`CaveId`) REFERENCES `Cave` (`CaveId`);

--
-- Constraints for table `CaveUser`
--
ALTER TABLE `CaveUser`
ADD CONSTRAINT `CaveUser_ibfk_2` FOREIGN KEY (`UserId`) REFERENCES `User` (`UserId`),
ADD CONSTRAINT `CaveUser_ibfk_1` FOREIGN KEY (`CaveId`) REFERENCES `Cave` (`CaveId`);

--
-- Constraints for table `MediaBody`
--
ALTER TABLE `MediaBody`
ADD CONSTRAINT `MediaBody_ibfk_1` FOREIGN KEY (`MediaId`) REFERENCES `Media` (`MediaId`);

--
-- Constraints for table `MediaSetSessions`
--
ALTER TABLE `MediaSetSessions`
ADD CONSTRAINT `MediaSetSessions_ibfk_1` FOREIGN KEY (`MediaId`) REFERENCES `Media` (`MediaId`);

--
-- Constraints for table `UserData`
--
ALTER TABLE `UserData`
ADD CONSTRAINT `UserData_ibfk_1` FOREIGN KEY (`UserId`) REFERENCES `User` (`UserId`);

--
-- Constraints for table `UserMedia`
--
ALTER TABLE `UserMedia`
ADD CONSTRAINT `UserMedia_ibfk_2` FOREIGN KEY (`MediaId`) REFERENCES `Media` (`MediaId`),
ADD CONSTRAINT `UserMedia_ibfk_1` FOREIGN KEY (`UserId`) REFERENCES `User` (`UserId`);

--
-- Constraints for table `UserSession`
--
ALTER TABLE `UserSession`
ADD CONSTRAINT `UserSession_ibfk_1` FOREIGN KEY (`UserId`) REFERENCES `User` (`UserId`);

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
