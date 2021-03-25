<?php
/***
 * A simple script to convert a webdht
 * challenge from one jobid to another.
 */
function dhtKey($jobid, $from, $to)
{
    $key = $jobid."_node_".$from."_$to";
    echo $key."\n";
    $key = hash("sha1", $key);
    echo $key."\n";
    return $key;
}

define("OLDID", "AF9F53A080FF98A63C78A354C8823AAC37CABCBC");
define("NEWID", "D2513044EAE2E17D1661715B975C30414BA1FDB0");

function convert($from, $to)
{
    $key = dhtKey(OLDID, $from, $to);
    $query = mysql_query("SELECT value, version FROM CrypTool2 WHERE vkey = '$key'");

    echo "Fetching from $from to $to\n";
    $num = mysql_num_rows($query);
    echo "Got $num entries\n";
    if($num == 0)
        return;
    
    $value = mysql_result($query, 0, 0);
    $version = mysql_result($query, 0, 1) +1;
    $len = strlen($value);
    if($len < 32)
    {
        echo "got invalid entry, len =$len\n";
        return;
    }
    // plain data without hash
    $data = substr($value, 0, $len-32);
    $oldhash = substr($value, $len-32, 32);

    //sanity check
    $calc_oldhash = hash("sha256", $data.OLDID, true);
    if($calc_oldhash != $oldhash)
    {
        echo "invalid old hash!";
        return;
    }

    $calc_newhash = hash("sha256", $data.NEWID, true);

    $newvalue = mysql_real_escape_string($data.$calc_newhash);
    $newkey = dhtKey(NEWID, $from, $to);
    echo "writing $newkey\n";
    mysql_query("REPLACE INTO CrypTool2 VALUES ('$newkey', '$newvalue', $version)");
}

mysql_connect("localhost", "user", "password");
mysql_select_db("mysqldht");

// call convert here with all nodes!
// convert(0,16383);
// convert(0,8191);
// ...
?>

