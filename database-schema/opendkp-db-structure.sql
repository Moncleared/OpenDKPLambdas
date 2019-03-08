-- MySQL dump 10.13  Distrib 5.7.17, for Win64 (x86_64)
-- ------------------------------------------------------
-- Server version	5.6.39-log

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `adjustments`
--

DROP TABLE IF EXISTS `adjustments`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `adjustments` (
  `clientId` varchar(255) NOT NULL,
  `id_adjustment` int(11) NOT NULL AUTO_INCREMENT,
  `id_character` int(11) NOT NULL,
  `name` varchar(45) NOT NULL,
  `description` varchar(45) DEFAULT NULL,
  `value` double NOT NULL DEFAULT '0',
  `timestamp` datetime NOT NULL,
  PRIMARY KEY (`id_adjustment`,`clientId`),
  UNIQUE KEY `id_adjustment_UNIQUE` (`id_adjustment`),
  KEY `fk_adjust_char_id_idx` (`id_character`),
  KEY `fkey_adjust_clientid_idx` (`clientId`),
  CONSTRAINT `fkey_adjust_char_id` FOREIGN KEY (`id_character`) REFERENCES `characters` (`id_character`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fkey_adjust_clientid` FOREIGN KEY (`clientId`) REFERENCES `clients` (`clientId`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=3533 DEFAULT CHARSET=latin1 COMMENT='a table to store manual dkp adjustments';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `admin_settings`
--

DROP TABLE IF EXISTS `admin_settings`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `admin_settings` (
  `clientId` varchar(255) NOT NULL,
  `setting_name` varchar(45) NOT NULL,
  `setting_value` longtext NOT NULL,
  `updated_by` varchar(45) NOT NULL,
  `updated_timestamp` datetime NOT NULL,
  PRIMARY KEY (`setting_name`,`clientId`),
  UNIQUE KEY `setting_name_UNIQUE` (`setting_name`),
  KEY `fkey_admin_clientId_idx` (`clientId`),
  CONSTRAINT `fkey_admin_clientId` FOREIGN KEY (`clientId`) REFERENCES `clients` (`clientId`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COMMENT='table holds data related to admin settings';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `audit`
--

DROP TABLE IF EXISTS `audit`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `audit` (
  `clientId` varchar(255) NOT NULL,
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `CognitoUser` varchar(45) NOT NULL,
  `Timestamp` datetime NOT NULL,
  `Action` varchar(45) NOT NULL,
  `OldValue` longtext,
  `NewValue` longtext,
  PRIMARY KEY (`id`,`clientId`),
  UNIQUE KEY `id_UNIQUE` (`id`),
  KEY `fkey_audit_clientid_idx` (`clientId`),
  CONSTRAINT `fkey_audit_clientid` FOREIGN KEY (`clientId`) REFERENCES `clients` (`clientId`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=117 DEFAULT CHARSET=latin1 COMMENT='a table to track changes for audit review';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `cache`
--

DROP TABLE IF EXISTS `cache`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `cache` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `clientId` varchar(255) NOT NULL,
  `cache_name` varchar(45) NOT NULL,
  `cache_value` longtext NOT NULL,
  `cache_expires` datetime NOT NULL,
  `cache_updated` datetime NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `id_UNIQUE` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=17 DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `characters`
--

DROP TABLE IF EXISTS `characters`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `characters` (
  `clientId` varchar(256) NOT NULL,
  `id_character` int(11) NOT NULL AUTO_INCREMENT,
  `id_associated` int(11) NOT NULL DEFAULT '-1',
  `active` tinyint(4) NOT NULL DEFAULT '1',
  `name` varchar(45) NOT NULL,
  `rank` varchar(45) DEFAULT NULL,
  `class` varchar(45) DEFAULT NULL,
  `level` int(11) DEFAULT NULL,
  `race` varchar(45) DEFAULT NULL,
  `gender` varchar(45) DEFAULT NULL,
  `guild` varchar(45) DEFAULT NULL,
  PRIMARY KEY (`id_character`,`clientId`),
  UNIQUE KEY `id_UNIQUE` (`id_character`),
  UNIQUE KEY `name_UNIQUE` (`name`),
  KEY `fkey_clientid_idx` (`clientId`),
  CONSTRAINT `fkey_clientid` FOREIGN KEY (`clientId`) REFERENCES `clients` (`clientId`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=1881 DEFAULT CHARSET=latin1 COMMENT='a table used to store character data';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `clients`
--

DROP TABLE IF EXISTS `clients`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `clients` (
  `clientId` varchar(256) NOT NULL,
  `name` varchar(256) NOT NULL,
  `subdomain` varchar(256) NOT NULL,
  `identity` varchar(256) NOT NULL,
  `userPool` varchar(256) NOT NULL,
  `webClientId` varchar(256) NOT NULL,
  `assumedRole` varchar(256) DEFAULT NULL,
  `website` varchar(256) DEFAULT NULL,
  `forums` varchar(256) DEFAULT NULL,
  PRIMARY KEY (`clientId`),
  UNIQUE KEY `clientId_UNIQUE` (`clientId`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `items`
--

DROP TABLE IF EXISTS `items`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `items` (
  `id_item` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(45) NOT NULL,
  `lucylink` varchar(45) DEFAULT NULL,
  PRIMARY KEY (`id_item`)
) ENGINE=InnoDB AUTO_INCREMENT=158004 DEFAULT CHARSET=latin1 COMMENT='a table to track dkp items';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `items_x_characters`
--

DROP TABLE IF EXISTS `items_x_characters`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `items_x_characters` (
  `transaction_id` int(11) NOT NULL AUTO_INCREMENT,
  `clientId` varchar(255) NOT NULL,
  `character_id` int(11) NOT NULL,
  `item_id` int(11) NOT NULL,
  `raid_id` int(11) NOT NULL DEFAULT '-1',
  `dkp` double NOT NULL DEFAULT '0',
  PRIMARY KEY (`transaction_id`,`clientId`,`character_id`,`item_id`,`raid_id`),
  UNIQUE KEY `transaction_id_UNIQUE` (`transaction_id`),
  KEY `fkey_items_character_idx` (`character_id`),
  KEY `fkey_items_item_idx` (`item_id`),
  KEY `fkey_items_raid_id_idx` (`raid_id`),
  KEY `fkey_items_clientId_idx` (`clientId`),
  CONSTRAINT `fkey_items_character` FOREIGN KEY (`character_id`) REFERENCES `characters` (`id_character`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fkey_items_clientId` FOREIGN KEY (`clientId`) REFERENCES `clients` (`clientId`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fkey_items_item` FOREIGN KEY (`item_id`) REFERENCES `items` (`id_item`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fkey_items_raid_id` FOREIGN KEY (`raid_id`) REFERENCES `raids` (`id_raid`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=28518 DEFAULT CHARSET=latin1 COMMENT='a table to map character and items';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `pools`
--

DROP TABLE IF EXISTS `pools`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `pools` (
  `id_pool` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(45) NOT NULL,
  `description` varchar(45) DEFAULT NULL,
  `order` int(11) NOT NULL DEFAULT '99',
  PRIMARY KEY (`id_pool`),
  UNIQUE KEY `id_UNIQUE` (`id_pool`)
) ENGINE=InnoDB AUTO_INCREMENT=21 DEFAULT CHARSET=latin1 COMMENT='a table to define individual dkp pools';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `raids`
--

DROP TABLE IF EXISTS `raids`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `raids` (
  `clientId` varchar(255) NOT NULL,
  `id_raid` int(11) NOT NULL AUTO_INCREMENT,
  `id_pool` int(11) NOT NULL,
  `name` varchar(45) NOT NULL,
  `timestamp` datetime NOT NULL,
  `updated_by` varchar(45) DEFAULT 'system',
  `updated_timestamp` datetime DEFAULT NULL,
  PRIMARY KEY (`id_raid`,`clientId`),
  UNIQUE KEY `id_raid_UNIQUE` (`id_raid`),
  KEY `fkey_raids_pool_id_idx` (`id_pool`),
  KEY `fkey_raids_clientid_idx` (`clientId`),
  CONSTRAINT `fkey_raids_clientid` FOREIGN KEY (`clientId`) REFERENCES `clients` (`clientId`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fkey_raids_pool_id` FOREIGN KEY (`id_pool`) REFERENCES `pools` (`id_pool`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=902 DEFAULT CHARSET=latin1 COMMENT='a table to store dkp raids';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ticks`
--

DROP TABLE IF EXISTS `ticks`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ticks` (
  `clientId` varchar(255) NOT NULL,
  `tick_id` int(11) NOT NULL AUTO_INCREMENT,
  `raid_id` int(11) NOT NULL,
  `value` double NOT NULL DEFAULT '0',
  `description` varchar(45) DEFAULT NULL,
  PRIMARY KEY (`tick_id`,`clientId`),
  UNIQUE KEY `tick_id_UNIQUE` (`tick_id`),
  KEY `fkey_entries_raids_id_idx` (`raid_id`),
  KEY `fkey_entries_clientId_idx` (`clientId`),
  CONSTRAINT `fkey_entries_clientId` FOREIGN KEY (`clientId`) REFERENCES `clients` (`clientId`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fkey_entries_raids_id` FOREIGN KEY (`raid_id`) REFERENCES `raids` (`id_raid`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=7421 DEFAULT CHARSET=latin1 COMMENT='a table to correlate characters to raids';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ticks_x_characters`
--

DROP TABLE IF EXISTS `ticks_x_characters`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ticks_x_characters` (
  `clientId` varchar(255) NOT NULL,
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `id_character` int(11) NOT NULL,
  `id_tick` int(11) NOT NULL,
  PRIMARY KEY (`id`,`clientId`),
  UNIQUE KEY `id_UNIQUE` (`id`),
  KEY `fkey_tick_charId_idx` (`id_character`),
  KEY `fkey_tick_tickId_idx` (`id_tick`),
  KEY `fkey_tick_clientId_idx` (`clientId`),
  CONSTRAINT `fkey_tick_charId` FOREIGN KEY (`id_character`) REFERENCES `characters` (`id_character`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fkey_tick_clientId` FOREIGN KEY (`clientId`) REFERENCES `clients` (`clientId`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fkey_tick_tickId` FOREIGN KEY (`id_tick`) REFERENCES `ticks` (`tick_id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=529541 DEFAULT CHARSET=latin1 COMMENT='a table to connect ticks to characters';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `user_requests`
--

DROP TABLE IF EXISTS `user_requests`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `user_requests` (
  `clientId` varchar(255) NOT NULL,
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `requestor` varchar(45) NOT NULL,
  `request_type` int(11) NOT NULL,
  `request_status` int(11) NOT NULL,
  `request_details` longtext NOT NULL,
  `request_timestamp` datetime NOT NULL,
  `request_approver` varchar(45) NOT NULL,
  `reviewed_timestamp` datetime NOT NULL,
  PRIMARY KEY (`id`,`clientId`),
  UNIQUE KEY `id_UNIQUE` (`id`),
  KEY `fkey_userrequests_clientId_idx` (`clientId`),
  CONSTRAINT `fkey_userrequests_clientId` FOREIGN KEY (`clientId`) REFERENCES `clients` (`clientId`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=74 DEFAULT CHARSET=latin1 COMMENT='a table to hold pending requests from users to admins';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `user_x_character`
--

DROP TABLE IF EXISTS `user_x_character`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `user_x_character` (
  `clientId` varchar(255) NOT NULL,
  `user` varchar(45) NOT NULL,
  `id_character` int(11) NOT NULL,
  `approved_by` varchar(45) NOT NULL,
  PRIMARY KEY (`user`,`id_character`,`clientId`),
  KEY `userCharFkey_idx` (`id_character`),
  KEY `fkey_clientId_idx` (`clientId`),
  CONSTRAINT `fkey_userxchar_clientId` FOREIGN KEY (`clientId`) REFERENCES `clients` (`clientId`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `userCharFkey` FOREIGN KEY (`id_character`) REFERENCES `characters` (`id_character`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COMMENT='a table to relate users to their characters';
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2019-03-08 13:45:24
