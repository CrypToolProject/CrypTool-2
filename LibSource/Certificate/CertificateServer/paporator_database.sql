-- phpMyAdmin SQL Dump
-- version 3.2.0.1
-- http://www.phpmyadmin.net
--
-- Host: localhost
-- Erstellungszeit: 23. März 2011 um 12:20
-- Server Version: 5.1.36
-- PHP-Version: 5.3.0

SET SQL_MODE="NO_AUTO_VALUE_ON_ZERO";

--
-- Datenbank: `certificates`
--

-- --------------------------------------------------------

--
-- Tabellenstruktur für Tabelle `ca`
--

CREATE TABLE IF NOT EXISTS `ca` (
  `serial` bigint(20) unsigned NOT NULL,
  `dn` varchar(250) NOT NULL,
  `dateofissue` datetime NOT NULL,
  `dateofexpire` datetime NOT NULL,
  `ca_pkcs12` blob NOT NULL,
  `tls_pkcs12` blob NOT NULL,
  PRIMARY KEY (`serial`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Tabellenstruktur für Tabelle `peers`
--

CREATE TABLE IF NOT EXISTS `peers` (
  `serialnumber` bigint(20) unsigned NOT NULL,
  `email` varchar(60) NOT NULL,
  `avatar` varchar(40) NOT NULL,
  `world` varchar(40) NOT NULL,
  `dateofissue` datetime NOT NULL,
  `dateofexpire` datetime NOT NULL,
  `caSerialnumber` bigint(20) unsigned NOT NULL,
  `certificate` blob,
  `pkcs12` blob NOT NULL,
  `pwdCode` varchar(60) DEFAULT NULL,
  `datePasswordReset` datetime NOT NULL,
  `programName` varchar(60) DEFAULT NULL,
  `programVersion` varchar(80) DEFAULT NULL,
  `optionalInfo` varchar(255) DEFAULT NULL,
  `extensions` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`email`,`caSerialnumber`),
  UNIQUE KEY `avatarCaSerial` (`serialnumber`,`avatar`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Tabellenstruktur für Tabelle `registrationrequests`
--

CREATE TABLE IF NOT EXISTS `registrationrequests` (
  `email` varchar(60) NOT NULL,
  `avatar` varchar(40) NOT NULL,
  `world` varchar(40) NOT NULL,
  `dateofrequest` datetime NOT NULL,
  `caSerialnumber` bigint(20) unsigned NOT NULL,
  `verificationCode` varchar(60) DEFAULT NULL,
  `verified` tinyint(1) unsigned NOT NULL,
  `pwdHash` varchar(40) NOT NULL,
  `programName` varchar(60) DEFAULT NULL,
  `programVersion` varchar(80) DEFAULT NULL,
  `optionalInfo` varchar(255) DEFAULT NULL,
  `authorized` tinyint(1) unsigned NOT NULL,
  `extensions` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`email`,`caSerialnumber`),
  UNIQUE KEY `avatarCaSerial` (`avatar`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Tabellenstruktur für Tabelle `undeliveredemails`
--

CREATE TABLE IF NOT EXISTS `undeliveredemails` (
  `emailIndex` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `type` tinyint(1) NOT NULL,
  `caSerialnumber` bigint(20) unsigned NOT NULL,
  `email` varchar(60) NOT NULL,
  `dateOfAttempt` datetime NOT NULL,
  PRIMARY KEY (`emailIndex`)
) ENGINE=MyISAM  DEFAULT CHARSET=latin1 AUTO_INCREMENT=9 ;
