
param(
[string] $IP = "127.0.0.1",
[int] $Port = 514,
[string] $SampleFile = ".\syslogsample.csv",
[int] $SleepTimer = 1000
)

$i = import-csv $SampleFile


[System.Net.Sockets.UdpClient] $u = New-Object -TypeName System.Net.Sockets.UdpClient 

$Address = [System.Net.IPAddress]::Parse($IP)

$fac = @{ kernel = 0;
userlevel = 1;
mailsystem = 2;
systemdaemons = 3;
authorization = 4;
syslog = 5;
printer = 6;
news = 7;
uucp = 8;
clock = 9;
securityauth = 10;
ftp = 11;
ntp = 12;
logaudit = 13;
logalert = 14;
clockdaemon = 15;
local0 = 16;
local1 = 17;
local2 = 18;
local3 = 19;
local4 = 20;
local5 = 21;
local6 = 22;
local7 = 23;
}

$sev = @{emergency = 0;
alert = 1;
critical = 2;
error = 3;
warning = 4;
notice = 5;
informaitonal = 6;
debug = 7;
}

#$i[0].Context
#$i[0].Severity
#$i[0].Message
#$i[0].Hostname
#$i[0].Facility
#$i[0].IPAddress

foreach ($j in $i){

	#Pri is the same as 8*(Facility) + Severity
	
	[int] $f = if($fac[$j.Facility.ToLower()] -ne $null){ [int]$fac[$j.Facility.ToLower()] }else{ 17 }
	[int] $s =  [int]$sev[$j.Severity.tolower()]
	[int] $pri = ($f * 8) + $s

	[string] $d = [DateTime]$j.Time | get-date -Format "MMM dd hh:mm:ss"

	[string] $o = "<" + $pri.ToString() + "> " + $d + " " + $j.hostname  + " "  + $j.message

	#for($x = 1; $x -lt 20; $o += $j.message){}
	

	$b = [byte[]]$o.ToCharArray()
	$o
	$u.Send($b, $b.Length,$Address,$Port)
	sleep -Milliseconds $SleepTimer
}
