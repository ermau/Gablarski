<?php
// Copyright (c) 2010, Eric Maupin
// All rights reserved.

// Redistribution and use in source and binary forms, with
// or without modification, are permitted provided that
// the following conditions are met:

// - Redistributions of source code must retain the above
//   copyright notice, this list of conditions and the
//   following disclaimer.

// - Redistributions in binary form must reproduce the above
//   copyright notice, this list of conditions and the
//   following disclaimer in the documentation and/or other
//   materials provided with the distribution.

// - Neither the name of Gablarski nor the names of its
//   contributors may be used to endorse or promote products
//   derived from this software without specific prior
//   written permission.

// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS
// AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED
// WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
// PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE
// GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
// THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
// DAMAGE.

$host = 'localhost';
$port = 6112;
$serverInfoOnly = false;

error_reporting (E_ALL);
define ('QueryServer', 34);
define ('QueryServerResult', 36);
define ('PermissionDenied', 32);

$socket = fsockopen ('udp://' . $host, $port, $errorNumber, $error);
if (!$socket)
	quit ($socket, $error);

$queryServerMessage = pack ('CLSC', 0x2A, 0, QueryServer, $serverInfoOnly);
fwrite ($socket, $queryServerMessage, strlen ($queryServerMessage));

$sanity = readByte ($socket);
if ($sanity != 42)
	quit ($socket, 'Sanity check failed');

$messageType = readUInt16 ($socket);
if ($messageType == PermissionDenied)
	quit ($socket, 'QueryServer permission denied');

if ($messageType != QueryServerResult)
	quit ($socket, 'Unknown response');

$users = array();
$channels = array();

if (!$serverInfoOnly)
{
	$userCount = readInt32 ($socket);
	for ($i = 0; $i < $userCount; $i++)
		$users[] = readUser ($socket);

	$channelCount = readInt32 ($socket);
	for ($i = 0; $i < $channelCount; $i++)
		$channels[] = readChannel ($socket);
}

$serverInfo = readServerInfo ($socket);
fclose ($socket);

$data = array ('users' => $users, 'channels' => $channels, 'server' => $serverInfo);

if (!isset ($_GET['format']) || $_GET['format'] == 'json')
{
	echo json_encode ($data);
}
else if ($_GET['format'] == 'debug')
{
	echo '<pre>',print_r ($data),'</pre>';
}

function readUser ($socket)
{
	return array(
		'UserId'            => readInt32 ($socket),
		'Username'          => readString ($socket),
		'CurrentChannelId'  => readInt32 ($socket),
		'Nickname'          => readString ($socket),
		'Phonetic'          => readString ($socket),
		'IsMuted'           => readByte ($socket),
		'Status'            => readByte ($socket),
		'Comment'           => readString ($socket)
	);
}

function readChannel ($socket)
{
	return array (
		'ChannelId'         => readInt32 ($socket),
		'ParentChannelId'   => readInt32 ($socket),
		'ReadOnly'          => readByte ($socket),
		'UserLimit'         => readInt32 ($socket),
		'Name'              => readString ($socket),
		'Description'       => readString ($socket)
	);
}

function readServerInfo ($socket)
{
	return array (
		'Name'          => readString ($socket),
		'Description'   => readString ($socket),
		'Logo'          => readString ($socket),
		'Passworded'    => readByte ($socket)
	);
}

function quit ($socket, $message)
{
	fclose ($socket);
	die ($message);
}

function read ($socket, $len)
{
	$responseBuffer = fread ($socket, $len);
	if (strlen ($responseBuffer) == 0)
		quit ($socket, 'Terminated early');

	return $responseBuffer;
}

function readInt32 ($socket)
{
	$buffer = read ($socket, 4);
	$values = unpack ('l', $buffer);

	return $values[1];
}

function readUInt16 ($socket)
{
	$buffer = read ($socket, 2);
	$values = unpack ('S', $buffer);

	return $values[1];
}

function readString ($socket)
{
	$str = '';
	$char = '';

	while ($char != "\0")
	{
		$char = read ($socket, 1);

		if ($char != "\0")
			$str = $str . $char;
	}

	return $str;
}

function readByte ($socket)
{
	$buffer = read ($socket, 1);
	$values = unpack ('C', $buffer);

	return $values[1];
}

?>