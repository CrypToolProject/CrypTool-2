<?php
error_reporting(0);
include 'config.php';
$conn = mysql_connect($db_host, $db_username, $db_password);

if(strpos($_SERVER['HTTP_ACCEPT_LANGUAGE'],'de') !== FALSE){
	include 'locale.de.php';
}else{
	include 'locale.en.php';
}


if(!$conn || !mysql_select_db($db_name))
{
    header('HTTP/1.1 503 Service Temporarily Unavailable');
    echo('HTTP/1.1 503 Service Temporarily Unavailable');
    exit();
}

//Check if verificationCode is set - if not we stop the script and return a bad request
if(!isset($_GET['verificationCode'])){
    header('HTTP/1.1 400 Bad Request');
    echo"'HTTP/1.1 400 Bad Request - no verification code given";	
    exit();
}

//obtain data belonging to the verification code; if none can be fetched we do not have
//a valid verificationCode and return a bad request
$verificationCode = mysql_real_escape_string($_GET['verificationCode']);
$query = "SELECT email, avatar, world, programName FROM `registrationrequests` WHERE verificationCode = '$verificationCode'";
$res = mysql_query($query);

if(!$res || mysql_num_rows($res)==0)
{
	header('HTTP/1.1 400 Bad Request');
	echo"'HTTP/1.1 400 Bad Request - unnknown verification code given";	
	exit();
}

$row = mysql_fetch_array($res,MYSQL_ASSOC);
$email = $row["email"];
$avatar = $row["avatar"];
$world = $row["world"];
$programName = $row["programName"];

mysql_query("UPDATE `registrationrequests` SET verified=1 WHERE verificationCode = '$verificationCode'");

if(strpos(strtolower($programName),"cryp") !== FALSE){
	include 'ct2_view.php';
}else{
	include 'unknown_view.php';
}

?>
