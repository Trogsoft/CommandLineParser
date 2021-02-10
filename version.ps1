$cd = get-date
$sd = get-date -Hour 0 -Minute 0 -Second 0
$ts = $cd - $sd
$bn = ($ts.TotalSeconds % 3600).toString("0000")
$rn = $Env:BuildID
$ver = "0.1.$rn.$bn";

Write-Host "0.1";
Write-Host $rn;
Write-Host $bn;
Write-Host "Version = $ver"

Write-Host "##vso[task.setvariable variable=ver;isOutput=true]$($ver)"