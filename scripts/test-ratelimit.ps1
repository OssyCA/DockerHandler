param(
    [Parameter(Mandatory = $true)][string]$ApiKey,
    [string]$BaseUrl = "http://localhost:5064",
    [int]$Count = 15
)

function Invoke-Burst {
    param([string]$Label, [string[]]$Header)

    Write-Host "$Label" -NoNewline

    for ($i = 1; $i -le $Count; $i++) {
          $code = & curl.exe -s -k -o NUL -w "%{http_code}" @Header "$BaseUrl/containers"
        Write-Host " $code" -NoNewline
    }

    Write-Host ""
}

Invoke-Burst -Label "Utan nyckel:" -Header @()
Write-Host "  Forvantat: 10 st 401, sedan 429 resten av minuten."
Write-Host ""

Write-Host "Avvisat svar:"
& curl.exe -s -k -D - -o NUL "$BaseUrl/containers" | Select-String -Pattern "HTTP/|retry-after"
& curl.exe -s -k "$BaseUrl/containers"
Write-Host ""
Write-Host ""

Invoke-Burst -Label "Med nyckel: " -Header @("-H", "X-Api-Key: $ApiKey")
Write-Host "  Forvantat: samma kod hela vagen (200, eller 503 om Docker ar av). Gransen ar 60."
