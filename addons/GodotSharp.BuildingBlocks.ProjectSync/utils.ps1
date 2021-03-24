function Get-RelativePath($path, $from) {
    Push-Location $from
    try { Resolve-path -Relative $path }
    finally { Pop-Location }
}

function Format-Json([Parameter(ValueFromPipeline)]$json) {
    $indent = 0
    $result = ($json -Split '\n' |% {
        if ($_ -match '[\}\]]') { $indent-- }
        $line = (' ' * $indent * 2) + $_.TrimStart().Replace(':  ', ': ')
        if ($_ -match '[\{\[]') { $indent++ }
        $line
    }) -Join "`n"
    $result.Replace('\u0027', "'").Replace('\u003c', "<").Replace('\u003e', ">").Replace('\u0026', "&")
}
