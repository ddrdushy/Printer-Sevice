<?php
     
error_reporting(E_ALL);

/* Get the port for the WWW service. */
$service_port = 11000;

/* Get the IP address for the target host. */
$address = gethostbyname('localhost');

/* Create a TCP/IP socket. */
$socket = socket_create(AF_INET, SOCK_STREAM, SOL_TCP);
if ($socket === false) {
    echo "socket_create() failed: reason: " . 
         socket_strerror(socket_last_error()) . "\n";
}

echo "Attempting to connect to '$address' on port '$service_port'...";
$result = socket_connect($socket, $address, $service_port);
if ($result === false) {
    echo "socket_connect() failed.\nReason: ($result) " . 
          socket_strerror(socket_last_error($socket)) . "\n";
}

$homepage = file_get_contents('http://pngimg.com/upload/small/apple_PNG12429.png');
$homepage=base64_encode($homepage);
$ftype="png";
$printer="img";
$fname="name";
$jsonData="{\"ftype\":\"".$ftype."\",\"fname\":\"".$fname."\",\"printer\":\"".$printer."\",\"data\":\"".$homepage."\"}";
echo $jsonData ;

echo "Sending HTTP HEAD request...";
socket_write($socket, $jsonData, strlen($jsonData));
echo "OK.\n";


socket_close($socket);
?> 