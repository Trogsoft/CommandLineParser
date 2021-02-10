$cd = get-date
$sd = get-date -Hour 0 -Minute 0 -Second 0
$ts = $cd - $sd
$bn = ($ts.TotalSeconds % 3600)
$rn = $Env:BuildID

$Env:VER = "0.1.$rn.$bn";

