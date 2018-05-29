-- phpMyAdmin SQL Dump
-- version 4.2.12deb2+deb8u2
-- http://www.phpmyadmin.net
--
-- Host: localhost
-- Generation Time: May 28, 2018 at 08:48 PM
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

-- --------------------------------------------------------

--
-- Table structure for table `Cave`
--

CREATE TABLE IF NOT EXISTS `Cave` (
`CaveId` int(11) NOT NULL,
  `Name` varchar(128) NOT NULL,
  `Description` mediumtext NOT NULL,
  `LocationId` int(11) DEFAULT NULL,
  `IsDeleted` tinyint(1) NOT NULL DEFAULT '0',
  `DateDeleted` datetime DEFAULT NULL,
  `CreatedDate` date NOT NULL,
  `Saved` tinyint(1) NOT NULL DEFAULT '0'
) ENGINE=InnoDB AUTO_INCREMENT=82 DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Table structure for table `CaveData`
--

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

CREATE TABLE IF NOT EXISTS `CaveMedia` (
  `CaveId` int(11) NOT NULL,
  `MediaId` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Table structure for table `CaveUser`
--

CREATE TABLE IF NOT EXISTS `CaveUser` (
  `CaveId` int(11) NOT NULL,
  `UserId` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Table structure for table `DataTemplateItem`
--

CREATE TABLE IF NOT EXISTS `DataTemplateItem` (
  `DataTemplateId` int(11) NOT NULL,
  `Name` text NOT NULL,
  `Description` text NOT NULL,
  `MetaData` text NOT NULL,
  `Type` text NOT NULL,
  `DefaultValue` text NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Table structure for table `DataTemplates`
--

CREATE TABLE IF NOT EXISTS `DataTemplates` (
  `DataTemplateId` int(11) NOT NULL,
  `Name` int(11) NOT NULL,
  `Description` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Table structure for table `Global`
--

CREATE TABLE IF NOT EXISTS `Global` (
  `Key` varchar(128) NOT NULL,
  `Value` text NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Table structure for table `History`
--

CREATE TABLE IF NOT EXISTS `History` (
`HistoryId` int(11) NOT NULL,
  `UserId` int(11) DEFAULT NULL,
  `CaveId` int(11) DEFAULT NULL,
  `SurveyId` int(11) DEFAULT NULL,
  `MediaId` int(11) DEFAULT NULL,
  `EventDateTime` datetime NOT NULL,
  `Description` text NOT NULL,
  `Data` mediumtext NOT NULL
) ENGINE=InnoDB AUTO_INCREMENT=151 DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Table structure for table `Media`
--

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
-- Table structure for table `Project`
--

CREATE TABLE IF NOT EXISTS `Project` (
  `ProjectId` int(11) NOT NULL,
  `Name` text NOT NULL,
  `Description` text NOT NULL,
  `IsPublic` tinyint(1) NOT NULL DEFAULT '0',
  `IsActive` tinyint(1) NOT NULL DEFAULT '1'
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Table structure for table `ProjectCave`
--

CREATE TABLE IF NOT EXISTS `ProjectCave` (
  `ProjectId` int(11) NOT NULL,
  `CaveId` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Table structure for table `ProjectData`
--

CREATE TABLE IF NOT EXISTS `ProjectData` (
  `ProjectId` int(11) NOT NULL,
  `Name` varchar(128) NOT NULL,
  `Type` varchar(128) NOT NULL,
  `MetaData` text NOT NULL,
  `Value` text NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Table structure for table `ProjectMedia`
--

CREATE TABLE IF NOT EXISTS `ProjectMedia` (
  `ProjectId` int(11) NOT NULL,
  `MediaId` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Table structure for table `ProjectUser`
--

CREATE TABLE IF NOT EXISTS `ProjectUser` (
  `ProjectId` int(11) NOT NULL,
  `UserId` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Table structure for table `User`
--

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
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Table structure for table `UserData`
--

CREATE TABLE IF NOT EXISTS `UserData` (
  `UserId` int(11) NOT NULL,
  `Name` varchar(128) NOT NULL,
  `Type` varchar(128) NOT NULL,
  `MetaData` text NOT NULL,
  `Value` text NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Table structure for table `UserSession`
--

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
-- Indexes for table `Project`
--
ALTER TABLE `Project`
 ADD PRIMARY KEY (`ProjectId`);

--
-- Indexes for table `ProjectCave`
--
ALTER TABLE `ProjectCave`
 ADD KEY `CaveId` (`CaveId`), ADD KEY `ProjectId` (`ProjectId`);

--
-- Indexes for table `ProjectData`
--
ALTER TABLE `ProjectData`
 ADD KEY `ProjectId` (`ProjectId`);

--
-- Indexes for table `ProjectMedia`
--
ALTER TABLE `ProjectMedia`
 ADD KEY `ProjectId` (`ProjectId`), ADD KEY `MediaId` (`MediaId`);

--
-- Indexes for table `ProjectUser`
--
ALTER TABLE `ProjectUser`
 ADD KEY `ProjectId` (`ProjectId`), ADD KEY `UserId` (`UserId`);

--
-- Indexes for table `User`
--
ALTER TABLE `User`
 ADD PRIMARY KEY (`UserId`);

--
-- Indexes for table `UserData`
--
ALTER TABLE `UserData`
 ADD KEY `UserId` (`UserId`);

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
MODIFY `CaveId` int(11) NOT NULL AUTO_INCREMENT,AUTO_INCREMENT=82;
--
-- AUTO_INCREMENT for table `History`
--
ALTER TABLE `History`
MODIFY `HistoryId` int(11) NOT NULL AUTO_INCREMENT,AUTO_INCREMENT=151;
--
-- AUTO_INCREMENT for table `Media`
--
ALTER TABLE `Media`
MODIFY `MediaId` int(11) NOT NULL AUTO_INCREMENT;
--
-- AUTO_INCREMENT for table `User`
--
ALTER TABLE `User`
MODIFY `UserId` int(11) NOT NULL AUTO_INCREMENT,AUTO_INCREMENT=7;
--
-- Constraints for dumped tables
--

--
-- Constraints for table `CaveData`
--
ALTER TABLE `CaveData`
ADD CONSTRAINT `CaveData_ibfk_1` FOREIGN KEY (`CaveId`) REFERENCES `Cave` (`CaveId`) ON DELETE CASCADE;

--
-- Constraints for table `CaveLocation`
--
ALTER TABLE `CaveLocation`
ADD CONSTRAINT `CaveLocation_ibfk_1` FOREIGN KEY (`CaveId`) REFERENCES `Cave` (`CaveId`) ON DELETE CASCADE;

--
-- Constraints for table `CaveMedia`
--
ALTER TABLE `CaveMedia`
ADD CONSTRAINT `CaveMedia_ibfk_2` FOREIGN KEY (`MediaId`) REFERENCES `Media` (`MediaId`) ON DELETE CASCADE,
ADD CONSTRAINT `CaveMedia_ibfk_1` FOREIGN KEY (`CaveId`) REFERENCES `Cave` (`CaveId`) ON DELETE CASCADE;

--
-- Constraints for table `CaveUser`
--
ALTER TABLE `CaveUser`
ADD CONSTRAINT `CaveUser_ibfk_2` FOREIGN KEY (`UserId`) REFERENCES `User` (`UserId`) ON DELETE CASCADE,
ADD CONSTRAINT `CaveUser_ibfk_1` FOREIGN KEY (`CaveId`) REFERENCES `Cave` (`CaveId`) ON DELETE CASCADE;

--
-- Constraints for table `ProjectCave`
--
ALTER TABLE `ProjectCave`
ADD CONSTRAINT `ProjectCave_ibfk_2` FOREIGN KEY (`ProjectId`) REFERENCES `Project` (`ProjectId`) ON DELETE CASCADE,
ADD CONSTRAINT `ProjectCave_ibfk_1` FOREIGN KEY (`CaveId`) REFERENCES `Cave` (`CaveId`) ON DELETE CASCADE;

--
-- Constraints for table `ProjectData`
--
ALTER TABLE `ProjectData`
ADD CONSTRAINT `ProjectData_ibfk_1` FOREIGN KEY (`ProjectId`) REFERENCES `Project` (`ProjectId`) ON DELETE CASCADE;

--
-- Constraints for table `ProjectMedia`
--
ALTER TABLE `ProjectMedia`
ADD CONSTRAINT `ProjectMedia_ibfk_2` FOREIGN KEY (`MediaId`) REFERENCES `Media` (`MediaId`) ON DELETE CASCADE,
ADD CONSTRAINT `ProjectMedia_ibfk_1` FOREIGN KEY (`ProjectId`) REFERENCES `Project` (`ProjectId`) ON DELETE CASCADE;

--
-- Constraints for table `ProjectUser`
--
ALTER TABLE `ProjectUser`
ADD CONSTRAINT `ProjectUser_ibfk_2` FOREIGN KEY (`UserId`) REFERENCES `User` (`UserId`) ON DELETE CASCADE,
ADD CONSTRAINT `ProjectUser_ibfk_1` FOREIGN KEY (`ProjectId`) REFERENCES `Project` (`ProjectId`) ON DELETE CASCADE;

--
-- Constraints for table `UserData`
--
ALTER TABLE `UserData`
ADD CONSTRAINT `UserData_ibfk_1` FOREIGN KEY (`UserId`) REFERENCES `User` (`UserId`) ON DELETE CASCADE;

--
-- Constraints for table `UserSession`
--
ALTER TABLE `UserSession`
ADD CONSTRAINT `UserSession_ibfk_1` FOREIGN KEY (`UserId`) REFERENCES `User` (`UserId`) ON DELETE CASCADE;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
