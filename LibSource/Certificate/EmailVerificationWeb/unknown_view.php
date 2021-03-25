<?php echo '<?xml version="1.0" encoding="iso-8859-1"?>'; ?>
<html>
<head>
	<meta http-equiv="Content-Type" content="text/html; charset=iso-8859-1" />
	<title>Email Verification</title>
	<style media="screen" type="text/css">
		table{
			font-family: Verdana, sans-serif, Helvetica,  Arial ;
		}
	</style>
</head>
<body topmargin="50">
<center>
	<table border="0" width="500" height="500">
		<tr>
			<td align="center" valign="top">
				<?php echo $THANK_YOU ?>.<br>
				<?PHP echo $DATA_ACTIVATED ?>:
				<br>
				<br>
				<img src="images/account.png"><br>
				<b><?PHP echo $USERNAME ?>: <?php echo "$avatar"; ?></b><br>
				<b><?PHP echo $EMAIL ?>: <?php echo "$email"; ?></b>
			</td>
		</tr>
	</table>
</center>
</body>
</html>
